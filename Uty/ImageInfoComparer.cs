using System;
using System.Collections.Generic;

namespace Marmi
{
    //class CompareClass
    //{
    //}

    /********************************************************************************/
    // �\�[�g�p��r�N���X
    // ���R����\�[�g���邽�߂̔�r�N���X
    /********************************************************************************/

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