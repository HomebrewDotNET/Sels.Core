using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using Sels.Core.Extensions;

namespace Sels.Core.Wpf.Components.Dialogs
{
    public static class DialogHelper
    {
        public static bool AskForConfirmation(string title, string message)
        {
            title.ValidateVariable(nameof(title));
            message.ValidateVariable(nameof(message));

            var confirmation = MessageBox.Show(message, title, System.Windows.MessageBoxButton.YesNo);

            return  confirmation == MessageBoxResult.Yes;
        }

        public static bool SelectFile(out FileInfo file, string filter = null)
        {
            file = null;

            var dialog = new OpenFileDialog();

            if (filter.HasValue()) dialog.Filter = filter;

            var result = dialog.ShowDialog();

            if(result.HasValue && result.Value)
            {
                file = new FileInfo(dialog.FileName);

                return true;
            }

            return false;
        }

        public static bool SelectSaveLocation(out FileInfo file, string filter = null)
        {
            file = null;

            var dialog = new SaveFileDialog();

            if (filter.HasValue()) dialog.Filter = filter;

            var result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                file = new FileInfo(dialog.FileName);

                return true;
            }

            return false;
        }

        public static void SaveFile(FileInfo file, string defaultSaveName = null)
        {
            if(file != null && file.Exists)
            {
                var dialog = new SaveFileDialog();
                dialog.DefaultExt = file.GetExtensionName();
                dialog.InitialDirectory = Directory.GetCurrentDirectory();
                dialog.FileName = defaultSaveName.HasValue() ? $"{defaultSaveName}{file.Extension}" : file.Name;

                var result = dialog.ShowDialog();

                if (result.HasValue && result.Value)
                {
                    file.CopyTo(dialog.FileName);
                }
            }
        }
    }
}
