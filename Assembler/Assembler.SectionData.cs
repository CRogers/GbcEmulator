using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RomTools;

namespace Assembler
{
    public static partial class Assembler
    {
        private static class SectionData
        {
            public static Dictionary<string, int> GetSectionData(string[] code, out Dictionary<string, ushort> constants, out byte[] assembled)
            {
                var validCode = GetValidSectionsData(code);

                List<Match> constantsRemoved;
                constants = GetConstants(validCode, out constantsRemoved);

                var dbDw = GetDbDw(constantsRemoved);

                assembled = CombineData(dbDw);

                return dbDw.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Item1);
            }

            private static List<Match> GetValidSectionsData(IEnumerable<string> code)
            {
                Regex line = new Regex(@"([a-zA-Z_]\w+)\s+(db|dw|eq)\s+([^;]*)");
                code = code.RemoveWhitespaceComment();

                foreach (var codeLine in code)
                    if (!line.IsMatch(codeLine))
                        throw new ArgumentException("Error trying to parse '" + codeLine + "' for the data section.");

                return code.Select(c => line.Match(c)).ToList();
            }

            public static Dictionary<string, ushort> GetConstants(List<Match> codeMatches)
            {
                List<Match> changed;
                return GetConstants(codeMatches, out changed);
            }

            private static Dictionary<string, ushort> GetConstants(List<Match> codeMatches, out List<Match> constantsRemoved)
            {
                var eqs = codeMatches.Where(m => m.Groups[2].Value == "eq");
                constantsRemoved = codeMatches.ToList();
                constantsRemoved.RemoveAll(m => m.Groups[2].Value == "eq");

                return eqs.ToDictionary(m => m.Groups[1].Value, m => (ushort)short.Parse(m.Groups[3].Value));
            }

            /// <summary>
            /// Gets the data for db and dw statements. Returns in the format { name, { byteOffset, assembledDataInBytes } }
            /// </summary>
            /// <param name="codeMatches"></param>
            /// <returns></returns>
            private static Dictionary<string, Tuple<int, byte[]>> GetDbDw(IEnumerable<Match> codeMatches)
            {
                var ret = new Dictionary<string, Tuple<int, byte[]>>();
                int totalByteOffset = 0;

                foreach (var m in codeMatches)
                {
                    string name = m.Groups[1].Value;
                    string type = m.Groups[2].Value;
                    string data = m.Groups[3].Value;
                    byte[] bytes = ParseDataMarkup(data, type == "db");

                    ret.Add(name, Tuple.Create(totalByteOffset, bytes));

                    totalByteOffset += bytes.Length;
                }

                return ret;
            }

            private static byte[] ParseDataMarkup(string data, bool byteNotWord)
            {
                data = data.Trim() + ',';
                const string end = @"\s*[,;]";
                const string times = @"(\s+times\s+(?<times>\d{1,9}))?";
                Regex stringWithQuotes = new Regex(@"""((\\""|[^""])+)""" + times + end);
                Regex number = new Regex((byteNotWord ? byteFormat : wordFormat) + times + end);

                var matches = new List<Tuple<int, byte[]>>();

                foreach (Match m in stringWithQuotes.Matches(data))
                {
                    string withQuotes = m.Groups[1].Value;
                    string escapedCorrected = withQuotes.Replace(@"\""", "\"").Replace(@"\n", "\n").Replace(@"\r", "\r");
                    byte[] bytes = byteNotWord
                                       ? Encoding.ASCII.GetBytes(escapedCorrected)
                                       : Encoding.Unicode.GetBytes(escapedCorrected);

                    if (m.Groups["times"].Value != "")
                        bytes = bytes.ExtendRight(int.Parse(m.Groups["times"].Value));

                    matches.Add(Tuple.Create(m.Index, bytes));

                    data = data.Substring(0, m.Index) + "".PadRight(m.Length, '#') + data.Substring(m.Index + m.Length);
                }

                foreach (Match m in number.Matches(data))
                {
                    var capture = m.Groups[1].Value;
                    int num = capture.Contains("h") ? int.Parse(capture.TrimEnd('h'), NumberStyles.AllowHexSpecifier) : int.Parse(capture);

                    byte[] bytes;
                    bytes = byteNotWord ? new[]{ (byte)num } : BitConverter.GetBytes((ushort) num);

                    if (m.Groups["times"].Value != "")
                        bytes = bytes.ExtendRight(int.Parse(m.Groups["times"].Value));

                    matches.Add(Tuple.Create(m.Index, bytes));
                }

                matches.Sort((t, s) => t.Item1.CompareTo(s.Item1));

                return CombineArrays(matches);
            }

            private static byte[] CombineData(Dictionary<string, Tuple<int, byte[]>> data)
            {
                var orderedData = data.Select(kvp => kvp.Value).ToList();
                orderedData.Sort((a, b) => a.Item1.CompareTo(b.Item1));

                return CombineArrays(orderedData);
            }

            private static T2[] CombineArrays<T1, T2>(IList<Tuple<T1, T2[]>> tuples)
            {
                return tuples.Aggregate(new T2[0].AsEnumerable(), (i, t) => i.Concat(t.Item2)).ToArray();
            }
        }
    }
}
