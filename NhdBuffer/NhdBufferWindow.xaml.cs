using System;
using System.Windows;

namespace NhdBuffer
{
  public partial class NhdBufferWindow : Window
  {
    private readonly bool canView720;
    private readonly bool canView1080;
    private readonly WindowStyle designedWindowStyle;

    private NhdBufferWindowMode mode;

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
    }

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

    private void quit(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
