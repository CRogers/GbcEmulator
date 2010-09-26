using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using GbcEmulator;
using GbcEmulator.Memory;

namespace Assembler
{
    internal class Program
    {
        internal static RegexOpcode[] Opcodes;

        static void Main(string[] args)
        {
            XmlSerializer xmls = new XmlSerializer(typeof(Opcode[][]));

            using (var ms = new MemoryStream(Encoding.ASCII.GetBytes(Properties.Resources.opcode_dict)))
            {
                var temp = ((Opcode[][]) xmls.Deserialize(ms));
                Opcodes = temp[0].Select(o => new RegexOpcode(o, null)).Concat(temp[1].Select(o => new RegexOpcode(o, 0xCB))).ToArray();
            }

            if (Debugger.IsAttached)
                args = new[] { "test_sections.asm", "test_sections.bin" };
                //args = new[] { "-d", "test.bin", "test_d.asm" };

            if(args[0] == "-d")
            {
                var data = File.ReadAllBytes(args[1]);
                var disassembled = Disassembler.Disassemble(data, true, false, true, true);
                File.WriteAllText(args[2], disassembled);
            }
            else
            {
                var code = File.ReadAllText(args[0]);
                var assembled = Assembler.Assemble(code);

                File.WriteAllBytes(args[1], assembled);

                if (Debugger.IsAttached)
                {
                    var gb = new GameBoy(assembled);
                    gb.RunCode(0x100);
                }
            }
        }
    }
}
