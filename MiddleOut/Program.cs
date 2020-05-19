using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MiddleOut
{
    // C# version for compressing files, directories, etc on the fly

    class Program
    {
        public static string ZipFileName;
        public static string Pass;
        public static List<string> ArgList = new List<string>();

        public enum PathType { NonExisting = 0, File = 1, Directory = 2 };
        public static PathType GetPathType(string path)
        {
            if (File.Exists(path))
                return PathType.File;
            if (Directory.Exists(path))
                return PathType.Directory;
            return PathType.NonExisting;
        }

        static void ParseArgs(string[] arguments)
        {
            // Takes in x params with the second to last arg being the zip file and the last being 
            // an optional password

            try
            {
                Pass = arguments[arguments.Length - 1];

                if (GetPathType(Pass) == PathType.NonExisting)
                {
                    ZipFileName = arguments[arguments.Length - 2];
                    if (ZipFileName.EndsWith("\\") || ZipFileName.EndsWith("\\*"))
                    {
                        string sanitizedPath = ZipFileName.TrimEnd(ZipFileName[ZipFileName.Length - 1]);
                        ZipFileName = sanitizedPath;
                    }
                    if (GetPathType(ZipFileName) == PathType.NonExisting)
                    {
                        if (ZipFileName.EndsWith(".zip"))
                        {
                            //do nothing, it's a good filename
                        }
                        else
                        {
                            ZipFileName = ZipFileName + ".zip";
                        }
                    }
                    //else if (GetPathType(ZipFileName) == PathType.File)
                    //{
                    //    Console.WriteLine("\n[-] ERROR: Zip file already exists!\n[-] ERROR: Exiting...");
                    //    System.Environment.Exit(1);
                    //}
                    else
                    {
                        ZipFileName = arguments[arguments.Length - 1];
                        Pass = null;
                        if (ZipFileName.EndsWith(".zip"))
                        {
                            //do nothing, it's a good filename
                        }
                        else
                        {
                            ZipFileName = ZipFileName + ".zip";
                        }
                    }
                }

                if ((GetPathType(Pass)) == PathType.File || (GetPathType(Pass)) == PathType.Directory)
                {
                    Console.WriteLine("\n[-] ERROR: Zip file already exists or you didn't specify a zip file!\n[-] ERROR: Exiting...");
                    System.Environment.Exit(1);
                }
                else
                {
                    //System.Environment.Exit(1);
                }

                List<string> list = new List<string>(arguments);
                if (Pass != null)
                {
                    Console.WriteLine("\n[+] Continuing with password\n");
                    list.RemoveAt(list.Count - 1);
                    list.RemoveAt(list.Count - 1);
                }
                else
                {
                    Console.WriteLine("\n[+] Continuing without password\n");
                    list.RemoveAt(list.Count - 1);
                }
                ArgList = list;
            }

            catch (Exception e)
            {
                Console.WriteLine("\n[-] Something bad happened with the arguments passed");
                Console.WriteLine("Please see the exception below: \n\n");
                Console.WriteLine(e);
                return;
            }
        }

        public static void Zipper(string[] zipList, string password, string location)
        {
            using (ZipFile zip = new ZipFile())
            {
                zip.Password = password;
                foreach (var filename in zipList)
                {
                    try
                    {
                        zip.AddFile(filename);
                    }
                    catch (System.UnauthorizedAccessException)
                    {
                        //pass
                    }
                    catch (System.ArgumentException)
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

            if (args.Length < 2)
            {
                Console.WriteLine("\n[-] You didn't provide enough arguments");
                Console.WriteLine("\nTo use, add in the files to compress (any amount), the second to last flag is the zip file name and the final flag is the password (optional). For some examples:\n");
                Console.WriteLine(" > MiddleOut.exe test.txt this.txt file.exe zipArchive.zip");
                Console.WriteLine(" > MiddleOut.exe * zipArchive.zip");
                Console.WriteLine(" > MiddleOut.exe someDirectory\\test.txt someOtherDirectory\\* zipArchive.zip");
                return;
            }

            // Parse the arguments and go from there
            ParseArgs(args);

            foreach (string item in ArgList)
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
                                wildcardList.Add(a);
                            }
                            catch (System.UnauthorizedAccessException)
                            {
                                //pass
                            }
                            catch (System.ArgumentException)
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
                    catch (System.UnauthorizedAccessException)
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
                        catch (System.UnauthorizedAccessException)
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

            //Do one final check (supports: MiddleOut.exe testfile test test) being executed twice in a row
            if (GetPathType(ZipFileName) == PathType.File)
            {
                Console.WriteLine("\n[-] ERROR: Zip file already exists!\n[-] ERROR: Exiting...");
                System.Environment.Exit(1);
            }
            string[] compressArray;
            compressArray = CompressList.Distinct().ToArray();
            Zipper(compressArray, Pass, Path.GetFullPath(ZipFileName));

            Console.WriteLine("[+] Finished compression, save location: \n" + Path.GetFullPath(ZipFileName));
        }
    }
}
