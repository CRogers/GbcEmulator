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
            Regex onlywhitespace = new Regex(@"^\s*$");

            var output = new List<byte>();

            // Add a space on the end so regex can do proper anchoring
            code = code.Where(s => !onlywhitespace.IsMatch(s)).Select(s => s + " ").ToArray();

            for(int i = 0; i < code.Length; i++)
            {
                RegexOpcode op = Program.Opcodes.FirstOrDefault(opcode => opcode.Regex.IsMatch(code[i]));
                if(op == null)
                    throw new ApplicationException(string.Format("Line {0} is incorrect: '{1}' does not exist, or a number in it is too big/small/malformed for the instruction.",i,code[i]));

                output.Add(op.Code);
                if (op.BytesFollowing > 0)
                {
                    Match m = op.Regex.Match(code[i]);
                    int n;
                    if(!int.TryParse(m.Groups[1].Value, out n))
                        throw new ApplicationException(string.Format("The value {0} cannot be parsed", m.Groups[1].Value));

                    if (op.BytesFollowing >= 1)
                        output.Add(n > 0 ? (byte)n : (byte)(sbyte)n);

                    int top = n >> 8;
                    if (op.BytesFollowing == 2)
                        output.Add(top > 0 ? (byte)top : (byte)(sbyte)top);
                }
            }

            return output.ToArray();
        }
    }
}
