using System;
using System.Drawing;
using System.Windows.Forms;

namespace EasyView.Media
{
    class ImageMedia : IMedia
    {
        private Image image = null;

        public void LoadAndSet(string filename, PictureBox pixbox)
        {
            try
            {
                image = Image.FromFile(filename);
                pixbox.Image = image;
            }
            catch(Exception /*e*/)
            {
                image = null;
                pixbox.Image = pixbox.ErrorImage;
            }
        }

        public string GetInfo()
        {
            return image != null ? $"{image.Width} x {image.Height}" : "**NULL**";
        }

        public void Stop() { }
    }
}
