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

        

        public MusicHelper(string embeddedResourceName, Action<string> mhlog = null)
        {
           _log = mhlog ?? (_ => { }); //- Safe no-op to prevent NullReffException


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
                _player.Load(); // loads WAV into memory SYNCHRONUSLY!!! this a reason why we delay the mhlog print until UI render pass finishes
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