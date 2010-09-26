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

        public RomInfo()
        {
            NintendoGraphic = new byte[]
                                  {
                                      0xCE, 0xED, 0x66, 0x66, 0xCC, 0x0D, 0x00, 0x0B, 0x03, 0x73, 0x00, 0x83, 0x00, 0x0C,
                                      0x00, 0x0D, 0x00, 0x08, 0x11, 0x1F, 0x88, 0x89, 0x00, 0x0E, 0xDC, 0xCC, 0x6E, 0xE6,
                                      0xDD, 0xDD, 0xD9, 0x99, 0xBB, 0xBB, 0x67, 0x63, 0x6E, 0x0E, 0xEC, 0xCC, 0xDD, 0xDC,
                                      0x99, 0x9F, 0xBB, 0xB9, 0x33, 0x3E
                                  };
            RomName = "Unnamed";
            CartridgeInfo = new CartridgeInfo(0x1B); // MBC5 with Battery
            RomSize = new RomSize(1); // 64 Kilobytes
            RamSize = new RamSize(3); // 32 Kilobytes
            OldLincenseeCode = 0x33;
        }

        public RomInfo(byte[] rom)
        {
            Rom = new ReadOnlyArray<byte>(rom);

            NintendoGraphic = new byte[48];
            Array.Copy(rom, 0x0104, NintendoGraphic, 0, 48);

            var name = new byte[14];
            Array.Copy(rom, 0x0134, name, 0, 14);
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
