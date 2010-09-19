using System;
using RomTools.Metadata;

namespace RomTools.Emulator
{
    public partial class GameBoy
    {
        private readonly Registers r = new Registers();
        public Registers Registers { get { return r; } }

        private readonly MemoryManagementUnit mmu;

        public RomInfo romInfo { get; private set; }

        private bool halted = false;
        private bool stopped = false;
    
        public GameBoy(byte[] rom)
        {
            romInfo = new RomInfo(rom);
            mmu = new MemoryManagementUnit(romInfo, r);
            InitOpcodes();
        }

        public void RunCode(byte address)
        {
            r.PC = address;

            for (; r.PC < romInfo.Rom.Length; r.PC++)
            {
                byte op = romInfo.Rom[r.PC];
                Action lambda = op == 0xCB ? cbopcodes[romInfo.Rom[++r.PC]] : opcodes[op];
                lambda();

                if (stopped)
                    break;
            }
        }
    }
}
