using System.Linq;

namespace GbcEmulator.Gpu
{
    // http://imrannazar.com/GameBoy-Emulation-in-JavaScript:-GPU-Timings

    /// <summary>
    /// The current mode of operation.
    /// </summary>
    public enum Mode
    {
        HBlank, VBlank, Oam, Vram
    }

    public class Gpu
    {
        public Timer Timer { get; set; }

        public const int ScreenWidth = 160;
        public const int ScreenHeight = 144;

        private Mode mode = Mode.HBlank;

        private int clocks = 0;
        private int curLine = 0;

        public uint[] Screen { get; set; }

        public Gpu(Timer timer)
        {
            Timer = timer;

            // Make screen white
            uint white = ColorExtensions.Color(255, 255, 255, 255);
            Screen = new uint[ScreenWidth * ScreenHeight].Select(u => white).ToArray();
        }

        public void Step()
        {
            // BUG: This should be some sort of clock from the CPU which is incremented differently for each instruction?
            clocks += Timer.Div;

            // BUG: Add some clocks from CPU
            switch (mode)
            {
                case Mode.HBlank:
                    if(clocks >= 204)
                    {
                        clocks = 0;
                        curLine++;

                        if (curLine == 143)
                        {
                            mode = Mode.VBlank;
                            // TODO: Push data to screen 
                        }
                        else
                            mode = Mode.Oam;
                    }
                    break;

                case Mode.VBlank:
                    if(clocks >= 456)
                    {
                        clocks = 0;
                        curLine++;

                        if(curLine > 153)
                        {
                            mode = Mode.Oam;
                            curLine = 0;
                        }
                    }
                    break;

                case Mode.Oam:
                    if(clocks >= 80)
                    {
                        clocks = 0;
                        mode = Mode.Vram;
                    }
                    break;

                case Mode.Vram:
                    if(clocks >= 172)
                    {
                        clocks = 0;
                        mode = Mode.HBlank;

                        // TODO: Write a scanline to the framebuffer
                    }
                    break;
            }
        }
    }
}
