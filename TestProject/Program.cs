using System.IO;
using RomTools;
using GbcEmulator;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] rom = File.ReadAllBytes("Pokemon Silver.gbc");
            var ri = new RomInfo(rom);

            var r = new Registers();

            r.A = 255;

            r.B = 52;
            r.SignedB -= 30;

            r.SignedC = -15;
            r.C += 9;

            r.SignedE = 0;

            var g = new GameBoy(new byte[]{ 0x3E, 128, 0x06, 64, 0x80 });
            //g.Start();
        }
    }
}
