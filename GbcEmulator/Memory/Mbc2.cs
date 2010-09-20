using RomTools;

namespace GbcEmulator.Memory
{
    /// <summary>
    /// MBC2 (max 256KByte ROM and 512x4 bits RAM)
    /// </summary>
    public class Mbc2 : Mbc1
    {
        public Mbc2(RomInfo romInfo) : base(romInfo)
        {
            Eram = new byte[512 * 4];
        }

        // Enable/Disable RAM
        // The least significant bit of the upper address byte must be zero to enable/disable cart RAM.
        public override void Write0000_1FFF(int addr, byte b)
        {
            if (addr.GetBit(8))
                base.Write0000_1FFF(addr, b);
        }

        // Rom bank Number
        // The least significant bit of the upper address byte must be one to select a ROM bank.
        // XXXXBBBB - X = Don't cares, B = bank select bits
        public override void Write2000_3FFF(int addr, byte b)
        {
            if (addr.GetBit(8))
                Rombank = (b & 0xF);
        }

        // "ERAM"
        // The MBC2 doesn't support external RAM, instead it includes 512x4 bits of built-in RAM (in the MBC2 chip itself).
        // As the data consists of 4bit values, only the lower 4 bits of the "bytes" in this memory area are used.
        public override byte ReadEram(int addr)
        {
            return Eram[RamBank*512 + (addr & 0x1FF)];
        }

        public override void WriteEram(int addr, byte b)
        {
            Eram[RamBank * 512 + (addr & 0x1FF)] = (byte)(b & 0xF);
        }
    }
}
