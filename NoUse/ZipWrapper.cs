using System;
using System.IO;						//Directory, File
using ICSharpCode.SharpZipLib.Zip;		//Sharp Zip
using System.Windows.Forms;				//DialogResult


namespace Marmi
{
	/********************************************************************************/
	// ICSharpCode.SharpZipLib.Zip�����b�s���O����N���X�B
	/********************************************************************************/
	public class ZipWrapper : IArchiver
	{
		private string m_filename;			//Open���Ă���Zip�t�@�C����
		//private string m_password;			//�p�X���[�h������
		private bool m_isOpen = false;		//�p�X���[�h�F�؊܂߃I�[�v�����Ă��邩

		private ZipFile m_zEntries;			//ZipEntry[]�z�������

		public bool isOpen
		{
			get { return m_isOpen; }
		}

		public int itemCount
		{
			get { return (int)m_zEntries.Count; }
		}

		public ZipWrapper()
		{
			m_filename = null;
			//m_password = null;
			m_zEntries = null;
			m_isOpen = false;
		}

		public bool Open(string Filename)
		{
			//if (m_fs != null)
			//    Close();
			//m_fs = File.OpenRead(Filename);
			//m_zStream = new ZipInputStream(m_fs);
			//m_zStream.Password = m_password;

			m_isOpen = false;
			m_filename = Filename;

			m_zEntries = new ZipFile(Filename);

			if (m_zEntries.Count == 0)
			{
				return false;
			}

			//�p�X���[�h�`�F�b�N
			try
			{
				//�悳���ȃt�@�C����T���B
				string testFilename = "";
				for (int i = 0; i < m_zEntries.Count; i++)
				{
					testFilename = m_zEntries[i].Name;
					if (testFilename[testFilename.Length - 1] != '/')
						break;
				}
				Stream st = GetStream(testFilename);
			}
			catch
			{
				return false;
			}

			m_isOpen = true;	//�p�X���[�h�F�؍ς�
			return true;
		}

		public void Close()
		{
			//if (m_zStream != null)
			//{
			//    m_zStream.Close();
			//    m_zStream.Dispose();
			//    m_zStream = null;
			//}
			//if (m_fs != null)
			//{
			//    m_fs.Close();
			//    m_fs.Dispose();
			//    m_fs = null;
			//}
			m_zEntries.Close();

			m_filename = null;
			m_zEntries = null;
			m_isOpen = false;
		}

		public Stream GetStream(string filename)
		{
			{
				int passwordRetry = 0;
				while (true)
				{
					try
					{
						ZipEntry ze = m_zEntries.GetEntry(filename);
						if (ze != null)
						{
							//��O���������Ȃ��ꍇ�͐���ɓǂݍ��߂��B
							Stream st = m_zEntries.GetInputStream(ze);
							return st;

							////ver0.93
							////MemoryStream�ɃR�s�[����B
							//MemoryStream ms = new MemoryStream((Int32)st.Length);
							//int len = 0;
							//byte[] buf = new byte[65536];
							//while ((len = st.Read(buf, 0, buf.Length)) > 0)
							//    ms.Write(buf, 0, len);
							//st.Close();
							//return ms;
						}
						else
							return null;
					}
					catch // (Exception e)
					{
						passwordRetry++;
						if (passwordRetry > 3)
							break;

						//Debug.WriteLine(e.Message);
						FormPassword pf = new FormPassword();
						if (pf.ShowDialog() == DialogResult.OK)
						{
							m_zEntries.Password = pf.PasswordText;
							//m_password = pf.PasswordText;
							//m_zEntries.Password = m_password;
						}
						else
						{
							//Cancel���ꂽ�̂Ŗ߂�
							passwordRetry = 0;
							//return null;
						}
					}
				}//while

				throw new Exception("PasswordFail");
				//return null;
			}
		}

		public ArchiveItem Item(int index)
		{
			return new ArchiveItem(
				m_zEntries[index].Name,
				m_zEntries[index].DateTime,
				(ulong)m_zEntries[index].Size,
				m_zEntries[index].IsDirectory
				);
		}

		//public DateTime GetDateTime(string filename)
		//{
		//    try
		//    {
		//        ZipEntry ze = m_zEntries.GetEntry(filename);
		//        if (ze != null)
		//        {
		//            return ze.DateTime;
		//        }
		//        else
		//            return DateTime.MinValue;	//�l�^�Ȃ̂� null�͕Ԃ��Ȃ�
		//    }
		//    catch //(Exception e)
		//    {
		//        return DateTime.MinValue;
		//    }
		//}

	}
}
