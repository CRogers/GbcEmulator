using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Assembler
{
    public class Opcode
    {
        [XmlAttribute]
        public string Op { get; set; }
        [XmlAttribute]
        public string OpRegex { get; set; }
        [XmlAttribute]
        public byte Code { get; set; }
        [XmlAttribute]
        public int BytesFollowing { get; set; }
        [XmlAttribute]
        public string Description { get; set; }

        public Opcode()
        {
        }

        public Opcode(string op, string opRegex, byte code, int bytesFollowing, string description)
        {
            Op = op;
            OpRegex = opRegex;
            Code = code;
            BytesFollowing = bytesFollowing;
            Description = description;
        }
    }

    public class RegexOpcode : Opcode
    {
        public Regex Regex { get; set; }

        public RegexOpcode(Opcode opcode)
        {
            Op = opcode.Op;
            Code = opcode.Code;
            BytesFollowing = opcode.BytesFollowing;
            Description = opcode.Description;

            Regex = new Regex(opcode.OpRegex, RegexOptions.IgnoreCase);
        }
    }
}
