using System;
using System.Collections.Generic;
/*
�\�[�g�p��r�N���X
���R����\�[�g���邽�߂̔�r�N���X
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

        //�\�[�g�Ώۂ̃v���p�e�B
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
                    //Windows�̋@�\�𗘗p����
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