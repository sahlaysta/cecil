Cecil
=====

Fork of the Cecil library: https://github.com/jbevain/cecil

When modifying an assembly, adding many embedded resources would use an equally large amount of RAM. This fork fixes that by using a temp file for the resource buffer, so disk is used instead of RAM

NOTE: This fork is not a fix for large embedded resource files, but rather many smaller embedded resource files. For the former purpose, the solution is to split it into multiple embedded resource files a reasonable size, say 16 mb per split

## Usage

Beforehand, set the static property: <code>Mono.Cecil.EmbeddedResource.EmbeddedResourceStream</code> as shown in this usage example

```c#
using Mono.Cecil;
using System;
using System.IO;

namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {
            //set temp file
            const string tempFilePath = @"C:\temp.tmp";
            var tempFile = new FileStream(
                tempFilePath,
                FileMode.Create,
                FileAccess.ReadWrite); //readwrite required
            Mono.Cecil.EmbeddedResource.EmbeddedResourceStream = tempFile;
            
            
            using (tempFile)
            {
                //read assembly
                var assembly = AssemblyDefinition.ReadAssembly(
                    @"C:\assembly.exe");

                //(do stuff)

                //write assembly
                using (assembly)
                {
                    assembly.Write(
                        @"C:\newassembly.exe");
                }
            }
            Mono.Cecil.EmbeddedResource.EmbeddedResourceStream = null;
            
            //delete temp file
            File.Delete(tempFilePath);
        }
    }
}
```
