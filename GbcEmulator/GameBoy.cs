using System;
using GbcEmulator.Cpu;
using GbcEmulator.Memory;
using RomTools;

namespace GbcEmulator
{
    public partial class GameBoy
    {
        public IMemoryManagementUnit Mmu { get; set; }
        public ICpu Cpu { get; set; }

        public ReadOnlyArray<byte> Rom { get; private set; }

        public GameBoy(byte[] rom)
        {
            var romInfo = new RomInfo(rom);
            Rom = romInfo.Rom;
            Mmu = new MemoryManagementUnit(romInfo);
            Cpu = new Z80(Rom, Mmu);
        }

        public GameBoy(byte[] rom, Func<GameBoy, IMemoryManagementUnit> func)
        {
            Rom = new ReadOnlyArray<byte>(rom);
            Mmu = func(this);
            Cpu = new Z80(Rom, Mmu);
        }

        public void Start()
        {
            Cpu.RunCode(0x100);
        }
    }
}
