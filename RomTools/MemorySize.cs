using System;

namespace RomTools
{
    public class RomSize
    {
        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Number of banks at 16384 bytes per bank
        /// </summary>
        public int Banks
        {
            get { return Size/0x4000; }
        }

        public RomSize(byte b0148)
        {
            int b = b0148;

            if(b <= 6)
                Size = 0x8000 * (b+1);

            switch (b)
            {
                case 0x52:
                    Size = 72*0x4000;
                    break;

                case 0x53:
                    Size = 80*0x4000;
                    break;

                case 0x54:
                    Size = 96*0x4000;
                    break;
            }

            if(Size == 0)
                throw new ArgumentException("That byte (value: " + b + ") is not a valid RomSize identifier.");
        }

        public int GetCode()
        {
            var sizeLog2 = Math.Log(Size, 2);
            if (sizeLog2 - (int)sizeLog2 < 1e-12)
                return Size / 0x8000 - 1;

            switch (Size)
            {
                case 72*0x4000:
                    return 0x52;

                case 80 * 0x4000:
                    return 0x53;

                case 96 * 0x4000:
                    return 0x54;
            }

            // Return largest size for saftey:
            return 6;
        }
    }

    public class RamSize
    {
        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Number of banks at variable size, nominally 8kB per bank
        /// </summary>
        public int Banks { get; set; }

        public RamSize(byte b0149)
        {
            int b = b0149;

            if(b > 4)
                throw new ArgumentException("That byte (value: " + b + ") is not a valid RamSize identifier.");

            if (b == 1)
            {
                Size = 2*0x400;
                Banks = 1;
            }

            switch (b)
            {
                case 1:
                    Size = 2*1024;
                    Banks = 1;
                    break;

                case 2:
                    Size = 8 * 1024;
                    Banks = 1;
                    break;

                case 3:
                    Size = 32 * 1024;
                    Banks = 4;
                    break;

                case 4:
                    Size = 128 * 1024;
                    Banks = 16;
                    break;
            }
        }

        public int GetCode()
        {
            switch (Size)
            {
                case 0:
                    return 0;

                case 2*1024:
                    return 1;

                case 8*1024:
                    return 2;

                case 32*1024:
                    return 3;

                case 128*1024:
                    return 4;
            }

            return 4;
        }
    }
}
