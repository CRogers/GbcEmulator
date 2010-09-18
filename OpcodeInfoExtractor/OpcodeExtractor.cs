using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Assembler;

namespace OpcodeInfoExtractor
{
    class OpcodeExtractor
    {
        public static void ExtractOpcodes(string dataFile, string cbdataFile)
        {
            // This code extracts information from this webpage describing opcodes written by Imran Nazar.
            // All credit goes to him for this fantastic resource, as well as his project to write a Z80 emulator in javascript.
            // http://imrannazar.com/Gameboy-Z80-Opcode-Map

            // BUG: Need to sort out extended table as well (for 0xCB codes)

            string data = File.ReadAllText(dataFile);
            string cbdata = File.ReadAllText(cbdataFile);

            var opcodes = new [] { GetOpcodes(data), GetOpcodes(cbdata) };

            var xmls = new XmlSerializer(typeof(Opcode[][]));
            using (var fs = File.Create(@"opcode_dict.xml"))
                xmls.Serialize(fs, opcodes);

            // Copy the file over so the Assembler has the opcode definitions
            File.Copy("opcode_dict.xml",@".\..\..\..\Assembler\Resources\opcode_dict.xml",true);
        }

        private static Opcode[] GetOpcodes(string data)
        {
            // Regex + HTML, naughty :O
            Regex rows = new Regex(@"<tr>((.|\n)*?)</tr>");
            Regex eachRow = new Regex(@"<td><abbr title=""(?<abbr>[^""]*?)"">(?<op>.*?)</abbr></td>");

            // Working for numeric regexs.
            // 0_255:    [,\s](2[0-5][0-5]|2[0-4]\d|1?\d?\d)[,\s]
            // -128_0:   [,\s](-(1[0-2][0-8]|1[01]\d|\d?\d))[,\s]
            // 0_100h:   ([0-9A-F]{1,2})h
            // Both:     [,\s]((-(1[0-2][0-8]|1[01]\d|\d?\d))|(2[0-5][0-5]|2[0-4]\d|1?\d?\d))[,\s]
            //
            // 0_65535:  [,\s](6553[0-5]|655[0-2]\d|65[0-4]\d\d|6[0-4]\d{1,3}|[0-5]?\d{1,4})[,\s]
            // -65536_0: [,\s](-(3276[0-8]|327[0-5]\d|32[0-6]\d\d|3[01]\d{1,3}|[0-2]?\d{1,4}))[,\s]
            // 0_10000h: [0-9A-F]h
            // Both:     [,\s]((6553[0-5]|655[0-2]\d|65[0-4]\d\d|6[0-4]\d{1,3}|[0-5]?\d{1,4})|(-(3276[0-8]|327[0-5]\d|32[0-6]\d\d|3[01]\d{1,3}|[0-2]?\d{1,4})))[,\s]

            // Parens left off end so it can incorporate a \w+ for matching labels in JP and JR operations
            const string byteregex = @"([0-9A-F]{1,2}h|-(1[0-2][0-8]|1[01]\d|\d?\d)|(2[0-5][0-5]|2[0-4]\d|1?\d?\d)";
            const string shortregex = @"([0-9A-F]{1,4}h|(6553[0-5]|655[0-2]\d|65[0-4]\d\d|6[0-4]\d{1,3}|[0-5]?\d{1,4})|(-(3276[0-8]|327[0-5]\d|32[0-6]\d\d|3[01]\d{1,3}|[0-2]?\d{1,4}))";

            var opcodes = new Opcode[256];

            MatchCollection rowmc = rows.Matches(data);
            for (int i = 0; i < rowmc.Count; i++)
            {
                string row = rowmc[i].Groups[1].Value;
                MatchCollection mc = eachRow.Matches(row);
                for (int j = 0; j < mc.Count; j++)
                {
                    int b = (i << 4) + j;

                    string op = mc[j].Groups["op"].Value;
                    op = op.Replace('n', '#');
                    int bytesFollowing = op.Where(c => c == '#').Count();

                    // Dynamically create regexs for the opcodes, so it can match for whitespace/constants etc and escapes brackets (parens)
                    string opregex = op.Replace(" ", @"\s+").Replace("(", @"\(").Replace(")", @"\)").Replace(",", @"\s*,\s*")
                                       .Replace("##", shortregex).Replace("#", byteregex);

                    // Add on an option to match labels in jump commands
                    if (op.Substring(0, 2) == "JP" && bytesFollowing > 0)
                        opregex += @"|[_a-zA-Z]\w+";

                    // Close the right paren of the first capturing group
                    if (bytesFollowing > 0)
                        opregex += ")";

                    opregex += @"[\s;]";

                    opcodes[b] = new Opcode(op, opregex, (byte)b, bytesFollowing, mc[j].Groups["abbr"].Value);
                }
            }

            // Remove non-existant instructions and Extended operation ones (for now)
            return opcodes.Where(o => o.Op != "XX" && o.Op != "Ext ops").ToArray();
        }
    }
}
