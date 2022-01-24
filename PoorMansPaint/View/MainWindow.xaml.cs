using PoorMansPaint.CustomCanvas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PoorMansPaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly int EaselToCanvasMargin = 30;
        public static Dictionary<string, DrawingTool> ToolPrototypes = null;

        public static readonly RoutedCommand SaveLoadCommand = new RoutedCommand();
        public static readonly RoutedCommand UndoCommand = new RoutedCommand();
        public static readonly RoutedCommand RedoCommand = new RoutedCommand();
        public static readonly RoutedCommand ChooseDrawingToolCommand = new RoutedCommand();
        private DrawingSaverLoader saverLoader;

        private static void CreateToolPrototypes()
        {
            ToolPrototypes = new Dictionary<string, DrawingTool>();

            // search for .dll that have ShapeDrawingTool-derived classes
            var exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            Trace.WriteLine(exeFolder);
            var dlls = new DirectoryInfo(exeFolder).GetFiles("*.dll");

            foreach (var dll in dlls)
            {
                var assembly = Assembly.LoadFile(dll.FullName);
                var types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (type.IsClass)
                    {
                        if (type.IsSubclassOf(typeof(DrawingTool)))
                        {
                            var tool = Activator.CreateInstance(type) as DrawingTool;
                            ToolPrototypes.Add(tool.MagicWord, tool);
                        }
                    }
                }
            }
        }

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
            thumbBoth.DragCompleted += OnResizeDragCompleted;

            viewer.ScrollChanged += OnScrollChanged;

            // zooming-related callbacks
            easel.MouseWheel += OnEaselMouseWheel;

            // drawing related stuff
            easel.MouseDown += OnEaselMouseDown;
            easel.MouseMove += OnEaselMouseMove;
            easel.MouseUp += OnEaselMouseUp;
            if (ToolPrototypes == null) CreateToolPrototypes();
            canvas.DrawingTool = ToolPrototypes["pencil"];
            AddShapeDrawingToolButtons();

            // rasterize command
            CommandBindings.Add(new CommandBinding(
                SaveLoadCommand,
                SaveLoadCommand_Executed,
                SaveLoadCommand_CanExecute));

            // undo redo commands
            CommandBindings.Add(new CommandBinding(
                UndoCommand,
                UndoCommand_Executed,
                UndoCommand_CanExecute));
            CommandBindings.Add(new CommandBinding(
                RedoCommand,
                RedoCommand_Executed,
                RedoCommand_CanExecute));

            // select tool command
            CommandBindings.Add(new CommandBinding(
                ChooseDrawingToolCommand,
                ChooseDrawingToolCommand_Executed,
                ChooseDrawingToolCommand_CanExecute));

            // saving and loading
            saverLoader = new DrawingSaverLoader(this);
        }

        private void AddShapeDrawingToolButtons()
        {
            foreach (var item in ToolPrototypes)
            {
                var tool = item.Value as ShapeDrawingTool;
                if (tool == null) continue;
                var button = new RadioButton()
                {
                    Content = tool.ButtonIcon,
                    ToolTip = new ToolTip() { Content = tool.ToolTipContent },
                };
                button.Command = ChooseDrawingToolCommand;
                button.CommandParameter = tool.MagicWord;
                shapesPanel.Children.Add(button);
            }
        }

        private void OnEaselMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                canvas.DrawingTool.StartDrawingAt(canvas, e.GetPosition(canvas));
            }
        }

        private void OnEaselMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //Trace.WriteLine(e.GetPosition(this));
                if (canvas.DrawingTool.IsDrawing())
                {
                    canvas.DrawingTool.ContinueDrawingAt(e.GetPosition(canvas));
                }
            }
            base.OnMouseMove(e);
        }

        private void OnEaselMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (canvas.DrawingTool.IsDrawing())
            {
                canvas.DrawingTool.FinishDrawing();
            }
        }

        private void OnEaselMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // ctrl + mouse wheel to zoom
            if (Keyboard.Modifiers != ModifierKeys.Control) return;
            Point point = e.GetPosition(easel);
            Trace.WriteLine(point);
            if (e.Delta > 0)
            {
                // mouse wheel rotate down, zoom in
                canvas.ZoomOut();
            }
            else
            {
                // mouse wheel rotate up, zoom out
                canvas.ZoomIn();
            }

            //Trace.WriteLine(e.GetPosition(easel));
            //Trace.WriteLine("------------");
            viewer.ScrollToHorizontalOffset(point.X - viewer.ViewportWidth / 2 / canvas.CurrentZoom);
            viewer.ScrollToVerticalOffset(point.Y - viewer.ViewportHeight / 2 / canvas.CurrentZoom);
            //Trace.WriteLine((point.X - viewer.ViewportWidth / 2 / canvas.CurrentZoom) + "," + (point.Y - viewer.ViewportHeight / 2 / canvas.CurrentZoom));
            //Trace.WriteLine(viewer.ContentHorizontalOffset + "," + viewer.ContentVerticalOffset);
            e.Handled = true;
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
            if (e.ViewportHeightChange != 0 || e.ViewportWidthChange != 0)
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
            //canvas.RealWidth = rectPreviewResize.Width - 2;
            canvas.Commander.Command(
                new ChangeCanvasRealSizeCommand(
                    rectPreviewResize.Width - 2,
                    canvas.RealHeight));
            rectPreviewResize.Visibility = Visibility.Hidden;
        }

        private void OnResizeVerticalDragCompleted(object sender, DragCompletedEventArgs e)
        {
            //canvas.RealHeight = rectPreviewResize.Height - 2;
            canvas.Commander.Command(
                new ChangeCanvasRealSizeCommand(
                    canvas.RealWidth, 
                    rectPreviewResize.Height - 2));
            rectPreviewResize.Visibility = Visibility.Hidden;
        }

        private void OnResizeDragCompleted(object sender, DragCompletedEventArgs e)
        {
            canvas.Commander.Command(
                new ChangeCanvasRealSizeCommand(
                    rectPreviewResize.Width - 2, 
                    rectPreviewResize.Height - 2));
            rectPreviewResize.Visibility = Visibility.Hidden;
        }

        private void SaveLoadCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            char command = ((string)e.Parameter)[0];
            if (command.Equals('s')) e.CanExecute = saverLoader.CanBeSaved();
            else e.CanExecute = true;
        }

        // https://docs.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/imaging-overview?view=netframeworkdesktop-4.8
        private void SaveLoadCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // open SaveFileDialog and save bitmap to file
            string parameter = (string)e.Parameter;
            char command = parameter[0];
            string extension = parameter.Substring(1);
            switch (command)
            {
                case 'n':
                    saverLoader.New();
                    break;
                case 'o':
                    saverLoader.Load();
                    break;
                case 's':
                    saverLoader.Save();
                    break;
                case 'e':
                    saverLoader.ExportToNewFile(extension);
                    break;
                default:
                    throw new InvalidOperationException("Command: " + command);
            }
            e.Handled = true;
        }

        private void UndoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = canvas.Commander.IsUndoingPossible();
        }

        private void UndoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            canvas.Commander.Undo();
        }
        
        private void RedoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = canvas.Commander.IsRedoingPossible();
        }

        private void RedoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            canvas.Commander.Redo();
        }

        private void ChooseDrawingToolCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ChooseDrawingToolCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string arg = (string)e.Parameter;
            if (ToolPrototypes.ContainsKey(arg))
            {
                canvas.DrawingTool = ToolPrototypes[arg];
            }
        }
    }
}
