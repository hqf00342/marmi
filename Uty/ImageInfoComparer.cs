using System;
using System.Collections.Generic;
/*
ソート用比較クラス
自然言語ソートするための比較クラス
*/
namespace Marmi
{
    public class ImageInfoComparer : IComparer<ImageInfo>
    {
        public enum Target
        {
            Filename,
            CreateDate,
            OriginalIndex
        }

        //ソート対象のプロパティ
        private readonly Target _sortTarget;

        public ImageInfoComparer(Target sortTarget)
        {
            _sortTarget = sortTarget;
        }

        public int Compare(ImageInfo x, ImageInfo y)
        {
            switch (_sortTarget)
            {
                case Target.Filename:
                    //Windowsの機能を利用する
                    return Win32.StrCmpLogicalW(x.Filename, y.Filename);

                case Target.CreateDate:
                    return DateTime.Compare(x.CreateDate, y.CreateDate);

                case Target.OriginalIndex:
                default:
                    return x.OrgIndex - y.OrgIndex;
            }
        }
    }
}