using System;
using System.Collections.Generic;
using System.Linq;

namespace RomTools
{
    public partial class GameBoy
    {
        private Dictionary<byte, Action> opcodes;

        private static ushort BytesToUShortLE(byte a, byte b)
        {
            return (ushort)(a | (b << 8));
        }

        private ushort ReadUShort()
        {
            return BytesToUShortLE(rom[++r.PC], rom[++r.PC]);
        }

        private ushort ReadUShort(ushort address)
        {
            return BytesToUShortLE(rom[address], rom[address + 1]);
        }

        private byte ReadByte()
        {
            return rom[++r.PC];
        }

        private byte ReadByte(int address)
        {
            return rom[address];
        }

        private void WriteUShort(int address, ushort value)
        {
            rom[address] = (byte)(value & 0xFF);
            rom[address + 1] = (byte)(value >> 8);
        }

        private void WriteByte(int address, byte value)
        {
            rom[address] = value;
        }

        private void Swap<T>(ref T a, ref T b)
        {
            T t = a;
            a = b;
            b = t;
        }

        private void Call(ushort address)
        {
            WriteByte(r.SP - 1, r.SPh); 
            WriteByte(r.SP - 2, r.SPl); 
            r.SP -= 2; 
            r.PC = address;
        }

        private void Call()
        {
            Call(ReadUShort());
        }

        private void Ret()
        {
            r.PCl = ReadByte(r.SP);
            r.PCh = ReadByte(r.SP + 1);
            r.SP += 2;
        }

        private void RegisterStore()
        {
            rsv.AF = r.AF;
            rsv.BC = r.BC;
            rsv.DE = r.DE;
            rsv.HL = r.HL;
        }

        private void RegisterRecover()
        {
            r.AF = rsv.AF;
            r.BC = rsv.BC;
            r.DE = rsv.DE;
            r.HL = rsv.HL;
        }

