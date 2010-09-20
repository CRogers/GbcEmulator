namespace GbcEmulator.Memory
{
    public interface IMemoryManagementUnit
    {
        byte ReadByte(int addr);
        ushort ReadUShort(int addr);

        void WriteByte(int addr, byte value);
        void WriteUShort(int addr, ushort value);
    }
}
