using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Marmi
{
    public delegate void SusieMenuBuilder(
        string shortName,
        string longName,
        EventHandler handler);

    public class Susie : IDisposable
    {
        private List<SusiePlugin> plugins = new List<SusiePlugin>();

        public string[] Versions { get; private set; }

        public string Filter { get; private set; }

        public Susie()
        {
            Initialize();
        }

        private void Initialize()
        {
            Dispose(true);
            plugins.Clear();
            RegistryKey regkey = Registry.CurrentUser.OpenSubKey(
                @"Software\Takechin\Susie\Plug-in", false);
            if (regkey != null)
            {
                LoadSpi((string)regkey.GetValue("Path"));
                regkey.Close();
            }
            LoadSpi(Application.StartupPath);

            Versions = plugins.ConvertAll<string>(spi => spi.Version).ToArray();

            StringBuilder s = new StringBuilder("全てのファイル(*.*)|*.*");
            plugins.ForEach(spi => s.Append('|').Append(spi.Filter));
            Filter = s.ToString();
        }

        private void LoadSpi(string folder)
        {
            try
            {
                if (string.IsNullOrEmpty(folder))
                    return;
                foreach (string s in Directory.GetFiles(folder, "*.spi"))
                {
                    SusiePlugin spi = SusiePlugin.Load(s);
                    if (spi != null && !plugins.Exists(i => i.Version == spi.Version))
                    {
                        plugins.Add(spi);
                    }
                }
            }
            catch
            {
            }
        }

        public Bitmap GetPicture(string file)
        {
            Bitmap bmp = null;
            try
            {
                byte[] buf = File.ReadAllBytes(file);
                plugins.Find(spi =>
                {
                    bmp = spi.GetPicture(file, buf);
                    return bmp != null;
                });
            }
            catch
            {
            }
            return bmp;
        }

        public List<SusieFileInfo> GetArchiveInfo(string file)
        {
            List<SusieFileInfo> sfi = null;
            plugins.Find(spi =>
            {
                sfi = spi.GetArchiveInfo(file);
                return sfi != null;
            });
            return sfi;
        }

        public byte[] GetFile(string file, SusieFileInfo sfi)
        {
            byte[] buf = null;
            plugins.Find(spi =>
            {
                buf = spi.GetFile(file, sfi);
                return buf != null;
            });
            return buf;
        }

        public byte[] GetFile(string file, int pos, int filesize)
        {
            byte[] buf = null;
            plugins.Find(spi =>
            {
                buf = spi.GetFile(file, pos, filesize);
                return buf != null;
            });
            return buf;
        }

        public void BuildConfigMenu(IntPtr parent, SusieConfigType fnc,
            SusieMenuBuilder builder)
        {
            plugins.ForEach(delegate (SusiePlugin spi)
            {
                EventHandler handler = spi.GetConfigHandler(parent, fnc);
                if (handler != null)
                {
                    builder(spi.Name, spi.Version, handler);
                }
            });
        }

        //~Susie()
        //{
        //    Dispose(false);
        //}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                plugins.ForEach(delegate (SusiePlugin spi)
                {
                    spi.Dispose();
                });
                plugins.Clear();
            }
        }

        public bool isSupportedExtentions(string extention)
        {
            foreach (var spi in plugins)
            {
                if (spi.Filter.ToLower().Contains(extention))
                    return true;
            }
            return false;
        }

        public bool isSupportPdf()
        {
            return isSupportedExtentions("pdf");
        }
    }
}