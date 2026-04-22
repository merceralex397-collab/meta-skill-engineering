using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using FluentAssertions;
using MetaSkillStudio.Views;
using Xunit;

namespace MetaSkillStudio.Tests.Views
{
    [Collection("WPF isolation")]
    public class StartupRegressionTests
    {
        [Fact]
        public void AppResources_And_MainWindow_Should_Load_Without_Xaml_Errors()
        {
            Exception? captured = null;

            var thread = new Thread(() =>
            {
                App? app = null;
                MainWindow? window = null;

                try
                {
                    app = new App();
                    app.InitializeComponent();
                    window = new MainWindow();
                }
                catch (Exception ex)
                {
                    captured = ex;
                }
                finally
                {
                    window?.Close();
                    app?.Shutdown();
                    ResetApplicationSingleton();
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            captured.Should().BeNull();
        }

        private static void ResetApplicationSingleton()
        {
            var applicationType = typeof(Application);
            applicationType.GetField("_appInstance", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, null);
            applicationType.GetField("_resourceAssembly", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, null);
            applicationType.GetField("_appCreatedInThisAppDomain", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, false);
        }
    }
}
