using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
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
        /// �t�@�C����MD5���v�Z����B
        /// </summary>
        /// <param name="filename">�Ώۂ̃t�@�C��</param>
        /// <returns>16�i��������</returns>
        public static string CalcMd5(string filename)
        {
            //�t�@�C�����J��
            System.IO.FileStream fs = File.OpenRead(filename);

            //�n�b�V���l���v�Z����
            var md5 = System.Security.Cryptography.MD5.Create();
            byte[] bs = md5.ComputeHash(fs);

            //�t�@�C�������
            fs.Close();

            return BitConverter.ToString(bs).ToLower().Replace("-", "");
        }

        //public static string TryMakedir(string dirname)
        //{
        //    string trydir = dirname;
        //    int trynum = 0;

        //    while (!Directory.Exists(trydir))
        //        trydir = string.Format("{0}{1}", dirname, trynum++);

        //    Directory.CreateDirectory(trydir);
        //    return trydir;
        //}

        /// <summary>
        /// �ċA�I�ɏ��ɂ�W�J
        /// �P�Ƃŗ��p����ƃu���b�N����̂ŃX���b�h���ŗ��p����邱�Ƃ�z��
        /// </summary>
        /// <param name="archivename">���ɖ�</param>
        /// <param name="extractDir">�W�J��</param>
        //public static void RecurseExtractAll(string archivename, string extractDir)
        //{
        //    var sz = new SevenZipWrapper();
        //    sz.Open(archivename);
        //    sz.ExtractAll(extractDir);

        //    string[] exfiles = Directory.GetFiles(extractDir);
        //    foreach (string file in exfiles)
        //    {
        //        if (IsSupportArchiveFile(file))
        //        {
        //            string extDirName = GetUniqueDirname(file);
        //            Debug.WriteLine(file, extDirName);
        //            RecurseExtractAll(file, extDirName);
        //        }
        //    }
        //}

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

        //public static Thread AsyncRecurseExtractAll(string archivename, string extractDir)
        //{
        //    void tsAction()
        //    {
        //        RecurseExtractAll(archivename, extractDir);
        //    }

        //    Thread th = new Thread(tsAction)
        //    {
        //        Name = "RecurseExtractAll",
        //        IsBackground = true
        //    };
        //    th.Start();

        //    //���ӏ��������Ă���
        //    MakeAttentionTextfile(extractDir);

        //    //Thread��Ԃ�
        //    return th;
        //}

        public static void MakeAttentionTextfile(string extractFolder)
        {
            //�t�H���_�ɒ��ӊ��N�̃e�L�X�g�����Ă���
            try
            {
                string attentionFilename = Path.Combine(
                    extractFolder,
                    "���̃t�H���_�͏����Ă����S�ł�.txt");
                string[] texts = {
                    "���̃t�@�C����Marmi.exe�ɂ���č쐬���ꂽ�ꎞ�t�H���_�ł�",
                    "Marmi.exe���N�����Ă��Ȃ��ꍇ�A���S�ɍ폜�ł��܂�"};

                File.WriteAllLines(
                    attentionFilename,
                    texts,
                    System.Text.Encoding.UTF8);
            }
            catch
            {
                //�ʂɍ쐬�ł��Ȃ��Ă������̂ŗ�O�͂��ׂĕ��u
                //throw;
            }
        }

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

        [Conditional("DEBUG")]
        public static void WriteLine(string format, params object[] args)
        {
            Debug.WriteLine(string.Format(format, args), DateTime.Now.ToString());
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string s)
        {
            Debug.WriteLine(string.Format("{0} {1}", DateTime.Now.ToString(), s));
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