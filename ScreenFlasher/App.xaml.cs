using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NhdBuffer;

namespace ScreenFlasher
{
  public partial class App : Application
  {
    private void startup(object sender, StartupEventArgs e)
    {
      var nhdBufferWindow = new NhdBufferWindow();
      nhdBufferWindow.Show();
    }
  }
}
