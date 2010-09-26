using System;
using System.Diagnostics;
using GbcEmulator.Memory;
using RomTools;

namespace GbcEmulator.Cpu
{
    public partial class Z80 : ICpu
    {
        private readonly Registers r = new Registers();
        public Registers Registers { get { return r; } }

        private IMemoryManagementUnit mmu;
        public IMemoryManagementUnit Mmu
        {
            get { return mmu; }
            set { mmu = value; }
        }

        public ReadOnlyArray<byte> rom { get; private set; }

        private bool halted = false;
        private bool stopped = false;


        public Z80(ReadOnlyArray<byte> rom, IMemoryManagementUnit mmu)
        {
            InitOpcodes();

            this.mmu = mmu;
            this.rom = rom;
        }


        public void RunCode(ushort address)
        {
            r.PC = address;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (; r.PC < rom.Length; r.PC++)
            {
                byte op = rom[r.PC];
                Action lambda = op == 0xCB ? cbopcodes[rom[++r.PC]] : opcodes[op];
                lambda();

                if (stopped)
                    break;
            }
            sw.Stop();
        }
    }
}
