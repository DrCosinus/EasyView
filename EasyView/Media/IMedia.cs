using System.Windows.Forms;

namespace EasyView.Media
{
    interface IMedia
    {
        void LoadAndSet(string filename, PictureBox form);
        void Stop();
        string GetInfo();
    }
}
