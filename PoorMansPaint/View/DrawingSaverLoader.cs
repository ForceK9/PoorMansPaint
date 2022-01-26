using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PoorMansPaint
{
    // support class for MainWindow that handles saving and loading
    public class DrawingSaverLoader
    {
        public static readonly string PaintProjectExtension = ".paint";
        public static readonly string XamlFilter = $"Paint project (*{PaintProjectExtension})|*{PaintProjectExtension}";
        public MainWindow Window { get; }
        public string Filter { 
            get
            {
                string filter = ImageEncoder.Filter;
                return filter + "|" + XamlFilter;
            } 
        }
        private string? fileName;
        private string? savedXamlString;

        public DrawingSaverLoader (MainWindow window)
        {
            Window = window;
            fileName = null;
            savedXamlString = null;
        }

        public bool CanBeSaved()
        {
            string xamlStr = XamlWriter.Save(Window.canvas.DrawingGroup);
            return savedXamlString == null && Window.canvas.IsModified() ||
                savedXamlString != null && !savedXamlString.Equals(xamlStr);
        }

        public void Save()
        {
            if (fileName == null) 
                fileName = AskForXamlName();
            if (fileName == null) return;

            savedXamlString = XamlWriter.Save(Window.canvas.DrawingGroup);
            SaveDrawingTo(fileName); 
        }
        public void ExportToNewFile(string? chosenExtension = null)
        {
            string? fileName = AskForFileName(chosenExtension); 
            if (fileName == null) return;
            SaveDrawingTo(fileName);
        }

        protected void SaveDrawingTo(string file)
        {
            if (file is null) return;

            // get the correct encoder
            string extension = file.Substring(file.LastIndexOf('.'));
            if (extension.Equals(PaintProjectExtension)) SaveToXaml(file);
            else if (ImageEncoder.CanEncodeThisFormat(extension))
            {
                BitmapSource bmp = Window.canvas.CreateBitmap();
                ImageEncoder.EncodeBitmap(bmp, file);
            }
            else
            {
                MessageBox.Show(
                    "Unsupported file format.",
                    Window.Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        protected string? AskForXamlName()
        {
            // ask for save path and image type
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = XamlFilter;
            dialog.AddExtension = true;
            dialog.FileName = "Untitled";
            //Trace.WriteLine(dialog.FilterIndex);
            if (dialog.ShowDialog() == false) return null;

            return dialog.FileName;
        }
        protected string? AskForFileName(string? chosenExtension = null)
        {
            // ask for save path and image type
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = this.Filter;
            dialog.AddExtension = true;
            dialog.FileName = "Untitled"; 
            dialog.FilterIndex = ImageEncoder.FilterIndexes.Count + 1; // .xaml
            // FilterIndex is 1-based ... why?
            if (chosenExtension != null)
            {
                if (ImageEncoder.FilterIndexes.ContainsKey(chosenExtension))
                    dialog.FilterIndex = ImageEncoder.FilterIndexes[chosenExtension];
            }
            //Trace.WriteLine(dialog.FilterIndex);
            if (dialog.ShowDialog() == false) return null;
            return dialog.FileName;
        }

        protected void SaveToXaml(string file)
        {
            if (file == null) throw new NullReferenceException(file);
            FileStream stream = new FileStream(file, FileMode.Create);
            XamlWriter.Save(Window.canvas.DrawingGroup, stream);
            stream.Close();
        }

        public void New()
        {
            if (CanBeSaved())
            {
                // user already does something
                MessageBoxResult res = MessageBox.Show(
                    "Do you want to save your changes?",
                    Window.Title,
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        Save();
                        break;
                    case MessageBoxResult.No:
                        break;
                    default: return;
                }
            }
            Window.canvas.Reset();
            fileName = null; savedXamlString = null;
        }

        public void Load()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = XamlFilter;
            if (dialog.ShowDialog() == false) return;

            if (CanBeSaved()) {
                // user already does something
                MessageBoxResult res = MessageBox.Show(
                    "Do you want to save your changes?", 
                    Window.Title, 
                    MessageBoxButton.YesNoCancel, 
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        Save();
                        break;
                    case MessageBoxResult.No:
                        break;
                    default: return;
                }
            }

            fileName = dialog.FileName;
            LoadFromXaml(fileName);
        }

        private void LoadFromXaml(string fileName)
        {
            try
            {
                FileStream stream = new FileStream(fileName, FileMode.Open);
                DrawingGroup drawing = (DrawingGroup)XamlReader.Load(stream);
                Window.canvas.Load(drawing);
                savedXamlString = XamlWriter.Save(drawing);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error opening and parsing Paint project. The file may be corrupted or in a wrong format.", Window.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
    }
}
