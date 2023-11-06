using Rendering;
using System.Diagnostics;
using System.Text.Json;

namespace Snake {
    internal class AppContext : ApplicationContext {

        private readonly string _appConfigPath;

        private readonly JsonSerializerOptions _jsonSerializerOptions = new() { IncludeFields = true, WriteIndented = true };

        private readonly GridRendererForm _gridRenderer;

        private readonly GridRendererContext _gridRendererContext;

        private readonly SnakeGame _snakeGame;

        private readonly Input _input;

        public AppContext() {
            _appConfigPath = string.Concat(Application.StartupPath, "app-config.cfg");
            AppConfiguration appConfig = LoadAppConfiguration();
            _gridRendererContext = new GridRendererContext();
            _gridRenderer = new GridRendererForm(
                appConfig.GameName,
                new Size((int)appConfig.WindowSize.X, (int)appConfig.WindowSize.Y),
                appConfig.GridSize,
                _gridRendererContext);

            _input = new Input(appConfig.Keybindings, _gridRenderer);
            _snakeGame = new SnakeGame(appConfig, _input, _gridRenderer);
            _gridRenderer.FormClosed += new FormClosedEventHandler(OnClose);
            _gridRenderer.Show();

            Stopwatch stopwatch = Stopwatch.StartNew();
            double ticks = stopwatch.Elapsed.TotalMilliseconds;
            while (!_gridRenderer.IsDisposed) {
                _input.Update();
                _snakeGame.Update((float)(stopwatch.Elapsed.TotalMilliseconds - ticks) / 100);
                ticks = stopwatch.Elapsed.TotalMilliseconds;
                _gridRenderer.Render();
                Application.DoEvents();
            }
        }

        private AppConfiguration LoadAppConfiguration() {
            AppConfiguration appConfiguration = AppConfiguration.Default;

            if (File.Exists(_appConfigPath)) {
                string text = File.ReadAllText(_appConfigPath);
                appConfiguration = JsonSerializer.Deserialize<AppConfiguration>(text, _jsonSerializerOptions);
            } else {
                File.WriteAllText(_appConfigPath, JsonSerializer.Serialize(appConfiguration, _jsonSerializerOptions));
            }

            return appConfiguration;
        }

        private void OnClose(object? sender, FormClosedEventArgs e) {
            Application.Exit();
        }

    }
}
