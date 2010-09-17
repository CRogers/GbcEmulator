using System.Diagnostics;
using System.IO;

namespace OpcodeInfoExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Debugger.IsAttached)
                args = new[] { "opcodes.txt" };
                //args = new[] {"-t", "opmap.txt", "cbopmap.txt", "operations.txt"};

            if(args[0] == "-t")
                TimingsExtractor.ExtractTimings(File.ReadAllText(args[1]), File.ReadAllText(args[2]), File.ReadAllText(args[3]));
            else
                OpcodeExtractor.ExtractOpcodes(args[0]);
        }
    }

    
}
