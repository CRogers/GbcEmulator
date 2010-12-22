namespace GbcEmulator.Gpu
{
    public static class ColorExtensions
    {
        public static byte Red(this uint color)
        {
            return (byte)(color >> 24);
        }

        public static byte Green(this uint color)
        {
            return (byte)((color & 0x00FF0000) >> 16);
        }

        public static byte Blue(this uint color)
        {
            return (byte)((color & 0x0000FF00) >> 8);
        }

        public static byte Alpha(this uint color)
        {
            return (byte)(color & 0x000000FF);
        }


        public static uint Red(this uint color, byte red)
        {
            return (uint)(color & (0xFFFFFFFF & (red << 24)));
        }

        public static uint Green(this uint color, byte green)
        {
            return (uint)(color & (0xFFFFFFFF & (green << 16)));
        }

        public static uint Blue(this uint color, byte blue)
        {
            return (uint)(color & (0xFFFFFFFF & (blue << 8)));
        }

        public static uint Alpha(this uint color, byte alpha)
        {
            return color & (0xFFFFFFFF & alpha);
        }


        public static uint Color(byte red, byte green, byte blue, byte alpha = (byte)255)
        {
            return (uint)(red << 24 | green << 16 | blue << 8 | alpha);
        }
    }
}
