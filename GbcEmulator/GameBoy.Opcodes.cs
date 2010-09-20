using System;
using System.Collections.Generic;
using RomTools;

namespace GbcEmulator
{
    public partial class GameBoy
    {
        private Dictionary<byte, Action> opcodes;
        private Dictionary<byte, Action> cbopcodes;

        private ushort ReadUShort()
        {
            ushort addr = r.PC;
            r.PC += 2;
            return mmu.ReadUShort(addr);
        }

        private byte ReadByte()
        {
            return mmu.ReadByte(++r.PC);
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

            // BUG: Need to add 'm' and 't' timings somehow!

            opcodes = new Dictionary<byte, Action>
                          {
                              // ADDED INSTRUCTIONS for GAMEBOY (Modified Z80)
                              // http://www.z80.info/z80gboy.txt

                              // LD (word), SP
                              {0x08, () => mmu.WriteUShort(ReadUShort(), r.SP)},

                              // STOP: Stop the processor. BUG: Implementation guessed, possibly wrong
                              {0x10, () => stopped = true }, 

                              {0x22, () => mmu.WriteByte(r.HL++, r.A)}, // LD (HLI), A                              
                              {0x2A, () => r.A = mmu.ReadByte(r.HL++)}, // LD A, (HLI)
                              {0x32, () => mmu.WriteByte(r.HL--, r.A)}, // LD (HLD), A                              
                              {0x3A, () => r.A = mmu.ReadByte(r.HL--)}, // LD A, (HLD)

                              // RETI
                              {0xD9, () => { r.IFF2 = r.IFF1; r.IFF1 = true; r.Address = mmu.ReadUShort(r.SP); r.SP += 2; } },
                              
                              {0xE0, () => mmu.WriteByte(ReadByte() + 0xFF00, r.A)},            // LD (byte), A
                              {0xE2, () => mmu.WriteByte(r.C + 0xFF00, r.A)},                   // LD (C), A

                              {0xE8, () => r.SP = (ushort) (r.SP + (sbyte) ReadUShort())},      // ADD SP, offset; offset is signed BUG: Flags?
                              {0xEA, () => mmu.WriteByte(ReadUShort(), r.A)},                   // LD (word), A

                              {0xF0, () => r.A = mmu.ReadByte(0xFF00 + ReadByte())},            // LD A, (byte)
                              {0xF8, () => r.HL = (ushort) (r.SP + (sbyte) ReadUShort())},      // LD HL, offset; offset is signed

                              {0xFA, () => r.A = mmu.ReadByte(ReadUShort())},                   // LD A, (word)



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
                              {0x7E, () => r.A = mmu.ReadByte(r.HL)},
                              {0x0A, () => r.A = mmu.ReadByte(r.BC)},
                              {0x1A, () => r.A = mmu.ReadByte(r.DE)},

                              // LD B, X
                              {0x47, () => r.B = r.A},
                              {0x40, () => r.B = r.B},
                              {0x41, () => r.B = r.C},
                              {0x42, () => r.B = r.D},
                              {0x43, () => r.B = r.E},
                              {0x44, () => r.B = r.H},
                              {0x45, () => r.B = r.L},
                              {0x46, () => r.B = mmu.ReadByte(r.HL)},

                              // LD C, X
                              {0x4F, () => r.C = r.A},
                              {0x48, () => r.C = r.B},
                              {0x49, () => r.C = r.C},
                              {0x4A, () => r.C = r.D},
                              {0x4B, () => r.C = r.E},
                              {0x4C, () => r.C = r.H},
                              {0x4D, () => r.C = r.L},
                              {0x4E, () => r.C = mmu.ReadByte(r.HL)},

                              // LD D, X
                              {0x57, () => r.D = r.A},
                              {0x50, () => r.D = r.B},
                              {0x51, () => r.D = r.C},
                              {0x52, () => r.D = r.D},
                              {0x53, () => r.D = r.E},
                              {0x54, () => r.D = r.H},
                              {0x55, () => r.D = r.L},
                              {0x56, () => r.D = mmu.ReadByte(r.HL)},

                              // LD E, X
                              {0x5F, () => r.E = r.A},
                              {0x58, () => r.E = r.B},
                              {0x59, () => r.E = r.C},
                              {0x5A, () => r.E = r.D},
                              {0x5B, () => r.E = r.E},
                              {0x5C, () => r.E = r.H},
                              {0x5D, () => r.E = r.L},
                              {0x5E, () => r.E = mmu.ReadByte(r.HL)},

                              // LD H, X
                              {0x67, () => r.H = r.A},
                              {0x60, () => r.H = r.B},
                              {0x61, () => r.H = r.C},
                              {0x62, () => r.H = r.D},
                              {0x63, () => r.H = r.E},
                              {0x64, () => r.H = r.H},
                              {0x65, () => r.H = r.L},
                              {0x66, () => r.H = mmu.ReadByte(r.HL)},

                              // LD L, X
                              {0x6F, () => r.L = r.A},
                              {0x68, () => r.L = r.B},
                              {0x69, () => r.L = r.C},
                              {0x6A, () => r.L = r.D},
                              {0x6B, () => r.L = r.E},
                              {0x6C, () => r.L = r.H},
                              {0x6D, () => r.L = r.L},
                              {0x6E, () => r.L = mmu.ReadByte(r.HL)},

                              // LD (HL), X
                              {0x77, () => mmu.WriteByte(r.HL, r.A)},
                              {0x70, () => mmu.WriteByte(r.HL, r.B)},
                              {0x71, () => mmu.WriteByte(r.HL, r.C)},
                              {0x72, () => mmu.WriteByte(r.HL, r.D)},
                              {0x73, () => mmu.WriteByte(r.HL, r.E)},
                              {0x74, () => mmu.WriteByte(r.HL, r.H)},
                              {0x75, () => mmu.WriteByte(r.HL, r.L)},

                              // LD X, byte
                              {0x3E, () => r.A = ReadByte()},
                              {0x06, () => r.B = ReadByte()},
                              {0x0E, () => r.C = ReadByte()},
                              {0x16, () => r.D = ReadByte()},
                              {0x1E, () => r.E = ReadByte()},
                              {0x26, () => r.H = ReadByte()},
                              {0x2E, () => r.L = ReadByte()},
                              {0x36, () => mmu.WriteByte(r.HL, ReadByte())},

                              // LD (X), A
                              {0x02, () => mmu.WriteByte(r.BC, r.A)},
                              {0x12, () => mmu.WriteByte(r.DE, r.A)},

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
                              {0x87, () => Add(r.A)},
                              {0x80, () => Add(r.B)},
                              {0x81, () => Add(r.C)},
                              {0x82, () => Add(r.D)},
                              {0x83, () => Add(r.E)},
                              {0x84, () => Add(r.H)},
                              {0x85, () => Add(r.L)},
                              {0x86, () => Add(mmu.ReadByte(r.HL))},
                              {0xC6, () => Add(ReadByte())},

                              // Add Byte with Carry-In Instructions

                              // ADC A, X
                              {0x8F, () => Adc(r.A)},
                              {0x88, () => Adc(r.B)},
                              {0x89, () => Adc(r.C)},
                              {0x8A, () => Adc(r.D)},
                              {0x8B, () => Adc(r.E)},
                              {0x8C, () => Adc(r.H)},
                              {0x8D, () => Adc(r.L)},
                              {0x8E, () => Adc(mmu.ReadByte(r.HL))},
                              {0xCE, () => Adc(ReadByte())},

                              // Subtract Byte Instructions

                              // SUB A, X
                              {0x97, () => Sub(r.A)},
                              {0x90, () => Sub(r.B)},
                              {0x91, () => Sub(r.C)},
                              {0x92, () => Sub(r.D)},
                              {0x93, () => Sub(r.E)},
                              {0x94, () => Sub(r.H)},
                              {0x95, () => Sub(r.L)},
                              {0x96, () => Sub(mmu.ReadByte(r.HL))},
                              {0xD6, () => Sub(ReadByte())},

                              // Subtract Byte With Borrow-In Instructions

                              // SBC A, X
                              {0x9F, () => Sbc(r.A)},
                              {0x98, () => Sbc(r.B)},
                              {0x99, () => Sbc(r.C)},
                              {0x9A, () => Sbc(r.D)},
                              {0x9B, () => Sbc(r.E)},
                              {0x9C, () => Sbc(r.H)},
                              {0x9D, () => Sbc(r.L)},
                              {0x9E, () => Sbc(mmu.ReadByte(r.HL))},
                              {0xDE, () => Sbc(ReadByte())},

                              // Double Byte Add Instructions

                              // ADD HL, X
                              {0x09, () => Add(r.BC)},
                              {0x19, () => Add(r.DE)},
                              {0x29, () => Add(r.HL)},
                              {0x39, () => Add(r.SP)},

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
                              {0x3C, () => Inc(r.A)},
                              {0x04, () => Inc(r.B)},
                              {0x0C, () => Inc(r.C)},
                              {0x14, () => Inc(r.D)},
                              {0x1C, () => Inc(r.E)},
                              {0x24, () => Inc(r.H)},
                              {0x2C, () => Inc(r.L)},
                              {0x34, () => mmu.WriteByte(r.HL, Inc(mmu.ReadByte(r.HL)))},

                              // Decrement Byte Instructions

                              // DEC X
                              {0x3D, () => Dec(r.A)},
                              {0x05, () => Dec(r.B)},
                              {0x0D, () => Dec(r.C)},
                              {0x15, () => Dec(r.D)},
                              {0x1D, () => Dec(r.E)},
                              {0x25, () => Dec(r.H)},
                              {0x2D, () => Dec(r.L)},
                              {0x35, () => mmu.WriteByte(r.HL, Dec(mmu.ReadByte(r.HL)))},

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
                                  r.FlagZ = r.A == 0;

                                      r.A = v;
                                  } },

