using System;
using System.Windows.Media;

namespace NhdBuffer
{
  public class VirtualDisplay
  {
    public const int Width = 640;
    public const int Height = 360;

    public VirtualDisplay()
    {
      this.ImageData = new uint[Width, Height];
    }

    public uint[,] ImageData { get; }

    public bool IsRunning { get; private set; }
    public DateTime SimulationTime { get; private set; }
    public long FrameCounter { get; private set; }

    public void Start(DateTime startingSimulationTime)
    {
      this.IsRunning = true;
      this.SimulationTime = startingSimulationTime;
    }

    public void Stop()
    {
      this.IsRunning = false;
    }

    public void ProcessOneFrame(TimeSpan deltaTime)
    {
      if (this.IsRunning)
      {
        this.SimulationTime += deltaTime;
        this.FrameCounter++;
      }
    }
  }
}
