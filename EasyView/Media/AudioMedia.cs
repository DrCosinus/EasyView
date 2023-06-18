using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Media;

namespace EasyView.Media
{
    class AudioMedia : IMedia
    {
        MediaPlayer player;
        public void LoadAndSet(string filename, PictureBox pixbox)
        {
            pixbox.Image = null;
            player = new MediaPlayer();
            player.Open(new Uri(filename));
            player.Play();
        }

        public string GetInfo() { return $"[AUDIO] {player.NaturalDuration}"; }

        public void Stop() 
        { 
            player.Stop();
            player = null;
        }
    }
}
