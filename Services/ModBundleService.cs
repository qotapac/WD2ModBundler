using System;
using System.IO;
using System.Diagnostics;
using System.Windows;
using WD2ModBundler.Helpers;

namespace WD2ModBundler.Services
{
    public class ModBundleService
    {
        /// <summary>
        /// Combines multiple mod archives into a single patch bundle.
        /// </summary>
        /// <param name="ModArchivePath">Folder containing mod archives (.zip, .7z, etc.)</param>
        /// <param name="log">Delegate to log messages to UI</param>
        /// <param name="progress">Delegate to report progress (0-100%) to UI</param>
        public void CombineMods(string ModArchivePath, Action<string> log, Action<int> progress = null)
        {
            try
            {
                // -----------------------------
                //  Prepare temporary workspace
                // -----------------------------
                string tempPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp");

                // If Temp folder exists, delete it completely to start fresh
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);

                // Create a new Temp folder for mod extraction
                Directory.CreateDirectory(tempPath);

                // -----------------------------
                //  Compute total steps for progress tracking
                // -----------------------------
                int totalMods = Directory.GetFiles(ModArchivePath).Length;       // Number of archives to extract
                int totalPatchFolders = Directory.GetDirectories(tempPath).Length; // Will update after extraction
                int totalSteps = totalMods + totalPatchFolders + 2;             // Include final Patch3.fat & Patch3.dat
                int currentStep = 0;

                // -----------------------------
                //  Extract all mod archives
                // -----------------------------
                foreach (string ModArchive in Directory.GetFiles(ModArchivePath))
                {
                    // Determine extraction path for this archive
                    string extractPath = Path.Combine(tempPath, Path.GetFileNameWithoutExtension(ModArchive));
                    Directory.CreateDirectory(extractPath);

                    // Find 7-Zip executable
                    string sevenZipPath = ArchiveHelper.Find7Zip();

                    log?.Invoke($"Extracting {ModArchive}...");

                    // Run 7-Zip to extract archive to the folder
                    ProcessHelper.RunProcess(sevenZipPath, $"x \"{ModArchive}\" -o\"{extractPath}\" -y");

                    log?.Invoke($"Finished extracting {ModArchive}");

                    // Update progress
                    currentStep++;
                    progress?.Invoke((int)((double)currentStep / totalSteps * 100));
                }

                // Update totalPatchFolders after extraction
                totalPatchFolders = Directory.GetDirectories(tempPath).Length;
                totalSteps = totalMods + totalPatchFolders + 2; // recalc total steps including extracted patches

                // -----------------------------
                //  Prepare Patch folder
                // -----------------------------
                string patchFolder = Path.Combine(Directory.GetCurrentDirectory(), "Patch");

                // Ensure Patch folder is empty
                if (Directory.Exists(patchFolder))
                    Directory.Delete(patchFolder, true);

                Directory.CreateDirectory(patchFolder);

                // Path to WD2Extract.exe
                string wd2Extract = PatchToolsHelper.GetExtractExePath();
                log?.Invoke("All mods extracted. Running WD2Extract...");

                // -----------------------------
                //  Run WD2Extract on each mod folder
                // -----------------------------
                foreach (string modFolder in Directory.GetDirectories(tempPath))
                {
                    // Search recursively for patch3.fat in the extracted mod folder
                    string[] fatFiles = Directory.GetFiles(modFolder, "patch3.fat", SearchOption.AllDirectories);

                    if (fatFiles.Length == 0)
                        throw new FileNotFoundException($"patch3.fat not found in {modFolder}");

                    string modPatch3Fat = fatFiles[0];

                    // Extract patch3.dat into Patch folder
                    ProcessHelper.RunProcess(wd2Extract, $"\"{modPatch3Fat}\" \"{patchFolder}\" /overwrite");

                    // Update progress
                    currentStep++;
                    progress?.Invoke((int)((double)currentStep / totalSteps * 100));

                    log?.Invoke($"Processed patch3.dat from {modFolder}");
                }

                log?.Invoke("All patch3.dat extracted. Running WD2Pack...");

                // -----------------------------
                //  Pack final combined patch
                // -----------------------------
                string wd2Pack = PatchToolsHelper.GetPackExePath();
                string myModsBundleFolder = Path.Combine(Directory.GetCurrentDirectory(), "MyModsBundle");

                // Ensure MyModsBundle folder exists
                if (!Directory.Exists(myModsBundleFolder))
                    Directory.CreateDirectory(myModsBundleFolder);

                // Run WD2Pack with working directory set to MyModsBundle
                string workingDir = myModsBundleFolder;
                ProcessHelper.RunProcess(wd2Pack, $"\"{patchFolder}\" patch3.fat", workingDir);

                // -----------------------------
                //  Update progress for final patch files
                // -----------------------------
                currentStep++; // Patch3.fat created
                progress?.Invoke((int)((double)currentStep / totalSteps * 100));

                currentStep++; // Patch3.dat created
                progress?.Invoke((int)((double)currentStep / totalSteps * 100));

                // -----------------------------
                //  Log completion
                // -----------------------------
                log?.Invoke("All mods combined successfully, check MyModsBundle folder!");
                MessageBox.Show("Mods combined to MyModsBundle folder!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Display any errors to the user
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}