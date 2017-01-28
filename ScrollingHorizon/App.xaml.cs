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
    private static readonly Color groundColor2 = Colors.DarkOliveGreen;

    private const double skyBandDepth = 8.0;
    private const double groundBandDepth = 1.0;

    private double cameraPosition = 0.0;
    private const double cameraMovementSpeed = 4.0;
    private const double depthOfField = 60.0;

    public App()
    {
      this.random = new Random();
    }

    private readonly Random random;

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
      var skyColorInt1 = skyColor1.AsBgrUint();
      var skyColorInt2 = skyColor2.AsBgrUint();

      var depthMultiplier = Math.Exp(1.0 / depthOfField);
      var currentDepth = depthMultiplier;
      for (var j = 0; j < VirtualDisplay.Height / 2; j++)
      {
        var rowDepth = currentDepth + this.cameraPosition;
        var bandNumber = (int) (rowDepth / skyBandDepth);
        var currentColorInt = bandNumber % 2 == 0 ? skyColorInt1 : skyColorInt2;

        for (var i = 0; i < VirtualDisplay.Width; i++)
          virtualDisplay.ImageData[i, j] = currentColorInt;

        currentDepth *= depthMultiplier;
      }
    }

    private void drawGround(VirtualDisplay virtualDisplay)
    {
      var groundColorInt1 = groundColor1.AsBgrUint();
      var groundColorInt2 = groundColor2.AsBgrUint();

      var depthMultiplier = Math.Exp(1.0 / depthOfField);
      var currentDepth = depthMultiplier;
      for (var j = 0; j < VirtualDisplay.Height / 2; j++)
      {
        var rowDepth = currentDepth + this.cameraPosition;
        var bandNumber = (int) (rowDepth / groundBandDepth);
        var currentColorInt = bandNumber % 2 == 0 ? groundColorInt1 : groundColorInt2;

        for (var i = 0; i < VirtualDisplay.Width; i++)
          virtualDisplay.ImageData[i, VirtualDisplay.Height - 1 - j] = currentColorInt;

        currentDepth *= depthMultiplier;
      }
    }
  }
}
