using Pathfinding.AStar;
using Rendering;
using System.Numerics;

namespace Snake {
    internal class SnakeGame {
        private readonly GridRendererForm _gridRenderer;

        private readonly Input _input;

        private readonly AssetLoader _assetLoader;

        private readonly AudioSource _musicAudioSource;

        private readonly AudioSource _sfxAudioSource;

        private readonly List<Snake> _snakes = new List<Snake>();

        private readonly NodeGrid _nodeGrid;

        private readonly Font _scoreFont;

        private readonly Font _titleFont;

        private readonly Font _subtitleFont;

        private readonly AppConfiguration _appConfig;

        private readonly List<Vector2> _foodPositions = new();

        private readonly AudioAsset _backgroundMusic;

        private readonly AudioAsset _endgameSFX;

        private readonly AudioAsset _eatFoodSFX;

        private readonly BitmapAsset _splashTitleImage;

        private readonly BitmapAsset _gameOverImage;

        private float _time;

        private GameState _gameState;

        public SnakeGame(AppConfiguration appConfiguration, Input input, GridRendererForm gridRenderer) {
            _appConfig = appConfiguration;
            _gridRenderer = gridRenderer;
            _input = input;
            _nodeGrid = new(_appConfig.GridSize, _appConfig.GridSize);
            _scoreFont = new Font(SystemFonts.DefaultFont.FontFamily, 20, FontStyle.Bold);
            _titleFont = new Font(SystemFonts.DefaultFont.FontFamily, 24, FontStyle.Bold);
            _subtitleFont = new Font(SystemFonts.DefaultFont.FontFamily, 12, FontStyle.Regular);
            _assetLoader = new AssetLoader();
            _musicAudioSource = new AudioSource();
            _sfxAudioSource = new AudioSource();

            _gridRenderer.OnRender += new RenderEventHandler(Render);

            _snakes.Add(new Snake(_appConfig.PlayerStartPosition, 3, Brushes.Blue, true));

            if (_appConfig.EnableBot) {
                _snakes.Add(new Snake(_appConfig.BotStartPosition, 3, Brushes.Red));
            }

            _splashTitleImage = _assetLoader.GetAsset<BitmapAsset>("Title");
            _gameOverImage = _assetLoader.GetAsset<BitmapAsset>("gameover-title");
            _backgroundMusic = _assetLoader.GetAsset<AudioAsset>("background-music");
            _endgameSFX = _assetLoader.GetAsset<AudioAsset>("endgame-sfx");
            _eatFoodSFX = _assetLoader.GetAsset<AudioAsset>("collect-food-sfx");


            Reset();
        }

        private void Reset() {
            _time = 0;
            _gameState = GameState.Splash;
            _input.Reset();
            foreach (Snake snake in _snakes) {
                snake.Direction = Vector2.Zero;
                snake.Segments.Clear();
                Vector2 position = _appConfig.PlayerStartPosition;
                if (!snake.IsPlayer) {
                    position = _appConfig.BotStartPosition;
                }
                for (int i = 0; i < 3; i++) {
                    snake.Segments.Add(position + (Vector2.UnitY * i));
                }
            }

            _musicAudioSource.PlayAudio(_backgroundMusic, true);
        }

        public void Update(float dt) {
            if (_gameState == GameState.Splash && _input.AnyKey) {
                _gameState = GameState.Playing;
                _musicAudioSource.StopAudio();
            }

            if (_gameState != GameState.Playing) {
                if ((_gameState == GameState.Win || _gameState == GameState.Lose) && _input.AnyKey) {
                    Reset();
                }
                return;
            }

            if (_input.MouseClicked && _appConfig.AllowPlacingFood) {
                _foodPositions.Add(_input.ClickPosition);
            }

            if (_time >= 1f / _appConfig.SnakeMoveSpeed) {
                _time = 0;
            } else {
                _time += dt;
                return;
            }

            UpdateNodeGrid();

            if (_foodPositions.Count < 2) {
                Vector2 position = new(Random.Shared.Next(_nodeGrid.Width), Random.Shared.Next(_nodeGrid.Height));
                Vector2? output = _nodeGrid.GetClosestUnblockedPosition(position);
                if (output.HasValue) {
                    _foodPositions.Add(output.Value);
                }
            }

            for (int i = 0; i < _snakes.Count; i++) {
                if (_snakes[i].IsPlayer) {
                    _snakes[i].Direction = _input.InputDirection;
                }

                if (_foodPositions.Count > 0) {
                    if (!_snakes[i].IsPlayer) {
                        int foodIndex = GetClosestFoodIndex(_snakes[i].Segments[0]);
                        AStarPathFinder.FindPath(_snakes[i].Segments[0], _foodPositions[foodIndex], _nodeGrid, (path) => {
                            if (path.Length > 1) {
                                _snakes[i].Direction = path[1] - _snakes[i].Segments[0];
                            }
                        });
                    }

                    for (int j = 0; j < _foodPositions.Count; j++) {
                        if (_foodPositions[j] == _snakes[i].Segments[0]) {
                            _foodPositions.RemoveAt(j);
                            _snakes[i].Segments.Add(_snakes[i].Segments[0]);
                            if (_snakes[i].IsPlayer) {
                                _sfxAudioSource.PlayAudio(_eatFoodSFX);
                            }
                        }
                    }
                }

                for (int j = _snakes[i].Segments.Count - 1; j >= 0; j--) {
                    if (_snakes[i].Direction == Vector2.Zero) {
                        continue;
                    }

                    if (j == 0) {
                        if (_nodeGrid.GetNode(_snakes[i].Segments[j] + _snakes[i].Direction).Blocked) {
                            if (_snakes[i].IsPlayer) {
                                _gameState = GameState.Lose;
                                _sfxAudioSource.PlayAudio(_endgameSFX);
                            } else {
                                _gameState = GameState.Win;
                                _sfxAudioSource.PlayAudio(_endgameSFX);
                            }
                            return;
                        }

                        _snakes[i].Segments[j] += _snakes[i].Direction;
                    } else {
                        _snakes[i].Segments[j] += (_snakes[i].Segments[j - 1] - _snakes[i].Segments[j]);
                    }
                }
            }
        }

