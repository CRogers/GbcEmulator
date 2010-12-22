using System;
using GbcEmulator.Cpu;
using GbcEmulator.Memory;
using GbcEmulator.Gpu;
using RomTools;

namespace GbcEmulator
{
    public class GameBoy
    {
        public IMemoryManagementUnit Mmu { get; set; }
        public ICpu Cpu { get; set; }
        public Gpu.Gpu Gpu { get; set; }
        public Timer Timer { get; set; }

        public ReadOnlyArray<byte> Rom { get; private set; }

        public GameBoy(byte[] rom)
        {
            var romInfo = new RomInfo(rom);
            Rom = romInfo.Rom;
            Mmu = new MemoryManagementUnit(romInfo, Timer);
            Init();
        }

        public GameBoy(byte[] rom, Func<GameBoy, IMemoryManagementUnit> func)
        {
            Rom = new ReadOnlyArray<byte>(rom);
            Mmu = func(this);
            Init();
        }

        private void Init()
        {
            Cpu = new Z80(Rom, Mmu);
            Timer = new Timer();
            Gpu = new Gpu.Gpu(Timer);
        }

        public void Start()
        {
            Cpu.RunCode(0x100);
        }
    }
}