                              {0x2F, () => { r.A = (byte) ~r.A; r.FlagH = true; r.FlagN = true; }},             // CPL
                              {0x37, () => { r.FlagC = true; r.FlagH = false; r.FlagN = false; } },             // SCF
                              {0x3F, () => { bool p = r.FlagC; r.FlagC = !p; r.FlagH = p; r.FlagN = false; } }, // CCF

                              // Rotate Instructions

                              // RXCA
                              {0x07, () => { bool bit7 = r.A.GetBit(7); r.A = (byte)(r.A << 1).SetBit(0, bit7); r.FlagC = bit7; r.FlagH = false; r.FlagN = false; } },
                              {0x0F, () => { bool bit0 = r.A.GetBit(0); r.A = (byte)(r.A >> 1).SetBit(7, bit0); r.FlagC = bit0; r.FlagH = false; r.FlagN = false; } },
                              
                              // RXA
                              {0x17, () => { bool bit7 = r.A.GetBit(7); r.A = (byte)(r.A << 1).SetBit(0, r.FlagC); r.FlagC = bit7; r.FlagH = false; r.FlagN = false; } }, 
                              {0x1F, () => { bool bit0 = r.A.GetBit(0); r.A = (byte)(r.A >> 1).SetBit(7, r.FlagC); r.FlagC = bit0; r.FlagH = false; r.FlagN = false; } }, 

