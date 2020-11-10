# MiddleOut

To use, pass in the files to compress (any amount) or pass it a text file of files, and supply an optional password. MiddleOut also accepts UNC paths as well.

## This tool was created to compress files through the command line and will work with Cobalt Strike's execute-assembly.

## Usage

```
MiddleOut.exe
MiddleOut.exe -i test.txt
MiddleOut.exe -i test.txt,another.txt -o output.zip
MiddleOut.exe -i ..\..\SomeFolder\* -o outputfile.zip
MiddleOut.exe -f filesToCompress.txt -o output.zip -p password
```