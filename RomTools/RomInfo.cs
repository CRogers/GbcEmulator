using System;
using System.Linq;

namespace RomTools
{
    // Taken from GB Spec at http://www.devrs.com/gb/files/gbspec.txt

    public class RomInfo
    {
        public byte[] Rom { get; private set; }

        public byte[] NintendoGraphic { get; private set; }
        public string RomName { get; private set; }
        public bool IsColor { get; private set; }
        public short LicenseeCode { get; private set; }
        public bool IsSuperGb { get; private set; }
        public CartridgeInfo CartridgeInfo { get; private set; }
        public RomSize RomSize { get; private set; }
        public RamSize RamSize { get; private set; }
        public bool Japanese { get; private set; }
        public byte OldLincenseeCode { get; private set; }
        public byte MaskRomVersionNumber { get; private set; }
        public byte ComplementCheck { get; private set; }
        public short Checksum { get; private set; }

        public RomInfo(byte[] rom)
        {
            Rom = rom;

            NintendoGraphic = new byte[48];
            Array.Copy(rom, 0x0104, NintendoGraphic, 0, 48);

            byte[] name = new byte[16];
            Array.Copy(rom, 0x0134, name, 0, 16);
            RomName = new string(name.Where(b => b != 0).Select(b => (char) b).ToArray());

            IsColor = rom[0x0143] == 0x80;
            LicenseeCode = (short)((rom[0x0144] << 8) | rom[0x0145]);
            IsSuperGb = rom[0x0146] == 3;

            CartridgeInfo = new CartridgeInfo(rom[0x0147]);
            RomSize = new RomSize(rom[0x0148]);
            RamSize = new RamSize(rom[0x0149]);

            Japanese = rom[0x014A] == 1;
            OldLincenseeCode = rom[0x01B];
            MaskRomVersionNumber = rom[0x014C];

            ComplementCheck = rom[0x014D];
            Checksum = (short)((rom[0x014E] << 8) | rom[0x014F]);
        }
    }
}
