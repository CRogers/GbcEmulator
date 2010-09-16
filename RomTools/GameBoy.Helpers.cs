namespace RomTools
{
    public partial class GameBoy
    {
        #region Arithmetic Operations
        /*private void FlagSZSet(int value)
        {
            
        }

        private byte Add(byte a, byte b)
        {
            var result = a + b;
        
        }*/
        #endregion

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

        #region Rotate Operations
        private void RlFlags(byte result)
        {
            r.FlagZ = result == 0;
            r.FlagP = result % 2 == 0;
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
            r.FlagS = result < 0;

            return result;
        }

        private byte Sra(byte register)
        {
            r.FlagC = register.GetBit(0);
            // Bit 7 is unchanged during right shift!
            var result = (byte)((register >> 1) + (register & 0x80));
            RlFlags(result);
            r.FlagS = result < 0;

            return result;
        }

        private byte Srl(byte register)
        {
            r.FlagC = register.GetBit(0);
            // Bit 7 is reset during right shift (normal)
            var result = (byte)(register >> 1);
            RlFlags(result);
            r.FlagS = result < 0;

            return result;
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