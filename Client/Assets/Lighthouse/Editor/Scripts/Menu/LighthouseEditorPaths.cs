using System.Reflection;
using UnityEditor.PackageManager;

namespace Lighthouse.Editor.Menu
{
    static class LighthouseEditorPaths
    {
        static string packageBasePath;

        // Returns "Packages/com.lisearcheleeds.lighthouse" when installed as a package,
        // or "Assets/Lighthouse" when the files are directly under Assets (development project).
        public static string PackageBasePath
        {
            get
            {
                if (packageBasePath == null)
                {
                    var info = PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());
                    packageBasePath = info?.assetPath ?? "Assets/Lighthouse";
                }
                return packageBasePath;
            }
        }
    }
}
