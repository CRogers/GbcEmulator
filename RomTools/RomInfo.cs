using System;
using System.Linq;

namespace RomTools
{
    // Taken from GB Spec at http://www.devrs.com/gb/files/gbspec.txt

    public class RomInfo
    {
        public ReadOnlyArray<byte> Rom { get; set; }

        public byte[] NintendoGraphic { get; set; }
        public string RomName { get; set; }
        public bool IsColor { get; set; }
        public ushort LicenseeCode { get; set; }
        public bool IsSuperGb { get; set; }
        public CartridgeInfo CartridgeInfo { get; set; }
        public RomSize RomSize { get; set; }
        public RamSize RamSize { get; set; }
        public bool Japanese { get; set; }
        public byte OldLincenseeCode { get; set; }
        public byte MaskRomVersionNumber { get; set; }
        public byte ComplementCheck { get; set; }
        public ushort Checksum { get; set; }

        public RomInfo() { }

        public RomInfo(byte[] rom)
        {
            Rom = new ReadOnlyArray<byte>(rom);

            NintendoGraphic = new byte[48];
            Array.Copy(rom, 0x0104, NintendoGraphic, 0, 48);

            var name = new byte[16];
            Array.Copy(rom, 0x0134, name, 0, 16);
            RomName = new string(name.Where(b => b != 0).Select(b => (char) b).ToArray());

            IsColor = rom[0x0143] == 0x80;
            LicenseeCode = (ushort)((rom[0x0144] << 8) | rom[0x0145]);
            IsSuperGb = rom[0x0146] == 3;

            CartridgeInfo = new CartridgeInfo(rom[0x0147]);
            RomSize = new RomSize(rom[0x0148]);
            RamSize = new RamSize(rom[0x0149]);

            Japanese = rom[0x014A] == 1;
            OldLincenseeCode = rom[0x01B];
            MaskRomVersionNumber = rom[0x014C];

            ComplementCheck = rom[0x014D];
            Checksum = (ushort)((rom[0x014E] << 8) | rom[0x014F]);
        }
    }
}
