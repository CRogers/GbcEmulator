using RomTools;

namespace GbcEmulator.Memory
{
    public enum MemoryMode
    {
        M16_8, M4_32
    }

    public class MemoryBankController
    {
        public byte[] Wram { get; set; }
        public byte[] Eram { get; set; }
        public byte[] Zram { get; set; }

        public int Rombank { get; set; }
        public int RamBank { get; set; }

        public int RomOffset 
        {
            get { return 0x4000 * Rombank; } 
        }
        public int RamOffset
        {
            get { return 0x2000 * RamBank; }
        }

        public bool ExternalRamOn { get; set; }
        public MemoryMode Mode { get; set; }

        public MemoryBankController(RomInfo romInfo)
        {
            Eram = new byte[romInfo.RamSize.Size];
            Wram = new byte[0x1FFF];
            Zram = new byte[128];

            Rombank = 0;
            RamBank = 0;
            ExternalRamOn = false;
            Mode = MemoryMode.M16_8;
        }
    }
}
