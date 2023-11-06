using System.Numerics;

namespace Snake {
    internal class Input {
        private readonly char[] _moveDirectionKeys = new char[] {
            'w','s','a','d'
        };

        private readonly Vector2[] _moveDirections = new Vector2[] {
            -Vector2.UnitY,
            Vector2.UnitY,
            -Vector2.UnitX,
            Vector2.UnitX
        };

        private Vector2 _inputDirection;

        private Vector2 _clickPosition;

        private bool _clickedLastFrame;

        private bool _clicked;

        private bool _anyKeyLastFrame;

        private bool _anyKey;

        public Vector2 InputDirection => _inputDirection;

        public Vector2 ClickPosition => _clickPosition;

        public bool MouseClicked => _clickedLastFrame;

        public bool AnyKey => _anyKeyLastFrame;

        public Input(Keybinds keybinds, Form form) {

            _moveDirectionKeys[0] = keybinds.UpKey;
            _moveDirectionKeys[1] = keybinds.DownKey;
            _moveDirectionKeys[2] = keybinds.LeftKey;
            _moveDirectionKeys[3] = keybinds.RightKey;

            _inputDirection = -Vector2.UnitY;
            form.KeyPress += Form_KeyPress;
            form.MouseClick += Form_Click;
        }

        private void Form_Click(object? sender, MouseEventArgs e) {
            _clickPosition = new Vector2(e.Location.X, e.Location.Y);
            _clicked = true;
        }

        private void Form_KeyPress(object? sender, KeyPressEventArgs e) {
            _anyKey = true;
            for (int i = 0; i < _moveDirectionKeys.Length; i++) {
                if (_moveDirectionKeys[i] == e.KeyChar && _inputDirection != -_moveDirections[i]) {
                    _inputDirection = _moveDirections[i];
                }
            }
        }

        public void Update() {
            _clickedLastFrame = _clicked;
            _clicked = false;

            _anyKeyLastFrame = _anyKey || _clickedLastFrame;
            _anyKey = false;
        }

        public void Reset() {
            _anyKey = false;
            _clicked = false;
            _clickedLastFrame = false;
            _anyKeyLastFrame = false;
            _inputDirection = -Vector2.UnitY;
        }

        public struct Keybinds {
            public char UpKey { get; set; }

            public char DownKey { get; set; }

            public char LeftKey { get; set; }

            public char RightKey { get; set; }

            public static Keybinds Default => new() {
                UpKey = 'w',
                DownKey = 's',
                LeftKey = 'a',
                RightKey = 'd'
            };
        }
    }
}
