using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RomTools
{
    public partial class GameBoy
    {
        private readonly Registers r = new Registers();
        public Registers Registers { get { return r; } }
        
        private readonly Registers rsv = new Registers();
        public Registers Rsv { get { return rsv; } }

        private bool halted = false;
        private bool stopped = false;

        private byte[] rom;
    
        public GameBoy(byte[] rom)
        {
            InitOpcodes();
            this.rom = rom;

            FindMissingOpcodes();
        }

        public void RunCode(byte address)
        {
            r.PC = address;

            for (; r.PC < rom.Length; r.PC++)
            {
                byte op = rom[r.PC];
                Action lambda = op == 0xCB ? cbopcodes[rom[++r.PC]] : opcodes[op];
                lambda();

                if (stopped)
                    break;
            }
        }
    }
}
