using Sels.Core.Extensions.Linq;
using Sels.Core.TestTool.Filter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Sels.Core.TestTool.ExportEntities;
using Sels.Core.Components.Parameters;
using Sels.Core.Components.Parameters.Parameters;
using Sels.Core.TestTool.Models;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Sels.Core.Components.Logging;
using Sels.Core.Localization;
using System.Reflection;

namespace Sels.Core.TestTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Helper.Console.Run(() => {
                LoggingServices.RegisterLogger(CreateLogger(LogLevel.Information));

                Localizer.Setup(x =>
                {
                    x.ScanIn(Assembly.GetExecutingAssembly()).UseLoggers(LoggingServices.Loggers);
                });

                var person = new PersonInfo<int>()
                {
                    Id = 1,
                    Name = "Jens",
                    FamilyName = "Sels",
                    BirthDay = DateTime.Parse("04/01/1998 00:13:45"),
                    IsGraduated = true
                };

                var type = person.GetType();

                var cultures = new string[] { "nl-BE", "en-US", null };

                foreach(var culture in cultures)
                {
                    LoggingServices.Log($"Localizing person in {culture ?? "Default"}");

                    LoggingServices.Log(Localizer.Object.Get(type, culture) + ':');
                    foreach(var property in new PropertyInfo[] { type.GetProperty(nameof(PersonInfo.Name)), type.GetProperty(nameof(PersonInfo.FamilyName)), type.GetProperty(nameof(PersonInfo.BirthDay)), type.GetProperty(nameof(PersonInfo.IsGraduated)) })
                    {
                        LoggingServices.Log($"{Localizer.Object.Get(property, culture)}: {property.GetValue(person)}");
                    }

                    LoggingServices.Log(Environment.NewLine);
                }
            });
        }

        internal static ILogger CreateLogger(LogLevel level = LogLevel.Trace)
        {
            return LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "hh:mm:ss ";
                }).SetMinimumLevel(level);
            }).CreateLogger("Console");
        }
    }
}
