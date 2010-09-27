using System;
using System.Data;
using RomTools;

namespace GbcEmulator.Memory
{
    public class MemoryManagementUnit : IMemoryManagementUnit
    {
        // Info about gameboy MMU used from:
        // http://meatfighter.com/gameboy/TheNintendoGameboy.pdf
        // http://imrannazar.com/GameBoy-Emulation-in-JavaScript:-Memory
        // http://www.devrs.com/gb/files/gbspec.txt

        // BUG: GPU, Keypad interrupts etc not implemented

        private readonly RomInfo ri;

        private MemoryBankController mbc;

        public Timer Timer { get; set; }


        public MemoryManagementUnit(RomInfo romInfo, Timer timer)
        {
            ri = romInfo;
            mbc = MemoryBankController.Factory(romInfo);
            Timer = timer;
        }


        public byte ReadByte(int address)
        {
            switch (address & 0xF000)
            {
                // Read only
                case 0x0000:
                    // BUG: BIOS reading here? Need to research GB start up process
                    return ri.Rom[address];

                // Read only
                // 16KB ROM Bank 00 (in cartridge, fixed at bank 00)
                case 0x1000:
                case 0x2000:
                case 0x3000:
                    return ri.Rom[address];

                // 16KB ROM Bank 01..NN (in cartridge, switchable bank nunber)
                case 0x4000:
                case 0x5000:
                case 0x6000:
                case 0x7000:
                    return mbc.Read4000_7FFF(address);

                // 8KB Video RAM (VRAM) (switchable bank 0-1 in CGB Mode)
                case 0x8000:
                case 0x9000:
                    throw new NotImplementedException();

                // 8KB External RAM (in cartridge, switchable bank, if any)
                case 0xA000:
                case 0xB000:
                    return mbc.ReadEram(address);

                // 4KB Work RAM Bank 0 (WRAM))
                case 0xC000:
                // Same as C000-DDFF (ECHO) (typically not used)
                case 0xD000:
                case 0xE000:
                    return mbc.ReadWram(address);

                case 0xF000:
                    switch (address & 0x0F00)
                    {
                        // Same as C000-DDFF (ECHO) (typically not used)
                        case 0x0000: case 0x0100: case 0x0200: case 0x0300: 
                        case 0x0400: case 0x0500: case 0x0600: case 0x0700: 
                        case 0x0800: case 0x0900: case 0x0A00: case 0x0B00:
                        case 0x0C00: case 0x0D00:
                            return mbc.ReadWram(address);

                        case 0x0E00:
                            // Sprite Attribute Table (OAM)
                            if((address & 0x00FF) < 0xA0)
                                throw new NotImplementedException();

                            throw new InvalidOperationException("Cannot access memory location: " + address.ToString("X"));

                        // Zeropage ram, I/O and interrupts
                        // BUG: This bit could be laid out wrong; more stuff in 0x0F00 - 0x0F80
                        // 0xFFxx
                        case 0x0F00:
                            switch(address & 0x00F0)
                            {
                                // 0xFF0x
                                case 0x0:
                                    switch (address & 0x000F)
                                    {
                                        // Joypad
                                        case 0:
                                            throw new NotImplementedException();

                                        // Timer and Divider Registers
                                        case 4:
                                            return Timer.Div;
                                        case 5:
                                            return Timer.Tima;
                                        case 6:
                                            return Timer.Tma;
                                        case 7:
                                            return Timer.Tac;

                                        // Interrupt register? BUG: What interrupt register should this be?
                                    }
                                    break;

                                // I/O Ports
                                case 0x10: case 0x20: case 0x30: case 0x40:
                                case 0x50: case 0x60: case 0x70:
                                    throw new NotImplementedException();

                                case 0x80: case 0x90: case 0xA0: case 0xB0: 
                                case 0xC0: case 0xD0: case 0xE0: case 0xF0:
                                    // 0xFFFF = interrupt enable register. BUG: Should move somewhere else?
                                    return mbc.Zram[address & 0x7F];
                            }
                            break;
                    }
                    break;
            }

            throw new InvalidOperationException("Something went wrong when accessing memory location " + address.ToString("X"));
        }

        public ushort ReadUShort(int address)
        {
            return (ushort)(ReadByte(address) | (ReadByte(address+1) << 8));
        }

        public void WriteByte(int address, byte value)
        {
            switch (address & 0xF000)
            {
                case 0x0000:
                case 0x1000:
                    mbc.Write0000_1FFF(address, value);
                    break;

                case 0x2000:
                case 0x3000:
                    mbc.Write2000_3FFF(address, value);
                    break;

                case 0x4000:
                case 0x5000:
                    mbc.Write4000_5FFF(address, value);
                    break;

                case 0x6000:
                case 0x7000:
                    mbc.Write6000_7FFF(address, value);
                    break;

                // VRAM
                case 0x8000:
                case 0x9000:
                    throw new NotImplementedException();

                // ERAM
                case 0xA000:
                case 0xB000:
                    mbc.WriteEram(address, value);
                    break;

                // WRAM
                case 0xC000:
                case 0xD000:
                case 0xE000:
                    mbc.WriteWram(address, value);
                    break;

                case 0xF000:
                    switch (address & 0x0F00)
                    {
                        // Same as C000-DDFF (ECHO) (typically not used)
                        case 0x0000: case 0x0100: case 0x0200: case 0x0300: 
                        case 0x0400: case 0x0500: case 0x0600: case 0x0700: 
                        case 0x0800: case 0x0900: case 0x0A00: case 0x0B00:
                        case 0x0C00: case 0x0D00:
                            mbc.WriteWram(address, value);
                            break;

                        // Sprite Attribute Table (OAM)
                        case 0x0E00:
                            throw new NotImplementedException();

                        // Zeropage ram, I/O and interrupts
                        // BUG: This bit could be laid out wrong; more stuff in 0x0F00 - 0x0F80
                        case 0x0F00:
                            switch(address & 0x00F0)
                            {
                                // 0xFF0x
                                case 0x0:
                                    switch (address & 0x000F)
                                    {
                                        // Joypad
                                        case 0:
                                            throw new NotImplementedException();

                                        // Timer and Divider Registers
                                        case 4:
                                            Timer.Div = value;
                                            break;
                                        case 5:
                                            Timer.Tima = value;
                                            break;
                                        case 6:
                                            Timer.Tma = value;
                                            break;
                                        case 7:
                                            Timer.SetTac(value);
                                            break;

                                        // Interrupt register? BUG: What interrupt register should this be?
                                    }
                                    break;

                                // I/O Ports
                                case 0x10: case 0x20: case 0x30: case 0x40:
                                case 0x50: case 0x60: case 0x70:
                                    throw new NotImplementedException();

                                case 0x80: case 0x90: case 0xA0: case 0xB0: 
                                case 0xC0: case 0xD0: case 0xE0: case 0xF0:
                                    // 0xFFFF = interrupt enable register. BUG: Should move somewhere else?
                                    mbc.Zram[address & 0x7F] = value;
                                    break;
                            }
                            break;
                    }
                    break;
            }
        }

        public void WriteUShort(int address, ushort value)
        {
            WriteByte(address, (byte)(value & 0xFF));
            WriteByte(address+1, (byte)(value >> 8));
        }
    }
}
