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

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _currentPathGeometry = new PathGeometry();
                PathFigure figure = new PathFigure()
                {
                    StartPoint = GetRoundedPoint(e.GetPosition(this)),
                };
                Point tmp = new Point(figure.StartPoint.X, figure.StartPoint.Y);
                figure.Segments.Add(new LineSegment(tmp, true));
                _currentPathGeometry.Figures.Add(figure);
                Commander.Command(new DrawWithPencilCommand(_currentPathGeometry));
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //Trace.WriteLine(e.GetPosition(this));
                PathFigure figure = _currentPathGeometry.Figures[0];
                figure.Segments.Add(new LineSegment(
                    GetRoundedPoint(e.GetPosition(this)), 
                    true));
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (_currentPathGeometry != null)
            {
                _currentPathGeometry.Freeze();
                /*
                DrawingVisual drawingVisual = new DrawingVisual();
                using (var draw = drawingVisual.RenderOpen())
                {
                    draw.DrawDrawing(_drawingGroup);
                }
                var bmp = new RenderTargetBitmap((int)Width, (int)Height, 96, 96, PixelFormats.);
                bmp.Render(drawingVisual);
                Image image = new Image();
                image.Source = bmp;
                this.Children.Clear();
                this.Children.Add(image);*/
            }
            base.OnMouseUp(e);
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
