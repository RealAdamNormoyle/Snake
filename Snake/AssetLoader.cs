using System.Diagnostics;
using System.Text.Json;

namespace Snake {
    internal class AssetLoader {

        private readonly string _dataLocation = String.Concat(Application.StartupPath, "/data/");

        private readonly string _manifestLocation = String.Concat(Application.StartupPath, "/asset-manifest.json");

        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions() { IncludeFields = true, WriteIndented = true };

        private readonly Dictionary<string, (int, IAsset)> _assetRegister = new Dictionary<string, (int, IAsset)>();

        private AssetManifest _assetManifest;

        public AssetLoader() {
            if (!Directory.Exists(_dataLocation)) {
                Directory.CreateDirectory(_dataLocation);
            }

            LoadAssetManifest();
            PreloadAssets();
        }

        private void PreloadAssets() {
            for (int i = 0; i < _assetManifest.Assets.Length; i++) {
                string path = string.Concat(_dataLocation, _assetManifest.Assets[i].Path);
                if (!File.Exists(path)) {
                    Debug.WriteLine($"Error loading asset: File does not exist ({path})");
                    continue;
                }

                IAsset asset = null;
                try {
                    byte[] bytes = File.ReadAllBytes(path);
                    if (bytes != null) {
                        switch (_assetManifest.Assets[i].Type) {
                            case "Bitmap":
                                asset = new BitmapAsset();
                                asset.Load(bytes);
                                break;
                            case "Audio":
                                asset = new AudioAsset();
                                asset.Load(bytes);
                                break;
                            default:
                                break;
                        }
                    }
                } catch (Exception e) {
                    Debug.WriteLine($"ERROR: Failed to load asset from manifest {path}");
                    Debug.WriteLine(e);
                    continue;
                }

                if (asset == null) {
                    continue;
                }

                _assetRegister.Add(_assetManifest.Assets[i].Name, (i, asset));
            }
        }

        private void LoadAssetManifest() {
            if (!File.Exists(_manifestLocation)) {
                _assetManifest = new AssetManifest() {
                    Assets = new AssetConfig[0]
                };

                string json = JsonSerializer.Serialize(_assetManifest, _serializerOptions);
                File.WriteAllText(_manifestLocation, json);
                Debug.WriteLine($"ERROR: Failed to load asset manifest");
                return;
            }

            string jsonString = File.ReadAllText(_manifestLocation);
            _assetManifest = JsonSerializer.Deserialize<AssetManifest>(jsonString, _serializerOptions);
        }

        public T GetAsset<T>(string asset) where T : IAsset {
            if (!_assetRegister.TryGetValue(asset, out (int index, IAsset data) assetData)) {
                Debug.WriteLine($"ERROR: Failed to load asset {asset}");
                return default;
            }

            if (assetData.data is not T) {
                return default;
            }

            return (T)assetData.data;
        }


        private struct AssetConfig {
            public string Name;
            public string Path;
            public string Type;
        }

        private struct AssetManifest {
            public AssetConfig[] Assets { get; set; }
        }
    }

    public interface IAsset {
        public void Load(byte[] bytes);
    }

    public class BitmapAsset : IAsset {
        private Bitmap _bitmap;

        public Bitmap Bitmap => _bitmap;

        public void Load(byte[] bytes) {
            using Stream stream = new MemoryStream(bytes);
            try {
                _bitmap = new Bitmap(stream);
            } catch (Exception e) {
                _bitmap = new Bitmap(1, 1);
                Debug.WriteLine(e);
            }
        }
    }

    public class AudioAsset : IAsset {
        private byte[] _bytes;

        public byte[] Bytes => _bytes;

        public void Load(byte[] bytes) {
            _bytes = bytes;
        }
    }
}