                              // Logical Byte Instructions
                              
                              // AND X
                              {0xA7, () => And(r.A)},
                              {0xA0, () => And(r.B)},
                              {0xA1, () => And(r.C)},
                              {0xA2, () => And(r.D)},
                              {0xA3, () => And(r.E)},
                              {0xA4, () => And(r.H)},
                              {0xA5, () => And(r.L)},
                              {0xA6, () => And(mmu.ReadByte(r.HL))},
                              {0xE6, () => And(ReadByte())},

                              // XOR X
                              {0xAF, () => Xor(r.A)},
                              {0xA8, () => Xor(r.B)},
                              {0xA9, () => Xor(r.C)},
                              {0xAA, () => Xor(r.D)},
                              {0xAB, () => Xor(r.E)},
                              {0xAC, () => Xor(r.H)},
                              {0xAD, () => Xor(r.L)},
                              {0xAE, () => Xor(mmu.ReadByte(r.HL))},
                              {0xEE, () => Xor(ReadByte())},

                              // OR X
                              {0xB7, () => Or(r.A)},
                              {0xB0, () => Or(r.B)},
                              {0xB1, () => Or(r.C)},
                              {0xB2, () => Or(r.D)},
                              {0xB3, () => Or(r.E)},
                              {0xB4, () => Or(r.H)},
                              {0xB5, () => Or(r.L)},
                              {0xB6, () => Or(mmu.ReadByte(r.HL))},
                              {0xF6, () => Or(ReadByte())},

                              // CP X
                              {0xBF, () => Cp(r.A)},
                              {0xB8, () => Cp(r.B)},
                              {0xB9, () => Cp(r.C)},
                              {0xBA, () => Cp(r.D)},
                              {0xBB, () => Cp(r.E)},
                              {0xBC, () => Cp(r.H)},
                              {0xBD, () => Cp(r.L)},
                              {0xBE, () => Cp(mmu.ReadByte(r.HL))},
                              {0xFE, () => Cp(ReadByte())},

