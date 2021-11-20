using Sels.Core.Components.Console;
using Sels.Core.Components.Performance;
using Sels.ObjectValidationFramework.PerformanceTool.Entities.Simple;
using Sels.ObjectValidationFramework.PerformanceTool.PerformanceCase;
using Sels.ObjectValidationFramework.PerformanceTool.Profiles;
using Sels.ObjectValidationFramework.Templates.Profile;
using System;

namespace Sels.ObjectValidationFramework.PerformanceTool
{
    class Program
    {
        // Constants
        private const bool Verbose = false;

        static void Main(string[] args)
        {
            ConsoleHelper.Run(() =>
            {
                Console.WriteLine("Starting up");
                TestValidationPerformanceValidObject<Person, PersonValidationProfile>(SimpleObjects.ValidAdultPerson, "Validation performance test", "Valid Person validation", 10000);
                TestValidationPerformanceValidObject<Person, PersonValidationProfile>(SimpleObjects.InvalidAdultPerson, "Validation performance test", "Invalid Person validation", 10000);
                TestValidationPerformanceProfileCreation(10000);
            });
        }


        static void TestValidationPerformanceValidObject<TObject, TProfile>(TObject objectToValidate, string profilerName, string caseName, int numberOfRuns) where TProfile : ValidationProfile<string>, new()
        {
            using (var profiler = new PerformanceProfiler<string, string, string>(profilerName))
            {
                Console.WriteLine($"Setting up Performance Profiler {profiler.Identifier}");
                profiler.AddCase(new ValidationPerformanceCase<TObject>(caseName, numberOfRuns, objectToValidate, new TProfile(), Verbose));

                Console.WriteLine($"Running Performance Cases on {profiler.Identifier}");
                var results = profiler.RunAllCases();

                foreach (var result in results)
                {
                    Console.WriteLine(result);
                }
            }
        }

        static void TestValidationPerformanceProfileCreation(int numberOfRuns)
        {
            using (var profiler = new PerformanceProfiler<string, string, string>("Validation test profile creation"))
            {
                Console.WriteLine($"Setting up Performance Profiler {profiler.Identifier}");
                profiler.AddCase("Person Profile creation", numberOfRuns, x => { var profile = new PersonValidationProfile(); });

                Console.WriteLine($"Running Performance Cases on {profiler.Identifier}");
                var results = profiler.RunAllCases();

                foreach (var result in results)
                {
                    Console.WriteLine(result);
                }
            }
        }


    }
}
