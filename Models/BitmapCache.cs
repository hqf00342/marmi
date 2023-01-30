/********************************************************************************
BitmapCache

Bitmapをキャッシュするクラス
番号とタグによって管理する。
********************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Marmi.Models
{
    internal class BitmapCache
    {
        private readonly List<TagBitmap> _list = new List<TagBitmap>();

        public void ClearAll()
        {
            _list.Clear();
        }

        public Bitmap GetBitmap(int index, CacheTag tag)
        {
            var item = _list.FirstOrDefault(x => x.Index == index && x.Tag == tag);
            if (item == null)
            {
                //見つからないときはnullを返す。
                return null;
            }
            else
            {
                item.ReferenceCount++;
                item.LastReferTime = DateTime.Now;
                return item.Bitmap;
            }
        }

        public bool Add(int index, CacheTag tag, Bitmap bitmap)
        {
            var exists = _list.Any(x => x.Index == index && x.Tag == tag);
            if (exists) return false;

            _list.Add(new TagBitmap(index, tag, bitmap));
            return true;
        }

        public void Clear(CacheTag Tag)
        {
            _list.RemoveAll(x => x.Tag == Tag);
        }

        public void Clear(int index)
        {
            _list.RemoveAll(x => x.Index == index);
        }

        private class TagBitmap
        {
            private byte[] _rawImage = null;

            public Bitmap Bitmap { get; set; }

            //public Bitmap Bitmap
            //{
            //    get=>GetBitmapFromRawImage();
            //    set=>Store(value);
            //}

            public int Index { get; set; }

            public CacheTag Tag { get; set; }

            public DateTime CreateTime { get; set; }

            public int ReferenceCount { get; set; } = 0;

            public DateTime LastReferTime { get; set; }

            public TagBitmap(int index, CacheTag tag, Bitmap bmp)
            {
                Bitmap = bmp;
                Index = index;
                Tag = tag;
                CreateTime = DateTime.Now;
                LastReferTime = DateTime.Now;
            }

            private void Store(Bitmap bmp)
            {
                if (bmp != null)
                {
                    //pngで保存
                    var ms = new MemoryStream();
                    bmp.Save(ms, ImageFormat.Png);
                    ms.Close();
                    _rawImage = ms.ToArray();
                }
            }

            private Bitmap GetBitmapFromRawImage()
            {
                if (_rawImage != null)
                {
                    try
                    {
                        var ic = new ImageConverter();
                        return ic.ConvertFrom(_rawImage) as Bitmap;
                    }
                    catch
                    {
                    }
                }
                return null;
            }
        }
    }
}