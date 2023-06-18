using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace EasyView.Media
{
    interface IMediaClient
    {
        Size Size { get; }

        void SetTitle(string title);
        void SetErrorMessage(string errorMessage);
        void SetImage(Image image);
    }
}
