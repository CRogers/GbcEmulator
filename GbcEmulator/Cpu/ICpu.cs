using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GbcEmulator.Cpu
{
    public interface ICpu
    {
        Registers Registers { get; }
        void RunCode(ushort address);
    }
}
