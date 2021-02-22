using System;
using System.Collections.Generic;

namespace Marmi
{
    //class CompareClass
    //{
    //}

    /********************************************************************************/
    // ソート用比較クラス
    // 自然言語ソートするための比較クラス
    /********************************************************************************/

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
                    return Win32.StrCmpLogicalW(x.filename, y.filename);

                case Target.CreateDate:
                    return DateTime.Compare(x.createDate, y.createDate);

                case Target.OriginalIndex:
                default:
                    return x.nOrgIndex - y.nOrgIndex;
            }
        }
    }
}