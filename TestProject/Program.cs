using System.IO;
using GbcEmulator.Cpu;
using RomTools;
using GbcEmulator;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] rom = File.ReadAllBytes("Pokemon Silver.gbc");

            var gb = new GameBoy(rom);
            gb.Start();
        }
    }
}
