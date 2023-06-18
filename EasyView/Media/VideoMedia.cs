using System;
using System.Windows.Forms;
using System.Drawing;

namespace EasyView.Media
{
    class VideoMedia : IMedia
    {
        public void LoadAndSet(string filename, PictureBox form) { }
        public string GetInfo() { return "[VIDEO]"; }
        public void Stop() { }
    }
}
