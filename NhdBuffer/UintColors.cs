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
      var foregroundAlpha = foreground.GetAlpha();
      var backgroundAlpha = background.GetAlpha();
      var residualAlpha = backgroundAlpha * (0xff - foregroundAlpha) >> 8;
      var outAlpha = Convert.ToByte(foregroundAlpha + residualAlpha);
      if (outAlpha == 0)
        return 0x00000000;

      var foregroundRed = foreground.GetRed();
      var backgroundRed = background.GetRed();
      var outRed = Convert.ToByte((foregroundRed * foregroundAlpha + backgroundRed * residualAlpha) / outAlpha);

      var foregroundGreen = foreground.GetGreen();
      var backgroundGreen = background.GetGreen();
      var outGreen = Convert.ToByte((foregroundGreen * foregroundAlpha + backgroundGreen * residualAlpha) / outAlpha);

      var foregroundBlue = foreground.GetBlue();
      var backgroundBlue = background.GetBlue();
      var outBlue = Convert.ToByte((foregroundBlue * foregroundAlpha + backgroundBlue * residualAlpha) / outAlpha);

      return BuildUint(outAlpha, outRed, outGreen, outBlue);
    }

    public static byte GetAlpha(this uint bgra)
    {
      return Convert.ToByte(bgra >> 24);
    }

    public static byte GetRed(this uint bgra)
    {
      return Convert.ToByte((bgra & 0x00ff0000) >> 16);
    }

    public static byte GetGreen(this uint bgra)
    {
      return Convert.ToByte((bgra & 0x0000ff00) >> 8);
    }

    public static byte GetBlue(this uint bgra)
    {
      return Convert.ToByte(bgra & 0x000000ff);
    }

    public static uint ScaleBy(this uint bgra, double t)
    {
      t = MoreMath.Clamp(t, 0, 1);

      var red = Convert.ToByte(bgra.GetRed() * t);
      var green = Convert.ToByte(bgra.GetGreen() * t);
      var blue = Convert.ToByte(bgra.GetBlue() * t);

      return BuildUint(red, green, blue);
    }
  }
}
