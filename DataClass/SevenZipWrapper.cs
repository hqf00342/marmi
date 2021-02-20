#define X86ONLY		//x86

using System;
using System.Diagnostics;				//Debug, Stopwatch
using System.IO;						//Directory, File
using System.Threading;

//Sharp Zip
using System.Windows.Forms;				//DialogResult
using SevenZip;
using ArchivedFiles = System.Collections.ObjectModel.ReadOnlyCollection<SevenZip.ArchiveFileInfo>;

namespace Marmi
{
    //�C���^�[�t�F�[�X
    //  IArchiver: ZipWrapper�Ɠ����C���^�[�t�F�[�X��񋟂��� ver1.31�őΏۊO
    //  IDisposavle: using�Ŏg�����߂ɕK�v
    //class SevenZipWrapper : IArchiver,IDisposable
    public class SevenZipWrapper : IDisposable
    {
        private string m_filename;              //Open���Ă���Zip�t�@�C����
        private bool m_isOpen = false;          //�p�X���[�h�F�؊܂߃I�[�v�����Ă��邩
        private SevenZipExtractor m_7z;
        private Thread m_ExtractAllThread;      //�S�𓀗p�X���b�h

        //public string m_TempDir;				//�ꎞ�W�J��̏���
        //volatile private bool isAsyncExtractionFinished;	//�񓯊��W�J���I��������Ƃ������t���O
        //private object locker = new object();	//�X���b�h�Z�[�t�ɂ��邽�ߓǍ����b�N�����邽�߂�object

        //�p�X���[�h�̕ۑ�
        //���̃N���X�͂P���������킯�ł͂Ȃ��̂�static�Ŏ����Ă���
        //���������Ƃ��̖��ݒ�Bstring.empty�̂Ƃ��̓p�X���[�h����
        private static string m_password;

        //�C�x���g�n���h���[
        //AyncExtractAll�ł̃t�@�C���W�J�p
        public event Action<string> ExtractEventHandler;        //�P�t�@�C���������Ƃ̃C�x���g

        public event EventHandler ExtractAllEndEventHandler;    //���S�W�J������̃C�x���g

        public bool isCancelExtraction { get; set; }

        public bool isOpen { get { return m_isOpen; } }

        /// <summary>
        ///���ɂɂ���A�C�e������Ԃ�
        ///�f�B���N�g����1�t�@�C���Ƃ��ĕԂ��̂Œ���
        /// </summary>
        public int itemCount
        {
            get { return m_7z.ArchiveFileData.Count; }
        }

        /// <summary>
        /// ���ɂ��\���b�h���ǂ����Ԃ�
        /// </summary>
        public bool isSolid
        {
            get { return m_7z.IsSolid; }
        }

        public ArchivedFiles Items
        {
            get { return m_7z.ArchiveFileData; }
        }

        public string Filename
        {
            get { return m_filename; }
        }

        public SevenZipWrapper()
        {
            m_filename = null;
            m_7z = null;
            m_isOpen = false;

            //�����I��32bit���C�u������
            SevenZipExtractor.SetLibraryPath(
                Path.Combine(Application.StartupPath, "7z.dll"));
            //setLibrary32or64();

            //ver1.10 �L�����Z�������̂��߂̏�����
            isCancelExtraction = false;
        }

        public SevenZipWrapper(string filename) : this()
        {
            if (!Open(filename))
                throw new FileNotFoundException();
        }

        public SevenZipWrapper(string filename, string password) : this()
        {
            if (string.IsNullOrEmpty(password))
                m_password = string.Empty;
            else
                m_password = password;

            if (!Open(filename))
                throw new FileNotFoundException();
        }

