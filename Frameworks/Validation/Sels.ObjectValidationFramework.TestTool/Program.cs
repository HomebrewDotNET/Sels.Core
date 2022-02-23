using Sels.Core;
using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.TestTool.Objects;
using Sels.ObjectValidationFramework.TestTool.ValidationProfiles;
using System;
using System.Collections.Generic;

namespace Sels.ObjectValidationFramework.TestTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Helper.Console.Run(() =>
            {
                Console.WriteLine("Testing person validation");

                var profile = new PersonValidationProfile();

                var validPerson = new Person()
                {
                    FirstName = "Jens",
                    LastName = "Sels",
                    NickName = "Dragonborn",
                    Age = 22,
                    Gender = Gender.Male,
                    Parent = new Person()
                    {
                        FirstName = "Jimmy",
                        LastName = "Sels",
                        NickName = "Undefined",
                        Age = 46,
                        Gender = Gender.Male
                    }
                };

                var inValidPerson = new Person()
                {
                    Id = 1,
                    FirstName = "",
                    LastName = null,
                    Age = -19,
                    Children = new List<Person>() { 
                        new Person()
                        {
                            Id = 2,
                            FirstName = null,
                            LastName = "\t",
                            Age = 0,
                            Owner = new Person()
                            {
                                Id = 4,
                                Age = 999
                            }
                        },
                        new Person()
                        {
                            Id = 3,
                            FirstName = "Some name",
                            LastName = "Same last name",
                            Age = 36,
                            Gender = Gender.Male                        
                        }
                    }
                };

                var validErrors = profile.Validate(validPerson);

                Console.WriteLine($"Errors on valid person: {Environment.NewLine}{(validErrors.HasValue() ? validErrors.JoinStringNewLine() : "None")}");

                var inValidErrors = profile.Validate(inValidPerson);

                Console.WriteLine($"Errors on invalid person: {Environment.NewLine}{(inValidErrors.HasValue() ? inValidErrors.JoinStringNewLine() : "None")}");

            });
        }
    }
}
