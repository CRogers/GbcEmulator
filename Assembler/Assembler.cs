using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RomTools;

namespace Assembler
{
    public static partial class Assembler
    {
        private const string byteFormat = @"([\dA-F]{1,2}h|(-(1[0-2][0-8]|1[01]\d|\d?\d))|(2[0-5][0-5]|2[0-4]\d|1?\d?\d))";
        private const string wordFormat = @"([\dA-F]{1,4}h|(6553[0-5]|655[0-2]\d|65[0-4]\d\d|6[0-4]\d{1,3}|[0-5]?\d{1,4})|(-(3276[0-8]|327[0-5]\d|32[0-6]\d\d|3[01]\d{1,3}|[0-2]?\d{1,4})))";
        private static readonly string[] rsts = Enumerable.Range(0, 8).Select(i => "rst" + (i * 8).ToString("X").PadLeft(2, '0')).ToArray();

        public static byte[] Assemble(string code)
        {
            var sections = SplitSections(code);

            var ri = SectionMetadata.GetRomInfo(sections["metadata"]);

            Dictionary<string, ushort> constants;
            byte[] dataDeclaresAssembled;
            var dataDeclares = SectionData.GetSectionData(sections["data"], out constants, out dataDeclaresAssembled);

            var dataReserved = SectionBss.GetSectionBss(sections["bss"]);

            var dataJoined = dataDeclares.Concat(dataReserved).ToDictionary();

            int offset = 0x150 + dataDeclaresAssembled.Length;
            byte[] offsetBytes = BitConverter.GetBytes((ushort) offset);

            // Startcode = nop, jp [offset]
            byte[] startCode = new byte[]{ 0, 195, offsetBytes[0], offsetBytes[1] };

            var dataCode = SectionCode.AssembleCode(sections, offset, constants, dataJoined);

            // BUG: No interrupt table
            var rom = RomBuilder.BuildRom(ri,
                                RomBuilder.BuildJumptable(dataCode["rst00"], dataCode["rst08"], dataCode["rst10"], dataCode["rst18"], dataCode["rst20"], dataCode["rst28"], dataCode["rst30"], dataCode["rst38"]),
                                RomBuilder.BuildInterruptTable(new byte[0], new byte[0], new byte[0], new byte[0], new byte[0]),
                                RomBuilder.BuildHeader(ri, startCode),
                                RomBuilder.BuildCode(dataDeclaresAssembled, dataCode["text"])
                );

            return rom;
        }

        /// <summary>
        /// Splits the assembly code into the section it should be in (so between .section XX and .end) and
        /// returns a dictionary of the lines of code in each section.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private static Dictionary<string, string[]> SplitSections(string code)
        {
            Regex section = new Regex(@"^\.section\s+([a-zA-Z]\w+)(.*?)\.end", RegexOptions.Multiline | RegexOptions.Singleline);
            var dict = new Dictionary<string, string[]>();

            foreach (Match match in section.Matches(code))
                dict.Add(match.Groups[1].Value, match.Groups[2].Value.SplitLines());

            return dict;
        }

        /// <summary>
        /// Remove all lines which are whitespace and comments.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private static IEnumerable<string> RemoveWhitespaceComment(this IEnumerable<string> code)
        {
            Regex whitespaceOrComment = new Regex(@"^\s*(;.*)?\s*$");
            return code.Where(s => !whitespaceOrComment.IsMatch(s));
        }

        private static string[] SplitLines(this string lines)
        {
            return lines.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
