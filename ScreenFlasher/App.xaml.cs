using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using NhdBuffer;

namespace ScreenFlasher
{
  public partial class App : Application
  {
    public static readonly int AnimationPeriod = 32;

    public App()
    {
      this.random = new Random();
      this.color = Colors.Black;
    }

    private readonly Random random;
    private Color color;

    private void startup(object sender, StartupEventArgs e)
    {
      var nhdBufferWindow = new NhdBufferWindow
      {
        Title = "Screen Flasher"
      };
      nhdBufferWindow.Show();
      nhdBufferWindow.StartVirtualDisplay(this.perFrame, DateTime.UtcNow);
    }

    private void perFrame(VirtualDisplay virtualDisplay, TimeSpan deltaTime)
    {
      var animationFrameIndex = virtualDisplay.FrameCounter % AnimationPeriod;
      if (animationFrameIndex == AnimationPeriod * 3 / 4)
        this.pickNewColor();

      var t = animationFrameIndex * (1.0 / AnimationPeriod);
      var tint = Math.Sin(2 * Math.PI * t) / 2 + .5;
      var currentR = (byte) (this.color.R * tint);
      var currentG = (byte) (this.color.G * tint);
      var currentB = (byte) (this.color.B * tint);
      var currentColor = Color.FromRgb(currentR, currentG, currentB);
      var currentColorInt = currentColor.AsBgraUint();

      for (var i = 0; i < VirtualDisplay.Width; i++)
        for (var j = 0; j < VirtualDisplay.Height; j++)
          virtualDisplay.ImageData[i, j] = currentColorInt;
    }

    private void pickNewColor()
    {
      var r = this.random.Next(256);
      var g = this.random.Next(256);
      var b = this.random.Next(256);
      var max = MoreMath.Max(r, g, b);
      var c = Math.Ceiling(256.0 / max);
      this.color = Color.FromRgb((byte) Math.Min(255, c * r),
                                 (byte) Math.Min(255, c * g),
                                 (byte) Math.Min(255, c * b));
    }
  }
}
