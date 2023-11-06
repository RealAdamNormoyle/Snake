using Snake;
using System.Numerics;

namespace Rendering {

    public delegate void RenderEventHandler(GridRendererContext context);

    public delegate void CellClickedEventHandler((Point, MouseButtons) data);

    internal class GridRendererForm : Form {

        private readonly GridRendererContext _gridRendererContext;

        private readonly Bitmap _gridTexture;

        private readonly Bitmap _textLayer;

        private readonly Bitmap[] _displayBuffers;

        private readonly Graphics[] _graphicsBuffers;

        private readonly int _gridWidthCells;

        private readonly int _cellWidth;

        private int _backBuffer;

        public event CellClickedEventHandler CellClicked;

        public event RenderEventHandler OnRender;

        public GridRendererForm(
            string title,
            Size windowSize,
            int gridSize,
            GridRendererContext gridRendererContext) {

            _gridRendererContext = gridRendererContext;
            _gridWidthCells = gridSize;
            _cellWidth = windowSize.Width / gridSize;
            _gridTexture = new Bitmap(_cellWidth * _gridWidthCells, _cellWidth * _gridWidthCells);
            _textLayer = new Bitmap(windowSize.Width, windowSize.Height);
            _gridRendererContext.TextGraphics = Graphics.FromImage(_textLayer);
            _gridRendererContext.CanvasSize = windowSize;

            _displayBuffers = new Bitmap[2]{
                new Bitmap(windowSize.Width, windowSize.Height),
                new Bitmap(windowSize.Width, windowSize.Height),
            };

            _graphicsBuffers = new Graphics[2] {
                Graphics.FromImage(_displayBuffers[0]),
                Graphics.FromImage(_displayBuffers[1])
            };

            MouseClick += new MouseEventHandler(OnClicked);
            DoubleBuffered = true;
            Name = title;
            Text = title;
            Size = windowSize;
            Size offset = windowSize - DisplayRectangle.Size;
            Size = windowSize + offset;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            BackgroundImage = _gridTexture;
            BackgroundImageLayout = ImageLayout.Tile;
            GenerateGrid(Graphics.FromImage(_gridTexture), _cellWidth, _gridWidthCells);
        }

        private void OnClicked(object? sender, MouseEventArgs e) {
            Point cellPosition = new() {
                X = (int)MathF.Floor(e.X / (float)_cellWidth),
                Y = (int)MathF.Floor(e.Y / (float)_cellWidth)
            };

            CellClicked?.Invoke((cellPosition, e.Button));
        }

        public void Render() {
            _gridRendererContext.TextGraphics.Clear(Color.Transparent);
            _gridRendererContext.RenderQueue.Clear();
            OnRender?.Invoke(_gridRendererContext);
            _gridRendererContext.TextGraphics.Flush();
            int frontBuffer = _backBuffer == 0 ? 1 : 0;
            BackgroundImage = _displayBuffers[frontBuffer];
            Refresh();

            Graphics graphics = _graphicsBuffers[_backBuffer];
            _backBuffer = frontBuffer;
            graphics.DrawImage(_gridTexture, new Point(0, 0));
            if (_gridRendererContext.RenderQueue != null) {
                for (int i = 0; i < _gridRendererContext.RenderQueue.Count; i++) {
                    RenderQueueItem entry = _gridRendererContext.RenderQueue[i];
                    Point pos = new Point((int)MathF.Floor(entry.Position.X * _cellWidth), (int)MathF.Floor(entry.Position.Y * _cellWidth));
                    pos.X += (int)((_cellWidth / 2) - ((entry.Size.Width * _cellWidth) / 2));
                    pos.Y += (int)((_cellWidth / 2) - ((entry.Size.Height * _cellWidth) / 2));

                    if (entry.Circle) {
                        if (entry.Fill) {
                            graphics.FillEllipse(entry.Color, new RectangleF(pos, entry.Size * _cellWidth));
                        } else {
                            graphics.DrawEllipse(new Pen(entry.Color), new RectangleF(pos, entry.Size * _cellWidth));
                        }
                    } else if (entry.Bitmap != null) {
                        graphics.DrawImage(entry.Bitmap, new RectangleF(entry.Position, entry.Size));

                    } else {
                        if (entry.Fill) {
                            graphics.FillRectangle(entry.Color, new RectangleF(pos, entry.Size * _cellWidth));
                        } else {
                            graphics.DrawRectangle(new Pen(entry.Color), new RectangleF(pos, entry.Size * _cellWidth));
                        }
                    }
                }
            }

            graphics.DrawImage(_textLayer, new Point());
            graphics.Flush();
        }

        private static void GenerateGrid(Graphics graphics, int cellSize, int gridSize) {
            graphics.Clear(Color.White);

            for (int y = 0; y < gridSize; y += 2) {
                for (int x = 0; x < gridSize; x += 2) {
                    graphics.FillRectangle(Brushes.LightBlue, new Rectangle(cellSize * x, cellSize * y, cellSize, cellSize));
                    graphics.FillRectangle(Brushes.LightBlue, new Rectangle(cellSize * (x + 1), cellSize * (y + 1), cellSize, cellSize));
                }
            }

            graphics.Flush();
        }
    }

    public class GridRendererContext {

        public List<RenderQueueItem> RenderQueue = new();

        public Graphics TextGraphics;

        public Size CanvasSize;

        public void DrawText(string text, Vector2 position, Vector2 pivot, Font font, Brush color) {
            if (TextGraphics == null)
                return;

            SizeF size = TextGraphics.MeasureString(text, font);
            TextGraphics.DrawString(text, font, color, new RectangleF(position.X - (size.Width * pivot.X), position.Y - (size.Height * pivot.Y), size.Width, size.Height));
        }

        public void DrawBitmap(Vector2 position, Vector2 size, Vector2 pivot, BitmapAsset asset) {
            RenderQueue.Add(new RenderQueueItem() {
                Bitmap = asset.Bitmap,
                Position = new PointF(position.X - (size.X * pivot.X), position.Y - (size.Y * pivot.Y)),
                Size = new SizeF(size.X, size.Y)
            });
        }
    }

    public struct RenderQueueItem {

        public PointF Position;

        public Brush Color;

        public SizeF Size;

        public bool Circle;

        public bool Fill;

        public Bitmap Bitmap;
    }
}
