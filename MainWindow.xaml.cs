using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualBasic.Logging;
using Microsoft.Win32; // For OpenFileDialog
using WD2ModBundler.Helpers;
using WD2ModBundler.Services;

namespace WD2ModBundler
{
    public partial class MainWindow : Window
    {
        private MusicHelper _musicHelper;
        private Action<string> _log;


        //Method for MainWindow.Loaded event
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Start music automatically after window is fully rendered
            _musicHelper?.Play();
        }

        public MainWindow()
        {
            InitializeComponent();

            _log = message =>
            {
                // Dispatcher ensures the UI is ready
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    Log(message);
                });
            };

            try
            {
                // Embedded WAV resource path (set Build Action = Embedded Resource)
                string wavResource = "WD2ModBundler.Assets.startup.wav";
                _log($"Loading WAV music from: {wavResource}");

                // Pass _log so MusicHelper can report its events
                _musicHelper = new MusicHelper(wavResource, _log);

                _log("MusicHelper initialized successfully!");

                // ASCII splash
                string resourcePath = "/WD2ModBundler;component/Assets/logo.png";
                string asciiArt = AsciiArtHelper.ConvertToAsciiFromResource(resourcePath, 80);
                LogTextBox.Text = asciiArt;
                LogTextBox.ScrollToEnd();

                // Subscribing MainWindow_Loaded method to MainWindow.Loaded event
                this.Loaded += MainWindow_Loaded;
            }
            catch (Exception ex)
            {
                _log($"Error in MainWindow constructor: {ex.Message}");
            }
        }



        #region File / Folder Selection Buttons
        //Bellow are different clickers
        private void Select7Zip_Click(object sender, RoutedEventArgs e)
        {
            string message = "Most common install paths are:\n" +
                             @"C:\Program Files\7-Zip" + "\n" +
                             @"C:\Program Files (x86)\7-Zip";

            MessageBox.Show(message, "7-Zip Info", MessageBoxButton.OK, MessageBoxImage.Information);

            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Select 7z.exe",
                Filter = "7-Zip Executable|7z.exe",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            };

            if (ofd.ShowDialog() == true)
            {
                string selectedPath = ofd.FileName;
                SevenZipPathTextBlock.Text = selectedPath;
                ArchiveHelper.Set7ZipPath(selectedPath);
            }
        }

        private void SelectModFolder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please select the folder containing mod archives.\nDo NOT extract the archives before selecting.",
                "Mod Folder Info", MessageBoxButton.OK, MessageBoxImage.Information);

            using (var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select folder with mod archives"
            })
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string selectedPath = dialog.SelectedPath;
                    FolderPathTextBlock.Text = selectedPath;
                    ModFolderHelper.SetModFolderPath(selectedPath);
                }
            }
        }

        private void SelectPatchTools_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please select the folder containing WD2 Patch Tools.\nFolder MUST contain WD2Extract.exe and WD2Pack.exe.",
                "WD2 Patch Tools Info", MessageBoxButton.OK, MessageBoxImage.Information);

            using (var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select folder with WD2 Patch Tools"
            })
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string selectedPath = dialog.SelectedPath;
                    PatchToolsPath.Text = selectedPath;
                    PatchToolsHelper.SetPatchToolsPath(selectedPath);
                }
            }
        }

        #endregion

        #region Music Controls

        private void PlayMusic_Click(object sender, RoutedEventArgs e)
        {
            if (_musicHelper != null)
                _musicHelper.Play();
            else
                _log("MusicHelper is null! Cannot play.");
        }

        private void StopMusic_Click(object sender, RoutedEventArgs e)
        {
            _musicHelper?.Stop();
        }

        #endregion

        #region Combine Mods

        private async void CombineMods_Click(object sender, RoutedEventArgs e)  //Async ensures that UI does not stop
        {
            try
            {
                string modFolderPath = ModFolderHelper.GetModFolderPath();
                var service = new ModBundleService();

                await Task.Run(() =>
                {
                    service.CombineMods(
                        modFolderPath,
                        message => Dispatcher.BeginInvoke(() => _log(message)),
                        percent => Dispatcher.BeginInvoke(() => ProgressBar.Value = percent));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Logging
        //Logging method
        public void Log(string message)
        {
            string line = $"$ {message}{Environment.NewLine}";
            LogTextBox.AppendText(line);
            LogTextBox.CaretIndex = LogTextBox.Text.Length;
            LogTextBox.ScrollToEnd();
        }

        #endregion
    }
}