        private void Render(GridRendererContext context) {
            int centerX = (context.CanvasSize.Width / 2);
            int centerY = (context.CanvasSize.Height / 2);


            if (_gameState == GameState.Splash) {

                if (_splashTitleImage != null) {
                    context.DrawBitmap(new Vector2(centerX, 0), new Vector2(256, 128), new Vector2(0.5f, 0f), _assetLoader.GetAsset<BitmapAsset>("Title"));
                } else {
                    context.DrawText(_appConfig.GameName, new Vector2(centerX, 50), new Vector2(0.5f, 0.5f), _titleFont, Brushes.DarkGreen);
                }

                context.DrawText(_appConfig.IntroSplashText, new Vector2(centerX, centerY + 50), new Vector2(0.5f, 0.5f), _subtitleFont, Brushes.Gray);
                return;
            }

            for (int i = 0; i < _snakes.Count; i++) {
                for (int j = 0; j < _snakes[i].Segments.Count; j++) {
                    context.RenderQueue.Add(new RenderQueueItem() {
                        Circle = true,
                        Color = _snakes[i].Color,
                        Size = new SizeF(1, 1),
                        Fill = true,
                        Position = new PointF(MathF.Floor(_snakes[i].Segments[j].X), MathF.Floor(_snakes[i].Segments[j].Y))
                    });
                }
            }

            for (int i = 0; i < _foodPositions.Count; i++) {
                context.RenderQueue.Add(new RenderQueueItem() {
                    Circle = true,
                    Color = Brushes.Green,
                    Size = new SizeF(1f, 1f),
                    Fill = true,
                    Position = new PointF(MathF.Floor(_foodPositions[i].X), MathF.Floor(_foodPositions[i].Y))
                });
            }

            if (_gameState == GameState.Win) {
                context.DrawBitmap(new Vector2(centerX, 50), new Vector2(280, 48), new Vector2(0.5f, 0.5f), _gameOverImage);
                context.DrawText(_snakes[0].Segments.Count.ToString(), new Vector2(centerX, centerY), new Vector2(0.5f, 0.5f), _titleFont, Brushes.Black);
                return;
            } else if (_gameState == GameState.Lose) {
                context.DrawBitmap(new Vector2(centerX, 50), new Vector2(280, 48), new Vector2(0.5f, 0.5f), _gameOverImage);
                context.DrawText(_snakes[0].Segments.Count.ToString(), new Vector2(centerX, centerY), new Vector2(0.5f, 0.5f), _titleFont, Brushes.Black);
                return;
            }

            if (_appConfig.EnableBot) {
                context.DrawText(_snakes[0].Segments.Count.ToString(), new Vector2(centerX - 50, 20), new Vector2(0.5f, 0.5f), _scoreFont, Brushes.Blue);
                context.DrawText("v", new Vector2(centerX, 20), new Vector2(0.5f, 0.5f), _scoreFont, Brushes.DarkGreen);
                context.DrawText(_snakes[1].Segments.Count.ToString(), new Vector2(centerX + 50, 20), new Vector2(0.5f, 0.5f), _scoreFont, Brushes.Red);
            } else {
                context.DrawText(_snakes[0].Segments.Count.ToString(), new Vector2(centerX, 20), new Vector2(0.5f, 0.5f), _scoreFont, Brushes.Blue);
            }

        }

        private void UpdateNodeGrid() {
            _nodeGrid.ClearBlocked();
            for (int i = 0; i < _snakes.Count; i++) {
                for (int j = 0; j < _snakes[i].Segments.Count; j++) {
                    _nodeGrid.SetBlocked(_snakes[i].Segments[j], true);
                }
            }
        }

        private int GetClosestFoodIndex(Vector2 position) {
            float bestDistance = float.MaxValue;
            int bestIndex = 0;
            for (int i = 0; i < _foodPositions.Count; i++) {
                float distance = Vector2.Distance(_foodPositions[i], position);
                if (distance < bestDistance) {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }
            return bestIndex;
        }

        private class Snake {
            public List<Vector2> Segments = new List<Vector2>();
            public Brush Color;
            public bool IsPlayer;
            public Vector2 Direction;

            public Snake(Vector2 position, int startSize, Brush color, bool isPlayer = false) {
                for (int i = 0; i < startSize; i++) {
                    Segments.Add(position + (Vector2.UnitY * i));
                }
                Color = color;
                IsPlayer = isPlayer;
            }
        }

        private enum GameState {
            Splash,
            Playing,
            Lose,
            Win
        }
    }
}
