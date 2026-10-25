using System;
using System.Windows;

class TestApp : Application {
    public TestApp() {
        this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
    }
    [STAThread]
    static void Main() {
        var app = new TestApp();
        Window win = new Window { Title = "Test Window", Width = 200, Height = 100 };
        win.Show();
        try {
            app.Run(win); // This should fail because win is already shown!
        } catch (Exception ex) {
            Console.WriteLine("CAUGHT: " + ex.Message);
        }
    }
}
