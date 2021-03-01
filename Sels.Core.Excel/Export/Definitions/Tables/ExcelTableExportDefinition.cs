using DocumentFormat.OpenXml.Spreadsheet;
using Sels.Core.Components.Display.ObjectLabel;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Excel.Export.Definitions.Tables
{
    public class ExcelTableExportDefinition<TResource> : BaseExcelExportDefinition
    {
        // Fields
        private readonly List<ExcelTableColumnDefinition<TResource>> _columnDefinitions = new List<ExcelTableColumnDefinition<TResource>>();

        // Properties
        public bool GenerateHeaders { get; }

        public override Type ResourceType => typeof(IEnumerable<TResource>);

        public ExcelTableExportDefinition(SeekMode seekmode, bool generateHeaders = true, object resourceIdentifier = null) : base(seekmode, resourceIdentifier)
        {
            GenerateHeaders = generateHeaders;
        }

        #region Setup
        public ExcelTableExportDefinition<TResource> AutoGenerate()
        {
            return AutoGenerate(typeof(TResource));
        }

        public ExcelTableExportDefinition<TResource> AutoGenerate(Type type)
        {
            var properties = type.GetPublicProperties();
            var primitiveProperties = properties.Where(x => x.PropertyType.IsString() || (x.PropertyType.IsPrimitive || x.PropertyType.IsValueType && !x.PropertyType.IsEnumerable()));
            //var complexProperties = properties.Where(x => !x.PropertyType.IsString() && (!x.PropertyType.IsPrimitive && !x.PropertyType.IsEnumerable()));

            foreach (var property in primitiveProperties)
            {
                var header = property.GetLabel();
                var valueGetter = new Func<TResource, object>(x => property.GetValue(x));
                var cellType = GetCellTypeFromType(property.PropertyType);

                AddColumn(header, valueGetter, cellType);
            }

            //foreach (var property in complexProperties)
            //{
            //    AutoGenerate(property.PropertyType);
            //}

            return this;
        }

        public ExcelTableExportDefinition<TResource> AddColumn<TResult>(Func<TResource, TResult> valueGetter, Func<TResource, CellFormula> formulaGetter = null, string insertBefore = null)
        {
            valueGetter.ValidateVariable(nameof(valueGetter));

            CellType columnCellType = GetCellTypeFromType(typeof(TResult));

            return AddColumn(string.Empty, x => valueGetter(x), columnCellType, formulaGetter, insertBefore);
        }

        public ExcelTableExportDefinition<TResource> AddColumn<TResult>(string header, Func<TResource, TResult> valueGetter, Func<TResource, CellFormula> formulaGetter = null, string insertBefore = null)
        {
            valueGetter.ValidateVariable(nameof(valueGetter));

            CellType columnCellType = GetCellTypeFromType(typeof(TResult));

            return AddColumn(header, x => valueGetter(x), columnCellType, formulaGetter, insertBefore);
        }

        public ExcelTableExportDefinition<TResource> AddColumn(Func<TResource, object> valueGetter, CellType columnCellType, Func<TResource, CellFormula> formulaGetter = null, string insertBefore = null)
        {
            valueGetter.ValidateVariable(nameof(valueGetter));

            return AddColumn(string.Empty, valueGetter, columnCellType, formulaGetter, insertBefore);
        }

        public ExcelTableExportDefinition<TResource> AddColumn(string header, Func<TResource, object> valueGetter, CellType columnCellType, Func<TResource, CellFormula> formulaGetter = null, string insertBefore = null)
        {
            valueGetter.ValidateVariable(nameof(valueGetter));

            var columnDefinition = new ExcelTableColumnDefinition<TResource>(header, valueGetter, columnCellType, formulaGetter);

            if (insertBefore.HasValue())
            {
                var column = _columnDefinitions.FirstOrDefault(x => x.Header == insertBefore);

                if (column.HasValue())
                {
                    _columnDefinitions.Insert(_columnDefinitions.IndexOf(column), columnDefinition);

                    return this;
                }
            }

            _columnDefinitions.Add(columnDefinition);

            return this;
        }

        public ExcelTableExportDefinition<TResource> AddHyperlinkColumn(string header, Func<TResource, object> valueGetter, Func<TResource, string> hyperlinkGetter, string insertBefore = null)
        {
            valueGetter.ValidateVariable(nameof(valueGetter));
            hyperlinkGetter.ValidateVariable(nameof(hyperlinkGetter));

            return AddColumn(header, valueGetter, CellType.String, x => ExcelHelper.GetHyperlinkCellFormula(hyperlinkGetter(x), valueGetter(x).ToString()), insertBefore);
        }

        public ExcelTableExportDefinition<TResource> RemoveColumnWithHeader(string header)
        {
            header.ValidateVariable(nameof(header));

            var column = _columnDefinitions.FirstOrDefault(x => x.Header == header);

            if (column.HasValue())
            {
                _columnDefinitions.Remove(column);
            }

            return this;
        }
        #endregion

        #region Export
        public override void Export(ExcelCursor cursor, object resource)
        {
            cursor.ValidateVariable(nameof(cursor));
            resource.ValidateVariable(x => resource is IEnumerable<TResource>, () => $"{nameof(resource)} must be of type {typeof(TResource)}");

            Export(cursor, cursor.CurrentRow, cursor.CurrentColumn, (IEnumerable<TResource>) resource);
        }

        private void Export(ExcelCursor cursor, uint startRow, uint startColumn, IEnumerable<TResource> resource)
        {
            if(resource.HasValue() && _columnDefinitions.HasValue())
            {
                if (GenerateHeaders)
                {
                    CreateHeaders(cursor, startColumn);
                }

                foreach (var item in resource)
                {
                    foreach(var column in _columnDefinitions)
                    {
                        var cellValue = column.GetValue(item);
                        var cellFormula = column.GetCellFormula(item);
                        var cellType = column.ColumnCellType;

                        cursor.SetValue(cellValue?.ToString(), cellType, cellFormula);

                        cursor.MoveToNext();
                    }

                    cursor.MoveDown(startColumn);
                }

                // Set end position after table
                cursor.MoveTo(startRow, startColumn + _columnDefinitions.Count.ToUInt32());
            }
        }

        private void CreateHeaders(ExcelCursor cursor, uint startColumn)
        {
            foreach(var column in _columnDefinitions)
            {
                var cellValue = column.Header;

                cursor.SetValue(cellValue, CellType.String);

                cursor.MoveToNext();
            }

            cursor.MoveDown(startColumn);
        }
        #endregion
    }
}
