using Microsoft.Extensions.DependencyInjection;

namespace CyberScanner
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            // Set window properties for desktop
            window.Width = 1200;
            window.Height = 800;
            window.Title = "CyberScanner - Network Analysis Tool";

            return window;
        }
    }
}