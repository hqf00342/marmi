#define X86ONLY		//x86

using SevenZip;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using ArchivedFiles = System.Collections.ObjectModel.ReadOnlyCollection<SevenZip.ArchiveFileInfo>;

namespace Marmi
{
    public class SevenZipWrapper : I7zipWrapper //: IDisposable
    {
        private string m_filename;
        private bool m_isOpen = false;          //�p�X���[�h�F�؊܂߃I�[�v�����Ă��邩
        private SevenZipExtractor m_7z;

        //�p�X���[�h
        //���̃N���X�͂P���������킯�ł͂Ȃ��̂�static�Ŏ����Ă���
        //���������Ƃ��̖��ݒ�Bstring.empty�̂Ƃ��̓p�X���[�h����
        private static string m_password;

        //�S�W�J���̃C�x���g�F�P�t�@�C������
        public event Action<string> ExtractEventHandler;

        //�S�W�J���̃C�x���g�F�S�t�@�C������
        public event EventHandler ExtractAllEndEventHandler;

        public bool IsCancelExtraction { get; set; }

        public bool IsOpen => m_isOpen;

        /// <summary>
        ///���ɂɂ���A�C�e������Ԃ�
        ///�f�B���N�g����1�t�@�C���Ƃ��ĕԂ��̂Œ���
        /// </summary>
        public int ItemCount => m_7z.ArchiveFileData.Count;

        /// <summary>
        /// ���ɂ��\���b�h���ǂ����Ԃ�
        /// </summary>
        public bool IsSolid => m_7z.IsSolid;

        public ArchivedFiles Items => m_7z.ArchiveFileData;

        public string Filename => m_filename;

        public SevenZipWrapper()
        {
            m_filename = null;
            m_7z = null;
            m_isOpen = false;

            //�����I��32bit���C�u������
            //SevenZipExtractor.SetLibraryPath(Path.Combine(Application.StartupPath, "7z.dll"));
            //SevenZipExtractor.SetLibraryPath(Path.Combine(Application.StartupPath, "7z64.dll"));
            SetLibraryPath();

            //ver1.10 �L�����Z�������̂��߂̏�����
            IsCancelExtraction = false;
        }

        ~SevenZipWrapper()
        {
            Close();
        }

        private void SetLibraryPath()
        {
            var libname = Environment.Is64BitOperatingSystem ? "7z64.dll" : "7z.dll";
            SevenZipExtractor.SetLibraryPath(Path.Combine(Application.StartupPath, libname));
        }

        /// <summary>
        /// �p�X���[�h���N���A����B
        /// �V�������ɂ�ݒ肷��Ƃ��i��Start()�j�̂Ƃ��̂�
        /// �Ăяo����邱�Ƃ�z��B
        /// </summary>
        public static void ClearPassword()
        {
            m_password = string.Empty;
        }

        //private void setLibrary32or64()
        //{
        //    //���C�u�����p�X�̐ݒ�
        //    if (IntPtr.Size == 4)
        //    {
        //        //32bit process
        //        SevenZipExtractor.SetLibraryPath(
        //            Path.Combine(Application.StartupPath, "7z.dll"));
        //    }
        //    else if (IntPtr.Size == 8)
        //    {
        //        //64bit process
        //        SevenZipExtractor.SetLibraryPath(
        //            Path.Combine(Application.StartupPath, "7z64.dll"));
        //    }
        //}

