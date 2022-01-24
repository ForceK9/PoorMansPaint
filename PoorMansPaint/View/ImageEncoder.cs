using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using System.Data;

namespace PoorMansPaint
{
    public class ImageEncoder
    {
        public delegate BitmapEncoder EncoderFactory();
        public static readonly Dictionary<string, int> FilterIndexes = new Dictionary<string, int>();
        public static readonly Dictionary<string, EncoderFactory> Prototypes = new Dictionary<string, EncoderFactory>();
        public static readonly string Filter;

        static ImageEncoder()
        {
            StringBuilder builder = new StringBuilder();
            string ext;
            int filterIndex = 1;

            // .png
            //builder.Append("|");
            //++filterIndex;
            ext = ".png";
            FilterIndexes.Add(ext, filterIndex);
            Prototypes.Add(ext, () => new PngBitmapEncoder());
            builder.Append("PNG (*.png)|*.png");

            // insert other file type here
            builder.Append("|"); 
            ++filterIndex;
            ext = ".jpeg";
            FilterIndexes.Add(ext, filterIndex);
            Prototypes.Add(ext, () => new JpegBitmapEncoder());
            builder.Append("JPEG (*.jpeg)|*.jpeg");

            builder.Append("|"); 
            ++filterIndex;
            ext = ".bmp";
            FilterIndexes.Add(ext, filterIndex);
            Prototypes.Add(ext, () => new BmpBitmapEncoder());
            builder.Append("BMP (*.bmp)|*.bmp");

            builder.Append("|");
            ++filterIndex;
            ext = ".gif";
            FilterIndexes.Add(ext, filterIndex);
            Prototypes.Add(ext, () => new GifBitmapEncoder());
            builder.Append("GIF (*.gif)|*.gif");

            // this one have multiple aliases
            builder.Append("|");
            ++filterIndex;
            ext = ".tif";
            FilterIndexes.Add(ext, filterIndex);
            Prototypes.Add(ext, () => new TiffBitmapEncoder());
            ext = ".tiff";
            FilterIndexes.Add(ext, filterIndex);
            Prototypes.Add(ext, () => new TiffBitmapEncoder());
            builder.Append("TIFF (*tif, *.tiff)|*tif;*.tiff");

            // finalize
            Filter = builder.ToString();
        }

        public static bool CanEncodeThisFormat(string extension)
        {
            return Prototypes.ContainsKey(extension);
        }

        public static void EncodeBitmap (BitmapSource bmp, string fileName)
        {
            // get correct encoder from the list of prototypes
            string extension = fileName.Substring(fileName.LastIndexOf('.'));
            if (!Prototypes.ContainsKey(extension))
                throw new NullReferenceException(extension);
            BitmapEncoder encoder = Prototypes[extension].Invoke();

            // use encoder to save bitmap to image
            FileStream stream = new FileStream(fileName, FileMode.Create);
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            encoder.Save(stream);
            stream.Close();
        }
    }
}