                              // Branch Control/Program Counter Load Instructions

                              // JP X, Address
                              {0xC3, () => { r.Address = ReadUShort(); }},
                              {0xC2, () => { var us = ReadUShort(); if (!r.FlagZ) r.Address = us; }},
                              {0xCA, () => { var us = ReadUShort(); if (r.FlagZ) r.Address = us; }},
                              {0xD2, () => { var us = ReadUShort(); if (!r.FlagC) r.Address = us; }},
                              {0xDA, () => { var us = ReadUShort(); if (r.FlagC) r.Address = us; }},
                              // 0xE2, 0xEA: JP PO/PE changed in gameboy
                              // 0xF2: JP P removed in gameboy
                              // 0xFA: JP M changed in gameboy

                              // JP (HL)
                              {0xE9, () => r.Address = r.HL },

                              // JR X
                              {0x18, () => r.Address = (ushort)(r.PC + (sbyte)ReadByte()) }, 
                              {0x20, () => { var sb = (sbyte)ReadByte(); if(!r.FlagZ) r.Address = (ushort)(r.PC + sb); } }, 
                              {0x28, () => { var sb = (sbyte)ReadByte(); if(r.FlagZ) r.Address = (ushort)(r.PC + sb); } }, 
                              {0x30, () => { var sb = (sbyte)ReadByte(); if(!r.FlagC) r.Address = (ushort)(r.PC + sb); } }, 
                              {0x38, () => { var sb = (sbyte)ReadByte(); if(r.FlagC) r.Address = (ushort)(r.PC + sb); } }, 

                              // CALL X
                              {0xCD, () => Call(ReadUShort()) },
                              {0xC4, () => { var us = ReadUShort(); if(!r.FlagZ) Call(us); } },
                              {0xCC, () => { var us = ReadUShort(); if(r.FlagZ) Call(us); } },
                              {0xD4, () => { var us = ReadUShort(); if(!r.FlagC) Call(us); } },
                              {0xDC, () => { var us = ReadUShort(); if(r.FlagC) Call(us); } },
                              // 0xE4, 0xEC: CALL PO/PE removed in gameboy
                              // 0xF4, 0xFC: CALL P/M removed in gameboy

                              // RET X
                              {0xC9, Ret },
                              {0xC0, () => { if(!r.FlagZ) Ret(); } },
                              {0xC8, () => { if(r.FlagZ) Ret(); } },
                              {0xD0, () => { if(!r.FlagC) Ret(); } },
                              {0xD8, () => { if(r.FlagC) Ret(); } },
                              // 0xE0, 0xE8: RET PO/PE changed in gameboy
                              // 0xF0: RET P changed in gameboy

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
                              {0xC5, () => { mmu.WriteByte(--r.SP, r.B); mmu.WriteByte(--r.SP, r.C); } },
                              {0xD5, () => { mmu.WriteByte(--r.SP, r.D); mmu.WriteByte(--r.SP, r.E); } },
                              {0xE5, () => { mmu.WriteByte(--r.SP, r.H); mmu.WriteByte(--r.SP, r.L); } },
                              {0xF5, () => { mmu.WriteByte(--r.SP, r.A); mmu.WriteByte(--r.SP, r.F); } },

                              // POP X
                              {0xC1, () => { r.C = mmu.ReadByte(r.SP); r.B = mmu.ReadByte(++r.SP); } },
                              {0xD1, () => { r.E = mmu.ReadByte(r.SP); r.D = mmu.ReadByte(++r.SP); } },
                              {0xE1, () => { r.L = mmu.ReadByte(r.SP); r.H = mmu.ReadByte(++r.SP); } },
                              {0xF1, () => { r.F = mmu.ReadByte(r.SP); r.A = mmu.ReadByte(++r.SP); } },

                              // No IN/OUT instructions for Gameboy

                          };


            // EXTENDED CB PREFIX OPCODES