        public bool Open(string ArchiveName)
        {
            //�����t�@�C����Open�ς݂Ȃ�Ȃɂ����Ȃ�
            if (m_isOpen && ArchiveName == m_filename)
                return true;

            //�t�@�C���̑��݂��m�F
            if (!File.Exists(ArchiveName))
                return false;

            //�I�[�v�����Ă�������Ă���
            if (m_isOpen)
            {
                Close();
            }

            m_isOpen = false;
            m_filename = ArchiveName;
            m_7z?.Dispose();

            //ver1.05 7zip�t�@�C���I�[�v�� static password�Ή���
            try
            {
                if (!string.IsNullOrEmpty(m_password))
                {
                    m_7z = new SevenZipExtractor(ArchiveName, m_password);

                    //���Ƀ`�F�b�N�B���Ă��Ȃ���
                    if (m_7z.FilesCount == 0)
                    {
                        m_7z.Dispose();
                        m_7z = null;
                        m_isOpen = false;
                        return false;
                    }

                    if (TryExtract())
                        return true;
                    //password check�Ɏ��s����ƃ_�C�A���O�őΉ�
                }
                else
                {
                    //�ʏ�ʂ��Open������
                    m_7z = new SevenZipExtractor(ArchiveName);
                }

                //�p�X���[�h�`�F�b�N
                //ver1.31 �f�B���N�g���łȂ��A�C�e����T��
                int testitem = GetFirstNonDirIndex();
                if (testitem == -1)
                    return false;
                //�p�X���[�h�`�F�b�N
                if (!m_7z.ArchiveFileData[testitem].Encrypted)
                {
                    //password�s�v�Ȃ̂�Open����
                    m_isOpen = true;
                    return true;
                }
                else
                {
                    //�p�X���[�h�F�؃t�H�[�����o��3��`�������W����
                    return TryPasswordCheck(ArchiveName);
                }//if(Encrypted)
            }
            catch (SevenZipArchiveException)
            {
                //�����炭7z�̃p�X���[�h�Ɉ�����������
                return TryPasswordCheck(ArchiveName);
            }
            finally
            {
                //�C�x���g�n���h���[�̓o�^
                if (m_7z != null)
                {
                    m_7z.ExtractionFinished += ExtractionFinished;
                    m_7z.FileExtractionFinished += FileExtractionFinished;
                }
            }
        }

        private bool TryPasswordCheck(string ArchiveName)
        {
            //�p�X���[�h�F�؃t�H�[�����o��3��`�������W����
            int passwordRetryRemain = 3;
            using (FormPassword pf = new FormPassword())
            {
                //���g���C�񐔕������[�v
                while (passwordRetryRemain > 0)
                {
                    if (pf.ShowDialog() == DialogResult.OK)
                    {
                        //�����Stream���������A�p�X���[�h�t��Stream���Đ���
                        m_7z.Dispose();
                        m_7z = new SevenZipExtractor(ArchiveName, pf.PasswordText);
                    }
                    else
                    {
                        //Cancel���ꂽ�̂Ŗ߂�
                        passwordRetryRemain = 0;
                        break;
                    }

                    //�p�X���[�h�`�F�b�N
                    if (TryExtract())
                    {
                        m_isOpen = true;    //�p�X���[�h�F�؍ς�
                        m_password = pf.PasswordText;
                        //SevenZipWrapper.m_password = pf.PasswordText;
                        //m_password = m_sevenzipExtractor.Password;
                        return true;
                    }
                    else
                    {
                        passwordRetryRemain--;
                        pf.PasswordText = "";   //password dialog��TextBox���N���A
                    }
                }

                //�p�X���[�h�F��3�񎸔s
                m_isOpen = false;   //�p�X���[�h�F�؎��s�ɂ��N���[�Y���
                return false;
            }//using
        }

        /// <summary>
        /// �p�X���[�h���K�v���ǂ����`�F�b�N���邽�߂�1�t�@�C���W�J����
        /// </summary>
        /// <returns>�W�J���s�����ꍇ���p�X���[�h��������false</returns>
        private bool TryExtract()
        {
            //�����ɓW�J�����A�p�X���[�h�F�؎��s�Ȃ��O�𔭐�������
            try
            {
                int testitem = GetFirstNonDirIndex();
                if (testitem == -1)
                    return false;

                using (var ms = new MemoryStream())
                {
                    m_7z.ExtractFile(testitem, ms);
                }
                //�����ɗ����Ƃ��͐���
                return true;
            }
            catch
            {
                //SevenZipArchiveException  : 7z���ɂ̃p�X���[�h���������Ȃ�����
                //ExtractionFailedException : zip���ɂ̃p�X���[�h���������Ȃ�����
                return false;
            }
        }

