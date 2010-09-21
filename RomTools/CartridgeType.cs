using System;

namespace RomTools
{
    // Taken from PAN document at: http://www.devrs.com/gb/files/gbspec.txt

    public enum MbcType
    {
        RomOnly = 0,
        Mbc1 = 1, Mbc2 = 2, Mbc3 = 3, Mbc5 = 5, Rumble = 6,
        Mmm01 = 7, PocketCamera = 8, BandaiTama5 = 9,
        HudsonHuC3 = 0xC3, HudsonHuC1 = 0xC1,
    }

    public class CartridgeInfo
    {
        public bool Rom { get; private set; }
        public bool Ram { get; private set; }
        public bool Battery { get; private set; }
        public bool Sram { get; private set; }
        public bool Timer { get; private set; }

        public MbcType MbcType { get; private set; }

        public CartridgeInfo(byte b0147)
        {
            int b = b0147;
            MbcType = MbcType.RomOnly;

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
                MbcType = MbcType.Rumble;

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

        public int GetCode()
        {
            switch (MbcType)
            {
                case MbcType.RomOnly:
                    return Ram ? Battery ? 9 : 8 : 0;

                case MbcType.Mbc1:
                    return Ram ? Battery ? 3 : 2 : 1;

                case MbcType.Mbc2:
                    return Battery ? 6 : 5;

                case MbcType.Mmm01:
                    return Sram ? Battery ? 0xD : 0xC : 0xB;

                case MbcType.Mbc3:
                    if (Timer)
                        return Ram ? 0x10 : 0xF;
                    else
                        return Ram ? Battery ? 0x13 : 0x12 : 0x11;

                case MbcType.Mbc5:
                    return Ram ? Battery ? 0x1B : 0x1A : 0x19;

                case MbcType.Rumble:
                    return Sram ? Battery ? 0x1E : 0x1D : 0x1C;

                case MbcType.PocketCamera:
                    return 0x1F;

                case MbcType.BandaiTama5:
                    return 0xFD;

                case MbcType.HudsonHuC3:
                    return 0xFE;

                case MbcType.HudsonHuC1:
                    return 0xFF;

            }

            throw new ApplicationException("This method needs to be updated, more stuff added to MbcType enum");
        }
    }
}