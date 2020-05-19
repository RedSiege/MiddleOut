# MiddleOut

## This tool was created to compress files through the command line and will work with Cobalt Strike's execute-assembly.

To use, add in the files to compress (any amount), the second to last flag is the zip file name, and the final flag is an optional password. 

For some examples:

`MiddleOut.exe test.txt this.txt file.exe zipArchive.zip`

`MiddleOut.exe test.txt this.txt file.exe zipArchive.zip SecurePassword123`

`MiddleOut.exe * zipArchive.zip`

`MiddleOut.exe someDirectory\test.txt someOtherDirectory\* zipArchive.zip`
