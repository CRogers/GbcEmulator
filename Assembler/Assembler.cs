using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Assembler
{
    internal static class Assembler
    {
        public static byte[] Assemble(string[] code)
        {
            Regex whitespaceOrComment = new Regex(@"^\s*(;.*)?\s*$");

            var output = new List<byte>();

            // Add a space on the end so regex can do proper anchoring
            code = code.Where(s => !whitespaceOrComment.IsMatch(s)).Select(s => s + " ").ToArray();

            var regexOpcodes = GetRegexOpcodes(code);
            var labelDict = GetLabels(code, regexOpcodes);

            for(int i = 0; i < code.Length; i++)
            {
                RegexOpcode op = regexOpcodes[i];

                output.Add(op.Code);
                if (op.BytesFollowing > 0)
                {
                    Match m = op.Regex.Match(code[i]);

                    // Group 1 is the number, either n, -n, nn or -nn, or a label
                    int n;
                    if (!int.TryParse(m.Groups[1].Value, out n))
                    {
                        // Special case for JP opcode, as it can have a label to jump to
                        if (op.Op.Substring(0, 2) == "JP")
                            n = labelDict[m.Groups[1].Value];
                        else
                            throw new ApplicationException(string.Format("The value {0} cannot be parsed", m.Groups[1].Value));
                    }

                    if (op.BytesFollowing >= 1)
                        output.Add(n > 0 ? (byte) n : (byte) (sbyte) n);

                    int top = n >> 8;
                    if (op.BytesFollowing == 2)
                        output.Add(top > 0 ? (byte) top : (byte) (sbyte) top);
                }
            }

            return output.ToArray();
        }

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

        private static Dictionary<string, ushort> GetLabels(string[] code, RegexOpcode[] regexOpcodes)
        {
            Regex label = new Regex(@"^([_a-zA-Z]\w+):");
            var labels = new Dictionary<string, ushort>();

            ushort pc = 0;

            for (int i = 0; i < code.Length; i++, pc++)
            {
                // If there is a label at the start of the line, add it and its position to the label dictionary
                if (label.IsMatch(code[i]))
                    labels.Add(label.Match(code[i]).Groups[1].Value, pc);

                pc += (ushort)regexOpcodes[i].BytesFollowing;
            }

            return labels;
        }
    }
}
