using System;

namespace RomTools.Metadata
{
    public class CartridgeInfo
    {
        public bool Rom { get; private set; }
        public bool Mbc1 { get; private set; }
        public bool Ram { get; private set; }
        public bool Battery { get; private set; }
        public bool Mbc2 { get; private set; }
        public bool Mmm01 { get; private set; }
        public bool Sram { get; private set; }
        public bool Mbc3 { get; private set; }
        public bool Timer { get; private set; }
        public bool Mbc5 { get; private set; }
        public bool Rumble { get; private set; }
        public bool PocketCamera { get; private set; }
        public bool BandaiTama5 { get; private set; }
        public bool HudsonHuC3 { get; private set; }
        public bool HudsonHuC1 { get; private set; }

        public CartridgeInfo(byte b0147)
        {
            int b = b0147;

            if (b.EqualsAny(7, 0xA, 0xE) || (b >= 0x14 && b <= 0x18) || (b >= 0x20 && b <= 0xFC))
                throw new ArgumentException("That byte (value: " + b + ") is not valid cartridge description.");

            if (b <= 0x1E)
                Rom = true;

            if (b >= 1 && b <= 3)
                Mbc1 = true;

            if (b.EqualsAny(2, 3, 8, 9, 0x12, 0x13, 0x1A, 0x1B))
                Ram = true;

            if (b.EqualsAny(3, 6, 9, 0xD, 0xF, 0x10, 0x13, 0x1B, 0x1E))
                Battery = true;

            if (b == 5 || b == 6)
                Mbc2 = true;

            if (b >= 0xB && b <= 0xD)
                Mmm01 = true;

            if (b == 0xC || b == 0xD)
                Sram = true;

            if (b >= 0xF && b <= 0x13)
                Mbc3 = true;

            if (b == 0xF || b == 0x10)
                Timer = true;

            if(b >= 0x1A && b <= 0x1E)
                Mbc5 = true;

            if (b >= 0xC && b <= 0x1E)
                Rumble = true;

            switch (b)
            {
                case 0x1F:
                    PocketCamera = true;
                    break;

                case 0xFD:
                    BandaiTama5 = true;
                    break;

                case 0xFE:
                    HudsonHuC3 = true;
                    break;

                case 0xFF:
                    HudsonHuC1 = true;
                    break;
            }
        }
    }
}