using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using NhdBuffer;

namespace ScrollingHorizon
{
  public partial class App : Application
  {
    private static readonly Color skyColor1 = Colors.MidnightBlue;
    private static readonly Color skyColor2 = Colors.MediumBlue;
    private static readonly Color groundColor1 = Colors.DarkGreen;
    private static readonly Color groundColor2 = Colors.ForestGreen;

    private const double skyBandDepth = 18.0;
    private const double groundBandDepth = 4.0;

    private double cameraPosition = 0.0;
    private const double cameraMovementSpeed = 12.0;
    private const double depthOfField = 45.0;

    private void startup(object sender, StartupEventArgs e)
    {
      var nhdBufferWindow = new NhdBufferWindow {
        Title = "Scrolling Horizon"
      };
      nhdBufferWindow.Show();
      nhdBufferWindow.StartVirtualDisplay(this.perFrame, DateTime.MinValue);
    }

    private void perFrame(VirtualDisplay virtualDisplay, TimeSpan deltaTime)
    {
      this.cameraPosition += cameraMovementSpeed * deltaTime.TotalSeconds;

      this.drawSky(virtualDisplay);
      this.drawGround(virtualDisplay);
    }

    private void drawSky(VirtualDisplay virtualDisplay)
    {
      this.drawBands(virtualDisplay, skyColor1, skyColor2, skyBandDepth, true);
    }

    private void drawGround(VirtualDisplay virtualDisplay)
    {
      this.drawBands(virtualDisplay, groundColor1, groundColor2, groundBandDepth, false);
    }

    private void drawBands(VirtualDisplay virtualDisplay, Color color1, Color color2, double bandDepth, bool drawFromTop)
    {
      var colorInt1 = color1.AsBgrUint();
      var colorInt2 = color2.AsBgrUint();

      var depthMultiplier = Math.Exp(1.0 / depthOfField);
      var currentDepth = depthMultiplier;
      for (var j = 0; j < VirtualDisplay.Height / 2; j++)
      {
        var rowDepth = currentDepth + this.cameraPosition;
        var bandNumber = (int) (rowDepth / bandDepth);
        var currentColorInt = bandNumber % 2 == 0 ? colorInt1 : colorInt2;
        var brightness = (1.0 - j * 2.0 / VirtualDisplay.Height) / 2.0 + .5;
        var fadedColor = currentColorInt.ScaleBy(brightness);

        for (var i = 0; i < VirtualDisplay.Width; i++)
          if (drawFromTop)
            virtualDisplay.ImageData[i, j] = fadedColor;
          else
            virtualDisplay.ImageData[i, VirtualDisplay.Height - 1 - j] = fadedColor;

        currentDepth *= depthMultiplier;
      }
    }
  }
}
