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
    internal class Program
    {
        public static string ZipFileName;

        public class Options
        {
            public static Options Instance { get; set; }

            // Command line options
            [Option('f', "file", Group = "Input", Required = true, HelpText = "Specify a text file containing files to compress (one per line)")]
            public string File { get; set; }

            [Option('i', "input", Group = "Input", Required = true, HelpText = "Specify multiple files to compress (separated by commas)")]
            public string Input { get; set; }

            [Option('o', "output", Required = false, HelpText = "Specify a zip file to output to", Default = null)]
            public string Output { get; set; }

            [Option('p', "password", Required = false, HelpText = "Specify a password to encrypt the zip file", Default = null)]
            public string Password { get; set; }

            [Option('s', "split", Required = false, Default = 0, HelpText = "Specify file size to split on (ex -s 65536 to split in 65k segments).  If the size exceeds this a separate file will be created.")]
            public int SplitSize { get; set; }
        }

        private static void DisplayHelp<T>(ParserResult<T> result)
        {
            HelpText helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                h.Heading = "MiddleOut version 1.2"; //change header
                h.Copyright = ""; //change copyright text
                h.AutoVersion = false;
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            Console.WriteLine(helpText);
            System.Environment.Exit(1);
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
            List<string> compressionList = new List<string>();

            // Parse arguments passed
            Parser parser = new Parser(with =>
            {
                with.CaseInsensitiveEnumValues = true;
                with.CaseSensitive = false;
                with.HelpWriter = null;
            });

            ParserResult<Options> parserResult = parser.ParseArguments<Options>(args);
            parserResult.WithParsed(o =>
            {
                Options.Instance = o;
            })
                .WithNotParsed(errs => DisplayHelp(parserResult));
            Options options = Options.Instance;

            ZipFileName = options.Output;
            if (string.IsNullOrEmpty(ZipFileName))
                ZipFileName = "MiddleOut__" + DateTime.Now.ToString("M-dd-yyyy_HH-mm-ss") + ".zip";

            else if (Path.GetExtension(ZipFileName) != ".zip")
                ZipFileName += ".zip";

            try
            {
                // If file was passed, add values to compress list
                if (!string.IsNullOrEmpty(options.File) && File.Exists(options.File))
                    compressionList = GetFiles.FileImport(options.File);
                
                // If anything other than a file was passed, get it
                else if (!string.IsNullOrEmpty(options.Input))
                    compressionList = GetFiles.GetAllFiles(options.Input);
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                System.Environment.Exit(1);
            }

            //Do one final check so we don't overwrite the zip file
            if (File.Exists(ZipFileName))
            {
                Console.WriteLine("\n[-] ERROR: Zip file already exists!\n[-] ERROR: Exiting...");
                Environment.Exit(1);
            }

            // Let's only get distinct vals
            string[] compressArray = compressionList.Distinct().ToArray();

            Console.WriteLine("\n[+] Working...\n");

            if (compressArray.Length == 0)
            {
                Console.WriteLine("[-] Error: File to compress does not exist");
                System.Environment.Exit(1);
            }

            try
            {
                if (options.SplitSize > 0)
                    Zipper(compressArray, options.Password, Path.GetFullPath(ZipFileName), options.SplitSize);
                else
                    Zipper(compressArray, options.Password, Path.GetFullPath(ZipFileName));
            }

            catch (ZipException e)
            {
                Console.WriteLine("Error zipping the file. \nFull error: {0}", e);
                System.Environment.Exit(1);
            }

            catch (FileNotFoundException e)
            {
                Console.WriteLine("Error zipping after testing, this should never be thrown. \nFull error: {0}", e);
                System.Environment.Exit(1);
            }

            Console.WriteLine("[+] Finished compression, save location: \n" + Path.GetFullPath(ZipFileName));
        }
    }
}
