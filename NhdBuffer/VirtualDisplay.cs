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
      this.imageData = new uint[Width, Height];
    }

    private readonly uint[,] imageData;

    public bool IsRunning { get; private set; }
    public DateTime SimulationTime { get; private set; }
    public long FrameCounter { get; private set; }

    private Action<uint[,]> preRenderer;

    public void Start(DateTime startingSimulationTime, Action<uint[,]> preRenderer=null)
    {
      this.IsRunning = true;
      this.SimulationTime = startingSimulationTime;
      this.preRenderer = preRenderer;
    }

    public void Stop()
    {
      this.IsRunning = false;
      this.preRenderer = null;
    }

    public void ProcessOneFrame(TimeSpan deltaTime)
    {
      if (this.IsRunning)
      {
        this.SimulationTime += deltaTime;
        this.FrameCounter++;
        this.preRenderer?.Invoke(this.imageData);
      }
    }
  }
}
