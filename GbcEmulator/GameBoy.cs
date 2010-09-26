using System;
using System.Diagnostics;
using GbcEmulator.Memory;
using RomTools;

namespace GbcEmulator
{
    public partial class GameBoy
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

        private GameBoy()
        {
            InitOpcodes();
        }
    
        public GameBoy(byte[] rom): this()
        {
            var romInfo = new RomInfo(rom);
            this.rom = romInfo.Rom;
            mmu = new MemoryManagementUnit(romInfo, r);
        }

        public GameBoy(byte[] rom, Func<GameBoy, IMemoryManagementUnit> func): this()
        {
            this.rom = new ReadOnlyArray<byte>(rom);
            mmu = func(this);
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
