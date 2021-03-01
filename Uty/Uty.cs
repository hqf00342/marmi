using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;

namespace Marmi
{
    public static class Uty
    {
        /// <summary>
        /// �Ή����ɂ��ǂ������`�F�b�N����
        /// </summary>
        /// <param name="archiveName">�`�F�b�N�Ώۂ̏��Ƀt�@�C����</param>
        /// <returns>�Ή����ɂȂ�true</returns>
        public static bool IsSupportArchiveFile(string archiveName)
        {
            var ext = Path.GetExtension(archiveName).ToLower();
            switch (ext)
            {
                case ".zip":
                case ".7z":
                case ".rar":
                case ".tar":
                case ".lzh":
                case ".gz":
                case ".tgz":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// �Ή����Ă���t�@�C���`�����ǂ������`�F�b�N
        /// ���ɁA�摜�Apdf�Ƃ��ă`�F�b�N
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool IsAvailableFile(string filename)
        {
            if (IsSupportArchiveFile(filename))
                return true;
            else if (IsPictureFilename(filename))
                return true;
            return filename.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// �摜�t�@�C�����ǂ����m�F����B�g���q�����Ċm�F
        /// </summary>
        /// <param name="sz">�t�@�C����</param>
        /// <returns>�摜�t�@�C���ł����true</returns>
        public static bool IsPictureFilename(string sz)
        {
            return Regex.Match(sz, @"\.(jpeg|jpg|jpe|png|gif|bmp|ico|tif|tiff)$", RegexOptions.IgnoreCase).Success;
        }

        /// <summary>
        /// �K�x�[�W�R���N�V����������B
        /// �S�W�F�l���[�V�������{
        /// </summary>
        public static void ForceGC()
        {
            //2021�N2��26��
            // GC�������̂Ȃ��
            //85KB�ȏ��Object��LargeObjectHeap�ɂ���̂�LOH�����k����ݒ���ǂ����ł����ق�������
            //System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;

            var before = GC.GetTotalMemory(false);
            GC.Collect();                   // �S�W�F�l���[�V������GC
            GC.WaitForPendingFinalizers();  // �t�@�C�i���C�Y�I���܂ő҂�
            GC.Collect();                   // �t�@�C�i���C�Y���ꂽObj��GC

            Debug.WriteLine($"ForceGC {before:N0}byte to {GC.GetTotalMemory(false):N0}byte");
        }

        /// <summary>
        /// �t�@�C�����i�A�[�J�C�u���j���x�[�X�Ƀ��j�[�N�ȓW�J�t�H���_����T���B
        /// ���ɂƓ����ꏊ�ŒT��
        /// </summary>
        /// <param name="archiveName">���ɖ�</param>
        /// <returns></returns>
        public static string GetUniqueDirname(string archiveName)
        {
            //string ext = Path.GetExtension(archiveName);
            int trynum = 0;

            string tryBaseName = Path.Combine(
                Path.GetDirectoryName(archiveName),
                Path.GetFileNameWithoutExtension(archiveName));

            //���̂܂܂̖��O�Ŏg���邩
            if (!Directory.Exists(tryBaseName))
                return tryBaseName;

            //�_���ȏꍇ�̓��j�[�N�Ȗ��O��T�����
            while (true)
            {
                string tryName = string.Format("{0}{1}", tryBaseName, trynum);
                if (Directory.Exists(tryName))
                    trynum++;
                else
                    return tryName;
            }
        }

        /// <summary>
        /// �ꎞ�f�B���N�g���Ɂu���̃t�H���_�͏����Ă����S�ł�.txt�v�����B
        /// </summary>
        /// <param name="tempDirName">�Ώۂ̃f�B���N�g��</param>
        public static void CreateAnnotationFile(string tempDirName)
        {
            try
            {
                File.WriteAllText(
                    Path.Combine(tempDirName, "���̃t�H���_�͏����Ă����S�ł�.txt"),
                    "���̃t�@�C����Marmi.exe�ɂ���č쐬���ꂽ�ꎞ�t�H���_�ł�" + Environment.NewLine +
                    "Marmi.exe���N�����Ă��Ȃ��ꍇ�A���S�ɍ폜�ł��܂�",
                    System.Text.Encoding.UTF8);
            }
            catch
            {
                //�ʂɍ쐬�ł��Ȃ��Ă������̂ŗ�O�͂��ׂĕ��u
                //throw;
            }
        }

        /// <summary>
        /// �ꎞ�f�B���N�g�����폜����
        /// </summary>
        /// <param name="tempDirName">�ꎞ�f�B���N�g��</param>
        public static void DeleteTempDir(string tempDirName)
        {
            if (Directory.Exists(tempDirName))
            {
                try
                {
                    //�ċA�I�ɏ���
                    Directory.Delete(tempDirName, true);
                }
                catch (Exception)
                {
                    MessageBox.Show($"{tempDirName}�̍폜���o���܂���ł���", "�t�H���_�폜�G���[");
                }
            }
        }

        /// <summary>
        /// ver1.35 �S�~���֑���
        /// </summary>
        /// <param name="filename"></param>
        public static void RecycleBin(string filename)
        {
            FileSystem.DeleteFile(
               filename,
               UIOption.OnlyErrorDialogs,
               RecycleOption.SendToRecycleBin);
        }

        public static string GetUsedMemory()
        {
            long used = GC.GetTotalMemory(false);
            if (used < 1000)
                return string.Format("Used:{0}bytes", used);
            else if (used < 1000000)
                return string.Format("Used:{0}Kbytes", used / 1000);
            else if (used < 1000000000)
                return string.Format("Used:{0}Mbytes", used / 1000000);
            else
                return string.Format("Used:{0}Gbytes", used / 1000000000);
        }
    }
}