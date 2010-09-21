using RomTools;

namespace GbcEmulator.Memory
{
    public class MbcRumble : Mbc5
    {
        public MbcRumble(RomInfo romInfo) : base(romInfo)
        {
        }

        public bool RumbleOn { get; protected set; }

        // Rambank Select -or- Motor On/Off
        //
        //  Writing a value (XXXXMBBB - X = Don't care, M = motor, B = bank select bits) into 4000-5FFF area        // will select an appropriate RAM bank at A000-BFFF if the cart contains RAM. RAM sizes are 64kbit or        // 256kbits. To turn the rumble motor on set M = 1, M = 0 turns it off.
        public override void Write4000_5FFF(int addr, byte b)
        {
            RamBank &= b & 0x7;
            RumbleOn = b.GetBit(3);
        }
    }
}
