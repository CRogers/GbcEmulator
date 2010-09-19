using System;

namespace RomTools.Metadata
{
    public class RomSize
    {
        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Size { get; private set; }

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
                Size = 0x8000 + 0x8000*b;

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
    }

    public class RamSize
    {
        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Number of banks at variable size, nominally 8kB per bank
        /// </summary>
        public int Banks { get; private set; }

        public RamSize(byte b0149)
        {
            int b = b0149;

            if(b > 4)
                throw new ArgumentException("That byte (value: " + b + ") is not a valid RamSize identifier.");

            if (b == 1)
            {
                Size = 2*0x800;
                Banks = 1;
            }

            switch (b)
            {
                case 1:
                    Size = 2*0x800;
                    Banks = 1;
                    break;

                case 2:
                    Size = 8 * 0x800;
                    Banks = 1;
                    break;

                case 3:
                    Size = 32 * 0x800;
                    Banks = 4;
                    break;

                case 4:
                    Size = 128 * 0x800;
                    Banks = 16;
                    break;
            }
        }
    }
}
