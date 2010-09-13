using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace RomTools
{
    [StructLayout(LayoutKind.Explicit, Size=12)]
    public class Registers
    {
        [FieldOffset(0)] public ushort AF;
        [FieldOffset(0)] public short SignedAF;
        [FieldOffset(0)] public byte F;
        [FieldOffset(0)] public sbyte SignedF;
        [FieldOffset(1)] public byte A;
        [FieldOffset(1)] public sbyte SignedA;

        [FieldOffset(2)] public ushort BC;
        [FieldOffset(2)] public short SignedBC;
        [FieldOffset(2)] public byte C;
        [FieldOffset(2)] public sbyte SignedC;
        [FieldOffset(3)] public byte B;
        [FieldOffset(3)] public sbyte SignedB;

        [FieldOffset(4)] public ushort DE;
        [FieldOffset(4)] public short SignedDE;  
        [FieldOffset(4)] public byte E;
        [FieldOffset(4)] public sbyte SignedE;
        [FieldOffset(5)] public byte D;
        [FieldOffset(5)] public sbyte SignedD;

        [FieldOffset(6)] public ushort HL;
        [FieldOffset(6)] public short SignedHL;
        [FieldOffset(6)] public byte L;
        [FieldOffset(6)] public sbyte SignedL;
        [FieldOffset(7)] public byte H;
        [FieldOffset(7)] public sbyte SignedH;

        [FieldOffset(8)] public ushort SP;
        [FieldOffset(8)] public byte SPl;
        [FieldOffset(9)] public byte SPh;

        [FieldOffset(10)] public ushort PC;
        [FieldOffset(10)] public byte PCl;
        [FieldOffset(11)] public byte PCh;

        /// <summary>
        /// Carry flag
        /// </summary>
        public bool FlagC
        {
            get { return F.GetBit(0); }
            set { F = F.SetBit(0, value); }
        }
        public int FlagCInt { get { return FlagC ? 1 : 0; } }

        /// <summary>
        /// Add/Subtract flag
        /// </summary>
        public bool FlagN
        {
            get { return F.GetBit(1); }
            set { F = F.SetBit(1, value); }
        }

        /// <summary>
        /// Parity/Overflow flag
        /// </summary>
        public bool FlagP
        {
            get { return F.GetBit(2); }
            set { F = F.SetBit(2, value); }
        }
        public bool FlagV { get { return FlagP; } set { FlagP = value; } }

        /// <summary>
        /// Half Carry flag
        /// </summary>
        public bool FlagH
        {
            get { return F.GetBit(4); }
            set { F = F.SetBit(4, value); }
        }

        /// <summary>
        /// Zero flag
        /// </summary>
        public bool FlagZ
        {
            get { return F.GetBit(6); }
            set { F = F.SetBit(6, value); }
        }

        /// <summary>
        /// Sign Flag
        /// </summary>
        public bool FlagS
        {
            get { return F.GetBit(7); }
            set { F = F.SetBit(7, value); }
        }


        public void Add(ref byte b, int value)
        {
            byte oldValue = b;
            b = (byte)(b + value);
            SetFlags(oldValue, b, value);

            FlagN = false;
            // http://www.spuify.co.uk/?p=220
            FlagH = (value ^ oldValue ^ b).GetBit(1);

        }

        public void SetFlags(byte oldValue, byte newValue, int valueAdded)
        {
            

            FlagS = newValue < 0;
            FlagZ = newValue == 0;
            FlagP = (int)oldValue + valueAdded > 255 || (int)oldValue - valueAdded < 0;
            // WARN: Should check to see if carry first!
            //FlagC = 
        }
    }
}
