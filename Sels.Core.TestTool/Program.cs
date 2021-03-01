using Sels.Core.Components.Backup;
using Sels.Core.Components.Console;
using Sels.Core.Components.Filtering.ObjectFilter;
using Sels.Core.Components.RecurrentAction;
using Sels.Core.Extensions;
using Sels.Core.TestTool.Filter;
using Sels.Core.TestTool.RecurrentActions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Sels.Core.Excel.Extensions;
using Sels.Core.Excel.Export;
using Sels.Core.Excel.Export.Definitions;
using Sels.Core.Excel;
using Sels.Core.TestTool.ExportEntities;
using Sels.Core.Excel.Export.Definitions.Tables;
using Sels.Core.Extensions.Io;

namespace Sels.Core.TestTool
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleHelper.Run(ExcelExportTest);
        }

        private static void DoRecurrentStuff()
        {
            var repeatingMethods = new RepeatingActionManager<string>();

            repeatingMethods.AddRecurrentAction(new RecurrentConsoleLogger("Jens", 1000, 5000, 5));
            repeatingMethods.AddRecurrentAction(new RecurrentConsoleLogger("Alex", 500, 8000, 3));
            repeatingMethods.AddRecurrentAction(new RecurrentConsoleLogger("Mafe", 1500, 3000, 4));

            repeatingMethods.StartAll();

            Console.ReadKey();

            repeatingMethods.StopAndWaitAll();
        }

        private static void DoBackUpStuff(int backUpCount)
        {
            const string backUpDirectoryName = "BackUps";
            const string backUpFileName = "BackUp.txt";

            BackupRetentionMode retentionMode = BackupRetentionMode.Amount;
            const int retentionValue = 10;

            var backUpDirectory = new DirectoryInfo(backUpDirectoryName);
            var backUpFile = new FileInfo(backUpFileName);

            var fileContent = Guid.NewGuid().ToString();

            backUpFile.Write(fileContent);

            Console.WriteLine($"Creating back up manager for {backUpFileName} in {backUpDirectory.FullName} with a retention of {retentionValue} (Mode: {retentionMode})");
            var backUpManager = new FileBackupManager(backUpFile, backUpDirectory, retentionMode, retentionValue);

            Console.WriteLine($"Creating back ups (Content: {fileContent})");

            for(int i = 0; i < backUpCount; i++)
            {
                backUpManager.CreateBackup();
            }

            backUpManager.Backups.ForceExecute(x => Console.WriteLine($"Backed Up File: Location: {x.BackedupFile.FullName} Successful:{x.Succesful} Backup Date: {x.BackupDate}"));

            Console.WriteLine($"Restoring earliest back up to {AppDomain.CurrentDomain.BaseDirectory}");

            var restoredBackUp = backUpManager.RestoreEarliestBackup(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory));

            Console.WriteLine($"Content from restored back up: {restoredBackUp.BackedupFile.Read()}");
        }

        private static void FilterTest()
        {
            var personFilter = new ObjectFilter<FilterPerson, FilterPerson>();

            var persons = new List<FilterPerson>()
            {
                new FilterPerson()
                {
                    FirstName = "Jens",
                    LastName = "Sels"
                },
                new FilterPerson()
                {
                    FirstName = "Lynn",
                    LastName = "Laridon"
                }
            };

            var filterPerson = new FilterPerson()
            {
                FirstName = "jens"
            };

            var filteredPersons = personFilter.Filter(filterPerson, persons);

            Console.WriteLine($"Filter was: {nameof(FilterPerson.FirstName)}: {filterPerson.FirstName}, {nameof(FilterPerson.LastName)}: {filterPerson.LastName}");

            Console.WriteLine($"Persons: {persons.Count}, Filtered: {filteredPersons.Count()}");

            foreach(var filteredPerson in filteredPersons)
            {
                Console.WriteLine($"{filteredPerson.FirstName} {filteredPerson.LastName} remained after filtering");
            }
        }

        private static void ExcelTest()
        {
            var list = new List<uint>()
            {
                1,
                27,
                126,
                8952,
                10000,
                731
            };

            foreach(var number in list)
            {
                var column = number.ToCellReference();
                var converted = column.ToCellIndex();

                Console.WriteLine($"Index {number} translates to column {column}. With reconverted value as {converted}");
            }
        }

        private static void ExcelExportTest()
        {
            var persons = new List<Person>()
            {
                new Person("Jens", "Sels", 1),
                new Person("Jarno", "Sels", 2),
                new Person("Lynn", "Laridon", 3),
                new Person("Jana", "Sels", 4),
                new Person("Peggy", "Van Bulck", 5),
                new Person("Jimmy", "Sels", 6),
                new Person("Brent", "Vanisterbecq", 7)
            };

            var jobs = new List<Job>()
            {
                new Job(1, ".NET Developer"),
                new Job(2, "Dakwerker"),
                new Job(3, "Studente"),
                new Job(4, "Scholier"),
                new Job(5, "Kassierster"),
                new Job(6, "Sales Manager"),
                new Job(7, "Analist")
            };

            const string excelPath = @"D:/Persons.xlsx";
            const string personSheet = "Persons";
            const string jobSheet = "Jobs";

            var exportProfile = new ExcelExportProfile();

            //exportProfile.AddTableExportDefinition<(string FirstName, string FamilyName, int JobId)>(personSheet, SeekMode.NewColumn, true, default
            //    ,("First Name", x => x.FirstName, CellType.String)
            //    ,("Family Name", x => x.FamilyName, CellType.String)
            //    ,("Job Title", x => x.JobId, CellType.Numeric));

            //exportProfile.AddTableExportDefinition<(int Id, string JobTitle)>(personSheet, SeekMode.NewColumn, true, default
            //    , ("Job Id", x => x.Id, CellType.Numeric)
            //    , ("Job Title", x => x.JobTitle, CellType.String));

            //exportProfile.AddTableExportDefinition<(int Id, string JobTitle)>(jobSheet, SeekMode.NewColumn, true, default
            //    , ("Job Id", x => x.Id, CellType.Numeric)
            //    , ("Job Title", x => x.JobTitle, CellType.String));

            exportProfile.AddAutoGeneratedTableExportDefinition<Person>(personSheet, SeekMode.NewColumn, true, default);

            exportProfile.AddAutoGeneratedTableExportDefinition<Job>(personSheet, SeekMode.NewColumnOnCurrentRow, true, default);

            var personTableExport = new ExcelTableExportDefinition<Person>(SeekMode.NewRow)
                                                                            .AddColumn("First Name", x => x.FirstName)
                                                                            .AddColumn("Last Name", x => x.LastName)
                                                                            .AddHyperlinkColumn("Profile Picture", x => $"{x.FirstName} {x.LastName}", x => $@"./ProfilePictures/{x.FirstName}{x.LastName}.jpg");

            exportProfile.AddExportDefinition(personSheet, personTableExport);

            exportProfile.AddAutoGeneratedTableExportDefinition<Job>(personSheet, SeekMode.NewColumnOnCurrentRow, true, default);



            exportProfile.AddAutoGeneratedTableExportDefinition<Job>(jobSheet, SeekMode.NewColumn, true, default);

            exportProfile.Export(excelPath, true, (null, persons), (null, jobs));
        }
    }
}
