using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace MiddleOut
{
    // C# version for compressing files, directories, etc on the fly

    class Program
    {
        public static void Main(string[] args)
        {
            // Block to take in x flags with the final flag being the zip file name
            string ZipFileName;
            List<string> ArgList = new List<string>();
            List<string> CompressList = new List<string>();
            string currentDirectory = Directory.GetCurrentDirectory();

            if(args.Length < 2)
            {
                    Console.WriteLine("Something wrong with aruguments :(");
                    Console.WriteLine("To use, add in the files to compress (any amount) and the final flag is the zip file name. For some examples:\n");
                    Console.WriteLine(" > MiddleOut.exe test.txt this.txt file.exe zipArchive.zip");
                    Console.WriteLine(" > MiddleOut.exe * zipArchive.zip");
                    Console.WriteLine(" > MiddleOut.exe someDirectory\\test.txt someOtherDirectory\\* zipArchive.zip");
                    return;
            }

            try
            {
                ZipFileName = args[args.Length - 1];
                if (ZipFileName.EndsWith(".zip"))
                {
                    //do nothing, it's a good file
                }
                else
                {
                    ZipFileName = ZipFileName + ".zip";
                }
                List<string> list = new List<string>(args);
                list.RemoveAt(args.Length - 1);
                ArgList = list;
            }

            catch
            {
                Console.WriteLine("Something wrong with aruguments :(");
                Console.WriteLine("To use, add in the files to compress (any amount) and the final flag is the zip file name. For some examples:\n");
                Console.WriteLine("MiddleOut.exe test.txt this.txt file.exe zipArchive.zip");
                Console.WriteLine("MiddleOut.exe * zipArchive.zip");
                Console.WriteLine("MiddleOut.exe someDirectory\\test.txt someOtherDirectory\\* zipArchive.zip");
                return;
            }

            if (File.Exists(ZipFileName))
            {
                Console.WriteLine("\n[*] ERROR: File already exists!\n[*] Error: Exiting...");
                return;
            }

            // Block to test if the file/directory is passed correctly and replace in array if not - Thanks Truncer :)
            // Also checks for existence of file/directory and exits if not there
            // Probably well overloaded but also checks for existence of a wildcard
            {
                foreach (string item in ArgList)
                {
                    if (item.EndsWith("\\") | item.EndsWith("\\*"))
                    {
                        string sanitizedPath = item.TrimEnd(item[item.Length - 1]);
                        if (Directory.Exists(sanitizedPath))
                        {
                            List<string> wildcardList = new List<string>(Directory.GetFiles(sanitizedPath, "*", SearchOption.AllDirectories));
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
                            Console.WriteLine("\n[*] ERROR: Invalid folder path provided!\n[*] Error: Exiting...");
                            return;
                        }
                    }

                    else if (Directory.Exists(item))
                    {
                        List<string> wildcardList = new List<string>(Directory.GetFiles(item, "*", SearchOption.AllDirectories));
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
                            List<string> wildcardList = new List<string>(Directory.GetFiles(currentDirectory, "*", SearchOption.AllDirectories));
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
                            Console.WriteLine("\n[*] ERROR: Invalid folder path provided!\n[*] Error: Exiting...");
                            return;
                        }
                    }
                }
            }

            // block to zip all files within the flag list
            using (Stream zipStream = new FileStream(Path.GetFullPath(ZipFileName), FileMode.Create, FileAccess.Write))
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                foreach (string singleFile in CompressList)
                {
                    if (File.Exists(singleFile))
                    {
                        if (Equals(singleFile, Path.GetFullPath(ZipFileName))) //need to escape the zip file that we created earlier
                        {
                            //pass    
                        }
                        else
                        {
                            //Console.WriteLine("Compressing file:" + CompressList[j] + "\n");
                            using (Stream fileStream = new FileStream(singleFile, FileMode.Open, FileAccess.Read))
                            using (Stream fileStreamInZip = archive.CreateEntry(singleFile).Open())
                                fileStream.CopyTo(fileStreamInZip);
                        }
                    }
                }
            Console.WriteLine("\n[+] Finished compression, exiting application...");
        }
    }
}