        /// <summary>
        /// ���ɓ��̃f�B���N�g���ł͂Ȃ��A�C�e����T��.�p�X���[�h�`�F�b�N�p�ɗ��p
        /// </summary>
        /// <returns>���ɓ��̃C���f�b�N�X�B������Ȃ��ꍇ��-1</returns>
        private int GetFirstNonDirIndex()
        {
            int index = 0;
            while (m_7z.ArchiveFileData[index].IsDirectory)
            {
                index++;
                if (index >= m_7z.ArchiveFileData.Count)
                    return -1;
            }
            return index;
        }

        /// <summary>
        /// ���ɂ����B�C�x���g�̉������s��
        /// </summary>
        public void Close()
        {
            if (m_7z != null)
            {
                m_7z.ExtractionFinished -= ExtractionFinished;
                m_7z.FileExtractionFinished -= FileExtractionFinished;
                m_7z.Dispose();
                m_7z = null;
            }

            m_filename = null;
            m_isOpen = false;
            //m_password = string.Empty;
        }

        public Stream GetStream(string filename)
        {
            try
            {
                if (m_7z != null)
                {
                    //UNDONE:2011�N7��30�� �T���l�C������nullrefer�̌�����
                    //UNDONE:�X���b�h�Z�[�t�łȂ��Ɠ���������Ď��{�B
                    //lock (locker)
                    //{
                    MemoryStream st = new MemoryStream();
                    m_7z.ExtractFile(filename, st);
                    st.Seek(0, SeekOrigin.Begin);
                    return st;
                    //}
                }
                else
                {
                    return null;
                }
            }
            catch (ExtractionFailedException e)
            {
                MessageBox.Show(
                    "���ɂ����Ă��܂�\n" + e.Message,
                    "�t�@�C���W�J�G���[");
                return null;
            }
        }

        public void ExtractAll(string ExtractDir)
        {
            if (m_7z == null)
                return;

            ////�C�x���g�n���h���[�̓o�^
            //add7zEvent();

            //�f�B���N�g����������ΐ���
            if (!Directory.Exists(ExtractDir))
                Directory.CreateDirectory(ExtractDir);

            //�񓯊��W�J�J�n�B
            try
            {
                m_7z.ExtractArchive(ExtractDir);
            }
            catch (SevenZipException e)
            {
                Debug.WriteLine($"7z Error : {e.GetType().Name} {e.Message}");
            }
            catch (PathTooLongException e)
            {
                //�ǂ����悤���Ȃ��̂Ŗ���
                Debug.WriteLine($"7z Error : {e.GetType().Name} {e.Message}");
            }
            catch (IOException e)
            {
                Debug.WriteLine($"7z Error : {e.GetType().Name} {e.Message}");
            }
        }

        //AsyncExtractAll���L�����Z������
        public void CancelExtractAll()
        {
            if (m_7z != null)
                IsCancelExtraction = true;
        }

        /// <summary>�C�x���g����</summary>
        private void ExtractionFinished(object sender, EventArgs e)
        {
            //�S�t�@�C�������C�x���g�𔭉�
            ExtractAllEndEventHandler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>�C�x���g����</summary>
        private void FileExtractionFinished(object sender, FileInfoEventArgs e)
        {
            //ver1.10 �L�����Z�������̒ǉ�
            if (IsCancelExtraction)
            {
                e.Cancel = true;
                Debug.WriteLine("7z�W�J���ɒ��f����������܂���");
            }

            //1�t�@�C���W�J�C�x���g�𔭉�
            ExtractEventHandler?.Invoke(e.FileInfo.FileName);
        }
    }
}