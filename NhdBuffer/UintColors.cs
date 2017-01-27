using System;
using System.Windows.Media;

namespace NhdBuffer
{
  public static class UintColors
  {
    public static uint AsBgrUint(this Color color)
    {
      return (uint) (color.R << 16 |
                     color.G << 8 |
                     color.B << 0);
    }

    public static uint BuildUint(byte r, byte g, byte b)
    {
      return (uint) (r << 16 |
                     g << 8 |
                     b << 0);
    }

    public static uint AsBgraUint(this Color color)
    {
      return (uint) (color.A << 24 |
                     color.R << 16 |
                     color.G << 8 |
                     color.B << 0);
    }

    public static uint BuildUint(byte a, byte r, byte g, byte b)
    {
      return (uint) (a << 24 |
                     r << 16 |
                     g << 8 |
                     b << 0);
    }

    public static uint BlendOver(this uint foreground, uint background)
    {
      var foregroundAlpha = getAlpha(foreground);
      var backgroundAlpha = getAlpha(background);
      var residualAlpha = backgroundAlpha * (0xff - foregroundAlpha) >> 8;
      var outAlpha = (byte) (foregroundAlpha + residualAlpha);
      if (outAlpha == 0)
        return 0x00000000;

      var foregroundRed = getRed(foreground);
      var backgroundRed = getRed(background);
      var outRed = (byte) ((foregroundRed * foregroundAlpha + backgroundRed * residualAlpha) / outAlpha);

      var foregroundGreen = getGreen(foreground);
      var backgroundGreen = getGreen(background);
      var outGreen = (byte) ((foregroundGreen * foregroundAlpha + backgroundGreen * residualAlpha) / outAlpha);

      var foregroundBlue = getBlue(foreground);
      var backgroundBlue = getBlue(background);
      var outBlue = (byte) ((foregroundBlue * foregroundAlpha + backgroundBlue * residualAlpha) / outAlpha);

      return BuildUint(outAlpha, outRed, outGreen, outBlue);
    }

    private static byte getAlpha(uint bgra)
    {
      return (byte) (bgra >> 24);
    }

    private static byte getRed(uint bgra)
    {
      return (byte) (bgra & 0x00ff0000 >> 16);
    }

    private static byte getGreen(uint bgra)
    {
      return (byte) (bgra & 0x0000ff00 >> 8);
    }

    private static byte getBlue(uint bgra)
    {
      return (byte) (bgra & 0x000000ff);
    }
  }
}
