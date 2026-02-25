using System;
using System.IO;

namespace WD2ModBundler.Helpers
{
    public static class PatchToolsHelper
    {
        private static string _patchToolsPath;

        public static void SetPatchToolsPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Patch tools path cannot be empty.");

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException("Selected patch tools folder does not exist.");

            string extractPath = Path.Combine(path, "WD2Extract.exe");
            string packPath = Path.Combine(path, "WD2Pack.exe");

            if (!File.Exists(extractPath))
                throw new FileNotFoundException("WD2Extract.exe not found in selected folder.");

            if (!File.Exists(packPath))
                throw new FileNotFoundException("WD2Pack.exe not found in selected folder.");

            _patchToolsPath = path;
        }

        public static string GetPatchToolsPath()
        {
            if (string.IsNullOrEmpty(_patchToolsPath))
                throw new InvalidOperationException("Patch tools path has not been set.");

            return _patchToolsPath;
        }

        public static string GetExtractExePath()
        {
            return Path.Combine(GetPatchToolsPath(), "WD2Extract.exe");
        }

        public static string GetPackExePath()
        {
            return Path.Combine(GetPatchToolsPath(), "WD2Pack.exe");
        }
    }
}