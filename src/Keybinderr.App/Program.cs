using System.Windows;

namespace Keybinderr.App;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var app = new App(args);
        app.Run();
    }
}

