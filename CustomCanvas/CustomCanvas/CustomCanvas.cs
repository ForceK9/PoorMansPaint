using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PoorMansPaint.CustomCanvas
{
    // a canvas that is resizable, zoomable and operations on it are undoable
    public class CustomCanvas : Canvas, INotifyPropertyChanged
    {
        public static readonly double[] ZoomLevel = { 0.125, 0.25, 0.5, 0.75, 1, 2, 3, 4, 5, 6, 7, 8 };
        protected static int DEFAULT_ZOOM_INDEX = 4;
        protected int _currentZoomIdx;
        public int CurrentZoomIndex
        {
            get { return _currentZoomIdx; }
            set
            {
                if (value < 0 || value >= ZoomLevel.Length || _currentZoomIdx == value)
                    return;
                _currentZoomIdx = value;
                ZoomLevelChanged?.Invoke(this, null);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentZoomIndex"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentZoom"));
            }
        }
        public event SizeChangedEventHandler ZoomLevelChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        private DrawingGroup _drawingGroup;
        public DrawingGroup DrawingGroup
        {
            get { return _drawingGroup; }
            set
            {
                _drawingGroup = value;
                if (value.ClipGeometry != null)
                {
                    Width = value.ClipGeometry.Bounds.Width;
                    Height = value.ClipGeometry.Bounds.Height;
                }
                else
                {
                    Width = value.Bounds.Width;
                    Height = value.Bounds.Height;
                }
                InvalidateVisual();
            }
        }
        private DrawingTool _drawingTool;
        public DrawingTool DrawingTool { 
            get { return _drawingTool; }
            set
            {
                _drawingTool = value;
                this.Cursor = _drawingTool.Cursor;
            }
        }
        public Pen Pen { get; set; }

        private ScaleTransform GetCanvasScale()
        {
            ScaleTransform? scale = LayoutTransform as ScaleTransform;
            if (scale == null) throw new InvalidCastException();
            return scale;
        }
        public double RealWidth
        {
            get
            {
                return Width * GetCanvasScale().ScaleX;
            }
            set
            {
                Width = value / GetCanvasScale().ScaleX;
            }
        }
        public double RealHeight
        {
            get
            {
                return Height * GetCanvasScale().ScaleY;
            }
            set
            {
                Height = value / GetCanvasScale().ScaleY;
            }
        }

        public double CurrentZoom { get { return ZoomLevel[CurrentZoomIndex]; } }
        public CanvasCommander Commander { get; }

        public CustomCanvas () : base()
        {
            ScaleTransform scale = new ScaleTransform();
            LayoutTransform = scale;
            SizeChanged += OnSizeChanged;
            ZoomLevelChanged += UpdateZoom;
            CurrentZoomIndex = DEFAULT_ZOOM_INDEX;
            _drawingGroup = new DrawingGroup(); 
            Commander = new CanvasCommander(this);
            Pen = new Pen(Brushes.Black, 1)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                LineJoin = PenLineJoin.Round,
            };
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Width"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Height"));
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            AddClipping();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            dc.DrawDrawing(_drawingGroup);
        }

        public void AddClipping()
        {
            // create clipping
            if (_drawingGroup.Children.Count > 0)
            {
                DrawingGroup backup = this.DrawingGroup;
                _drawingGroup = new DrawingGroup();
                _drawingGroup.Children.Add(backup);
            }
            _drawingGroup.ClipGeometry = new RectangleGeometry(
                new Rect(new Point(0, 0), new Point(Width, Height)));
        }

        public bool IsModified()
        {
            return Commander.IsUndoingPossible();
        }


        public bool ContainsPoint(Point pos)
        {
            return pos.X >= 0 && pos.Y >= 0 && pos.X <= Width && pos.Y <= Height;
        }

        protected void UpdateZoom(object sender, SizeChangedEventArgs e)
        {
            ScaleTransform scale = GetCanvasScale();
            scale.ScaleX = ZoomLevel[_currentZoomIdx];
            scale.ScaleY = ZoomLevel[_currentZoomIdx];
        }
        public void ZoomIn()
        {
            if (_currentZoomIdx < ZoomLevel.Length - 1) 
                CurrentZoomIndex++;
        }
        public void ZoomOut()
        {
            if (_currentZoomIdx > 0)
                CurrentZoomIndex--;
        }

        public void Reset()
        {
            _drawingGroup.Children.Clear();
            Commander.ClearHistory();
            InvalidateVisual();
        }

        public void Load (DrawingGroup drawing)
        {
            Reset();
            DrawingGroup = drawing;
        }

        public Point GetRoundedPoint(Point p)
        {
            return new Point(Math.Floor(p.X), Math.Floor(p.Y));
        }

        public BitmapSource CreateBitmap()
        {
            // copy canvas into a buffer Visual, because it has a margin from its parent
            // https://stackoverflow.com/questions/43844085/re-positioning-canvas-control
            DrawingVisual drawingVisual = new DrawingVisual();
            using (var draw = drawingVisual.RenderOpen())
            {
                draw.DrawRectangle(
                    new VisualBrush(this),
                    null,
                    new Rect(0, 0, Width, Height));
            }

            // turn canvas to bitmap
            RenderTargetBitmap bmp = new RenderTargetBitmap(
                (int)Width, (int)Height,
                96, 96,
                PixelFormats.Default);
            bmp.Render(drawingVisual);

            return bmp;
        }
    }
}
