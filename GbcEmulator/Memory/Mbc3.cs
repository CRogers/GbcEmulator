using System;
using RomTools;

namespace GbcEmulator.Memory
{
    class Mbc3 : Mbc1
    {
        public Mbc3(RomInfo romInfo) : base(romInfo)
        {
        }

        protected bool RtcAddressMapped = false;

        protected byte LastRtcWrite = 0;

        protected byte RtcS = 0;  // Seconds, 0-59
        protected byte RtcM = 0;  // Minutes, 0-59
        protected byte RtcH = 0;  // Hours, 0-23
        protected byte RtcDl = 0; // Lower 8 bits of day counter, 0-FFh
        protected byte RtcDh = 0; // Upper 1 bit of day count (bit 0), Halt Flag (bit 6, 0 = active, 1 = halted)
                                  // Day counter carry bit (bit 7, 1 = counter overflow)

        // ROM Bank Number (Write Only)
        //
        // The whole 7 bits of the ROM Bank Number are written directly to this address. As for the MBC1, 
        // writing a value of 00h, will select Bank 01h instead. All other values 01-7Fh select the 
        // corresponding ROM Banks.
        public override void Write2000_3FFF(int addr, byte b)
        {
            var bankNum = b & 0x7F;
            if (bankNum == 0)
                bankNum = 1;
            Rombank = bankNum;
        }

        // RAM Bank Number - or - RTC Register Select (Write Only)
        public override void Write4000_5FFF(int addr, byte b)
        {
            switch (b & 0xFF)
            {
                case 0: case 1: case 2: case 3:
                    RamBank = b & 0x03;
                    break;

                case 8: case 9: case 0xA: case 0xB: case 0xC:
                    RtcAddressMapped = true;
                    break;
            }
        }

        // Latch Clock Data (Write Only)
        //
        // When writing 00h, and then 01h to this register, the current time becomes latched into the RTC 
        // registers. The latched data will not change until it becomes latched again, by repeating the 
        // write 00h->01h procedure.
        public override void Write6000_7FFF(int addr, byte b)
        {
            if (LastRtcWrite == 0 && b == 1)
                LatchTime();

            LastRtcWrite = b;
        }

        // BUG: Should this be counting days or actually getting the time?
        private void LatchTime()
        {
            var dt = DateTime.Now;
            RtcS = (byte) dt.Second;
            RtcM = (byte) dt.Minute;
            RtcH = (byte) dt.Hour;
            RtcDh = (byte) (dt.DayOfYear & 0xFF);
            // BUG: What about halt/carry bit?
            RtcDl = (byte) (dt.DayOfYear & 0x100);
        }

        public override byte ReadEram(int addr)
        {
            if (RtcAddressMapped)
                switch (addr & 0xF)
                {
                    case 0x8:
                        return RtcS;

                    case 0x9:
                        return RtcM;

                    case 0xA:
                        return RtcH;

                    case 0xB:
                        return RtcDl;

                    case 0xC:
                        return RtcDh;
                }
            else
                return base.ReadEram(addr);

            throw new ApplicationException("This should never happen. If you see this, something is terribly wrong with boolean logic in your CLR.");
        }

        public override void WriteEram(int addr, byte b)
        {
            if (RtcAddressMapped)
                switch (addr & 0xF)
                {
                    case 0x8:
                        RtcS = b;
                        break;

                    case 0x9:
                        RtcM = b;
                        break;

                    case 0xA:
                        RtcH = b;
                        break;

                    case 0xB:
                        RtcDl = b;
                        break;

                    case 0xC:
                        RtcDh = b;
                        break;
                }
            else
                base.WriteEram(addr, b);
        }
    }
}
