using System;
using System.IO;
using System.Reflection;
using UnityEditor.PackageManager;
using UnityEngine;

namespace LighthouseExtends.ScreenStack.Editor.ScriptGenerator
{
    static class ScreenStackEditorPaths
    {
        static string packageBasePath;

        // Returns "Packages/com.lisearcheleeds.lighthouseextends.screenstack" when installed as a package,
        // or "Assets/LighthouseExtends/ScreenStack" when the files are directly under Assets (development project).
        public static string PackageBasePath
        {
            get
            {
                if (packageBasePath == null)
                {
                    var info = PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());
                    packageBasePath = info?.assetPath ?? "Assets/LighthouseExtends/ScreenStack";
                }
                return packageBasePath;
            }
        }

        public static string AssetPathToFsPath(string assetPath)
        {
            if (assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFullPath(Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length)));
            }

            if (assetPath.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase))
            {
                var info = PackageInfo.FindForAssetPath(assetPath);
                if (info != null)
                {
                    var relative = assetPath.Substring($"Packages/{info.name}/".Length);
                    return Path.GetFullPath(Path.Combine(info.resolvedPath, relative));
                }
            }

            return Path.GetFullPath(Path.Combine(Application.dataPath, assetPath));
        }
    }
}
