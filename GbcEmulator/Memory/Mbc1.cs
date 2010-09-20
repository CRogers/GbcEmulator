using System;
using RomTools;

namespace GbcEmulator.Memory
{
    /// <summary>
    /// MBC1 (max 2MByte ROM and/or 32KByte RAM)
    /// </summary>
    public class Mbc1 : MemoryBankController
    {
        public Mbc1(RomInfo romInfo) : base(romInfo)
        {
        }

        // Enable/Disable RAM
        // WARN: The 21h 40h 60h bug described below also applies here but is handled by the RomOffset property
        public override byte Read4000_7FFF(int addr)
        {
            return ri.Rom[RomOffset + (addr & 0x3FFF)];
        }

        // RAM Enable/Disable
        // Any value with 0Ah in the lower 4 bits enables RAM, and any other value disables RAM.
        public override void Write0000_1FFF(int addr, byte b)
        {
            ExternalRamOn = (b & 0x0F) == 0xA;
        }

        // Select ROM Bank Number
        // WARN: There is a Mbc1 and Mbc2 specific bug here: see http://www.devrs.com/gb/files/gbspec.txt
        // Writing to this address space selects the lower 5 bits of the ROM Bank Number (in range 01-1Fh). 
        // When 00h is written, the MBC translates that to bank 01h also. That doesn't harm so far, 
        // because ROM Bank 00h can be always directly accessed by reading from 0000-3FFF. 
        // But (when using the register below to specify the upper ROM Bank bits), the same happens 
        // for Bank 20h, 40h, and 60h. Any attempt to address these ROM Banks will select Bank 21h, 41h, and 61h instead.
        public override void Write2000_3FFF(int addr, byte b)
        {
            Rombank &= 0x60;
            int bankNum = b & 0x1F;
            if (bankNum % 0x20 == 0)
                bankNum++;
            Rombank |= bankNum;
        }

        // RAM Bank Number - or - Upper Bits of ROM Bank Number
        public override void Write4000_5FFF(int addr, byte b)
        {
            // Select Rambank from 0-3 using two LSB
            if (Mode == MemoryMode.M4_32)
                RamBank = b & 0x3;
            // Select bits 5 and 6 of Rombank using two LSB of value
            else
                Rombank |= ((b & 0x3) << 5);
        }

        // ROM/RAM Mode Select
        public override void Write6000_7FFF(int addr, byte b)
        {
            Mode = (b & 1) == 0 ? MemoryMode.M16_8 : MemoryMode.M4_32;
        }
    }
}
