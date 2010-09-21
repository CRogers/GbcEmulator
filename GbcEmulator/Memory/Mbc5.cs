using System;
using RomTools;

namespace GbcEmulator.Memory
{
    public class Mbc5 : Mbc1
    {
        public Mbc5(RomInfo romInfo)
            : base(romInfo)
        {
        }

        // Rom Bank Select
        //
        // The lower 8 bits of the 9-bit rom bank select is written to the 2000-2FFF area while the upper bit 
        // is written to the least significant bit of the 3000-3FFF area.
        public override void Write2000_3FFF(int addr, byte b)
        {
            Rombank &= 0x1FF;

            if (addr < 0x3000)
                Rombank &= b;

            else
                Rombank = Rombank.SetBit(8, b.GetBit(0));
        }

        // Ram Bank Select
        //
        // Writing a value (XXXXBBBB - X = Don't care, B = bank select bits) into 4000-5FFF area will select         // an appropriate RAM bank at A000-BFFF if the cart contains RAM.
        // Also, this is the first MBC that allows rom bank 0 to appear in the 4000-7FFF range by writing $000
        // to the rom bank select. (no 01h, 21h, 41h, 61h bug!)
        public override void Write4000_5FFF(int addr, byte b)
        {
            RamBank &= b & 0xF;
        }
    }
}