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
            long before = GC.GetTotalMemory(false);
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            Uty.WriteLine("ForceGC {0:N0} -> {1:N0}", before, GC.GetTotalMemory(false));
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
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "�ꎞ�t�H���_�̍폜���o���܂���ł���");
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

        /// <summary>
        /// unsafe string.ToArray()�𗘗p���Ȃ�
        /// ���I�m�ۂ���߂�
        /// ���O�̃f�B���N�g���m�F����߂�
        /// ������̒��������O�Ɋm�F
        /// unsafe�ōő�
        /// </summary>
        /// <param name="s1">��r������P</param>
        /// <param name="s2">��r������Q</param>
        /// <returns></returns>
        public static int Compare_unsafeFast(string s1, string s2)
        {
            //���l����x�ϊ��������ɂ��Ȃ���r������B
            //XP�ȍ~�̃\�[�g�����ɑΉ������͂��E�E�E

            //
            // 1�������`�F�b�N���J�n����
            //
            int p1 = 0;     // s1���w���|�C���^���Z�l
            int p2 = 0;     // s2���w���|�C���^���Z�l
            long num1 = 0;  // s1�Ɋ܂܂�鐔�l�B�傫�Ȑ��l�ɑΉ������邽��long
            long num2 = 0;  // s2�Ɋ܂܂�鐔�l�B�傫�Ȑ��l�ɑΉ������邽��long
            char c1;    //��r�����P c1 = s1[p1];
            char c2;    //��r�����Q c2 = s2[p2];
            int s1Len = s1.Length;
            int s2Len = s2.Length;

            unsafe
            {
                fixed (char* cp1 = s1)
                fixed (char* cp2 = s2)
                {
                    do
                    {
                        c1 = *(cp1 + p1);
                        c2 = *(cp2 + p2);

                        //c1��c2�̔�r���J�n����O�ɐ����������琔�l���[�h��
                        //���l���[�h�̏ꍇ�͐��l�ɕϊ����Ĕ�r
                        if (c1 >= '0' && c1 <= '9' && c2 >= '0' && c2 <= '9')
                        {
                            //s1�n��̕����𐔒lnum1�ɕϊ�
                            num1 = 0;
                            while (c1 >= '0' && c1 <= '9')
                            {
                                num1 = num1 * 10 + c1 - '0';
                                ++p1;
                                if (p1 >= s1Len)
                                    break;
                                c1 = s1[p1];
                            }

                            //s2�n��̕����𐔒lnum2�ɕϊ�
                            num2 = 0;
                            while (c2 >= '0' && c2 <= '9')
                            {
                                num2 = num2 * 10 + c2 - '0';
                                ++p2;
                                if (p2 >= s2Len)
                                    break;
                                c2 = s2[p2];
                            }

                            //���l�Ƃ��Ĕ�r
                            if (num1 != num2)
                                return (int)(num1 - num2);
                        }
                        else
                        {
                            //�P�ꕶ���Ƃ��Ĕ�r
                            if (c1 != c2)
                            {
                                if (c1 == '\\' || c1 == '/')
                                    return 1;
                                if (c2 == '\\' || c2 == '/')
                                    return -1;
                                return (int)(c1 - c2);
                            }
                            ++p1;
                            ++p2;
                        }
                    }
                    while (p1 < s1Len && p2 < s2Len);
                }//fixed
            }
            //�ǂ��炩���I�[�ɒB�����B���Ƃ͒����������B
            return s1Len - s2Len;
        }
    }
}