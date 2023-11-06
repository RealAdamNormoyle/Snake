using System.Numerics;

namespace Snake {

    internal struct AppConfiguration {

        public string GameName { get; set; }

        public Vector2 WindowSize { get; set; }

        public int GridSize { get; set; }

        public bool EnableBot { get; set; }

        public bool AllowPlacingFood { get; set; }

        public float SnakeMoveSpeed { get; set; }

        public int FoodCountTarget { get; set; }

        public string IntroSplashText { get; set; }

        public string GameOverText { get; set; }

        public Vector2 PlayerStartPosition { get; set; }

        public Vector2 BotStartPosition { get; set; }

        public Input.Keybinds Keybindings { get; set; }

        public static AppConfiguration Default => new() {
            GameName = "SNAKE",
            WindowSize = new Vector2(300, 300),
            GridSize = 30,
            EnableBot = true,
            AllowPlacingFood = false,
            SnakeMoveSpeed = 1,
            FoodCountTarget = 2,
            PlayerStartPosition = new Vector2(5, 15),
            BotStartPosition = new Vector2(25, 15),
            IntroSplashText = "PRESS ANY KEY TO START",
            GameOverText = "GAME OVER",
            Keybindings = Input.Keybinds.Default
        };

    }

}
