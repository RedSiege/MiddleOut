using Ionic.Zip;
using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using System.IO;
using System.Linq;

namespace MiddleOut
{
    // C# version for compressing files, directories, etc on the fly

    class Program
    {
        public static string ZipFileName;

        public enum PathType { NonExisting = 0, File = 1, Directory = 2 };

        public class Options
        {
            public static Options Instance { get; set; }

            // Command line options
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose")]
            public bool Verbose { get; set; }

            [Option('f', "file", Group = "Input", Required = true, HelpText = "Specify a text file containing files to compress (one per line)")]
            public string File { get; set; }

            [Option('i', "input", Group = "Input", Required = true, HelpText = "Specify multiple files to compress (separated by commas)")]
            public string Input { get; set; }

            [Option('o', "output", Required = false, HelpText = "Specify a zip file to output to", Default = null)]
            public string Output { get; set; }

            [Option('p', "password", Required = false, HelpText = "Specify a password to encrypt the zip file", Default = null)]
            public string Password { get; set; }

            [Option('s', "split", Required = false, Default = 0, HelpText = "Specify file size to split on.  If the size exceeds this a separate file will be created.")]
            public int SplitSize { get; set; }
        }

        static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                h.Heading = "MiddleOut v 1.1"; //change header
                h.Copyright = ""; //change copyright text
                h.AutoVersion = false;
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            Console.WriteLine(helpText);
            System.Environment.Exit(1);
        }

        public static PathType GetPathType(string path)
        {
            if (File.Exists(path))
                return PathType.File;
            if (Directory.Exists(path))
                return PathType.Directory;
            return PathType.NonExisting;
        }

        public static void Zipper(string[] zipList, string password, string location, int? splitSize = null)
        {
            using (ZipFile zip = new ZipFile())
            {
                zip.Password = password;

                if (splitSize.HasValue)
                    zip.MaxOutputSegmentSize = splitSize.Value;

                foreach (var filename in zipList)
                {
                    try
                    {
                        zip.AddFile(filename);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        //pass
                    }
                    catch (ArgumentException)
                    {
                        //pass
                    }  
                }
                zip.Save(location);
            }
        }

        public static void Main(string[] args)
        {
            // Block to take in x flags with the final flag being the zip file name

            List<string> CompressList = new List<string>();
            List<string> wildcardList = new List<string>();
            string currentDirectory = Directory.GetCurrentDirectory();

            // Parse arguments passed
            var parser = new Parser(with =>
            {
                with.CaseInsensitiveEnumValues = true;
                with.CaseSensitive = false;
                with.HelpWriter = null;
            });

            var parserResult = parser.ParseArguments<Options>(args);
            parserResult.WithParsed(o =>
            {
                Options.Instance = o;
            })
                .WithNotParsed(errs => DisplayHelp(parserResult, errs));
            var options = Options.Instance;
            Console.WriteLine();

            ZipFileName = options.Output;

            //If file was passed, add values to compresslist
            if (!String.IsNullOrEmpty(options.File) && File.Exists(options.File))
            {
                try
                {
                    using (StreamReader rdr = new StreamReader(options.File))
                    {
                        string line;
                        while ((line = rdr.ReadLine()) != null)
                        {
                            CompressList.Add(line);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to parse file, try only having one file per line\nFull error:");
                    Console.WriteLine(e);
                }
            }

            //If a list of URLs was passed
            else if (!String.IsNullOrEmpty(options.Input))
            {
                if (options.Input.Contains(','))
                {
                    try
                    {
                        CompressList = options.Input.Split(',').ToList();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Unable to parse list of files, try removing whitespace before/after path\nFull error:");
                        Console.WriteLine(e);
                    }
                }
                else
                {
                    CompressList.Add(options.Input);
                }
            }

            List<string> CompressListTemp = CompressList.ToList();


            //Go through each item passed
            foreach (string item in CompressListTemp)
            {
                if (item.EndsWith("\\") || item.EndsWith("\\*"))
                {
                    string sanitizedPath = item.TrimEnd(item[item.Length - 1]);
                    if (Directory.Exists(sanitizedPath))
                    {
                        foreach (var a in (Directory.GetFiles(sanitizedPath, "*", SearchOption.AllDirectories)))
                        {
                            try
                            {
                                wildcardList.Add(Path.GetFullPath(a));
                            }
                            catch (UnauthorizedAccessException)
                            {
                                //pass
                            }
                            catch (ArgumentException)
                            {
                                //pass
                            }
                        }
                        foreach (string i in wildcardList)
                        {
                            if (Equals(i, "*"))
                            {
                                //pass
                            }
                            else
                            {
                                CompressList.Add(i);
                            }
                        }
                    }

                    else
                    {
                        Console.WriteLine("\n[*] ERROR: Invalid folder path provided but still continuing");
                    }
                }

                else if (Directory.Exists(item))
                {
                    try
                    {
                        foreach (var a in (Directory.GetFiles(item, "*", SearchOption.AllDirectories)))
                        {
                            wildcardList.Add(a);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        //pass
                    }
                    foreach (string i in wildcardList)
                    {
                        if (Equals(i, "*"))
                        {
                            //pass
                        }
                        else
                        {
                            CompressList.Add(i);
                        }
                    }
                }
            
                else if (item.Contains("*"))
                // If all items in a given directory or current directory are to be zipped, need to account for the local zip file creation
                {
                    if (Equals(item, "*"))
                    // If all files in current dir are to be zipped
                    {
                        try
                        {
                            foreach (var a in (Directory.GetFiles(currentDirectory, "*", SearchOption.AllDirectories)))
                            {
                                wildcardList.Add(a);
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            //pass
                        }

                        CompressList.Remove(item);
                        foreach (string i in wildcardList)
                        {
                            if (Equals(i, "*"))
                            {
                                //pass
                            }
                            else
                            {
                                CompressList.Add(i);
                            }
                        }
                    }
                }

                else
                // If single files are to be zipped
                {
                    if (File.Exists(item))
                    {
                        CompressList.Add(item);
                    }
                    else
                    {
                        Console.WriteLine("[-] ERROR: Invalid file provided but still continuing\n");
                    }
                }
            }

            if (String.IsNullOrEmpty(ZipFileName))
            {
                ZipFileName = "MiddleOut__" + DateTime.Now.ToString("M-dd-yyyy_HH-mm-ss") + ".zip";
            }

            //Do one final check (supports: MiddleOut.exe testfile test test) being executed twice in a row
            if (File.Exists(ZipFileName))
            {
                Console.WriteLine("\n[-] ERROR: Zip file already exists!\n[-] ERROR: Exiting...");
                Environment.Exit(1);
            }
            string[] compressArray;
            if (wildcardList.Count() > 0)
            {
                foreach (string item in wildcardList)
                    CompressList.Add(item);
            }
            compressArray = CompressList.Distinct().ToArray();

            if (options.SplitSize > 0)
                Zipper(compressArray, options.Password, Path.GetFullPath(ZipFileName), options.SplitSize);
            else
                Zipper(compressArray, options.Password, Path.GetFullPath(ZipFileName));

            Console.WriteLine("[+] Finished compression, save location: \n" + Path.GetFullPath(ZipFileName));
        }
    }
}
