using System;
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
    private static readonly int bytesPerPixel = (pixelFormat.BitsPerPixel + 7) / 8;

    public NhdBufferWindow()
    {
      this.InitializeComponent();

      this.designedWindowStyle = this.WindowStyle;

      this.mode = NhdBufferWindowMode.Hd360;

      var screenWidth = SystemParameters.PrimaryScreenWidth;
      var screenHeight = SystemParameters.PrimaryScreenHeight;

      this.canView720 = screenWidth >= this.Display720.Width &&
                        screenHeight >= this.Display720.Height;
      this.View720MenuItem.IsEnabled = this.canView720;

      this.canView1080 = screenWidth >= this.Display1080.Width &&
                         screenHeight >= this.Display1080.Height;
      this.View1080MenuItem.IsEnabled = this.canView1080;

      this.display360Bitmap = new WriteableBitmap(VirtualDisplay.Width, VirtualDisplay.Height, dpi, dpi, pixelFormat, null);
      this.Display360.Source = this.display360Bitmap;
      this.display720Bitmap = new WriteableBitmap(VirtualDisplay.Width * 2, VirtualDisplay.Height * 2, dpi, dpi, pixelFormat, null);
      this.Display720.Source = this.display720Bitmap;
      this.display1080Bitmap = new WriteableBitmap(VirtualDisplay.Width * 3, VirtualDisplay.Height * 3, dpi, dpi, pixelFormat, null);
      this.Display1080.Source = this.display1080Bitmap;

      this.virtualDisplay = new VirtualDisplay();

      CompositionTarget.Rendering += this.onRenderFrame;
    }

    private readonly bool canView720;
    private readonly bool canView1080;
    private readonly WindowStyle designedWindowStyle;

    private readonly WriteableBitmap display360Bitmap;
    private readonly WriteableBitmap display720Bitmap;
    private readonly WriteableBitmap display1080Bitmap;
    private readonly VirtualDisplay virtualDisplay;

    private NhdBufferWindowMode mode;
    private TimeSpan lastRenderingTime;
    private Action<VirtualDisplay, TimeSpan> perFrameAction;

    public void View360(object sender, RoutedEventArgs e)
    {
      this.mode = NhdBufferWindowMode.Hd360;
      this.WindowState = WindowState.Normal;
      this.WindowStyle = this.designedWindowStyle;
      this.Topmost = false;
      this.Menu.Visibility = Visibility.Visible;
      this.Display360.Visibility = Visibility.Visible;
      this.Display720.Visibility = Visibility.Collapsed;
      this.Display1080.Visibility = Visibility.Collapsed;
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
        this.Display360.Visibility = Visibility.Collapsed;
        this.Display720.Visibility = Visibility.Visible;
        this.Display1080.Visibility = Visibility.Collapsed;
      }
    }

    public void View1080(object sender, RoutedEventArgs e)
    {
      if (this.canView1080)
      {
        this.mode = NhdBufferWindowMode.Hd1080;
        this.Left = 0;
        this.Top = 0;
        this.WindowState = WindowState.Maximized;
        this.WindowStyle = WindowStyle.None;
        this.Topmost = true;
        this.Menu.Visibility = Visibility.Collapsed;
        this.Display360.Visibility = Visibility.Collapsed;
        this.Display720.Visibility = Visibility.Collapsed;
        this.Display1080.Visibility = Visibility.Visible;
      }
    }

    private void onRenderFrame(object sender, EventArgs e)
    {
      var renderingEventArgs = (RenderingEventArgs) e;
      var renderingTime = renderingEventArgs.RenderingTime;

      if (this.virtualDisplay?.IsRunning == true)
      {
        var deltaTime = renderingTime - this.lastRenderingTime;
        this.perFrameAction?.Invoke(this.virtualDisplay, deltaTime);
        switch (this.mode)
        {
          case NhdBufferWindowMode.Hd360:
            this.render360Bitmap();
            break;
          case NhdBufferWindowMode.Hd720:
            this.render720Bitmap();
            break;
          case NhdBufferWindowMode.Hd1080:
            this.render1080Bitmap();
            break;
        }

        // TODO: Move to separate thread?
        //Task.Run(() => this.virtualDisplay.ProcessOneFrame(deltaTime));
        this.virtualDisplay.ProcessOneFrame(deltaTime);
      }

      this.lastRenderingTime = renderingTime;
    }

    private unsafe void render360Bitmap()
    {
      var virtualDisplayImageData = this.virtualDisplay.ImageData;

      this.display360Bitmap.Lock();

      var backBufferPointer = (uint) this.display360Bitmap.BackBuffer;
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
      this.display360Bitmap.AddDirtyRect(new Int32Rect(0, 0, this.display360Bitmap.PixelWidth, this.display360Bitmap.PixelHeight));
      this.display360Bitmap.Unlock();
    }

    private unsafe void render720Bitmap()
    {
      var virtualDisplayImageData = this.virtualDisplay.ImageData;
      const int width = VirtualDisplay.Width * 2;
      var stride = width * bytesPerPixel;

      this.display720Bitmap.Lock();

      var backBufferPointer0 = (uint) this.display720Bitmap.BackBuffer;
      var backBufferPointer1 = backBufferPointer0 + stride;
      for (var row = 0; row < VirtualDisplay.Height; row++)
      {
        for (var column = 0; column < VirtualDisplay.Width; column++)
        {
          var inputPixel = virtualDisplayImageData[column, row];
          var outputPixel = inputPixel & 0x00ffffff;
          *(uint*) backBufferPointer0 = outputPixel;
          backBufferPointer0 += 4;
          *(uint*) backBufferPointer0 = outputPixel;
          backBufferPointer0 += 4;
          *(uint*) backBufferPointer1 = outputPixel;
          backBufferPointer1 += 4;
          *(uint*) backBufferPointer1 = outputPixel;
          backBufferPointer1 += 4;
        }
        backBufferPointer0 = (uint) (backBufferPointer0 + stride);
        backBufferPointer1 = (uint) (backBufferPointer1 + stride);
      }
      this.display720Bitmap.AddDirtyRect(new Int32Rect(0, 0, this.display720Bitmap.PixelWidth, this.display720Bitmap.PixelHeight));
      this.display720Bitmap.Unlock();
    }

    private unsafe void render1080Bitmap()
    {
      var virtualDisplayImageData = this.virtualDisplay.ImageData;
      const int width = VirtualDisplay.Width * 3;
      var stride = width * bytesPerPixel;
      var doubleStride = stride * 2;

      this.display1080Bitmap.Lock();

      var backBufferPointer0 = (uint) this.display1080Bitmap.BackBuffer;
      var backBufferPointer1 = backBufferPointer0 + stride;
      var backBufferPointer2 = backBufferPointer1 + stride;
      for (var row = 0; row < VirtualDisplay.Height; row++)
      {
        for (var column = 0; column < VirtualDisplay.Width; column++)
        {
          var inputPixel = virtualDisplayImageData[column, row];
          var outputPixel = inputPixel & 0x00ffffff;
          *(uint*) backBufferPointer0 = outputPixel;
          backBufferPointer0 += 4;
          *(uint*) backBufferPointer0 = outputPixel;
          backBufferPointer0 += 4;
          *(uint*) backBufferPointer0 = outputPixel;
          backBufferPointer0 += 4;
          *(uint*) backBufferPointer1 = outputPixel;
          backBufferPointer1 += 4;
          *(uint*) backBufferPointer1 = outputPixel;
          backBufferPointer1 += 4;
          *(uint*) backBufferPointer1 = outputPixel;
          backBufferPointer1 += 4;
          *(uint*) backBufferPointer2 = outputPixel;
          backBufferPointer2 += 4;
          *(uint*) backBufferPointer2 = outputPixel;
          backBufferPointer2 += 4;
          *(uint*) backBufferPointer2 = outputPixel;
          backBufferPointer2 += 4;
        }
        backBufferPointer0 = (uint) (backBufferPointer0 + doubleStride);
        backBufferPointer1 = (uint) (backBufferPointer1 + doubleStride);
        backBufferPointer2 = (uint) (backBufferPointer2 + doubleStride);
      }
      this.display1080Bitmap.AddDirtyRect(new Int32Rect(0, 0, this.display1080Bitmap.PixelWidth, this.display1080Bitmap.PixelHeight));
      this.display1080Bitmap.Unlock();
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
