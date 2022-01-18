using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PoorMansPaint.View
{
    public class ZoomableCanvas : Canvas
    {
        public static readonly double[] ZoomLevel = { 0.125, 0.25, 0.5, 0.75, 1, 2, 3, 4, 5, 6, 7, 8 };
        protected static int DEFAULT_ZOOM_INDEX = 4;
        protected int currentZoomIdx;
        public event SizeChangedEventHandler ZoomLevelChanged;
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
        public ZoomableCanvas () : base()
        {
            ScaleTransform scale = new ScaleTransform();
            LayoutTransform = scale;
            ZoomLevelChanged += UpdateZoom;
            SetZoomLevelIndex(DEFAULT_ZOOM_INDEX);
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
    }
}
