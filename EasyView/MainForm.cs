using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using EasyView.Media;

namespace EasyView
{
    public partial class MainForm : FullScreenForm
    {
        delegate void KeyCallback();

        readonly Dictionary<Keys, KeyCallback> KeyBindings = new Dictionary<Keys, KeyCallback>();

        public string default_path = @"C:\";
        public string path;
        private IList<FileInfo> files;
        private int imageIndex = 0;
        private int ImageIndex
        {
            get => imageIndex;
            set
            {
                if (value != imageIndex || media == null)
                {
                    StopMedia();
                    imageIndex = value;
                    StartMedia();
                }
            }
        }
        private bool bReadSubDirectories = false;

        static IDictionary<string, IMedia> acceptableExtensions = new Dictionary<string, IMedia>();

        public MainForm()
        {
            InitializeComponent();
            StartMedia();

            GetFiles(default_path);

            MouseWheel += new MouseEventHandler(ThePictureBox_MouseWheel);

            KeyBindings.Clear();
            KeyBindings.Add(Keys.Space, RandomImage);
            KeyBindings.Add(Keys.Back, ReverseRandomImage);
            KeyBindings.Add(Keys.PageDown, NextImage);
            KeyBindings.Add(Keys.PageUp, PreviousImage);
            KeyBindings.Add(Keys.Escape, Escape);
            KeyBindings.Add(Keys.B, SwitchDisplayInfo);
            KeyBindings.Add(Keys.S, SwitchRecursivity);
            //KeyBindings.Add(Keys.W, SetWallPaper); // Function disabled until further notice

            acceptableExtensions.Add("jpg", new ImageMedia());
            acceptableExtensions.Add("jpeg", new ImageMedia());
            acceptableExtensions.Add("bmp", new ImageMedia());
            acceptableExtensions.Add("png", new ImageMedia());
            acceptableExtensions.Add("gif", new ImageMedia());
            acceptableExtensions.Add("tif", new ImageMedia());
            //acceptableExtensions.Add("tga", new ImageMedia());
            acceptableExtensions.Add("mpg", new AudioMedia());
            //acceptableExtensions.Add("avi", new VideoMedia());
            //acceptableExtensions.Add("wmv", new VideoMedia());
            //acceptableExtensions.Add("flv", new VideoMedia());
            acceptableExtensions.Add("mp3", new AudioMedia());
            acceptableExtensions.Add("wav", new AudioMedia());
        }

        #region Paint event
        static private string lastError = "";
        private string lastInfo = "";
        private readonly Font font = new Font("Verdana", 12, FontStyle.Bold);
        private readonly Brush brushShadow = new SolidBrush(Color.FromArgb(128, 0, 0, 0));
        private readonly Brush brushError = new SolidBrush(Color.FromArgb(64, 255, 0, 0));
        private readonly Brush brushInfo = new SolidBrush(Color.FromArgb(128, 255, 255, 0));
        //private Brush brushFrame = new SolidBrush(Color.FromArgb(64, 192, 192, 192));
        private SizeF sizeShadowOffset = new SizeF(1, 1);
        private SizeF extend = new SizeF(4, 4);
        private PointF ptZero = new PointF(0, 0);
        private bool bDisplayInfo = true;
        private IMedia media = null;

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            //if (ThePictureBox.Image == null)
            //    return;

            Graphics g = e.Graphics;

            PointF ptError = new PointF(10, 10);
            g.DrawString(lastError, font, brushShadow, ptError + sizeShadowOffset);
            g.DrawString(lastError, font, brushError, ptError);

            if (bDisplayInfo)
            {
                SizeF size = g.MeasureString(lastInfo, font);
                PointF ptInfo = new PointF(10, ThePictureBox.Height - 0 - size.Height);

                Matrix oldTransform = g.Transform;

                RectangleF rcFrame = new RectangleF(ptZero - extend, size + extend);
                Brush brushFrame =
                    new LinearGradientBrush(rcFrame, Color.FromArgb(64, 128, 128, 255), Color.FromArgb(128, 128, 128, 192), LinearGradientMode.Vertical);
                Brush brushReflectedFrame =
                    new LinearGradientBrush(rcFrame, Color.FromArgb(32, 128, 128, 255), Color.FromArgb(64, 128, 128, 192), LinearGradientMode.Vertical);
                Brush brushRefletedText = new LinearGradientBrush(rcFrame, Color.FromArgb(64, 255, 255, 0), Color.FromArgb(128, 255, 255, 0), LinearGradientMode.Vertical);

                // Normal Info
                g.ResetTransform();
                g.TranslateTransform(0, -size.Height + 1, MatrixOrder.Append);
                g.TranslateTransform(ptInfo.X, ptInfo.Y, MatrixOrder.Append);

                g.FillRectangle(brushFrame, rcFrame);
                //g.DrawRectangle(new Pen( Color.FromArgb( 128, 255, 255, 255 ) ), new Rectangle((int)rcFrame.X, (int)rcFrame.Y, (int)rcFrame.Width, (int)rcFrame.Height));
                g.DrawString(lastInfo, font, brushShadow, ptZero + sizeShadowOffset);
                g.DrawString(lastInfo, font, brushInfo, ptZero);

                // Reflected Info
                g.ResetTransform();
                g.TranslateTransform(0, -size.Height + 1, MatrixOrder.Append);
                g.ScaleTransform(1, -0.75f, MatrixOrder.Append);
                g.TranslateTransform(ptInfo.X, ptInfo.Y, MatrixOrder.Append);

                g.FillRectangle(brushReflectedFrame, rcFrame);
                //g.DrawRectangle(new Pen(Color.FromArgb(32, 255, 255, 255)), new Rectangle((int)rcFrame.X, (int)rcFrame.Y, (int)rcFrame.Width, (int)rcFrame.Height));
                g.DrawString(lastInfo, font, brushRefletedText, ptZero);

                g.Transform = oldTransform;
            }
        }
        #endregion

