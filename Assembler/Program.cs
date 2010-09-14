using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using RomTools;

namespace Assembler
{
    internal class Program
    {
        internal static RegexOpcode[] Opcodes;

        static void Main(string[] args)
        {
            XmlSerializer xmls = new XmlSerializer(typeof(Opcode[]));

            using (var ms = new MemoryStream(Encoding.ASCII.GetBytes(Properties.Resources.opcode_dict)))
                Opcodes = ((Opcode[])xmls.Deserialize(ms)).Select(o => new RegexOpcode(o)).ToArray();

            var code = File.ReadAllLines("test.asm");
            var assembled = Assembler.Assemble(code);

            File.WriteAllBytes("test.bin", assembled);

            var gb = new GameBoy(assembled);
            gb.RunCode(0);
        }
    }
}
