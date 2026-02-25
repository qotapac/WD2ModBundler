using System;
using System.IO;

namespace WD2ModBundler.Helpers
{
    public static class ModFolderHelper
    {
        private static string modFolderPath;

        public static void SetModFolderPath(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException("Selected mod folder does not exist.");

            modFolderPath = path;
        }

        public static string GetModFolderPath()
        {
            if (string.IsNullOrEmpty(modFolderPath))
                throw new InvalidOperationException("Mod folder path has not been set.");

            return modFolderPath;
        }
    }
}