        #region Key events
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (KeyBindings.ContainsKey(e.KeyCode))
                KeyBindings[e.KeyCode]();
        }
        #endregion

        #region Mouse events
        private void ThePictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
                Switch();
        }

        private void ThePictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
                NextImage();
            else
                PreviousImage();
        }
        #endregion

        #region Drag'N'Drop events
        private void ThePictureBox_DragEnter(object sender, DragEventArgs e)
        {
            //string[] formats = e.Data.GetFormats();
            if (e.Data.GetDataPresent("FileDrop"))
            {
                string[] filename = (string[])e.Data.GetData("FileDrop");

                if (IsAcceptableExtension(Path.GetExtension(filename[0])))
                    e.Effect = DragDropEffects.Link;
                else if (Directory.Exists(filename[0]))
                    e.Effect = DragDropEffects.Link;
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FileDrop"))
            {
                string[] filename = (string[])e.Data.GetData("FileDrop");

                AcceptFile(filename[0]);
            }
        }
        internal void AcceptFile(string FullFileName)
        {
            string DirName, FileName;
            if (Directory.Exists(FullFileName))
            {
                DirName = FullFileName;
                FileName = null;
            }
            else
            {
                DirName = Path.GetDirectoryName(FullFileName);
                FileName = Path.GetFileName(FullFileName);
            }

            GetFiles(DirName);
            int index = 0;
            if (!string.IsNullOrEmpty(FileName))
                foreach (FileInfo fi in files)
                {
                    if (fi.Name.ToLower() == FileName.ToLower())
                        break;
                    ++index;
                }
            ImageIndex = index;
            Activate();
        }
        #endregion

        #region Espace function
        internal void Escape()
        {
            if (IsMaximized)
                Switch();
            else
                Close();
        }
        #endregion

        #region Wallpaper function
        internal void SetWallPaper()
        {
            WallpaperHelper.Set(files[ImageIndex].FullName);
            WallpaperHelper.SetStyle(Style.Stretched);
        }
        #endregion

        #region Navigation (Next, Previous, Random, ...)
        readonly MyNoise rnd = new MyNoise(0);

        private void NextImage()
        {
            if (ImageIndex >= files.Count - 1)
                ImageIndex = 0;
            else
                ++ImageIndex;
        }

        private void RandomImage()
        {
            if (files.Count >= 1)
            {
                ImageIndex = rnd.Next(files.Count - 1);
            }
        }

        private void ReverseRandomImage()
        {
            if (files.Count >= 1)
            {
                ImageIndex = rnd.Previous(files.Count - 1);
            }
        }

        private void PreviousImage()
        {
            if (ImageIndex <= 0)
                ImageIndex = files.Count - 1;
            else
                --ImageIndex;
        }
        #endregion

        internal bool IsAcceptableExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension) || !extension.StartsWith("."))
                return false;
            return acceptableExtensions.Keys.Contains(extension.Substring(1).ToLower());
        }

        internal void GetFiles(string path)
        {
            this.path = path;
            DirectoryInfo dirinfo = new DirectoryInfo(path);
            FileInfo[] allfiles = dirinfo.GetFiles("*.*", bReadSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            files = new List<FileInfo>();
            foreach (FileInfo file in allfiles)
            {
                if (IsAcceptableExtension(file.Extension))
                    files.Add(file);
            }
        }

        private void StartMedia()
        {
            lastError = "";

            string Title;
            if (files == null || files.Count == 0)
                Title = "[0/0] ** No Files **";
            else
            {
                try
                {
                    media = acceptableExtensions[files[ImageIndex].Extension.Substring(1).ToLower()];
                    media.LoadAndSet(files[ImageIndex].FullName, ThePictureBox);
                }
                catch (Exception e)
                {
                    lastError = e.Message;
                    ThePictureBox.Image = (Image)ThePictureBox.ErrorImage.Clone();
                }

                ImageIndex %= files.Count;
                Title = "[" + (ImageIndex + 1) + "/" + files.Count + "] " + files[ImageIndex] + " ";
                lastInfo = Title;

                Title += media != null ? media.GetInfo() : "";

                ThePictureBox.Refresh();
            }
            Text = Title;
        }

        private void StopMedia()
        {
            if (media != null)
            {
                media.Stop();
                media = null;
            }
        }

        #region Recursivity switch function
        private void SwitchRecursivity()
        {
            bReadSubDirectories = !bReadSubDirectories;
            GetFiles(path);
            // todo: recompute index
            StartMedia();
            Invalidate(true);
        }
        #endregion

        #region Infobar switch function
        private void SwitchDisplayInfo()
        {
            bDisplayInfo = !bDisplayInfo;
            Invalidate(true);
        }
        #endregion

    }
}