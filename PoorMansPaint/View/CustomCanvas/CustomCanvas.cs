using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PoorMansPaint.View.CustomCanvas
{
    // a canvas that is resizable, zoomable and operations on it are undoable
    public class CustomCanvas : Canvas
    {
        public static readonly double[] ZoomLevel = { 0.125, 0.25, 0.5, 0.75, 1, 2, 3, 4, 5, 6, 7, 8 };
        protected static int DEFAULT_ZOOM_INDEX = 4;
        protected int currentZoomIdx;
        public event SizeChangedEventHandler ZoomLevelChanged;

        public DrawingGroup DrawingGroup { get; set; }
        protected PathGeometry? _currentPathGeometry;

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

        public double CurrentZoom { get { return ZoomLevel[currentZoomIdx]; } }
        public CanvasCommander Commander { get; }

        public CustomCanvas () : base()
        {
            ScaleTransform scale = new ScaleTransform();
            LayoutTransform = scale;
            ZoomLevelChanged += UpdateZoom;
            SetZoomLevelIndex(DEFAULT_ZOOM_INDEX);
            DrawingGroup = new DrawingGroup();
            Commander = new CanvasCommander(this);
        }
        private void SetZoomLevelIndex(int idx)
        {
            if (idx < 0 || idx >= ZoomLevel.Length || currentZoomIdx == idx) 
                return;
            currentZoomIdx = idx;
            ZoomLevelChanged?.Invoke(this, null);
        }

        protected void UpdateZoom(object sender, SizeChangedEventArgs e)
        {
            ScaleTransform scale = GetCanvasScale();
            scale.ScaleX = ZoomLevel[currentZoomIdx];
            scale.ScaleY = ZoomLevel[currentZoomIdx];
        }
        public void ZoomIn()
        {
            if (currentZoomIdx < ZoomLevel.Length - 1) 
                SetZoomLevelIndex(currentZoomIdx + 1);
        }
        public void ZoomOut()
        {
            if (currentZoomIdx > 0) 
                SetZoomLevelIndex(currentZoomIdx - 1);
        }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            dc.DrawDrawing(DrawingGroup);
        } 

        private Point GetRoundedPoint(Point p)
        {
            return new Point(Math.Floor(p.X), Math.Floor(p.Y));
        }

        public bool IsDrawing()
        {
            return _currentPathGeometry != null;
        }

        public void StartDrawingAt(Point pos)
        {
            // check in range
            if (!(pos.X >= 0 && pos.Y >= 0 && pos.X <= Width && pos.Y <= Height))
                return;

            // ok, start new drawing
            _currentPathGeometry = new PathGeometry();
            PathFigure figure = new PathFigure()
            {
                StartPoint = GetRoundedPoint(pos),
            };
            Point tmp = new Point(figure.StartPoint.X, figure.StartPoint.Y);
            figure.Segments.Add(new LineSegment(tmp, true));
            _currentPathGeometry.Figures.Add(figure);

            // drawing with pencil is an UndoableCommand
            Commander.Command(new DrawWithPencilCommand(_currentPathGeometry));
        }
        public void ContinueDrawingAt(Point pos)
        {
            if (!IsDrawing()) return;
            // check in range
            if (pos.X >= 0 && pos.Y >= 0 && pos.X <= Width && pos.Y <= Height)
            {
                // in range
                int lastSegmentIndex = _currentPathGeometry.Figures.Count - 1;  
                PathFigure figure = _currentPathGeometry.Figures[lastSegmentIndex];
                if (figure.StartPoint.Equals(new Point(-1, -1)))
                {
                    // (-1,-1) is 'null' point we assigned below
                    // which means we start a new segment
                    figure.StartPoint = pos;
                }

                // draw a line from the last point
                figure.Segments.Add(new LineSegment(
                    GetRoundedPoint(pos),
                    true));
            }
            else
            {
                // out of range, start a new segment but without a start point
                // the start point will be assigned when in range again
                PathFigure figure = new PathFigure() { StartPoint = new Point(-1,-1)};
                _currentPathGeometry.Figures.Add(figure);
            }

        }
        public void FinishDrawing()
        {
            if (_currentPathGeometry == null) return;
            _currentPathGeometry.Freeze();
            _currentPathGeometry = null;
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
