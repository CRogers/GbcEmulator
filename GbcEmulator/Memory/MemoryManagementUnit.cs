using System;
using System.Linq;
using RomTools;

namespace GbcEmulator.Memory
{
    public class MemoryManagementUnit
    {
        // Info about gameboy MMU used from:
        // http://meatfighter.com/gameboy/TheNintendoGameboy.pdf
        // http://imrannazar.com/GameBoy-Emulation-in-JavaScript:-Memory
        // http://www.devrs.com/gb/files/gbspec.txt

        // BUG: Mbc5 is unsupported
        // BUG: GPU, Keypad interrupts etc not implemented

        private readonly RomInfo ri;
        
        private byte[] bios;
        private readonly byte[] rom;

        private MemoryBankController mbc;

        private Registers r;


        public MemoryManagementUnit(RomInfo romInfo, Registers registers)
        {
            rom = romInfo.Rom.ToArray();
            ri = romInfo;
            mbc = new MemoryBankController(romInfo);
            r = registers;
        }


        public byte ReadByte(int address)
        {
            switch (address & 0xF000)
            {
                // Read only
                case 0x0000:
                    // BUG: BIOS reading here? Need to research GB start up process
                    return rom[address];

                // Read only
                // 16KB ROM Bank 00 (in cartridge, fixed at bank 00)
                case 0x1000:
                case 0x2000:
                case 0x3000:
                    return rom[address];

                // 16KB ROM Bank 01..NN (in cartridge, switchable bank nunber)
                case 0x4000:
                case 0x5000:
                case 0x6000:
                case 0x7000:
                    return rom[mbc.RomOffset + (address & 0x3FFF)];

                // 8KB Video RAM (VRAM) (switchable bank 0-1 in CGB Mode)
                case 0x8000:
                case 0x9000:
                    throw new NotImplementedException();

                // 8KB External RAM (in cartridge, switchable bank, if any)
                case 0xA000:
                case 0xB000:
                    return mbc.Eram[mbc.RamOffset + (address & 0x1FFF)];

                // 4KB Work RAM Bank 0 (WRAM)
                case 0xC000:
                // Same as C000-DDFF (ECHO) (typically not used)
                case 0xD000:
                case 0xE000:
                    return mbc.Wram[address & 0x1FFF];

                case 0xF000:
                    switch (address & 0x0F00)
                    {
                        // Same as C000-DDFF (ECHO) (typically not used)
                        case 0x0000: case 0x0100: case 0x0200: case 0x0300: 
                        case 0x0400: case 0x0500: case 0x0600: case 0x0700: 
                        case 0x0800: case 0x0900: case 0x0A00: case 0x0B00:
                        case 0x0C00: case 0x0D00:
                            return mbc.Wram[address & 0x1FFF];

                        case 0x0E00:
                            // Sprite Attribute Table (OAM)
                            if((address & 0x00FF) < 0xA0)
                                throw new NotImplementedException();

                            throw new InvalidOperationException("Cannot access memory location: " + address.ToString("X"));

                        // Zeropage ram, I/O and interrupts
                        // BUG: This bit could be laid out wrong; more stuff in 0x0F00 - 0x0F80
                        case 0x0F00:
                            {
                                int addr = address & 0x00FF;

                                // I/O Ports
                                if (addr < 0x80)
                                    throw new NotImplementedException();

                                // Zeropage RAM (ZRAM)
                                else if (addr < 0xFE)
                                    return mbc.Zram[address & 0x7F];

                                // Interrupt enable register BUG: What interrupt register should this be?
                                else
                                    return r.I;
                            }
                    }
                    break;
            }

            throw new InvalidOperationException("Something went wrong when accessing memory location " + address.ToString("X"));
        }

        public ushort ReadUShort(ushort address)
        {
            return (ushort)(ReadByte(address) | (ReadByte(address+1) << 8));
        }

        public void WriteByte(int address, byte value)
        {
            switch (address & 0xF000)
            {
                // Enable/Disable RAM
                case 0x0000:
                case 0x1000:
                    mbc.ExternalRamOn = (value & 0x0F) == 0xA;
                    break;

                // Select ROM Bank Number
                // WARN: There is a Mbc1 and Mbc2 specific bug here: see http://www.devrs.com/gb/files/gbspec.txt
                // Writing to this address space selects the lower 5 bits of the ROM Bank Number (in range 01-1Fh). 
                // When 00h is written, the MBC translates that to bank 01h also. That doesn't harm so far, 
                // because ROM Bank 00h can be always directly accessed by reading from 0000-3FFF. 
                // But (when using the register below to specify the upper ROM Bank bits), the same happens 
                // for Bank 20h, 40h, and 60h. Any attempt to address these ROM Banks will select Bank 21h, 41h, and 61h instead.
                case 0x2000:
                case 0x3000:
                    {
                        mbc.Rombank &= 0x60;
                        int bankNum = value & 0x1F;
                        switch (ri.CartridgeInfo.MbcType)
                        {
                            case MbcType.Mbc1:
                            case MbcType.Mbc2:
                                if (bankNum % 0x20 == 0)
                                    bankNum++;
                                break;
                        }
                        mbc.Rombank |= bankNum;
                        break;
                    }

                // RAM Bank Number - or - Upper Bits of ROM Bank Number
                case 0x4000:
                case 0x5000:
                    // Select Rambank from 0-3 using two LSB
                    if (mbc.Mode == MemoryMode.M4_32)
                        mbc.RamBank = value & 0x3;
                    // Select bits 5 and 6 of Rombank using two LSB of value
                    else
                        mbc.Rombank |= ((value & 0x3) << 5);
                    break;

                // BUG: Check differences for Mbc2, Mbc3, Mbc5 (and for the above)
                // ROM/RAM Mode Select
                case 0x6000:
                case 0x7000:
                    mbc.Mode = (value & 1) == 0 ? MemoryMode.M16_8 : MemoryMode.M4_32;
                    break;

                // VRAM
                case 0x8000:
                case 0x9000:
                    throw new NotImplementedException();

                case 0xA000:
                case 0xB000:
                    mbc.Eram[mbc.RamOffset + address & 0x1FFFF] = value;
                    break;

                case 0xC000:
                case 0xD000:
                case 0xE000:
                    mbc.Wram[address & 0x1FFF] = value;
                    break;

                case 0xF000:
                    switch (address & 0x0F00)
                    {
                        // Same as C000-DDFF (ECHO) (typically not used)
                        case 0x0000: case 0x0100: case 0x0200: case 0x0300: 
                        case 0x0400: case 0x0500: case 0x0600: case 0x0700: 
                        case 0x0800: case 0x0900: case 0x0A00: case 0x0B00:
                        case 0x0C00: case 0x0D00:
                            mbc.Wram[address & 0x1FFF] = value;
                            break;

                        case 0x0E00:
                            throw new NotImplementedException();

                        // Zeropage ram, I/O and interrupts
                        // BUG: This bit could be laid out wrong; more stuff in 0x0F00 - 0x0F80
                        case 0x0F00:
                            {
                                int addr = address & 0x00FF;

                                // I/O Ports
                                if (addr < 0x80)
                                    throw new NotImplementedException();

                                // Zeropage RAM (ZRAM)
                                else if (addr < 0xFE)
                                    mbc.Zram[address & 0x7F] = value;

                                // Interrupt enable register BUG: What interrupt register should this be? Interrupt or Interrupt Enable?
                                else
                                    r.I = value;
                                break;
                            }
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
