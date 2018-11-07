using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using ClientNamespace;
using CommonTypes.tuple;
using Tuple = CommonTypes.tuple.Tuple;

namespace ScriptClient
{
    public static class Program
    {
        public static void Main(string[] args)
        {

            var client = new Client();


            string inputFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"sampleClientScript.txt");
            
            if (args.Length > 1)
                inputFile = args[0];


            var instructions = System.IO.File.ReadAllLines(inputFile);

            ProcessLines(instructions, 1);

            // process the lines with instructions n times
            void ProcessLines(string[] lines, int n)
            {
                var onRepeat = false;
                var repeatCycle = 0;
                var repeatInstructions = new List<string>();

                for (var i = 0; i < n; i++)
                {
                    foreach (var line in lines)
                    {
                        var splitLine = line.Split(' ');

                        // if inside a begin-repeat
                        if (onRepeat)
                        {
                            if (splitLine[0] != "end-repeat")
                            {
                                repeatInstructions.Add(line);
                                continue;
                            }

                            ProcessLines(repeatInstructions.ToArray(), repeatCycle);
                            onRepeat = false;
                            repeatCycle = 0;
                            repeatInstructions.Clear();
                            continue;
                        }

                        switch (splitLine[0])
                        {
                            case "add":
                                Tuple tupleToAdd = ParseTuple(splitLine[1]);
                                //client.Write(tupleToAdd);
                                Console.WriteLine("Added: " + tupleToAdd);
                                break;
                            case "read":
                                TupleSchema tupleSchemaToRead = new TupleSchema(ParseTuple(splitLine[1]));
                                Tuple tupleToRead = tupleSchemaToRead.Schema;
                                //client.Read(tupleToRead); // TODO should be tupleSchema in read arguments
                                Console.WriteLine("Tried to Read: " + tupleToRead + ", and got: " +
                                                  "<insert here tuple>"); // TODO should receive a Tuple
                                break;
                            case "take":
                                TupleSchema tupleSchemaToTake = new TupleSchema(ParseTuple(splitLine[1]));
                                Tuple tupleToTake = tupleSchemaToTake.Schema;
                                //client.Read(tupleToTake); // TODO should be tupleSchema in take arguments
                                Console.WriteLine("Tried to Take: " + tupleToTake + ", and got: " +
                                                  "<insert here tuple>"); // TODO should receive a Tuple
                                break;
                            case "wait":
                                var time = int.Parse(splitLine[1]);
                                Thread.Sleep(time);
                                break;
                            case "begin-repeat":
                                onRepeat = true;
                                repeatCycle = int.Parse(splitLine[1]);
                                break;
                            case "end-repeat":
                                throw new NotSupportedException();
                        }
                    }
                }
            }

            // parses a string representation of a tuple 
            Tuple ParseTuple(string stringTuple)
            {
                var fields = new List<object>();

                var matches = Regex.Matches(stringTuple, "\"[^\"]+\"|\\w+[^\\)]+\\)");
                foreach (var match in matches)
                {
                    var field = match.ToString();
                    // if it's a string
                    if (field[0].Equals('"'))
                    {
                        // add to the list of object (the tuple fields)
                        fields.Add(field.Trim('"'));
                    }
                    // if it's an object
                    else
                    {
                        object[] classArgs = field.Split("(),\"".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        var classType = classArgs[0];
                        // convert strings to int if applicable
                        classArgs = Array.ConvertAll(classArgs.Skip(1).ToArray(),
                            s => int.TryParse(s.ToString(), out var i) ? i : s);

                        // instantiate dadTestObject using the arguments provided 
                        var dadTestObject = Activator.CreateInstance(
                            Type.GetType("CommonTypes.tuple.tupleobjects." + classType +
                                         ", CommonTypes, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null.") ??
                            throw new InvalidOperationException("Could not create instance using classType: " + classType)
                            , classArgs);

                        // add to the list of object (the tuple fields)
                        fields.Add(dadTestObject);
                    }
                }
                return new Tuple(fields);
            }

        }
    }
}