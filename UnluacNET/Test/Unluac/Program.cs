using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnluacNET;

namespace Unluac
{
    class Program
    {
        static string version = "1.2.2";

        static void PrintError(string message)
        {
            Console.Error.WriteLine("  error: {0}", message);
        }

        static void PrintError(string message, params object[] args)
        {
            PrintError(String.Format(message, args));
        }

        static void PrintUsage(bool error)
        {
            var output = (error) ? Console.Error : Console.Out;
            output.WriteLine("  usage: unluac <inFile> [:-nolog] <:outFile>");
        }

        static void PrintVersion(bool error)
        {
            var output = (error) ? Console.Error : Console.Out;
            output.WriteLine("unluac v{0}", version);
        }

        private static LFunction FileToFunction(string fn)
        {
            using (var fs = File.Open(fn, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var header = new BHeader(fs);

                return header.Function.Parse(fs, header);
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintVersion(true);
                PrintError("no input file provided");
                PrintUsage(true);

                Environment.Exit(1);
            }
            else
            {
                var fn = args[0];
                LFunction lMain = null;

                //Console.BufferHeight = 2500;

                /* Error Levels
                 * 
                 * 0 - Successfully decompiled file
                 * 1 - An error occurred while processing the input file
                 * 2 - The specified output file could not be created
                 */

                try
                {
                    lMain = FileToFunction(fn);
                }
                catch (Exception e)
                {
                    PrintVersion(true);
                    PrintError(e.Message);

                    Environment.Exit(1);
                }

                var d = new Decompiler(lMain);

                d.Decompile();

                var writeLog = true;

                if (args.Length > 1)
                {
                    // skip first arg
                    for (int i = 1; i < args.Length; i++)
                    {
                        var arg = args[i];

                        if (arg.StartsWith("-"))
                        {
                            switch (arg.ToLower().TrimStart('-'))
                            {
                            case "nolog":
                                writeLog = false;
                                continue;
                            default:
                                // just ignore it
                                if (writeLog)
                                    Console.WriteLine("unknown argument '{0}'", arg);
                                continue;
                            }
                        }
                        else
                        {
                            /** PROGRAM MUST TERMINATE ONCE FINISHED **/
                            var filename = arg;

                            try
                            {
                                using (var writer = new StreamWriter(filename, false, new UTF8Encoding(false)))
                                {
                                    d.Print(new Output(writer));

                                    writer.Flush();

                                    if (writeLog)
                                        Console.WriteLine("successfully decompiled to '{0}'", filename);

                                    Environment.Exit(0);
                                }
                            }
                            catch (Exception e)
                            {
                                PrintVersion(true);
                                PrintError(e.Message);

                                Environment.Exit(2);
                            }
                            /** PROGRAM MUST BE TERMINATED BY THIS POINT **/
                        }
                    }
                }

                // no output file was specified, spit out to console
                d.Print();

                if (System.Diagnostics.Debugger.IsAttached)
                    Console.ReadKey();

                Environment.Exit(0);
            }
        }
    }
}
