﻿using System;
using System.Linq;

namespace RomTools
{
    public class RomBuilder
    {
        public static byte[] BuildRom(RomInfo ri, byte[] jumptable, byte[] inttable, byte[] header, byte[] code)
        {
            var rom = new byte[ri.RomSize.Size];

            Array.Copy(jumptable,   0, rom, 0x00,  jumptable.Length.Max(0x40));
            Array.Copy(inttable,    0, rom, 0x40,  inttable.Length.Max(0x28));
            Array.Copy(header,      0, rom, 0x100, header.Length.Max(0x4F));
            Array.Copy(code,        0, rom, 0x150, code.Length.Max(rom.Length - 0x150));

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

            Array.Copy(vblank,                  0, inttable, 0x00, vblank.Length.Max(8));
            Array.Copy(lcdcStatus,              0, inttable, 0x08, lcdcStatus.Length.Max(8));
            Array.Copy(timerOverflow,           0, inttable, 0x10, timerOverflow.Length.Max(8));
            Array.Copy(serialTransferComplete,  0, inttable, 0x18, serialTransferComplete.Length.Max(8));
            Array.Copy(p10_13HighLow,           0, inttable, 0x20, p10_13HighLow.Length.Max(8));

            return inttable;
        }

        public static byte[] BuildHeader(RomInfo ri, byte[] startCode)
        {
            var header = new int[0x50];

            if(startCode.Length > 4)
                throw new ArgumentException("The startCode can be at most 4 bytes long!");
            
            header[0x43] = ri.IsColor ? 0x80 : 0;
            header[0x44] = ri.LicenseeCode >> 8;
            header[0x45] = ri.LicenseeCode & 0xFF;
            header[0x46] = ri.IsSuperGb ? 3 : 0;

            header[0x47] = ri.CartridgeInfo.GetCode();
            header[0x48] = ri.RomSize.GetCode();
            header[0x49] = ri.RamSize.GetCode();

            header[0x4A] = ri.Japanese ? 0 : 1;
            header[0x4B] = ri.OldLincenseeCode;
            header[0x4C] = ri.MaskRomVersionNumber;

            header[0x4D] = ri.ComplementCheck;
            header[0x4E] = ri.Checksum >> 8;
            header[0x4F] = ri.Checksum & 0xFF;

            var byteHeader = header.Select(i => (byte)i).ToArray();

            Array.Copy(startCode, byteHeader, startCode.Length.Max(4));
            Array.Copy(ri.NintendoGraphic, 0, byteHeader, 0x04, 48);
            Array.Copy(ri.RomName.PadRight(14, '\0').Select(c => (byte)(int)c).ToArray(), 0, byteHeader, 0x34, 14);

            return byteHeader;
        }

        public static byte[] BuildCode(byte[] dataDeclares, byte[] code)
        {
            var ret = new byte[dataDeclares.Length + code.Length];

            Array.Copy(dataDeclares, 0, ret, 0, dataDeclares.Length);
            Array.Copy(code, 0, ret, dataDeclares.Length, code.Length);

            return ret;
        }
    }
}
