using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Gw2_Launchbuddy.Helpers
{
    internal class FileDialog
    {
        public FileDialog()
        {
            fileDialog = new OpenFileDialog();
            fileDialog.FileOk += delegate (object sender, CancelEventArgs e)
            {
                if (String.IsNullOrWhiteSpace(((OpenFileDialog)sender).FileName))
                {
                    MessageBox.Show("No file selected", "Error");
                    e.Cancel = true;
                }
            };
        }

        public string FileName { get; private set; }

        public DialogResult Result { get; private set; }

        private ExistingEvents events { get; set; }

        private OpenFileDialog fileDialog { get; set; }

        private bool mediaLoaded { get; set; }

        [Flags]
        public enum MediaTypes
        {
            None = 0,
            Video = 1 << 0,
            Audio = 1 << 1,
            Image = 1 << 2,
        }
        [Flags]
        private enum ExistingEvents
        {
            None = 0,
            Media = 1 << 0,
            Extension = 1 << 1,
        }

        public FileDialog CheckForMedia(MediaTypes type, MediaElement mediaElement)
        {
            if (events.HasFlag(ExistingEvents.Media))
                throw new Exception("Media can only be checked once.");
            events |= ExistingEvents.Media;

            /*
            if (events.HasFlag(MediaTypes.Video) || events.HasFlag(MediaTypes.Audio))
                throw new Exception("Wrong overload used. MediaElement required for Video or Audio.");
                */

            fileDialog.FileOk += delegate (object sender, CancelEventArgs e)
            {
                try
                {
                    LoadMedia(mediaElement, ((OpenFileDialog)sender).FileName, () =>
                    {
                        bool test = false;
                        switch (type)
                        {
                            case MediaTypes.Audio:
                                test = mediaElement.HasAudio;
                                break;
                            case MediaTypes.Video:
                                test = mediaElement.HasVideo;
                                break;
                            case MediaTypes.Audio | MediaTypes.Video:
                                test = mediaElement.HasVideo && mediaElement.HasAudio;
                                break;
                        }


                        if (!test)
                        {
                            MessageBox.Show("Selected file contains incompatible " + type.ToString().Replace(", ", " or ") + " data.", "Error");
                            e.Cancel = true;
                        }
                        else
                            callback(this);
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            };
            return this;
        }

        public FileDialog CheckForMedia(MediaTypes type)
        {
            if (events.HasFlag(ExistingEvents.Media))
                throw new Exception("Media can only be checked once.");
            events |= ExistingEvents.Media;

            if ((type & (MediaTypes.Video | MediaTypes.Audio)) != MediaTypes.None)
                throw new Exception("Wrong overload used. MediaElement required for Video or Audio.");


            fileDialog.FileOk += delegate (object sender, CancelEventArgs e)
            {
                System.Drawing.Image imgInput = null;
                System.Drawing.Graphics gInput = null;
                try
                {
                    imgInput = System.Drawing.Image.FromFile(((OpenFileDialog)sender).FileName);
                    gInput = System.Drawing.Graphics.FromImage(imgInput);
                    System.Drawing.Imaging.ImageFormat thisFormat = imgInput.RawFormat;
                    callback(this);
                }
                catch (Exception)
                {
                    MessageBox.Show("Selected file contains incompatible " + type.ToString().Replace(", ", " or ") + " data.", "Error");
                    e.Cancel = true;
                }
                finally
                {
                    imgInput.Dispose();
                    gInput.Dispose();
                }
            };
            return this;
        }

        public FileDialog DefaultExt(string DefaultExt)
        {
            fileDialog.DefaultExt = DefaultExt;
            return this;
        }

        public FileDialog InitialDirectory(string InitialDirectory)
        {
            fileDialog.InitialDirectory = InitialDirectory;
            return this;
        }

        public FileDialog EnforceExt(params string[] Ext)
        {
            if (events.HasFlag(ExistingEvents.Extension))
                throw new Exception("Enforced extensions can only be set once.");
            events |= ExistingEvents.Extension;

            fileDialog.FileOk += delegate (object sender, CancelEventArgs e)
            {
                bool test = false;
                foreach (string ext in Ext)
                    test = test ? test : ((OpenFileDialog)sender).FileName.EndsWith(ext);
                if (!test)
                {
                    MessageBox.Show("Please select a filename with " + (Ext.Count() == 1 ? "the extension " : "one of the following extensions: ") + String.Join(" ", Ext), "Incorrect File Extension");
                    e.Cancel = true;
                }
            };
            return this;
        }

        public FileDialog Filter(string Filter)
        {
            fileDialog.Filter = Filter;
            return this;
        }

        public FileDialog Multiselect(bool Multiselect = true)
        {
            fileDialog.Multiselect = Multiselect;
            return this;
        }
        public void ShowDialog(Action<FileDialog> action)
        {
            callback = action;
            Result = fileDialog.ShowDialog();
            FileName = fileDialog.FileName;
            if (!events.HasFlag(ExistingEvents.Media))
                callback(this);
        }

        private Action<FileDialog> callback { get; set; }

        private void LoadMedia(System.Windows.Controls.MediaElement tester, string source, Action action)
        {
            onMediaLoad = action;
            tester.MediaOpened += MediaWait;
            tester.MediaFailed += MediaWait;
            tester.Source = new Uri(source, UriKind.RelativeOrAbsolute);
        }

        private Action onMediaLoad { get; set; }

        private void MediaWait(object sender, EventArgs e)
        {
            ((MediaElement)sender).MediaOpened -= MediaWait;
            ((MediaElement)sender).MediaFailed -= MediaWait;
            mediaLoaded = true;
            onMediaLoad();
        }

        private void TryCallback()
        {

        }
    }
}