using System;
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
        this.virtualDisplay.ProcessOneFrame(deltaTime);
        switch (this.mode)
        {
          case NhdBufferWindowMode.Hd360:
            this.render360PBitmap();
            break;
          case NhdBufferWindowMode.Hd720:
            this.render720PBitmap();
            break;
          case NhdBufferWindowMode.Hd1080:
            this.render1080PBitmap();
            break;
        }
      }

      this.lastRenderingTime = renderingTime;
    }

    private unsafe void render360PBitmap()
    {
      const int height = VirtualDisplay.Height;
      const int width = VirtualDisplay.Width;
      var stride = width * bytesPerPixel;

      var virtualDisplayImageData = this.virtualDisplay.ImageData;

      this.display360Bitmap.Lock();

      for (var row = 0; row < height; row++)
      {
        var backBufferPointer = (uint) this.display360Bitmap.BackBuffer + row * stride;

        for (var column = 0; column < width; column++)
        {
          var inputPixel = virtualDisplayImageData[column, row];
          var outputPixel = inputPixel & 0x00ffffff;
          *(uint*) backBufferPointer = outputPixel;
          backBufferPointer += 4;
        }
      }
      this.display360Bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
      this.display360Bitmap.Unlock();
    }

    private unsafe void render720PBitmap()
    {
      const int height = VirtualDisplay.Height * 2;
      const int width = VirtualDisplay.Width * 2;
      var stride = width * bytesPerPixel;

      var virtualDisplayImageData = this.virtualDisplay.ImageData;

      this.display720Bitmap.Lock();

      for (var row = 0; row < height; row++)
      {
        var backBufferPointer = (uint) this.display720Bitmap.BackBuffer + row * stride;

        for (var column = 0; column < width; column += 2)
        {
          var inputPixel = virtualDisplayImageData[column / 2, row / 2];
          var outputPixel = inputPixel & 0x00ffffff;
          *(uint*) backBufferPointer = outputPixel;
          backBufferPointer += 4;
          *(uint*) backBufferPointer = outputPixel;
          backBufferPointer += 4;
        }
      }
      this.display720Bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
      this.display720Bitmap.Unlock();
    }

    private unsafe void render1080PBitmap()
    {
      const int height = VirtualDisplay.Height * 3;
      const int width = VirtualDisplay.Width * 3;
      var stride = width * bytesPerPixel;

      var virtualDisplayImageData = this.virtualDisplay.ImageData;

      this.display1080Bitmap.Lock();

      for (var row = 0; row < height; row++)
      {
        var backBufferPointer = (uint) this.display1080Bitmap.BackBuffer + row * stride;

        for (var column = 0; column < width; column += 3)
        {
          var inputPixel = virtualDisplayImageData[column / 3, row / 3];
          var outputPixel = inputPixel & 0x00ffffff;
          *(uint*) backBufferPointer = outputPixel;
          backBufferPointer += 4;
          *(uint*) backBufferPointer = outputPixel;
          backBufferPointer += 4;
          *(uint*) backBufferPointer = outputPixel;
          backBufferPointer += 4;
        }
      }
      this.display1080Bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
      this.display1080Bitmap.Unlock();
    }

    private void quit(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
