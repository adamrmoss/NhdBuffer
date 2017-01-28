using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NhdBuffer
{
  public partial class NhdBufferWindow : Window
  {
    private const int dpi = 96;
    private static readonly PixelFormat pixelFormat = PixelFormats.Bgr32;

    public NhdBufferWindow()
    {
      this.InitializeComponent();

      this.designedWindowStyle = this.WindowStyle;

      this.mode = NhdBufferWindowMode.Hd360;

      var screenWidth = SystemParameters.PrimaryScreenWidth;
      var screenHeight = SystemParameters.PrimaryScreenHeight;

      this.canView720 = screenWidth >= VirtualDisplay.Width * 2 &&
                        screenHeight >= VirtualDisplay.Height * 2;
      this.View720MenuItem.IsEnabled = this.canView720;

      this.displayBitmap = new WriteableBitmap(VirtualDisplay.Width, VirtualDisplay.Height, dpi, dpi, pixelFormat, null);
      this.Display.Source = this.displayBitmap;
      this.virtualDisplay = new VirtualDisplay();

      Task.Run(() => this.loopForBackgroundThread());

      CompositionTarget.Rendering += this.onRenderFrame;
    }

    private readonly bool canView720;
    private readonly WindowStyle designedWindowStyle;

    private readonly WriteableBitmap displayBitmap;
    private readonly VirtualDisplay virtualDisplay;
    private readonly object processFrameLock = new object();

    private NhdBufferWindowMode mode;
    private TimeSpan lastRenderingTime;
    private Action<VirtualDisplay, TimeSpan> perFrameAction;
    private TimeSpan deltaTime;
    private bool readyToProcessFrame;

    public void View360(object sender, RoutedEventArgs e)
    {
      this.mode = NhdBufferWindowMode.Hd360;
      this.WindowState = WindowState.Normal;
      this.WindowStyle = this.designedWindowStyle;
      this.Topmost = false;
      this.Menu.Visibility = Visibility.Visible;
      this.Display.Visibility = Visibility.Visible;
      this.Display.Width = VirtualDisplay.Width;
      this.Display.Height = VirtualDisplay.Height;
    }

    public void View720(object sender, RoutedEventArgs e)
    {
      if (this.canView720)
      {
        this.mode = NhdBufferWindowMode.Hd720;
        this.WindowState = WindowState.Normal;
        this.WindowStyle = this.designedWindowStyle;
        this.Topmost = false;
        this.Menu.Visibility = Visibility.Visible;
        this.Display.Width = VirtualDisplay.Width * 2;
        this.Display.Height = VirtualDisplay.Height * 2;
      }
    }

    private void onRenderFrame(object sender, EventArgs e)
    {
      var renderingEventArgs = (RenderingEventArgs) e;
      var renderingTime = renderingEventArgs.RenderingTime;

      if (this.virtualDisplay?.IsRunning == true)
      {
        lock (this.processFrameLock)
        {
          this.deltaTime = renderingTime - this.lastRenderingTime;
          this.perFrameAction?.Invoke(this.virtualDisplay, this.deltaTime);
          this.renderBitmap();

          this.readyToProcessFrame = true;
        }
      }

      this.lastRenderingTime = renderingTime;
    }

    private unsafe void renderBitmap()
    {
      var virtualDisplayImageData = this.virtualDisplay.ImageData;

      this.displayBitmap.Lock();

      var backBufferPointer = (uint) this.displayBitmap.BackBuffer;
      for (var row = 0; row < VirtualDisplay.Height; row++)
      {
        for (var column = 0; column < VirtualDisplay.Width; column++)
        {
          var inputPixel = virtualDisplayImageData[column, row];
          var outputPixel = inputPixel & 0x00ffffff;
          *(uint*) backBufferPointer = outputPixel;
          backBufferPointer += 4;
        }
      }
      this.displayBitmap.AddDirtyRect(new Int32Rect(0, 0, this.displayBitmap.PixelWidth, this.displayBitmap.PixelHeight));
      this.displayBitmap.Unlock();
    }

    private void loopForBackgroundThread()
    {
      while (true)
      {
        lock (this.processFrameLock)
        {
          if (this.readyToProcessFrame)
          {
            this.virtualDisplay.ProcessOneFrame(this.deltaTime);
            this.readyToProcessFrame = false;
          }
          Thread.Yield();
        }
      }
    }

    public void StartVirtualDisplay(Action<VirtualDisplay, TimeSpan> perFrameAction, DateTime simulationStartTime)
    {
      this.perFrameAction = perFrameAction;
      this.virtualDisplay.Start(simulationStartTime);
    }

    public void StopVirtualDisplay()
    {
      this.perFrameAction = null;
      this.virtualDisplay.Stop();
    }

    private void quit(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
