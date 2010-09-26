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
        /// <summary>
        /// Deals with the sections of assembly which are "code" ie any "rstXX" section, any interrupt and the "text" section
        /// </summary>
        private static class SectionCode
        {
            public static Dictionary<string, byte[]> AssembleCode(Dictionary<string, string[]> sections, int offset, Dictionary<string, ushort> constants, Dictionary<string, int> data)
            {
                // Select only sections which are text or rstXX and replace constants/data, convert hex values, remove whitespaced or comment only lines
                // then add a space on the end for anchoring.
                var sectionsRstText = sections.Where(kvp => rsts.Contains(kvp.Key) || kvp.Key == "text")
                                              .Select(kvp => new KeyValuePair<string, string[]>(kvp.Key, ReplaceAllConstantsData(ChangeFromHex(kvp.Value), constants, data)
                                                                                                             .RemoveWhitespaceComment()
                                                                                                             .Select(s => s + " ")
                                                                                                             .ToArray()))
                                              .ToDictionary();

                // Add any rstXX that have been missing with empty code sections
                foreach (var rst in rsts)
                    if(!sectionsRstText.ContainsKey(rst))
                        sectionsRstText.Add(rst, new string[0]);

                // Change all the lines of code to RegexOpcodes give more useful information
                var regexOpcodes = sectionsRstText.Select(kvp => new KeyValuePair<string, RegexOpcode[]>(kvp.Key, GetRegexOpcodes(kvp.Value))).ToDictionary();

                // Sort the list so that rst00, rst08 ... text are in order
                var sectionsRstTextSorted = sectionsRstText.ToList();
                sectionsRstTextSorted.Sort((p, q) => p.Key.CompareTo(q.Key));

                // Get the labels for _all_ the sections
                var labels = GetLabels(sectionsRstTextSorted, regexOpcodes, offset);

                // Get the assembled bytes for _all the sections
                var assembledCode = sectionsRstTextSorted.Select(kvp => new KeyValuePair<string, byte[]>(kvp.Key, GetCode(kvp.Value, labels, regexOpcodes[kvp.Key]))).ToDictionary();

                return assembledCode;
            }

            /// <summary>
            /// Gets the assembled machine code for section of code
            /// </summary>
            /// <param name="code">The lines of code for this section</param>
            /// <param name="labelDict">A dict of labels and their absolute compiled positions</param>
            /// <param name="regexOpcodes">A list of RegexOpcodes where each RegexOpcode matches with each corresponding line of code</param>
            /// <returns></returns>
            private static byte[] GetCode(string[] code, Dictionary<string, ushort> labelDict, RegexOpcode[] regexOpcodes)
            {
                var output = new List<byte>();

                for (int i = 0; i < code.Length; i++)
                {
                    RegexOpcode op = regexOpcodes[i];

                    if (op.Prefix != null)
                        output.Add((byte)op.Prefix);

                    output.Add(op.Code);

                    if (op.BytesFollowing > 0)
                    {
                        Match m = op.Regex.Match(code[i]);

                        // Group 1 is the number, either n, -n, nn, -nn, h or hh or a label
                        int n;
                        if (!int.TryParse(m.Groups[1].Value, out n))
                        {
                            // Special case for JP opcode, as it can have a label to jump to
                            var first2Chars = op.Op.Substring(0, 2);
                            if (first2Chars == "JP" || first2Chars == "CA")
                                n = labelDict[m.Groups[1].Value];
                            else
                                throw new ApplicationException(string.Format("The value {0} cannot be parsed", m.Groups[1].Value));
                        }

                        if (op.BytesFollowing >= 1)
                            output.Add(n > 0 ? (byte)n : (byte)(sbyte)n);

                        int top = n >> 8;
                        if (op.BytesFollowing == 2)
                            output.Add(top > 0 ? (byte)top : (byte)(sbyte)top);
                    }
                }

                return output.ToArray();
            }

            /// <summary>
            /// Goes through each line of code - if it is a valid opcode, it assigns a RegexOpcode to it.
            /// </summary>
            /// <param name="code"></param>
            /// <returns></returns>
            private static RegexOpcode[] GetRegexOpcodes(string[] code)
            {
                var regexOpcodes = new RegexOpcode[code.Length];

                for (int i = 0; i < code.Length; i++)
                {
                    RegexOpcode op = Program.Opcodes.FirstOrDefault(opcode => opcode.Regex.IsMatch(code[i]));
                    if (op == null)
                        throw new ApplicationException(string.Format("Line {0} is incorrect: '{1}' does not exist, or a number in it is too big/small/malformed for the instruction.", i, code[i]));

                    regexOpcodes[i] = op;
                }

                return regexOpcodes;
            }

            /// <summary>
            /// Gets the labels' positions for every section of the code.
            /// </summary>
            /// <param name="sections">A **sorted** list of lines of code for each section (each section in alphabetical/rom layout order)</param>
            /// <param name="regexOpcodeDict">A Dictionary of each list of RegexOpcodes in each section</param>
            /// <param name="offset">Where the "text" section starts</param>
            /// <returns></returns>
            private static Dictionary<string, ushort> GetLabels(IList<KeyValuePair<string, string[]>> sections, Dictionary<string, RegexOpcode[]> regexOpcodeDict, int offset)
            {
                Regex label = new Regex(@"^\s*([_a-zA-Z]\w+):");
                var labels = new Dictionary<string, ushort>();

                int pc = 0;

                foreach (var section in sections)
                {
                    var sectionName = section.Key;
                    var code = section.Value;
                    var regexOpcodes = regexOpcodeDict[sectionName];

                    pc = sectionName == "text" ? offset : int.Parse(sectionName.Substring(3, 2), NumberStyles.HexNumber);

                    for (int i = 0; i < code.Length; i++, pc++)
                    {
                        // If there is a label at the start of the line, add it and its position to the label dictionary
                        if (label.IsMatch(code[i]))
                            labels.Add(label.Match(code[i]).Groups[1].Value, (ushort) pc);

                        pc += regexOpcodes[i].BytesFollowing;
                    }
                }
                return labels;
            }

            /// <summary>
            /// Search through the entire section, change each hex value to an ordinary one then return the changed lines of code
            /// </summary>
            /// <param name="strs">Lines of code for this section</param>
            /// <returns></returns>
            private static string[] ChangeFromHex(string[] strs)
            {
                StringBuilder sb = new StringBuilder(string.Join("\n", strs));

                Regex hex = new Regex(@"([0-9A-F]{1,4})h");
                int removedChars = 0;
                foreach (Match match in hex.Matches(sb.ToString()))
                {
                    sb.Remove(match.Index - removedChars, match.Length);
                    string replacement = Convert.ToUInt16(match.Groups[1].Value, 16).ToString();
                    sb.Insert(match.Index - removedChars, replacement);
                    removedChars += match.Length - replacement.Length;
                }

                return sb.ToString().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            }

            /// <summary>
            /// Replaces all constants/(reserved/declared data) with the number representing their offset.
            /// </summary>
            /// <param name="strs">Lines of code for this section</param>
            /// <param name="constants">Dictionary of constants</param>
            /// <param name="data">Dictionary of declared data</param>
            /// <returns></returns>
            private static string[] ReplaceAllConstantsData(string[] strs, Dictionary<string, ushort> constants, Dictionary<string, int> data)
            {
                StringBuilder sb = new StringBuilder(string.Join("\n", strs));

                foreach (var c in constants)
                    sb.Replace(c.Key, c.Value.ToString());

                foreach (var d in data)
                    sb.Replace(d.Key, d.Value.ToString());

                return sb.ToString().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
