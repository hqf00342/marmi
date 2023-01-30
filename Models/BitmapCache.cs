/********************************************************************************
BitmapCache

Bitmapをキャッシュするクラス
番号とタグによって管理する。
********************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Marmi.Models
{
    internal class BitmapCache
    {
        private readonly List<TagBitmap> _list = new List<TagBitmap>();

        public void ClearAll()
        {
            _list.Clear();
            GC.Collect();
        }

        public Bitmap GetBitmap(int index, CacheTag tag)
        {
            var item = _list.FirstOrDefault(x => x.Index==index && x.Tag == tag);
            if (item == null)
            {
                //見つからないときはnullを返す。
                return null;
            }
            else
            {
                item.ReferenceCount++;
                item.LastReferTime= DateTime.Now;
                return item.Bitmap;
            }
        }

        public bool Add(int index, CacheTag tag, Bitmap bitmap)
        {
            var exists = _list.Any(x => x.Index == index && x.Tag == tag);
            if (exists) return false;

            _list.Add(new TagBitmap(index,tag,bitmap));
            return true;
        }

        public void  Clear(CacheTag Tag)
        {
            _list.RemoveAll(x => x.Tag == Tag);
            GC.Collect();
        }

        public void Clear(int index)
        {
            _list.RemoveAll(x=>x.Index==index);
            GC.Collect();
        }

        private class TagBitmap
        {
            public Bitmap Bitmap { get; set; }
            public int Index { get; set; }
            public CacheTag Tag { get; set; }

            public DateTime CreateTime { get; set; }

            public int ReferenceCount { get; set; } = 0;

            public DateTime LastReferTime { get; set; }

            public TagBitmap(int index, CacheTag tag, Bitmap bmp)
            {
                Bitmap= bmp;
                Index= index;
                Tag= tag;
                CreateTime= DateTime.Now;
                LastReferTime= DateTime.Now;
            }
        }
    }

}
