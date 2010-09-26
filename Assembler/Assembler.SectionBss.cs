using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Assembler
{
    public static partial class Assembler
    {
        private static class SectionBss
        {
            public static Dictionary<string, int> GetSectionBss(string[] code)
            {
                var line = new Regex(@"([a-zA-Z_]\w+)\s+res(b|w)\s+" + wordFormat);
                int offset = 0;
                var dict = new Dictionary<string, int>();

                foreach (var s in code.RemoveWhitespaceComment())
                {
                    if (!line.IsMatch(s))
                        throw new ApplicationException("Cannot parse line in bss section: '" + s + "'");

                    Match m = line.Match(s);
                    string name = m.Groups[1].Value;
                    string type = m.Groups[2].Value;
                    string num = m.Groups[3].Value;

                    int cellSize = type == "b" ? 1 : 2;
                    int length = num.Contains('h') ? ushort.Parse(num, NumberStyles.HexNumber) : ushort.Parse(num);

                    dict.Add(name, offset);

                    offset += length * cellSize;
                }

                return dict;
            }
        }
    }
}
