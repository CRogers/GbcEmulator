using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RomTools
{
    public partial class GameBoy
    {
        public Registers Registers { get { return r; } }
        private Registers r = new Registers();

        private byte[] rom;    
    
        public GameBoy(byte[] rom)
        {
            InitOpcodes();
            this.rom = rom;
        }

        public void RunCode(byte address)
        {
            r.PC = address;

            for (; r.PC < rom.Length; r.PC++)
                opcodes[rom[r.PC]]();
        }
    }
}
