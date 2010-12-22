using System.Timers;
using SysTimer = System.Timers.Timer;
using RomTools;

namespace GbcEmulator
{
    public class Timer
    {
        // See TheNintendoGameboy.pdf page 31

        /// <summary>
        /// FF04 - DIV - Divider Register (R/W) 
        /// This register is incremented at rate of 16384Hz, writing any value to this register resets it to 00h.
        /// </summary>
        public byte Div { get; set; }

        /// <summary>
        /// FF05 - TIMA - Timer counter (R/W) 
        /// This timer is incremented by a clock frequency specified by the TAC register ($FF07). When the value overflows (gets bigger than FFh) then it will be 
        /// reset to the value specified in TMA (FF06), and an interrupt will be requested, as described below.
        /// </summary>
        public byte Tima { get; set; }

        /// <summary>
        /// FF06 - TMA - Timer Modulo (R/W) 
        /// When the TIMA overflows, this data will be loaded.
        /// </summary>
        public byte Tma { get; set; }

        /// <summary>
        /// FF07 - TAC - Timer Control (R/W)
        /// Bit 2    -   Timer Stop (0=Stop, 1=Start)
        /// Bits 1-0 -   Input Clock Select
        ///              00: 4096 Hz (~4194 Hz SGB)
        ///              01: 262144 Hz (~268400 Hz SGB)
        ///              10: 65536 Hz (~67110 Hz SGB)
        ///              11: 16384 Hz (~16780 Hz SGB)
        /// </summary>
        public byte Tac { get; private set; }

        public bool TimerOn { get; set; }

        private readonly SysTimer divTimer;
        private readonly SysTimer timaTimer;


        public Timer()
        {
            divTimer = new SysTimer(1000.0/16384) {AutoReset = true};
            divTimer.Elapsed += StepDiv;

            timaTimer = new SysTimer(1000.0/4096) {AutoReset = true};
            timaTimer.Elapsed += StepTima;

            TimerOn = true;

            divTimer.Start();
            timaTimer.Start();
        }

        private void StepDiv(object sender, ElapsedEventArgs eea)
        {
            Div++;
        }

        private void StepTima(object sender, ElapsedEventArgs eea)
        {
            Tima++;
            if(Tima == 0)
            {
                Tima = Tma;
                // BUG: Call interrupt
            }
        }

        public void SetTac(byte value)
        {
            TimerOn = value.GetBit(2);
            int freq = 4096;

            switch (value & 3)
            {
                case 0: freq = 4096; break;
                case 1: freq = 262144; break;
                case 2: freq = 65536; break;
                case 3: freq = 16384; break;
            }

            timaTimer.Interval = 1000.0/freq;
        }
    }
}
