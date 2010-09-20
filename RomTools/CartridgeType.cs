using System;

namespace RomTools
{
    // Taken from PAN document at: http://www.devrs.com/gb/files/gbspec.txt

    public enum MbcType
    {
        Mbc1 = 1, Mbc2 = 2, Mbc3 = 3, Mbc5 = 5, 
        Mmm01 = 6, PocketCamera = 7, BandaiTama5 = 8,
        HudsonHuC3 = 0xC3, HudsonHuC1 = 0xC1,
    }

    public class CartridgeInfo
    {
        public bool Rom { get; private set; }
        public bool Ram { get; private set; }
        public bool Battery { get; private set; }
        public bool Sram { get; private set; }
        public bool Timer { get; private set; }
        public bool Rumble { get; private set; }

        public MbcType MbcType { get; private set; }

        public CartridgeInfo(byte b0147)
        {
            int b = b0147;

            if (b.EqualsAny(7, 0xA, 0xE) || (b >= 0x14 && b <= 0x18) || (b >= 0x20 && b <= 0xFC))
                throw new ArgumentException("That byte (value: " + b + ") is not valid cartridge description.");

            if (b <= 0x1E)
                Rom = true;

            if (b >= 1 && b <= 3)
                MbcType = MbcType.Mbc1;

            if (b.EqualsAny(2, 3, 8, 9, 0x12, 0x13, 0x1A, 0x1B))
                Ram = true;

            if (b.EqualsAny(3, 6, 9, 0xD, 0xF, 0x10, 0x13, 0x1B, 0x1E))
                Battery = true;

            if (b == 5 || b == 6)
                MbcType = MbcType.Mbc2;

            if (b >= 0xB && b <= 0xD)
                MbcType = MbcType.Mmm01;

            if (b == 0xC || b == 0xD)
                Sram = true;

            if (b >= 0xF && b <= 0x13)
                MbcType = MbcType.Mbc3;

            if (b == 0xF || b == 0x10)
                Timer = true;

            if(b >= 0x1A && b <= 0x1E)
                MbcType = MbcType.Mbc5;

            if (b >= 0xC && b <= 0x1E)
                Rumble = true;

            switch (b)
            {
                case 0x1F:
                    MbcType = MbcType.PocketCamera;
                    break;

                case 0xFD:
                    MbcType = MbcType.BandaiTama5;
                    break;

                case 0xFE:
                    MbcType = MbcType.HudsonHuC3;
                    break;

                case 0xFF:
                    MbcType = MbcType.HudsonHuC1;
                    break;
            }
        }
    }
}