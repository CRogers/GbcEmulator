using System;
using System.Linq;

namespace RomTools
{
    public class RomBuilder
    {
        public static byte[] BuildRom(byte[] jumptable, byte[] inttable, byte[] header, byte[] code)
        {
            var rom = new byte[32*1024];

            Array.Copy(jumptable,   0, rom, 0x00,  jumptable.Length.Max(0x40));
            Array.Copy(inttable,    0, rom, 0x48,  inttable.Length.Max(0x28));
            Array.Copy(header,      0, rom, 0x100, header.Length.Max(0x4F));
            Array.Copy(code,        0, rom, 0x150, code.Length.Max(0x40));

            return rom;
        }

        public static byte[] BuildJumptable(byte[] rst00, byte[] rst08, byte[] rst10, byte[] rst18, 
                                            byte[] rst20, byte[] rst28, byte[] rst30, byte[] rst38)
        {
            var jumptable = new byte[0x40];

            Array.Copy(rst00, 0, jumptable, 0x00, rst00.Length.Max(8));
            Array.Copy(rst08, 0, jumptable, 0x08, rst08.Length.Max(8));
            Array.Copy(rst10, 0, jumptable, 0x10, rst10.Length.Max(8));
            Array.Copy(rst18, 0, jumptable, 0x18, rst18.Length.Max(8));
            Array.Copy(rst20, 0, jumptable, 0x20, rst20.Length.Max(8));
            Array.Copy(rst28, 0, jumptable, 0x28, rst28.Length.Max(8));
            Array.Copy(rst30, 0, jumptable, 0x30, rst30.Length.Max(8));
            Array.Copy(rst38, 0, jumptable, 0x38, rst38.Length.Max(8));

            return jumptable;
        }

        public static byte[] BuildInterruptTable(byte[] vblank, byte[] lcdcStatus, byte[] timerOverflow,
                                                 byte[] serialTransferComplete, byte[] p10_13HighLow)
        {
            var inttable = new byte[0x28];

            Array.Copy(vblank,                  0, inttable, 0x40, vblank.Length.Max(8));
            Array.Copy(lcdcStatus,              0, inttable, 0x48, lcdcStatus.Length.Max(8));
            Array.Copy(timerOverflow,           0, inttable, 0x50, timerOverflow.Length.Max(8));
            Array.Copy(serialTransferComplete,  0, inttable, 0x58, serialTransferComplete.Length.Max(8));
            Array.Copy(p10_13HighLow,           0, inttable, 0x60, p10_13HighLow.Length.Max(8));

            return inttable;
        }

        public static byte[] BuildHeader(RomInfo ri, byte[] startCode)
        {
            var header = new int[0x4F];

            if(startCode.Length > 4)
                throw new ArgumentException("The startCode can be at most 4 bytes long!");
            
            header[0x143] = ri.IsColor ? 0x80 : 0;
            header[0x144] = ri.LicenseeCode >> 8;
            header[0x145] = ri.LicenseeCode & 0xFF;
            header[0x146] = ri.IsSuperGb ? 3 : 0;

            header[0x147] = ri.CartridgeInfo.GetCode();
            header[0x148] = ri.RomSize.GetCode();
            header[0x149] = ri.RamSize.GetCode();

            header[0x14A] = ri.Japanese ? 0 : 1;
            header[0x14B] = ri.OldLincenseeCode;
            header[0x14C] = ri.MaskRomVersionNumber;

            header[0x14D] = ri.ComplementCheck;
            header[0x14E] = ri.Checksum >> 8;
            header[0x14F] = ri.Checksum & 0xFF;

            var byteHeader = header.Select(i => (byte)i).ToArray();

            Array.Copy(startCode, byteHeader, startCode.Length);
            Array.Copy(ri.NintendoGraphic, 0, byteHeader, 0x104, 48);
            Array.Copy(ri.RomName.PadRight('\0').Select(c => (byte)(int)c).ToArray(), 0, byteHeader, 0x134, 16);

            return byteHeader;
        }
    }
}