        private void InitOpcodes()
        {
            // TODO: Remember to add added gbc specific instructions!

            // Resources used:
            // http://nemesis.lonestar.org/computers/tandy/software/apps/m4/qd/opcodes.html
            // http://github.com/Two9A/jsGB/blob/master/js/z80.js
            // http://www.z80.info/z80gboy.txt
            // http://www.devrs.com/gb/files/GBCPU_Instr.html

            // The following opcodes have been removed from the GameBoy Z80:
            // D3, DB, DD, E3, E4, EB, EC, ED, F2, F4, FC, FD

            // BUG: Missing: CB
            // BUG: Virtually all flag operations are broken!
            // BUG: Need to add 'm' and 't' timings somehow!

            opcodes = new Dictionary<byte, Action>
                          {
                              // ADDED INSTRUCTIONS for GAMEBOY (Modified Z80)
                              // http://www.z80.info/z80gboy.txt

                              // LD (word), SP
                              {0x08, () => WriteUShort(ReadUShort(), r.SP)},

                              // STOP: Stop the processor. BUG: Implementation guessed, possibly wrong
                              {0x10, () => stopped = true }, 

                              {0x22, () => WriteByte(r.HL++, r.A)}, // LD (HLI), A                              
                              {0x2A, () => r.A = ReadByte(r.HL++)}, // LD A, (HLI)
                              {0x32, () => WriteByte(r.HL--, r.A)}, // LD (HLD), A                              
                              {0x3A, () => r.A = ReadByte(r.HL--)}, // LD A, (HLD)

                              // RETI
                              {0xD9, () => { r.IFF2 = r.IFF1; r.IFF1 = true; RegisterStore(); r.PC = ReadUShort(r.SP); r.SP += 2; } },
                              
                              {0xE0, () => WriteByte(ReadByte() + 0xFF00, r.A)},            // LD (byte), A
                              {0xE2, () => WriteByte(r.C + 0xFF00, r.A)},                   // LD (C), A

                              {0xE8, () => r.SP = (ushort) (r.SP + (sbyte) ReadUShort())},  // ADD SP, offset; offset is signed
                              {0xEA, () => WriteByte(ReadUShort(), r.A)},                   // LD (word), A

                              {0xF0, () => r.A = ReadByte(0xFF00 + ReadByte())},            // LD A, (byte)
                              {0xF8, () => r.HL = (ushort) (r.SP + (sbyte) ReadUShort())},  // LD HL, offset; offset is signed

                              {0xFA, () => r.A = ReadByte(ReadUShort())},                   // LD A, (word)



                              // STANDARD Z80 INSTRUCTIONS.
                              // Removed instructions indicated

                              // 8 Bit Transfer Instructions

                              // LD A, X
                              {0x7F, () => r.A = r.A},
                              {0x78, () => r.A = r.B},
                              {0x79, () => r.A = r.C},
                              {0x7A, () => r.A = r.D},
                              {0x7B, () => r.A = r.E},
                              {0x7C, () => r.A = r.H},
                              {0x7D, () => r.A = r.L},

                              // LD A, (X)
                              {0x7E, () => r.A = ReadByte(r.HL)},
                              {0x0A, () => r.A = ReadByte(r.BC)},
                              {0x1A, () => r.A = ReadByte(r.DE)},

                              // LD B, X
                              {0x47, () => r.B = r.A},
                              {0x40, () => r.B = r.B},
                              {0x41, () => r.B = r.C},
                              {0x42, () => r.B = r.D},
                              {0x43, () => r.B = r.E},
                              {0x44, () => r.B = r.H},
                              {0x45, () => r.B = r.L},
                              {0x46, () => r.B = ReadByte(r.HL)},

                              // LD C, X
                              {0x4F, () => r.C = r.A},
                              {0x48, () => r.C = r.B},
                              {0x49, () => r.C = r.C},
                              {0x4A, () => r.C = r.D},
                              {0x4B, () => r.C = r.E},
                              {0x4C, () => r.C = r.H},
                              {0x4D, () => r.C = r.L},
                              {0x4E, () => r.C = ReadByte(r.HL)},

                              // LD D, X
                              {0x57, () => r.D = r.A},
                              {0x50, () => r.D = r.B},
                              {0x51, () => r.D = r.C},
                              {0x52, () => r.D = r.D},
                              {0x53, () => r.D = r.E},
                              {0x54, () => r.D = r.H},
                              {0x55, () => r.D = r.L},
                              {0x56, () => r.D = ReadByte(r.HL)},

                              // LD E, X
                              {0x5F, () => r.E = r.A},
                              {0x58, () => r.E = r.B},
                              {0x59, () => r.E = r.C},
                              {0x5A, () => r.E = r.D},
                              {0x5B, () => r.E = r.E},
                              {0x5C, () => r.E = r.H},
                              {0x5D, () => r.E = r.L},
                              {0x5E, () => r.E = ReadByte(r.HL)},

                              // LD H, X
                              {0x67, () => r.H = r.A},
                              {0x60, () => r.H = r.B},
                              {0x61, () => r.H = r.C},
                              {0x62, () => r.H = r.D},
                              {0x63, () => r.H = r.E},
                              {0x64, () => r.H = r.H},
                              {0x65, () => r.H = r.L},
                              {0x66, () => r.H = ReadByte(r.HL)},

                              // LD L, X
                              {0x6F, () => r.L = r.A},
                              {0x68, () => r.L = r.B},
                              {0x69, () => r.L = r.C},
                              {0x6A, () => r.L = r.D},
                              {0x6B, () => r.L = r.E},
                              {0x6C, () => r.L = r.H},
                              {0x6D, () => r.L = r.L},
                              {0x6E, () => r.L = ReadByte(r.HL)},

                              // LD (HL), X
                              {0x77, () => WriteByte(r.HL, r.A)},
                              {0x70, () => WriteByte(r.HL, r.B)},
                              {0x71, () => WriteByte(r.HL, r.C)},
                              {0x72, () => WriteByte(r.HL, r.D)},
                              {0x73, () => WriteByte(r.HL, r.E)},
                              {0x74, () => WriteByte(r.HL, r.H)},
                              {0x75, () => WriteByte(r.HL, r.L)},

                              // LD X, byte
                              {0x3E, () => r.A = ReadByte()},
                              {0x06, () => r.B = ReadByte()},
                              {0x0E, () => r.C = ReadByte()},
                              {0x16, () => r.D = ReadByte()},
                              {0x1E, () => r.E = ReadByte()},
                              {0x26, () => r.H = ReadByte()},
                              {0x2E, () => r.L = ReadByte()},
                              {0x36, () => WriteByte(r.HL, ReadByte())},

                              // LD (X), A
                              {0x02, () => WriteByte(r.BC, r.A)},
                              {0x12, () => WriteByte(r.DE, r.A)},

                              // 16 Bit Transfer Instructions

                              // LD X, word
                              {0x01, () => r.BC = ReadUShort()},
                              {0x11, () => r.DE = ReadUShort()},
                              {0x21, () => r.HL = ReadUShort()},
                              {0x31, () => r.SP = ReadUShort()},

                              // LD SQ, HL
                              {0xF9, () => r.SP = r.HL},

                              // Register Exchange Instructions

                              // 0xEB: EX DE,HL: Removed in GBC
                              // 0xE3: EX (SP), HL: Removed in GBC
                              
                              // Add Byte Instructions

                              // ADD A, X
                              {0x87, () => r.A += r.A},
                              {0x80, () => r.A += r.B},
                              {0x81, () => r.A += r.C},
                              {0x82, () => r.A += r.D},
                              {0x83, () => r.A += r.E},
                              {0x84, () => r.A += r.H},
                              {0x85, () => r.A += r.L},
                              {0x86, () => r.A += ReadByte(r.HL)},
                              {0xC6, () => r.A += ReadByte()},

                              // Add Byte with Carry-In Instructions

                              // ADC A, X
                              {0x8F, () => r.A += (byte) (r.A + r.FlagCInt)},
                              {0x88, () => r.A += (byte) (r.B + r.FlagCInt)},
                              {0x89, () => r.A += (byte) (r.C + r.FlagCInt)},
                              {0x8A, () => r.A += (byte) (r.D + r.FlagCInt)},
                              {0x8B, () => r.A += (byte) (r.E + r.FlagCInt)},
                              {0x8C, () => r.A += (byte) (r.H + r.FlagCInt)},
                              {0x8D, () => r.A += (byte) (r.L + r.FlagCInt)},
                              {0x8E, () => r.A += (byte) (ReadByte(r.HL) + r.FlagCInt)},
                              {0xCE, () => r.A += (byte) (ReadByte() + r.FlagCInt)},

                              // Subtract Byte Instructions

                              // SUB A, X
                              {0x97, () => r.A -= r.A},
                              {0x90, () => r.A -= r.B},
                              {0x91, () => r.A -= r.C},
                              {0x92, () => r.A -= r.D},
                              {0x93, () => r.A -= r.E},
                              {0x94, () => r.A -= r.H},
                              {0x95, () => r.A -= r.L},
                              {0x96, () => r.A -= ReadByte(r.HL)},
                              {0xD6, () => r.A -= ReadByte()},

                              // Subtract Byte With Borrow-In Instructions

                              // SBC A, X
                              {0x9F, () => r.A -= (byte) (r.A + r.FlagCInt)},
                              {0x98, () => r.A -= (byte) (r.B + r.FlagCInt)},
                              {0x99, () => r.A -= (byte) (r.C + r.FlagCInt)},
                              {0x9A, () => r.A -= (byte) (r.D + r.FlagCInt)},
                              {0x9B, () => r.A -= (byte) (r.E + r.FlagCInt)},
                              {0x9C, () => r.A -= (byte) (r.H + r.FlagCInt)},
                              {0x9D, () => r.A -= (byte) (r.L + r.FlagCInt)},
                              {0x9E, () => r.A -= (byte) (ReadByte(r.HL) + r.FlagCInt)},
                              {0xDE, () => r.A -= (byte) (ReadByte() + r.FlagCInt)},

                              // Double Byte Add Instructions

                              // ADD HL, X
                              {0x09, () => r.HL += r.BC},
                              {0x19, () => r.HL += r.DE},
                              {0x29, () => r.HL += r.HL},
                              {0x39, () => r.HL += r.SP},

                              // Control Instructions

                              // DI and EI
                              {0xF3, () => { r.IFF2 = r.IFF1; r.IFF1 = false; } },
                              {0xFB, () => { r.IFF2 = true; r.IFF1 = true; } },

                              // NOP and HLT
                              {0x00, () => { }},
                              {0x76, () => halted = true },
                              // BUG: Should really just loop nops, but we can just use a halted flag.

                              // Increment Byte Instructions

                              // INC X
                              {0x3C, () => r.A++},
                              {0x04, () => r.B++},
                              {0x0C, () => r.C++},
                              {0x14, () => r.D++},
                              {0x1C, () => r.E++},
                              {0x24, () => r.H++},
                              {0x2C, () => r.L++},
                              {0x34, () => WriteByte(r.HL, (byte) (ReadByte(r.HL) + 1))},

                              // Decrement Byte Instructions

                              // DEC X
                              {0x3D, () => r.A--},
                              {0x05, () => r.B--},
                              {0x0D, () => r.C--},
                              {0x15, () => r.D--},
                              {0x1D, () => r.E--},
                              {0x25, () => r.H--},
                              {0x2D, () => r.L--},
                              {0x35, () => WriteByte(r.HL, (byte) (ReadByte(r.HL) - 1))},

                              // Increment Register Pair Instructions

                              // INC X
                              {0x03, () => r.BC++},
                              {0x13, () => r.DE++},
                              {0x23, () => r.HL++},
                              {0x33, () => r.SP++},

                              // Decrement Register Pair Instructions

                              // DEC X
                              {0x0B, () => r.BC--},
                              {0x1B, () => r.DE--},
                              {0x2B, () => r.HL--},
                              {0x3B, () => r.SP--},

                              // Special Accumulator and Flag Instructions
                              
                              // DAA Implementation: http://www.msx.org/forumtopic7029.html
                              // BUG: Flags need sorting out. Is DAA even used on gameboys? I hate it.
                              {0x27, () => { 
                                      var v = r.A; 
                                      if(r.FlagN)
                                      {
                                          if(r.FlagH || (r.A & 0xF) > 9) v -= 6;
                                          if(r.FlagC || r.A > 0x99) v -= 0x60;
                                      } 
                                      else
                                      {
                                          if(r.FlagH || (r.A & 0xF) > 9) v += 6;
                                          if(r.FlagC || r.A > 0x99) v += 0x60;
                                      }
                                  r.FlagS = r.A.GetBit(7);
                                  r.FlagZ = r.A == 0;

                                      r.A = v;
                                  } },

                              {0x2F, () => r.A = (byte) ~r.A},  // CPL
                              {0x37, () => r.FlagC = true},     // SCF
                              {0x3F, () => r.FlagC = !r.FlagC}, // CCF

                              // Rotate Instructions

                              // RXCA
                              {0x07, () => { bool bit7 = r.A.GetBit(7); r.A = (byte)((r.A << 1) + (bit7 ? 1 : 0)); r.FlagC = bit7; } },
                              {0x0F, () => { bool bit0 = r.A.GetBit(0); r.A = (byte)((r.A >> 1) + (bit0 ? 0x80 : 0)); r.FlagC = bit0; } },
                              
                              // RXA
                              {0x17, () => r.A = (byte)((r.A << 1) + r.FlagCInt) }, 
                              {0x1F, () => r.A = (byte)((r.A << 1) + (r.FlagC ? 0x80 : 0)) }, 

                              // Logical Byte Instructions
                              
                              // AND X
                              {0xA7, () => r.A &= r.A},
                              {0xA0, () => r.A &= r.B},
                              {0xA1, () => r.A &= r.C},
                              {0xA2, () => r.A &= r.D},
                              {0xA3, () => r.A &= r.E},
                              {0xA4, () => r.A &= r.H},
                              {0xA5, () => r.A &= r.L},
                              {0xA6, () => r.A &= ReadByte(r.HL)},
                              {0xE6, () => r.A &= ReadByte()},

                              // XOR X
                              {0xAF, () => r.A ^= r.A},
                              {0xA8, () => r.A ^= r.B},
                              {0xA9, () => r.A ^= r.C},
                              {0xAA, () => r.A ^= r.D},
                              {0xAB, () => r.A ^= r.E},
                              {0xAC, () => r.A ^= r.H},
                              {0xAD, () => r.A ^= r.L},
                              {0xAE, () => r.A ^= ReadByte(r.HL)},
                              {0xEE, () => r.A ^= ReadByte()},

                              // OR X
                              {0xB7, () => r.A |= r.A},
                              {0xB0, () => r.A |= r.B},
                              {0xB1, () => r.A |= r.C},
                              {0xB2, () => r.A |= r.D},
                              {0xB3, () => r.A |= r.E},
                              {0xB4, () => r.A |= r.H},
                              {0xB5, () => r.A |= r.L},
                              {0xB6, () => r.A |= ReadByte(r.HL)},
                              {0xF6, () => r.A |= ReadByte()},

                              // CP X
                              {0xBF, () => r.FlagZ = r.A == r.A},
                              {0xB8, () => r.FlagZ = r.A == r.B},
                              {0xB9, () => r.FlagZ = r.A == r.C},
                              {0xBA, () => r.FlagZ = r.A == r.D},
                              {0xBB, () => r.FlagZ = r.A == r.E},
                              {0xBC, () => r.FlagZ = r.A == r.H},
                              {0xBD, () => r.FlagZ = r.A == r.L},
                              {0xBE, () => r.FlagZ = r.A == ReadByte(r.HL)},
                              {0xFE, () => r.FlagZ = r.A == ReadByte()},

                              // Branch Control/Program Counter Load Instructions

                              // JP X, Address
                              {0xC3, () => { r.PC = ReadUShort(); }},
                              {0xC2, () => { if (!r.FlagZ) r.PC = ReadUShort(); }},
                              {0xCA, () => { if (r.FlagZ) r.PC = ReadUShort(); }},
                              {0xD2, () => { if (!r.FlagC) r.PC = ReadUShort(); }},
                              {0xDA, () => { if (r.FlagC) r.PC = ReadUShort(); }},
                              // 0xE2, 0xEA: JP PO/PE changed in gameboy
                              // 0xF2: JP P removed in gameboy
                              // 0xFA: JP M changed in gameboy

                              // JP (HL)
                              {0xE9, () => r.PC = r.HL },

                              // JR X
                              {0x18, () => { r.PC = (ushort)(r.PC + (sbyte)ReadByte()); } }, 
                              {0x20, () => { if(!r.FlagZ) r.PC = (ushort)(r.PC + (sbyte)ReadByte()); } }, 
                              {0x28, () => { if(r.FlagZ) r.PC = (ushort)(r.PC + (sbyte)ReadByte()); } }, 
                              {0x30, () => { if(!r.FlagC) r.PC = (ushort)(r.PC + (sbyte)ReadByte()); } }, 
                              {0x38, () => { if(r.FlagC) r.PC = (ushort)(r.PC + (sbyte)ReadByte()); } }, 

                              // CALL X
                              {0xCD, Call },
                              {0xC4, () => { if(!r.FlagZ) Call(); } },
                              {0xCC, () => { if(r.FlagZ) Call(); } },
                              {0xD4, () => { if(!r.FlagC) Call(); } },
                              {0xDC, () => { if(r.FlagC) Call(); } },
                              // 0xE4, 0xEC: CALL PO/PE removed in gameboy
                              // 0xF4, 0xFC: CALL P/M removed in gameboy

                              // RET X
                              {0xC9, Ret },
                              {0xC0, () => { if(!r.FlagZ) Ret(); } },
                              {0xC8, () => { if(r.FlagZ) Ret(); } },
                              {0xD0, () => { if(!r.FlagC) Ret(); } },
                              {0xD8, () => { if(r.FlagC) Ret(); } },
                              // 0xE0, 0xE8: RET PO/PE changed in gameboy
                              // 0xF0: RET P changed ing gameboy

                              // RST X
                              {0xC7, () => Call(0) },
                              {0xCF, () => Call(8) },
                              {0xD7, () => Call(0x10) },
                              {0xDF, () => Call(0x18) },
                              {0xE7, () => Call(0x20) },
                              {0xEF, () => Call(0x28) },
                              {0xF7, () => Call(0x30) },
                              {0xFF, () => Call(0x38) },

                              // Stack Operation Instructions

                              // PUSH X
                              {0xC5, () => { WriteByte(r.SP-2, r.C); WriteByte(r.SP-1, r.B); r.SP -= 2; } },
                              {0xD5, () => { WriteByte(r.SP-2, r.E); WriteByte(r.SP-1, r.D); r.SP -= 2; } },
                              {0xE5, () => { WriteByte(r.SP-2, r.L); WriteByte(r.SP-1, r.H); r.SP -= 2; } },
                              {0xF5, () => { WriteByte(r.SP-2, r.F); WriteByte(r.SP-1, r.A); r.SP -= 2; } },

                              // POP X
                              {0xC1, () => { r.B = ReadByte(r.SP+1); r.C = ReadByte(r.SP); r.SP += 2; } },
                              {0xD1, () => { r.D = ReadByte(r.SP+1); r.E = ReadByte(r.SP); r.SP += 2; } },
                              {0xE1, () => { r.H = ReadByte(r.SP+1); r.L = ReadByte(r.SP); r.SP += 2; } },
                              {0xF1, () => { r.A = ReadByte(r.SP+1); r.F = ReadByte(r.SP); r.SP += 2; } },

                              // No IN/OUT instructions for Gameboy


                              // EXTENDED CB PREFIX OPCODES
                          };
        }

        private void FindMissingOpcodes()
        {
            var sorted = opcodes.Select(kvp => kvp.Key).ToList();
            sorted.Sort();

            var zeroto255 = Enumerable.Range(0, 255).Select(i => (byte)i);
            var missing = zeroto255.Where(i => !sorted.Contains(i)).ToArray();
            var missingnot = missing.Where(i => !new byte[] { 0xD3, 0xDB, 0xDD, 0xE3, 0xE4, 0xEB, 0xEC, 0xED, 0xF2, 0xF4, 0xFC, 0xFD }.Contains(i)).ToArray();

            var missingstr = string.Join(", ", missingnot.Select(b => b.ToString("X")));
        }
    }
}
