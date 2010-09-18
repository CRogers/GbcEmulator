using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assembler
{
    class Disassembler
    {
         public static string Disassemble(byte[] data, bool lowerCaseMnemonics, bool lowerCaseRegisters, bool addLineNumbers, bool addComments)
         {
             var codeDict = Program.Opcodes.ToDictionary(o => (ushort)(o.Code | (o.Prefix == null ? 0 : o.Prefix << 8)));
             StringBuilder output = new StringBuilder();

             for (int i = 0; i < data.Length; i++)
             {
                 Opcode opcode;
                 try
                 {
                     ushort d = data[i];
                     if (d == 0xCB)
                         d = (ushort)((0xCB << 8) | data[++i]);
                     opcode = codeDict[d];
                 }
                 catch (KeyNotFoundException knfe)
                 {
                     throw new ApplicationException(string.Format("The opcode at address {0} with value {1} could not be found", i.ToString("X"), data[i].ToString("X")) ,knfe);
                 }

                 string line = addLineNumbers ? ("L_" + i.ToString("X") + ":").PadRight(9) : "";

                 // Pad right after mnemonic so its like a tab 
                 var split = opcode.Op.Split(' ');

                 var left = split[0].PadRight(5);
                 if (lowerCaseMnemonics)
                     left = left.ToLowerInvariant();

                 // If it is not a single argument opcode (e.g. NOP, STOP, HALT, EI, DI), add a aspace after the comma (if any),
                 // make it lower case if required then add the bit before the space and the modified string
                 if (split.Length > 1)
                 {
                     split[1] = split[1].Replace(",", ", ");

                     if (lowerCaseRegisters)
                         split[1] = split[1].ToLowerInvariant();

                     line += left + split[1];
                 }
                 else
                     line += left;

                 Func<int,string> numberFormat = n => n.ToString();

                 var first2Chars = opcode.Op.Substring(0, 2);
                 if (addLineNumbers && (first2Chars == "JP" || first2Chars == "CA"))
                     numberFormat = n => "L_" + n.ToString("X");

                 if (addLineNumbers && opcode.Op.Substring(0, 2) == "JR")
                     numberFormat = n => ((sbyte) n).ToString();

                 if (opcode.BytesFollowing == 1)
                     line = line.Replace("#", numberFormat(data[++i]));

                 else if (opcode.BytesFollowing == 2)
                     line = line.Replace("##", numberFormat(data[++i] | data[++i] << 8));

                 if (addComments)
                     line = line.PadRight(30) + "; " + opcode.Description;

                 output.AppendLine(line);
             }

             return output.ToString();
         }
    }
}
