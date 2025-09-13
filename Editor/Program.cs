using System;
using Gtk;

class Program
{
  [STAThread]
  static int Main(string[] args)
  {
    // Initialize GTK
    Application.Init();

    // Create and show the main window
    var win = new MainWindow();
    win.ShowAll();

    // Run the application
    Application.Run();
    return 0;
  }
}
