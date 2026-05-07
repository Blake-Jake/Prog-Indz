using Microsoft.Extensions.DependencyInjection;
using System;

namespace EventMatch
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CRITICAL ERROR in App.ctor: {ex}");
                throw;
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            try
            {
                return new Window(new AppShell());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CRITICAL ERROR in CreateWindow: {ex}");
                throw;
            }
        }
    }
}