        ~SevenZipWrapper()
        {
            //m_password = string.Empty;
            //SevenZipWrapper.m_password = string.Empty;

            //�C�x���g�n���h���[�̉���
            if (m_7z != null)
            {
                m_7z.Extracting -= new EventHandler<ProgressEventArgs>(evtExtracting);
                m_7z.ExtractionFinished -= new EventHandler<EventArgs>(evtExtractionFinished);
                m_7z.FileExtractionFinished -= new EventHandler<FileInfoEventArgs>(evtFileExtractionFinished);
                //m_7z.FileExists -= new EventHandler<FileOverwriteEventArgs>(evtFileExists);
                //m_7z.FileExtractionStarted -= new EventHandler<FileInfoEventArgs>(evtFileExtractionStarted);
            }
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

        private void setLibrary32or64()
        {
            //���C�u�����p�X�̐ݒ�
            if (IntPtr.Size == 4)
            {
                //32bit process
                SevenZipExtractor.SetLibraryPath(
                    Path.Combine(Application.StartupPath, "7z.dll"));
            }
            else if (IntPtr.Size == 8)
            {
                //64bit process
                SevenZipExtractor.SetLibraryPath(
                    Path.Combine(Application.StartupPath, "7z64.dll"));
            }
        }

        public bool Open(string ArchiveName)
        {
            //�����t�@�C����Open�ς݂Ȃ�Ȃɂ����Ȃ�
            //Uty.WriteLine("7z.Open() isOpen={0}, arc={1}", m_isOpen, ArchiveName);
            if (m_isOpen && ArchiveName == m_filename)
                return true;

            Uty.WriteLine("7z.Open({0})", ArchiveName);

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

            if (m_7z != null)
            {
                m_7z.Dispose();
                m_7z = null;
            }

            //ver1.05 7zip�t�@�C���I�[�v�� static password�Ή���
            try
            {
                //if (SevenZipWrapper.m_password != string.Empty)
                if (!string.IsNullOrEmpty(SevenZipWrapper.m_password))
                {
                    m_7z = new SevenZipExtractor(ArchiveName, m_password);

                    //���Ƀ`�F�b�N�B���Ă��Ȃ���
                    //if (!m_sevenzipExtractor.Check())
                    if (m_7z.FilesCount == 0)
                    {
                        m_7z.Dispose();
                        m_7z = null;
                        m_isOpen = false;
                        return false;
                    }

                    if (TryPassword())
                        return true;
                    //password check�Ɏ��s����ƃ_�C�A���O�őΉ�
                }
                else
                {
                    //�ʏ�ʂ��Open������
                    m_7z = new SevenZipExtractor(ArchiveName);

                    //���Ƀ`�F�b�N�B���Ă��Ȃ���
                    //if (!m_sevenzipExtractor.Check()
                    //	|| m_sevenzipExtractor.FilesCount == 0)

                    //ver1.31���Ƀ`�F�b�N�̂��肾�������ǒ��~
                    //7z�p�X���[�h�t���������ŗ�O
                    //if (m_7z.FilesCount == 0)
                    //{
                    //    m_7z.Dispose();
                    //    m_7z = null;
                    //    m_isOpen = false;
                    //    return false;
                    //}
                }

                //�p�X���[�h�`�F�b�N
                //ver1.31 �f�B���N�g���łȂ��A�C�e����T��
                int testitem = getFirstNonDirItem();
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
                add7zEvent();
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
                    if (TryPassword())
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
                    };
                }

                //�p�X���[�h�F��3�񎸔s
                m_isOpen = false;   //�p�X���[�h�F�؎��s�ɂ��N���[�Y���
                return false;
            }//using
        }

        private void add7zEvent()
        {
            if (m_7z != null)
            {
                m_7z.Extracting += new EventHandler<ProgressEventArgs>(evtExtracting);
                m_7z.ExtractionFinished += new EventHandler<EventArgs>(evtExtractionFinished);
                m_7z.FileExtractionFinished += new EventHandler<FileInfoEventArgs>(evtFileExtractionFinished);
                //m_7z.FileExists += new EventHandler<FileOverwriteEventArgs>(evtFileExists);
                //m_7z.FileExtractionStarted += new EventHandler<FileInfoEventArgs>(evtFileExtractionStarted);
            }
        }

        /// <summary>
        /// �p�X���[�h���K�v���ǂ����`�F�b�N����
        /// </summary>
        /// <returns>�K�v�ȏꍇ��false</returns>
        private bool TryPassword()
        {
            //�����ɓW�J�����A�p�X���[�h�F�؎��s�Ȃ��O�𔭐�������
            try
            {
                int testitem = getFirstNonDirItem();
                if (testitem == -1) return false;

                using (MemoryStream ms = new MemoryStream())
                {
                    //m_7z.ExtractFile(0, ms);
                    m_7z.ExtractFile(testitem, ms);
                }
                //�����ɗ����Ƃ��͐���
                return true;
            }
            catch //(Exception e)
            {
                return false;
            }
        }

        private int getFirstNonDirItem()
        {
            //���ɓ��̃f�B���N�g���ł͂Ȃ��A�C�e����T��
            //�p�X���[�h�`�F�b�N�p�ɗ��p
            //������Ȃ��ꍇ��-1��Ԃ�
            int testitem = 0;
            while (m_7z.ArchiveFileData[testitem].IsDirectory)
            {
                testitem++;
                if (testitem >= m_7z.ArchiveFileData.Count)
                    return -1;
            }
            return testitem;
        }

