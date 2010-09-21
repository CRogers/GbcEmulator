using System;
using RomTools;

namespace GbcEmulator.Memory
{
    public enum MemoryMode
    {
        M16_8, M4_32
    }

    public abstract class MemoryBankController
    {
        protected readonly RomInfo ri;

        public byte[] Wram { get; protected set; }
        public byte[] Eram { get; protected set; }
        public byte[] Zram { get; protected set; }

        public int Rombank { get; protected set; }
        public int RamBank { get; protected set; }

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
            ri = romInfo;

            Eram = new byte[romInfo.RamSize.Size];
            Wram = new byte[0x1FFF];
            Zram = new byte[128];

            Rombank = 0;
            RamBank = 0;
            ExternalRamOn = false;
            Mode = MemoryMode.M16_8;
        }

        public static MemoryBankController Factory(RomInfo ri)
        {
            switch(ri.CartridgeInfo.MbcType)
            {
                case MbcType.Mbc1:
                    return new Mbc1(ri);
                case MbcType.Mbc2:
                    return new Mbc2(ri);
                case MbcType.Mbc3:
                    return new Mbc3(ri);
            }

            throw new NotSupportedException("MbcType: " + ri.CartridgeInfo.MbcType + " is not yet supported.");
        }


        public abstract byte Read4000_7FFF(int addr);

        // ERAM
        public virtual byte ReadEram(int addr)
        {
            return Eram[RamOffset + (addr & 0x1FFF)];
        }

        // WRAM
        public virtual byte ReadWram(int addr)
        {
            return Wram[addr & 0x1FFF];
        }

        // ZRAM
        public virtual byte ReadZram(int addr)
        {
            return Zram[addr & 0x7F];
        }

        public abstract void Write0000_1FFF(int addr, byte b);
        public abstract void Write2000_3FFF(int addr, byte b);
        public abstract void Write4000_5FFF(int addr, byte b);
        public abstract void Write6000_7FFF(int addr, byte b);
        
        // ERAM
        public virtual void WriteEram(int addr, byte b)
        {
            Eram[RamOffset + (addr & 0x1FFF)] = b;
        }

        // WRAM
        public virtual void WriteWram(int addr, byte b)
        {
            Wram[addr & 0x1FFF] = b;
        }

        // ZRAM
        public virtual void WriteZram(int addr, byte b)
        {
            Zram[addr & 0x7F] = b;
        }
    }
}
