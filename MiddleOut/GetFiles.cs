using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MiddleOut
{
    public class GetFiles
    {
        public static List<string> FilesToCompress = new List<string>();

        public static List<string> GetAllFiles(string files)
        {
            List<string> compressListTemp = new List<string>();

            // If a single file is passed, just add that right away
            if (File.Exists(files))
                compressListTemp.Add(files);
            
            // If they pass more than one file with a comma between
            else if (files.Contains(','))
            {
                compressListTemp = files.Split(',').ToList();
            }

            else
            {
                compressListTemp.Add(files);
            }

            // Let's go through the temporary list and determine what we got in there
            List<string> filesToCompressTemp = ParseAllFiles(compressListTemp);

            foreach (var file in filesToCompressTemp)
            {
                if (TestForFile(file))
                    FilesToCompress.Add(file);
            }

            return FilesToCompress;
        }

        private static List<string> ParseAllFiles(List<string> tempInputList)
        {
            List<string> compressListTemp = new List<string>();
            List<string> wildcardList = new List<string>();
            string currentDirectory = Directory.GetCurrentDirectory();

            foreach (string item in tempInputList)
            {
                string sanitizedPath;
                if (item.EndsWith("\\") || item.EndsWith("\\*"))
                    sanitizedPath = item.TrimEnd(item[item.Length - 1]);
                else
                    sanitizedPath = item;

                // We need to grab the directory and all subdirectories
                if (Directory.Exists(sanitizedPath))
                {
                    foreach (var a in (Directory.GetFiles(sanitizedPath, "*", SearchOption.AllDirectories)))
                    {
                        try
                        {
                            wildcardList.Add(Path.GetFullPath(a));
                        }

                        catch
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
                            compressListTemp.Add(i);
                        }
                    }
                }

                // If all items in a given directory or current directory are to be zipped, need to account for the local zip file creation
                else if (Equals(item, "*")) 
                {
                    try
                    {
                        foreach (var a in Directory.GetFiles(currentDirectory, "*", SearchOption.AllDirectories))
                        {
                            wildcardList.Add(a);
                        }
                    }

                    catch
                    {
                        //pass
                    }

                    compressListTemp.Remove(item);
                    foreach (string i in wildcardList)
                    {
                        if (Equals(i, "*"))
                        {
                            //pass
                        }

                        else
                        {
                            compressListTemp.Add(i);
                        }
                    }
                }
            }

            return compressListTemp;
        }

        public static List<string> FileImport(string importFile)
        {
            //If an input file was passed, add values to compression list
            if (!string.IsNullOrEmpty(importFile) && File.Exists(importFile))
            {

                using (StreamReader rdr = new StreamReader(importFile))
                {
                    string line;
                    while ((line = rdr.ReadLine()) != null)
                    {
                        if (TestForFile(line))
                            FilesToCompress.Add(line);
                    }
                }
            }

            return FilesToCompress;
        }

        private static bool TestForFile(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                return true;
            }

            return false;
        }
    }
}