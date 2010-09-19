namespace RomTools.Emulator
{
    public partial class GameBoy
    {
        // BUG: All FlagH setting for borrows from subractions could be wrong!

        private void FlagSZSet(byte value)
        {
            r.FlagZ = value == 0;
        }

        private void FlagSZHSet(byte value, byte initial, byte operand)
        {
            r.FlagZ = value == 0;
            r.FlagH = (r.A ^ initial ^ operand).GetBit(4);
        }


        //////////////////////////////////////////////////////////////////////////
        #region Arithmetic Operations
        private void Add(byte b)
        {
            var result = (byte)(r.A + b);
            r.FlagN = false;
            r.FlagC = result > byte.MaxValue;
            FlagSZHSet(result, r.A, b);

            r.A = result;
        }

        private void Adc(byte b)
        {
            var result = (byte)(r.A + b + r.FlagCInt);
            r.FlagN = false;
            r.FlagC = result > byte.MaxValue;
            FlagSZHSet(result, r.A, b);

            r.A = result;
        }

        private void Sub(byte b)
        {
            var result = (byte)(r.A - b);
            r.FlagN = false;
            r.FlagC = result < byte.MinValue;
            FlagSZHSet(result, r.A, b);

            r.A = result;
        }

        private void Sbc(byte b)
        {
            var result = (byte)(r.A - b - r.FlagCInt);
            r.FlagN = false;
            r.FlagC = result < byte.MinValue;
            FlagSZHSet(result, r.A, b);

            r.A = result;
        }

        private void Add(ushort u)
        {
            var result = r.HL + u;
            r.FlagH = (r.A ^ u ^ result).GetBit(12);
            r.FlagN = false;
            r.FlagC = result > byte.MaxValue;
            r.FlagZ = r.HL == 0;

            r.HL = (ushort) result;
        }

        private byte Inc(byte b)
        {
            var result = (byte)(b+1);
            r.FlagN = false;
            FlagSZHSet(result, r.A, b);

            return result;
        }

        private byte Dec(byte b)
        {
            var result = (byte)(b - 1);
            r.FlagN = true;
            FlagSZHSet(result, r.A, b);

            return result;
        }
        #endregion


        //////////////////////////////////////////////////////////////////////////
        #region Logical Operations
        private void And(byte b)
        {
            var result = (byte)(r.A & b);
            FlagSZSet(result);
            r.FlagH = true;
            r.FlagN = false;
            r.FlagC = false;

            r.A = result;
        }

        private void Or(byte b)
        {
            var result = (byte)(r.A | b);
            FlagSZSet(result);
            r.FlagH = false;
            r.FlagN = false;
            r.FlagC = false;

            r.A = result;
        }

        private void Xor(byte b)
        {
            var result = (byte)(r.A ^ b);
            FlagSZSet(result);
            r.FlagH = false;
            r.FlagN = false;
            r.FlagC = false;

            r.A = result;
        }

        private void Cp(byte b)
        {
            var result = (byte)(r.A - b);
            FlagSZHSet(result, r.A, b);
            r.FlagN = true;
            r.FlagC = result < 0;
        }
        #endregion
        

        //////////////////////////////////////////////////////////////////////////
        #region Bit Operations
        private void Bit(int bit, byte register)
        {
            r.FlagZ = !register.GetBit(bit);
            r.FlagH = true;
            r.FlagN = false;
        }

        private static byte Res(int bit, byte register)
        {
            return register.SetBit(bit, false);
        }

        private static byte Set(int bit, byte register)
        {
            return register.SetBit(bit, true);
        }
        #endregion


        //////////////////////////////////////////////////////////////////////////
        #region Rotate Operations
        private void RlFlags(byte result)
        {
            r.FlagZ = result == 0;
            r.FlagH = false;
            r.FlagN = false;
        }

        private byte Rlc(byte register)
        {
            r.FlagC = register.GetBit(7);
            var result = (byte)(register << 1);
            RlFlags(result);

            return result;
        }

        private byte Rl(byte register)
        {
            var result = (byte)(register << 1).SetBit(0, r.FlagC);
            RlFlags(result);

            return result;
        }

        private byte Rrc(byte register)
        {
            r.FlagC = register.GetBit(0);
            var result = (byte)(register << 1);
            RlFlags(result);

            return result;
        }

        private byte Rr(byte register)
        {
            var result = (byte)(register << 1).SetBit(7, r.FlagC);
            RlFlags(result);

            return result;
        }

        private byte Sla(byte register)
        {
            r.FlagC = register.GetBit(7);
            var result = (byte)(register << 1);
            RlFlags(result);

            return result;
        }

        private byte Sra(byte register)
        {
            r.FlagC = register.GetBit(0);
            // Bit 7 is unchanged during right shift!
            var result = (byte)((register >> 1) + (register & 0x80));
            RlFlags(result);

            return result;
        }

        private byte Srl(byte register)
        {
            r.FlagC = register.GetBit(0);
            // Bit 7 is reset during right shift (normal)
            var result = (byte)(register >> 1);
            RlFlags(result);

            return result;
        }
        #endregion


        //////////////////////////////////////////////////////////////////////////
        #region Call/Ret

        // BUG: Stack operations are broken until a proper memory management class is implemented
        private void Call(ushort address)
        {
            mmu.WriteByte(--r.SP, r.SPh);
            mmu.WriteByte(--r.SP, r.SPl);
            r.Address = address;
        }

        private void Ret()
        {
            r.PCl = mmu.ReadByte(r.SP++);
            r.PCh = mmu.ReadByte(r.SP++);
            r.PC--;
        }
        #endregion

        private byte Swap(byte register)
        {
            var result = (byte)(((register & 0x0F) << 4) | ((register & 0xF0) >> 4));
            r.FlagZ = result == 0;

            return result;
        }
    }
}