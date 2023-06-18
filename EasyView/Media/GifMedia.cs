using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace EasyView.Media
{
    public class GifImage
    {
        private readonly Image gifImage;
        private readonly FrameDimension dimension;
        private readonly int frameCount;
        private int currentFrame = -1;
        private bool reverse;
        private int step = 1;

        public GifImage(string path)
        {
            gifImage = Image.FromFile(path);
            dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
            frameCount = gifImage.GetFrameCount(dimension);
        }

        public bool ReverseAtEnd
        {
            get => reverse;
            set => reverse = value;
        }

        public Image GetNextFrame()
        {
            currentFrame += step;

            //if the animation reaches a boundary...
            if (currentFrame >= frameCount || currentFrame < 0)
            {
                if (reverse)
                {
                    step = -step;
                    currentFrame += step;
                }
                else
                {
                    currentFrame = 0;
                }
            }
            return GetFrame(currentFrame);
        }

        public Image GetFrame(int index)
        {
            gifImage.SelectActiveFrame(dimension, index);
            return (Image)gifImage.Clone();
        }
    }

    [Obsolete("For manual control of animation", true)]
    class GifMedia : IMedia
    {
        private GifImage gifImage;

        public void LoadAndSet(string filename, PictureBox form) { }
        public string GetInfo() { return ""; }
        public void Stop() { }

        public void Prepare(string filename)
        {
            gifImage = new GifImage(filename);
        }
        public string Play(PictureBox pixbox, ref string lastError)
        {
            string Title;

            try
            {
                pixbox.Image = gifImage.GetNextFrame();
                Title = pixbox.Image.Width + "x" + pixbox.Image.Height;

                lastError = "";
            }
            catch (Exception e)
            {
                pixbox.Image = (Image)pixbox.ErrorImage.Clone();

                Title = "** " + e.Message;
                lastError = e.Message;
            }

            return Title;
        }
    }
}
