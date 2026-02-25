using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Media;
using System.Reflection;
using System.Windows; // <-- needed for Application.Current.Dispatcher
using System.Windows.Threading;

namespace WD2ModBundler.Helpers
{
    public class MusicHelper
    {
        private SoundPlayer _player;
        private Stream _stream; // Keep alive the music stream
        private Action<string> _log;

        public MusicHelper(string embeddedResourceName, Action<string> log = null)
        {
            _log = log ?? (_ => { }); // Safe no-op
// Dispatcher bellow wraps the provided log to always run on the UI thread.
// This is needed because:
//    MainWindow constructor starts
//   |
//   | --> InitializeComponent()
//   | (TextBox created but not fully rendered yet)
//   |
//   | --> MusicHelper constructor
//         |
//         | --> _player.Load()(blocking, synchronous)
//         |
//         | --> _log("WAV loaded") called
//               |
//               | --> Dispatcher.BeginInvoke queues action
//               | (does NOT run yet)
//   |
//   | --> Constructor finishes, WPF finishes first render
//         |
//         | --> Dispatcher executes queued lambda
//               log?.Invoke("WAV loaded") runs on UI thread
//               TextBox shows the message

            _log = message =>
            {
                // Dispatcher ensures the UI is ready
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    log?.Invoke(message);
                });
            };

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                _stream = assembly.GetManifestResourceStream(embeddedResourceName);

                if (_stream == null)
                {
                    _log($"Failed to find embedded resource: {embeddedResourceName}");
                    return;
                }

                _player = new SoundPlayer(_stream);
                _player.Load(); // loads WAV into memory SYNCHRONUSLY!!! this a reason why we delay the log print until UI render pass finishes
                _log($"WAV music loaded successfully from resource: {embeddedResourceName}");
            }
            catch (Exception ex)
            {
                _log($"Error loading WAV: {ex.Message}");
            }
        }

        public void Play()
        {
            try
            {
                _player?.PlayLooping();
                _log("Music playback started.");
            }
            catch (Exception ex)
            {
                _log($"Error playing music: {ex.Message}");
            }
        }

        public void Stop()
        {
            try
            {
                _player?.Stop();
                _log("Music stopped.");
            }
            catch (Exception ex)
            {
                _log($"Error stopping music: {ex.Message}");
            }
        }
    }
}