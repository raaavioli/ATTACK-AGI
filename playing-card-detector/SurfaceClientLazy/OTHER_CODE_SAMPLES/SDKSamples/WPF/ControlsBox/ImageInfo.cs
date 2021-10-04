using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace ControlsBox
{   
    /// <summary>
    /// Simplest data type example, just shows a group name and an image
    /// </summary>
    public class ImageInfo
    {
        private readonly string fileName;
        private const string FolderPath = @"Resources\Images";
        private BitmapSource bitmap;

        public ImageInfo(string fileName, string groupName)
        {
            this.fileName = fileName;
            GroupName = groupName;
        }

        public ImageInfo(ImageInfo target)
        {
            if (target != null)
            {
                bitmap = target.bitmap;
                fileName = target.fileName;
                GroupName = target.GroupName;
            }
        }

        public BitmapSource Bitmap
        {
            get
            {
                if (bitmap == null)
                {
                    bitmap = new BitmapImage(new Uri(Path.Combine(FolderPath, fileName), UriKind.Relative));
                }

                return bitmap;
            }
        }

        public string GroupName
        {
            get;
            private set;
        }
    }
}
