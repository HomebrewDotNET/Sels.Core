using Sels.Core.Components.Backup;
using Sels.Core.Components.Console;
using Sels.Core.Components.Filtering.ObjectFilter;
using Sels.Core.Components.RecurrentAction;
using Sels.Core.Extensions.Linq;
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
using Sels.Core.Components.Parameters;
using Sels.Core.Components.Parameters.Parameters;
using Sels.Core.Components.Serialization.KeyValue;
using Sels.Core.TestTool.Models;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using Newtonsoft.Json;
using Sels.Core.Linux.Extensions;
using Sels.Core.Components.FileSizes.Byte;
using Sels.Core.Templates.FileSizes;
using Sels.Core.Components.FileSizes.Bit;
using Sels.Core.Components.FileSizes.Byte.Binary;
using Sels.Core.Components.FileSizes.Bit.Binary;
using Sels.Core.Linux.Components.FileSystem;
using Sels.Core.Linux.Components.LinuxCommand.Commands.PackageManager;
using Sels.Core.Linux.Components.LinuxCommand.Commands.Core;
using Sels.Core.Linux.Components.LinuxCommand.Commands;
using Sels.Core.Linux.Components.LinuxCommand.Commands.Screen;

namespace Sels.Core.TestTool
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleHelper.Run(TestLinuxDirectory);
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

            var personTableExport = new ExcelTableExportDefinition<Person>(x => x.SeekNextFreeRow())
                                                                            .AddColumn("First Name", x => x.FirstName)
                                                                            .AddColumn("Last Name", x => x.LastName)
                                                                            .AddHyperlinkColumn("Profile Picture", x => $"{x.FirstName} {x.LastName}", x => $@"./ProfilePictures/{x.FirstName}{x.LastName}.jpg");

            exportProfile.AddExportDefinition(personSheet, personTableExport);

            exportProfile.AddAutoGeneratedTableExportDefinition<Job>(personSheet, SeekMode.NewColumnOnCurrentRow, true, default);



            exportProfile.AddAutoGeneratedTableExportDefinition<Job>(jobSheet, SeekMode.NewColumn, true, default);

            exportProfile.Export(excelPath, true, (null, persons), (null, jobs));
        }

        private static void TestParameterizer()
        {
            // Constants
            const string EnvironmentName = "Environment";
            const string TesterName = "Tester";

            // Add global parameter
            GlobalParameters.AddParameter(EnvironmentName, () => Environment.MachineName);

            // Create parameterizer and add some test parameters
            var parameterizer = new Parameterizer()
                                        .AddParameter(TesterName, "Jens Sels");

            var text = @$"Hello from ${{{{{TesterName}}}}} in Environment ${{{{{EnvironmentName}}}}}.
            Today is ${{{{{DateTimeNowParameter.ParameterName}_dd/MM/yyyy}}}}.
            You request id is: ${{{{{NewGuidParameter.ParameterName}_One}}}}.
            You correlation id is: ${{{{{NewGuidParameter.ParameterName}}}}}.
            Remember to save your request id: ${{{{{NewGuidParameter.ParameterName}_One}}}}.";

            text = parameterizer.Apply(text);

            Console.WriteLine(text);
        }

        private static void DeserializeKeyValuePairs()
        {
            var data = @"
            Name: Jens
            FamilyName: Sels
            BirthDate: 1998-01-04
            Role: Developer
            Role: System Admin
            Earnings: 1950.96
            Earnings: 1037.24
            Earnings: 560.58
            IsGraduated: true
            ";

            var serializer = new KeyValueSerializer(settings: new KeyValueSerializerSettings(ParsingOption.TrimItem, ParsingOption.TrimKey, ParsingOption.TrimValue));

            var person = serializer.Deserialize<PersonInfo>(data);

            Console.WriteLine($"Result as json: {person.SerializeAsJson()}");
        }

        private static void TestAssignable()
        {
            var iListType = typeof(IList<string>);
            var list = new List<string>();

            Console.WriteLine($"{iListType} is assignable from {iListType}: {iListType.IsAssignableFrom(iListType)}");
            Console.WriteLine($"{iListType} is assignable from {list.GetType()}: {iListType.IsAssignableFrom(list.GetType())}");
            Console.WriteLine($"{iListType} is assignable to {list.GetType()}: {iListType.IsAssignableTo(list.GetType())}");
            Console.WriteLine($"{list} is assignable from {iListType}: {list.IsAssignableFrom(iListType)}");
            Console.WriteLine($"{list} is assignable to {iListType}: {list.IsAssignableTo(iListType)}");
        }

        private static void TestLinuxCommands()
        {
            Console.WriteLine(Environment.OSVersion);

            var packageCommand = new DpkgInfoCommand();

            var packages = new string[] { "sudo", "dpkg", "fail2ban", "ufw", "sels" };

            foreach (var package in packages)
            {
                packageCommand.PackageName = package;
                var result = packageCommand.Execute();

                Console.WriteLine("Command: " + packageCommand.BuildCommand());
                Console.WriteLine("Command result: " + result.SerializeAsJson());
                Console.WriteLine($"Command {result.Package} is {(result.IsInstalled ? "installed" : "not installed")}");
            }

            var lsCommand = new DynamicCommand("ls -la /mnt");
            Console.WriteLine("List result: " + lsCommand.Execute());
            var grepCommand = new GrepCommand("c");
            var teeCommand = new TeeCommand("/mnt/c/listResult.txt");

            var chainCommand = new ChainCommand(lsCommand, CommandChainer.Pipe, grepCommand, CommandChainer.Pipe, teeCommand);

            var chainResult = chainCommand.Execute();

            Console.WriteLine("Command: " + chainCommand.BuildCommand());
            Console.WriteLine("Command result: " + chainResult);

            // Screen
            packageCommand.PackageName = "screen";
            var screenInfo = packageCommand.Execute();

            if (screenInfo.IsInstalled)
            {
                var screenListCommand = new ScreenListCommand();
                var screenRunCommand = new ScreenRunCommand("sleep 60", "TestSleepName");

                Console.WriteLine("Screen run command: " + screenRunCommand.BuildCommand());
                screenRunCommand.Execute().GetResult();

                Console.WriteLine("Running screens: " + screenListCommand.Execute().GetResult().JoinStringNewLine());
            }
            else
            {
                Console.WriteLine("Screen is not installed");
            }
        }
    
        private static void TestFileSizes()
        {
            const long Bytes = 100000000;

            var kiloBytes = new KiloByte(Bytes);

            ConvertAndPrint<MegaByte>(kiloBytes);
            ConvertAndPrint<GigaByte>(kiloBytes);
            ConvertAndPrint<TeraByte>(kiloBytes);
            ConvertAndPrint<PetaByte>(kiloBytes);

            var fileSize = FileSize.CreateFromSize<TeraByte>(1);

            ConvertAndPrint<KiloByte>(fileSize);
            ConvertAndPrint<MegaByte>(fileSize);
            ConvertAndPrint<GigaByte>(fileSize);
            ConvertAndPrint<TeraByte>(fileSize);
            ConvertAndPrint<PetaByte>(fileSize);
            ConvertAndPrint<ExaByte>(fileSize);
            ConvertAndPrint<ZettaByte>(fileSize);
            ConvertAndPrint<YottaByte>(fileSize);

            ConvertAndPrint<KiloBit>(fileSize);
            ConvertAndPrint<MegaBit>(fileSize);
            ConvertAndPrint<GigaBit>(fileSize);
            ConvertAndPrint<TeraBit>(fileSize);
            ConvertAndPrint<PetaBit>(fileSize);
            ConvertAndPrint<ExaBit>(fileSize);
            ConvertAndPrint<ZettaBit>(fileSize);
            ConvertAndPrint<YottaBit>(fileSize);

            ConvertAndPrint<KibiByte>(fileSize);
            ConvertAndPrint<MebiByte>(fileSize);
            ConvertAndPrint<GibiByte>(fileSize);
            ConvertAndPrint<TebiByte>(fileSize);
            ConvertAndPrint<PebiByte>(fileSize);
            ConvertAndPrint<ExbiByte>(fileSize);
            ConvertAndPrint<ZebiByte>(fileSize);
            ConvertAndPrint<YobiByte>(fileSize);

            ConvertAndPrint<KibiBit>(fileSize);
            ConvertAndPrint<MebiBit>(fileSize);
            ConvertAndPrint<GibiBit>(fileSize);
            ConvertAndPrint<TebiBit>(fileSize);
            ConvertAndPrint<PebiBit>(fileSize);
            ConvertAndPrint<ExbiBit>(fileSize);
            ConvertAndPrint<ZebiBit>(fileSize);
            ConvertAndPrint<YobiBit>(fileSize);
        }

        private static void TestLinuxDirectory()
        {
            var directory = new LinuxDirectory("/mnt/g");

            Console.WriteLine("Directory path: " + directory.Source.FullName);
            Console.WriteLine("Free size bytes: " + directory.FreeSpace.ByteSize);
            Console.WriteLine("Free size GiB: " + directory.FreeSpace.ToSize<GibiByte>().Size);
            Console.WriteLine("Free size bytes from drive info: " + directory.Source.GetDriveInfo().AvailableFreeSpace);
        }

        private static void ConvertAndPrint<TFileSize>(FileSize fileSize) where TFileSize : FileSize, new()
        {
            var newFileSize = fileSize.ToSize<TFileSize>();

            Console.WriteLine($"{fileSize.ToDisplayString()} ({fileSize}) is {newFileSize.ToDisplayString()} ({newFileSize})");
        }
    }
}
