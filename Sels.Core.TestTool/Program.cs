using Sels.Core.Components.Backup;
using Sels.Core.Components.Console;
using Sels.Core.Components.Filtering.ObjectFilter;
using Sels.Core.Components.RecurrentAction;
using Sels.Core.Extensions.Execution.Linq;
using Sels.Core.Extensions.Io.FileSystem;
using Sels.Core.TestTool.Filter;
using Sels.Core.TestTool.RecurrentActions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Sels.Core.TestTool
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleHelper.Run(FilterTest);
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
            var personFilter = new ObjectFilter<Person, Person>();

            var persons = new List<Person>()
            {
                new Person()
                {
                    FirstName = "Jens",
                    LastName = "Sels"
                },
                new Person()
                {
                    FirstName = "Lynn",
                    LastName = "Laridon"
                }
            };

            var filterPerson = new Person()
            {
                FirstName = "jens"
            };

            var filteredPersons = personFilter.Filter(filterPerson, persons);

            Console.WriteLine($"Filter was: {nameof(Person.FirstName)}: {filterPerson.FirstName}, {nameof(Person.LastName)}: {filterPerson.LastName}");

            Console.WriteLine($"Persons: {persons.Count}, Filtered: {filteredPersons.Count()}");

            foreach(var filteredPerson in filteredPersons)
            {
                Console.WriteLine($"{filteredPerson.FirstName} {filteredPerson.LastName} remained after filtering");
            }
        }
    }
}
