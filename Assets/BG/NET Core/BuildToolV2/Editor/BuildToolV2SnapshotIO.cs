using System.IO;
using UnityEditor;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2SnapshotIO
    {
        public static void CaptureSnapshotFile(BuildSharedStateSnapshot snapshot, string relativePath)
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(relativePath))
                return;

            string normalizedRelativePath = relativePath.Replace("\\", "/");
            for (int i = 0; i < snapshot.Files.Count; i++)
            {
                if (string.Equals(snapshot.Files[i].RelativePath, normalizedRelativePath, System.StringComparison.OrdinalIgnoreCase))
                    return;
            }

            string fullPath = GetProjectFileFullPath(normalizedRelativePath);
            bool existed = File.Exists(fullPath);
            snapshot.Files.Add(new BuildSharedStateFileSnapshot
            {
                RelativePath = normalizedRelativePath,
                FullPath = fullPath,
                Existed = existed,
                Content = existed ? File.ReadAllBytes(fullPath) : System.Array.Empty<byte>(),
            });
        }

        public static void RestoreSnapshotFile(BuildSharedStateFileSnapshot snapshot)
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.FullPath))
                return;

            if (snapshot.Existed)
            {
                string directory = Path.GetDirectoryName(snapshot.FullPath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllBytes(snapshot.FullPath, snapshot.Content ?? System.Array.Empty<byte>());
            }
            else if (File.Exists(snapshot.FullPath))
            {
                File.Delete(snapshot.FullPath);
            }

            if (!string.IsNullOrWhiteSpace(snapshot.RelativePath) && snapshot.RelativePath.StartsWith("Assets/", System.StringComparison.OrdinalIgnoreCase))
                AssetDatabase.ImportAsset(snapshot.RelativePath, ImportAssetOptions.ForceUpdate);
        }

        public static string GetProjectFileFullPath(string relativePath)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."));
            return Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString())));
        }
    }
}