            cbopcodes = new Dictionary<byte, Action>
                            {
                                // Rotate Commands

                                // RLC X
                                {0x07, () => r.A = Rlc(r.A) },
                                {0x00, () => r.B = Rlc(r.B) },
                                {0x01, () => r.C = Rlc(r.C) },
                                {0x02, () => r.D = Rlc(r.D) },
                                {0x03, () => r.E = Rlc(r.E) },
                                {0x04, () => r.H = Rlc(r.H) },
                                {0x05, () => r.L = Rlc(r.L) },
                                {0x06, () => mmu.WriteByte(r.HL, Rlc(mmu.ReadByte(r.HL))) },

                                // RRC X
                                {0x0F, () => r.A = Rrc(r.A) },
                                {0x08, () => r.B = Rrc(r.B) },
                                {0x09, () => r.C = Rrc(r.C) },
                                {0x0A, () => r.D = Rrc(r.D) },
                                {0x0B, () => r.E = Rrc(r.E) },
                                {0x0C, () => r.H = Rrc(r.H) },
                                {0x0D, () => r.L = Rrc(r.L) },
                                {0x0E, () => mmu.WriteByte(r.HL, Rrc(mmu.ReadByte(r.HL))) },

                                // RL X
                                {0x17, () => r.A = Rl(r.A) },
                                {0x10, () => r.B = Rl(r.B) },
                                {0x11, () => r.C = Rl(r.C) },
                                {0x12, () => r.D = Rl(r.D) },
                                {0x13, () => r.E = Rl(r.E) },
                                {0x14, () => r.H = Rl(r.H) },
                                {0x15, () => r.L = Rl(r.L) },
                                {0x16, () => mmu.WriteByte(r.HL, Rl(mmu.ReadByte(r.HL))) },

                                // RR X
                                {0x1F, () => r.A = Rr(r.A) },
                                {0x18, () => r.B = Rr(r.B) },
                                {0x19, () => r.C = Rr(r.C) },
                                {0x1A, () => r.D = Rr(r.D) },
                                {0x1B, () => r.E = Rr(r.E) },
                                {0x1C, () => r.H = Rr(r.H) },
                                {0x1D, () => r.L = Rr(r.L) },
                                {0x1E, () => mmu.WriteByte(r.HL, Rr(mmu.ReadByte(r.HL))) },

                                // SLA X
                                {0x27, () => r.A = Sla(r.A) },
                                {0x20, () => r.B = Sla(r.B) },
                                {0x21, () => r.C = Sla(r.C) },
                                {0x22, () => r.D = Sla(r.D) },
                                {0x23, () => r.E = Sla(r.E) },
                                {0x24, () => r.H = Sla(r.H) },
                                {0x25, () => r.L = Sla(r.L) },
                                {0x26, () => mmu.WriteByte(r.HL, Sla(mmu.ReadByte(r.HL))) },

                                // SRA X
                                {0x2F, () => r.A = Sra(r.A) },
                                {0x28, () => r.B = Sra(r.B) },
                                {0x29, () => r.C = Sra(r.C) },
                                {0x2A, () => r.D = Sra(r.D) },
                                {0x2B, () => r.E = Sra(r.E) },
                                {0x2C, () => r.H = Sra(r.H) },
                                {0x2D, () => r.L = Sra(r.L) },
                                {0x2E, () => mmu.WriteByte(r.HL, Sra(mmu.ReadByte(r.HL))) },

                                // SWAP X
                                {0x37, () => r.A = Swap(r.A) },
                                {0x30, () => r.B = Swap(r.B) },
                                {0x31, () => r.C = Swap(r.C) },
                                {0x32, () => r.D = Swap(r.D) },
                                {0x33, () => r.E = Swap(r.E) },
                                {0x34, () => r.H = Swap(r.H) },
                                {0x35, () => r.L = Swap(r.L) },
                                {0x36, () => mmu.WriteByte(r.HL, Swap(mmu.ReadByte(r.HL))) },

                                // SRL X
                                {0x3F, () => r.A = Srl(r.A) },
                                {0x38, () => r.B = Srl(r.B) },
                                {0x39, () => r.C = Srl(r.C) },
                                {0x3A, () => r.D = Srl(r.D) },
                                {0x3B, () => r.E = Srl(r.E) },
                                {0x3C, () => r.H = Srl(r.H) },
                                {0x3D, () => r.L = Srl(r.L) },
                                {0x3E, () => mmu.WriteByte(r.HL, Srl(mmu.ReadByte(r.HL))) },

                                // BIT

                                // BIT 0, X
                                {0x47, () => Bit(0, r.A) },
                                {0x40, () => Bit(0, r.B) },
                                {0x41, () => Bit(0, r.C) },
                                {0x42, () => Bit(0, r.D) },
                                {0x43, () => Bit(0, r.E) },
                                {0x44, () => Bit(0, r.H) },
                                {0x45, () => Bit(0, r.L) },
                                {0x46, () => Bit(0, mmu.ReadByte(r.HL)) },

                                // BIT 1, X
                                {0x4F, () => Bit(1, r.A) },
                                {0x48, () => Bit(1, r.B) },
                                {0x49, () => Bit(1, r.C) },
                                {0x4A, () => Bit(1, r.D) },
                                {0x4B, () => Bit(1, r.E) },
                                {0x4C, () => Bit(1, r.H) },
                                {0x4D, () => Bit(1, r.L) },
                                {0x4E, () => Bit(1, mmu.ReadByte(r.HL)) },

                                // BIT 2, X
                                {0x57, () => Bit(2, r.A) },
                                {0x50, () => Bit(2, r.B) },
                                {0x51, () => Bit(2, r.C) },
                                {0x52, () => Bit(2, r.D) },
                                {0x53, () => Bit(2, r.E) },
                                {0x54, () => Bit(2, r.H) },
                                {0x55, () => Bit(2, r.L) },
                                {0x56, () => Bit(2, mmu.ReadByte(r.HL)) },

                                // BIT 3, X
                                {0x5F, () => Bit(3, r.A) },
                                {0x58, () => Bit(3, r.B) },
                                {0x59, () => Bit(3, r.C) },
                                {0x5A, () => Bit(3, r.D) },
                                {0x5B, () => Bit(3, r.E) },
                                {0x5C, () => Bit(3, r.H) },
                                {0x5D, () => Bit(3, r.L) },
                                {0x5E, () => Bit(3, mmu.ReadByte(r.HL)) },

                                // BIT 4, X
                                {0x67, () => Bit(4, r.A) },
                                {0x60, () => Bit(4, r.B) },
                                {0x61, () => Bit(4, r.C) },
                                {0x62, () => Bit(4, r.D) },
                                {0x63, () => Bit(4, r.E) },
                                {0x64, () => Bit(4, r.H) },
                                {0x65, () => Bit(4, r.L) },
                                {0x66, () => Bit(4, mmu.ReadByte(r.HL)) },

                                // BIT 5, X
                                {0x6F, () => Bit(5, r.A) },
                                {0x68, () => Bit(5, r.B) },
                                {0x69, () => Bit(5, r.C) },
                                {0x6A, () => Bit(5, r.D) },
                                {0x6B, () => Bit(5, r.E) },
                                {0x6C, () => Bit(5, r.H) },
                                {0x6D, () => Bit(5, r.L) },
                                {0x6E, () => Bit(5, mmu.ReadByte(r.HL)) },

                                // BIT 6, X
                                {0x77, () => Bit(6, r.A) },
                                {0x70, () => Bit(6, r.B) },
                                {0x71, () => Bit(6, r.C) },
                                {0x72, () => Bit(6, r.D) },
                                {0x73, () => Bit(6, r.E) },
                                {0x74, () => Bit(6, r.H) },
                                {0x75, () => Bit(6, r.L) },
                                {0x76, () => Bit(6, mmu.ReadByte(r.HL)) },

                                // BIT 7, X
                                {0x7F, () => Bit(7, r.A) },
                                {0x78, () => Bit(7, r.B) },
                                {0x79, () => Bit(7, r.C) },
                                {0x7A, () => Bit(7, r.D) },
                                {0x7B, () => Bit(7, r.E) },
                                {0x7C, () => Bit(7, r.H) },
                                {0x7D, () => Bit(7, r.L) },
                                {0x7E, () => Bit(7, mmu.ReadByte(r.HL)) },

                                // RES

                                // RES 0, X
                                {0x87, () => r.A = Res(0, r.A) },
                                {0x80, () => r.B = Res(0, r.B) },
                                {0x81, () => r.C = Res(0, r.C) },
                                {0x82, () => r.D = Res(0, r.D) },
                                {0x83, () => r.E = Res(0, r.E) },
                                {0x84, () => r.H = Res(0, r.H) },
                                {0x85, () => r.L = Res(0, r.L) },
                                {0x86, () => mmu.WriteByte(r.HL, Res(0, mmu.ReadByte(r.HL))) },

                                // RES 1, X
                                {0x8F, () => r.A = Res(1, r.A) },
                                {0x88, () => r.B = Res(1, r.B) },
                                {0x89, () => r.C = Res(1, r.C) },
                                {0x8A, () => r.D = Res(1, r.D) },
                                {0x8B, () => r.E = Res(1, r.E) },
                                {0x8C, () => r.H = Res(1, r.H) },
                                {0x8D, () => r.L = Res(1, r.L) },
                                {0x8E, () => mmu.WriteByte(r.HL, Res(1, mmu.ReadByte(r.HL))) },

                                // RES 2, X
                                {0x97, () => r.A = Res(2, r.A) },
                                {0x90, () => r.B = Res(2, r.B) },
                                {0x91, () => r.C = Res(2, r.C) },
                                {0x92, () => r.D = Res(2, r.D) },
                                {0x93, () => r.E = Res(2, r.E) },
                                {0x94, () => r.H = Res(2, r.H) },
                                {0x95, () => r.L = Res(2, r.L) },
                                {0x96, () => mmu.WriteByte(r.HL, Res(2, mmu.ReadByte(r.HL))) },

                                // RES 3, X
                                {0x9F, () => r.A = Res(3, r.A) },
                                {0x98, () => r.B = Res(3, r.B) },
                                {0x99, () => r.C = Res(3, r.C) },
                                {0x9A, () => r.D = Res(3, r.D) },
                                {0x9B, () => r.E = Res(3, r.E) },
                                {0x9C, () => r.H = Res(3, r.H) },
                                {0x9D, () => r.L = Res(3, r.L) },
                                {0x9E, () => mmu.WriteByte(r.HL, Res(3, mmu.ReadByte(r.HL))) },

                                // RES 4, X
                                {0xA7, () => r.A = Res(4, r.A) },
                                {0xA0, () => r.B = Res(4, r.B) },
                                {0xA1, () => r.C = Res(4, r.C) },
                                {0xA2, () => r.D = Res(4, r.D) },
                                {0xA3, () => r.E = Res(4, r.E) },
                                {0xA4, () => r.H = Res(4, r.H) },
                                {0xA5, () => r.L = Res(4, r.L) },
                                {0xA6, () => mmu.WriteByte(r.HL, Res(4, mmu.ReadByte(r.HL))) },

                                // RES 5, X
                                {0xAF, () => r.A = Res(5, r.A) },
                                {0xA8, () => r.B = Res(5, r.B) },
                                {0xA9, () => r.C = Res(5, r.C) },
                                {0xAA, () => r.D = Res(5, r.D) },
                                {0xAB, () => r.E = Res(5, r.E) },
                                {0xAC, () => r.H = Res(5, r.H) },
                                {0xAD, () => r.L = Res(5, r.L) },
                                {0xAE, () => mmu.WriteByte(r.HL, Res(5, mmu.ReadByte(r.HL))) },

                                // RES 6, X
                                {0xB7, () => r.A = Res(6, r.A) },
                                {0xB0, () => r.B = Res(6, r.B) },
                                {0xB1, () => r.C = Res(6, r.C) },
                                {0xB2, () => r.D = Res(6, r.D) },
                                {0xB3, () => r.E = Res(6, r.E) },
                                {0xB4, () => r.H = Res(6, r.H) },
                                {0xB5, () => r.L = Res(6, r.L) },
                                {0xB6, () => mmu.WriteByte(r.HL, Res(6, mmu.ReadByte(r.HL))) },

                                // RES 7, X
                                {0xBF, () => r.A = Res(7, r.A) },
                                {0xB8, () => r.B = Res(7, r.B) },
                                {0xB9, () => r.C = Res(7, r.C) },
                                {0xBA, () => r.D = Res(7, r.D) },
                                {0xBB, () => r.E = Res(7, r.E) },
                                {0xBC, () => r.H = Res(7, r.H) },
                                {0xBD, () => r.L = Res(7, r.L) },
                                {0xBE, () => mmu.WriteByte(r.HL, Res(7, mmu.ReadByte(r.HL))) },

                                // SET

                                // SET 0, X
                                {0xC7, () => r.A = Set(0, r.A) },
                                {0xC0, () => r.B = Set(0, r.B) },
                                {0xC1, () => r.C = Set(0, r.C) },
                                {0xC2, () => r.D = Set(0, r.D) },
                                {0xC3, () => r.E = Set(0, r.E) },
                                {0xC4, () => r.H = Set(0, r.H) },
                                {0xC5, () => r.L = Set(0, r.L) },
                                {0xC6, () => mmu.WriteByte(r.HL, Set(0, mmu.ReadByte(r.HL))) },

                                // SET 1, X
                                {0xCF, () => r.A = Set(1, r.A) },
                                {0xC8, () => r.B = Set(1, r.B) },
                                {0xC9, () => r.C = Set(1, r.C) },
                                {0xCA, () => r.D = Set(1, r.D) },
                                {0xCB, () => r.E = Set(1, r.E) },
                                {0xCC, () => r.H = Set(1, r.H) },
                                {0xCD, () => r.L = Set(1, r.L) },
                                {0xCE, () => mmu.WriteByte(r.HL, Set(1, mmu.ReadByte(r.HL))) },

                                // SET 2, X
                                {0xD7, () => r.A = Set(2, r.A) },
                                {0xD0, () => r.B = Set(2, r.B) },
                                {0xD1, () => r.C = Set(2, r.C) },
                                {0xD2, () => r.D = Set(2, r.D) },
                                {0xD3, () => r.E = Set(2, r.E) },
                                {0xD4, () => r.H = Set(2, r.H) },
                                {0xD5, () => r.L = Set(2, r.L) },
                                {0xD6, () => mmu.WriteByte(r.HL, Set(2, mmu.ReadByte(r.HL))) },

                                // SET 3, X
                                {0xDF, () => r.A = Set(3, r.A) },
                                {0xD8, () => r.B = Set(3, r.B) },
                                {0xD9, () => r.C = Set(3, r.C) },
                                {0xDA, () => r.D = Set(3, r.D) },
                                {0xDB, () => r.E = Set(3, r.E) },
                                {0xDC, () => r.H = Set(3, r.H) },
                                {0xDD, () => r.L = Set(3, r.L) },
                                {0xDE, () => mmu.WriteByte(r.HL, Set(3, mmu.ReadByte(r.HL))) },

                                // SET 4, X
                                {0xE7, () => r.A = Set(4, r.A) },
                                {0xE0, () => r.B = Set(4, r.B) },
                                {0xE1, () => r.C = Set(4, r.C) },
                                {0xE2, () => r.D = Set(4, r.D) },
                                {0xE3, () => r.E = Set(4, r.E) },
                                {0xE4, () => r.H = Set(4, r.H) },
                                {0xE5, () => r.L = Set(4, r.L) },
                                {0xE6, () => mmu.WriteByte(r.HL, Set(4, mmu.ReadByte(r.HL))) },

                                // SET 5, X
                                {0xEF, () => r.A = Set(5, r.A) },
                                {0xE8, () => r.B = Set(5, r.B) },
                                {0xE9, () => r.C = Set(5, r.C) },
                                {0xEA, () => r.D = Set(5, r.D) },
                                {0xEB, () => r.E = Set(5, r.E) },
                                {0xEC, () => r.H = Set(5, r.H) },
                                {0xED, () => r.L = Set(5, r.L) },
                                {0xEE, () => mmu.WriteByte(r.HL, Set(5, mmu.ReadByte(r.HL))) },

                                // SET 6, X
                                {0xF7, () => r.A = Set(6, r.A) },
                                {0xF0, () => r.B = Set(6, r.B) },
                                {0xF1, () => r.C = Set(6, r.C) },
                                {0xF2, () => r.D = Set(6, r.D) },
                                {0xF3, () => r.E = Set(6, r.E) },
                                {0xF4, () => r.H = Set(6, r.H) },
                                {0xF5, () => r.L = Set(6, r.L) },
                                {0xF6, () => mmu.WriteByte(r.HL, Set(6, mmu.ReadByte(r.HL))) },

                                // SET 7, X
                                {0xFF, () => r.A = Set(7, r.A) },
                                {0xF8, () => r.B = Set(7, r.B) },
                                {0xF9, () => r.C = Set(7, r.C) },
                                {0xFA, () => r.D = Set(7, r.D) },
                                {0xFB, () => r.E = Set(7, r.E) },
                                {0xFC, () => r.H = Set(7, r.H) },
                                {0xFD, () => r.L = Set(7, r.L) },
                                {0xFE, () => mmu.WriteByte(r.HL, Set(7, mmu.ReadByte(r.HL))) },
                                
                            };
        }
    }
}
