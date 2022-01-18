﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace PoorMansPaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly int EaselToCanvasMargin = 30;
        public static readonly RoutedCommand ResizeHorizontalCommand = new RoutedCommand();
        
        public MainWindow()
        {
            InitializeComponent();

            // resizing-related callbacks
            canvas.SizeChanged += OnCanvasChanged;
            canvas.ZoomLevelChanged += OnCanvasChanged;
            thumbHorizontal.DragStarted += OnResizeThumbDragStarted;
            thumbVertical.DragStarted += OnResizeThumbDragStarted;
            thumbBoth.DragStarted += OnResizeThumbDragStarted;

            thumbHorizontal.DragDelta += OnResizeThumbHorizontalDragDelta;
            thumbVertical.DragDelta += OnResizeThumbVerticalDragDelta;
            thumbBoth.DragDelta += OnResizeThumbHorizontalDragDelta;
            thumbBoth.DragDelta += OnResizeThumbVerticalDragDelta;

            thumbHorizontal.DragCompleted += OnResizeHorizontalDragCompleted;
            thumbVertical.DragCompleted += OnResizeVerticalDragCompleted;
            thumbBoth.DragCompleted += OnResizeHorizontalDragCompleted;
            thumbBoth.DragCompleted += OnResizeVerticalDragCompleted;

            viewer.ScrollChanged += OnScrollChanged;

            // zooming-related callbacks
            easel.MouseWheel += OnEaselMouseWheel;
        }

        private void OnEaselMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // ctrl + mouse wheel to zoom
            if (Keyboard.Modifiers != ModifierKeys.Control) return;
            if (e.Delta > 0)
            {
                // mouse wheel rotate down, zoom in
                canvas.ZoomOut();
            }
            else
            {
                // mouse wheel rotate up, zoom in
                canvas.ZoomIn();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RecalculateEaselSize();
            RelocateResizeThumbs();
        }

        private void RecalculateEaselSize()
        {
            easel.Width = Math.Max(canvas.RealWidth + 2 * EaselToCanvasMargin, viewer.ViewportWidth);
            easel.Height = Math.Max(canvas.RealHeight + 2 * EaselToCanvasMargin, viewer.ViewportHeight);
        }

        private void RelocateResizeThumbs()
        {
            int borderEdge = (int) FindResource("BorderEdge");
            int smallSquareEdge = (int)FindResource("SmallSquareEdge");
            int stroke = (int)FindResource("SmallSquareStroke");
            double centerOffset = borderEdge / 2;
            double offset = centerOffset - (smallSquareEdge/2 + stroke) + 1;
            double middleX = EaselToCanvasMargin + canvas.RealWidth / 2 - centerOffset;
            double rightX = EaselToCanvasMargin + canvas.RealWidth - offset;
            double middleY = EaselToCanvasMargin + canvas.RealHeight / 2 - centerOffset;
            double bottomY = EaselToCanvasMargin + canvas.RealHeight - offset;
            
            thumbHorizontal.SetValue(Canvas.LeftProperty, rightX);
            thumbHorizontal.SetValue(Canvas.TopProperty, middleY);

            thumbVertical.SetValue(Canvas.LeftProperty, middleX);
            thumbVertical.SetValue(Canvas.TopProperty, bottomY);

            thumbBoth.SetValue(Canvas.LeftProperty, rightX);
            thumbBoth.SetValue(Canvas.TopProperty, bottomY);
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            RecalculateEaselSize();
        }

        private void OnCanvasChanged(object sender, SizeChangedEventArgs e)
        {
            // place the 3 thumbs in their correct positions
            RelocateResizeThumbs();

            // resize the easel to keep it a certain margin away from the canvas
            RecalculateEaselSize();
        }

        private void OnResizeThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            rectPreviewResize.Visibility = Visibility.Visible;
            rectPreviewResize.SetValue(Canvas.LeftProperty,
                (double)canvas.GetValue(Canvas.LeftProperty) - 1);
            rectPreviewResize.SetValue(Canvas.TopProperty,
                (double)canvas.GetValue(Canvas.TopProperty) - 1);
            rectPreviewResize.Width = canvas.RealWidth + 2;
            rectPreviewResize.Height = canvas.RealHeight + 2;
        }

        private void OnResizeThumbHorizontalDragDelta(object sender, DragDeltaEventArgs e)
        {
            double newWidth = canvas.RealWidth + e.HorizontalChange;
            if (newWidth >= 0)
            {
                rectPreviewResize.Width = newWidth;
            }
            else
            {
                rectPreviewResize.Width = 0;
            }
            rectPreviewResize.Width += 2;
        }

        private void OnResizeThumbVerticalDragDelta(object sender, DragDeltaEventArgs e)
        {
            double newHeight = canvas.RealHeight + e.VerticalChange;
            if (newHeight >= 0)
            {
                rectPreviewResize.Height = newHeight;
            }
            else
            {
                rectPreviewResize.Height = 0;
            }
            rectPreviewResize.Height += 2;
        }

        private void OnResizeHorizontalDragCompleted(object sender, DragCompletedEventArgs e)
        {
            canvas.RealWidth = rectPreviewResize.Width - 2;
            rectPreviewResize.Visibility = Visibility.Hidden;
        }

        private void OnResizeVerticalDragCompleted(object sender, DragCompletedEventArgs e)
        {
            canvas.RealHeight = rectPreviewResize.Height - 2;
            rectPreviewResize.Visibility = Visibility.Hidden;
        }
    }
}
