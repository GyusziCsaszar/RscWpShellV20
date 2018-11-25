using Launcher_AppMini.Resources;

namespace Launcher_AppMini
{
	/// <summary>
	/// Provides access to string resources.
	/// </summary>
    public class LocalizedStrings
    {
        private static AppResources _localizedResources = new AppResources();

        public AppResources LocalizedResources { get { return _localizedResources; } }
    }
}