        public void Close()
        {
            if (m_7z != null)
                m_7z.Dispose();

            m_7z = null;
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

        //ver1.31 �v���p�e�B�ő�p
        //public ArchiveItem Item(int index)
        //{
        //    return new ArchiveItem(
        //        m_7z.ArchiveFileData[index].FileName,
        //        m_7z.ArchiveFileData[index].CreationTime,
        //        m_7z.ArchiveFileData[index].Size,
        //        m_7z.ArchiveFileData[index].IsDirectory
        //        );
        //}

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
            Debug.WriteLine(ExtractDir, "7z�W�J�J�n");
            //isAsyncExtractionFinished = false;
            //m_sevenzipExtractor.BeginExtractArchive(m_TempDir);
            try
            {
                m_7z.ExtractArchive(ExtractDir);
            }
            catch (SevenZipException e)
            {
                Debug.Write("7zError::");
                Debug.WriteLine(e.Message, e.StackTrace);
                //throw e;
            }
            catch (PathTooLongException e)
            {
                //�ǂ����悤���Ȃ��̂Ŗ���
                Debug.Write("7zError::");
                Debug.WriteLine(e.Message, e.StackTrace);
                //throw e;
            }
            catch (IOException e)
            {
                Debug.Write("7zError::");
                Debug.WriteLine(e.Message, e.StackTrace);
            }
        }

        public void AsyncExtractAll(string zippedFilename, string extractFolder)
        {
            //�X���b�h�������Ă��Ȃ����Ƃ��m�F����
            if (m_ExtractAllThread != null)
            {
                if (m_ExtractAllThread.IsAlive)
                    return;
                m_ExtractAllThread.Abort();
            }

            //extractor�̓X���b�h�O�Ő����ł��Ȃ��̂Ŕj��
            if (m_7z != null)
            {
                m_7z.Dispose();
                m_7z = null;
            }

            ThreadStart tsAction = () =>
            {
                Open(zippedFilename);
                ExtractAll(extractFolder);
                //ExtractAll(extractFolder, true);
            };

            m_ExtractAllThread = new Thread(tsAction);
            m_ExtractAllThread.Name = "7zExtractor Thread";
            m_ExtractAllThread.IsBackground = true;
            m_ExtractAllThread.Start();
            //m_ExtractAllThread.Join();

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

        //AsyncExtractAll���L�����Z������
        public void CancelAsyncExtractAll()
        {
            Debug.WriteLine("Extract is Calceling...");
            if (m_7z != null)
                isCancelExtraction = true;
        }

        public bool ExtractFile(string extractFilename, string path)
        {
            try
            {
                string outfile = Path.Combine(path, extractFilename);
                using (FileStream fs = File.Open(outfile, FileMode.Create))
                {
                    m_7z.ExtractFile(extractFilename, fs);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        //
        ////////////////////////////////////////////////////////////////////////////////////////////////
        // �C�x���g�n���h���[
        //

        //void evtFileExists(object sender, FileOverwriteEventArgs e)
        //{
        //    Debug.WriteLine(e.FileName, "SevenZipWrapper - �t�@�C�������łɑ��݂��Ă��܂�");
        //    //e.Cancel = true;
        //}

        private void evtExtractionFinished(object sender, EventArgs e)
        {
            //�P�i�𓀂ł��\�������̂ŃR�����g�A�E�g
            //Debug.WriteLine("SevenZipWrapper - �W�J����");

            //�C�x���g�o�^�`�F�b�N
            if (ExtractAllEndEventHandler != null)
            {
                Debug.WriteLine("SevenZipWrapper - �W�J����");
                ExtractAllEndEventHandler(this, EventArgs.Empty);
            }
        }

        private void evtFileExtractionFinished(object sender, FileInfoEventArgs e)
        {
            //ver1.10 �L�����Z�������̒ǉ�
            if (isCancelExtraction)
            {
                e.Cancel = true;
                Debug.WriteLine("7z�W�J���ɒ��f����������܂���");
            }

            //1�t�@�C���W�J���ɃC�x���g����
            if (ExtractEventHandler != null)
                ExtractEventHandler(e.FileInfo.FileName);
        }

        private void evtExtracting(object sender, ProgressEventArgs e)
        {
            //ver1.10 �L�����Z�������̒ǉ�
            //�����������ł̃L�����Z���͌����Ȃ��͗l�B
            //�����̂�FileExtractionFinished()
            if (isCancelExtraction)
            {
                e.Cancel = true;
                Debug.WriteLine("7z�W�J���ɒ��f����������܂���", "m_sevenzipExtractor_Extracting()");
            }
        }

        #region IDisposable �����o

        void IDisposable.Dispose()
        {
            //throw new Exception("The method or operation is not implemented.");
            if (m_7z != null)
                m_7z.Dispose();
        }

        #endregion IDisposable �����o
    }
}