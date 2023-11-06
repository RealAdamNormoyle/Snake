using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Devices;

namespace Snake {
    internal class AudioSource {
        private readonly Dictionary<AudioAsset, Audio> _audioSources = new();

        private AudioAsset _activeAudio;

        public void PlayAudio(AudioAsset audioAsset, bool loop = false) {
            if (!_audioSources.ContainsKey(audioAsset)) {
                _audioSources.Add(audioAsset, new Audio());
            }

            if (_audioSources.TryGetValue(audioAsset, out Audio audio) && audio != null) {
                _activeAudio = audioAsset;
                AudioPlayMode playMode = loop ? AudioPlayMode.BackgroundLoop : AudioPlayMode.Background;
                audio.Play(audioAsset.Bytes, playMode);
            }
        }

        public void StopAudio() {
            if (_activeAudio == null)
                return;

            if (_audioSources.TryGetValue(_activeAudio, out Audio audio) && audio != null) {
                audio.Stop();
            }
        }
    }
}
