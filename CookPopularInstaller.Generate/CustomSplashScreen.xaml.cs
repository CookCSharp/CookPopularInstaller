using SplashScreen;
using System.Diagnostics;
using System.Reflection;


namespace CookPopularInstaller.Generate
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    [SplashScreen(MinimumVisibilityDuration = 0, FadeoutDuration = 0.2)]
    public partial class CustomSplashScreen
    {
        /// <summary>
        /// Gets the file description.
        /// </summary>
        public FileVersionInfo FileVersionInfo { get; } = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

        public CustomSplashScreen()
        {
            InitializeComponent();
        }
    }
}
