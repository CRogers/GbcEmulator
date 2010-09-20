namespace GbcEmulator.Memory
{
    public class FlatMMU : IMemoryManagementUnit
    {
        private byte[] ram;

        public FlatMMU(int size)
        {
            ram = new byte[size];
        }

        public byte ReadByte(int addr)
        {
            return ram[addr];
        }

        public ushort ReadUShort(int addr)
        {
            return (ushort)(ram[addr] | (ram[addr+1] << 8));
        }

        public void WriteByte(int addr, byte value)
        {
            ram[addr] = value;
        }

        public void WriteUShort(int addr, ushort value)
        {
            ram[addr] = (byte)(value & 0xFF);
            ram[addr + 1] = (byte)(value >> 8);
        }
    }
}
