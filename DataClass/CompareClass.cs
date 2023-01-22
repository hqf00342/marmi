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

	public class NaturalOrderComparer2 : IComparer<ImageInfo>
	{
		public int Compare(ImageInfo x, ImageInfo y)
		{
			//return NaturalOrderCompareOriginal(x.filename, y.filename);
			return Compare_unsafeFast(x.filename, y.filename);
		}

		[Obsolete]
		public int NaturalOrderCompareOriginal(string s1, string s2)
		{
			//���l����x�ϊ��������ɂ��Ȃ���r������B	
			//XP�ȍ~�̃\�[�g�����ɑΉ������͂��E�E�E

			//�K�w���`�F�b�N
			int lev1 = 0;	//x�̊K�w
			int lev2 = 0;	//y�̊K�w
			for (int i = 0; i < s1.Length; i++)
				if (s1[i] == '/' || s1[i] == '\\') lev1++;
			for (int i = 0; i < s2.Length; i++)
				if (s2[i] == '/' || s2[i] == '\\') lev2++;

			if (lev1 != lev2)
				return lev1 - lev2;

			//
			// ����K�w�Ȃ̂�1�������`�F�b�N���J�n����
			//
			int p1 = 0;		// s1���w���|�C���^
			int p2 = 0;		// s2���w���|�C���^
			long num1 = 0;	// s1�Ɋ܂܂�鐔�l�B�傫�Ȑ��l�ɑΉ������邽��long
			long num2 = 0;	// s2�Ɋ܂܂�鐔�l�B�傫�Ȑ��l�ɑΉ������邽��long

			do
			{
				char c1 = s1[p1];
				char c2 = s2[p2];

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
						if (p1 >= s1.Length)
							break;
						c1 = s1[p1];
					}

					//s2�n��̕����𐔒lnum2�ɕϊ�
					num2 = 0;
					while (c2 >= '0' && c2 <= '9')
					{
						num2 = num2 * 10 + c2 - '0';
						++p2;
						if (p2 >= s2.Length)
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
						return (int)(c1 - c2);
					++p1;
					++p2;
				}
			}
			while (p1 < s1.Length && p2 < s2.Length);

			//�ǂ��炩���I�[�ɒB�����B���Ƃ͒����������B
			return s1.Length - s2.Length;
		}

		/// <summary>
		/// NaturalSort3���P�B���[�v���ł̓��I�ϐ��m�ۂ���߂�
		/// ������̒��������O�Ɋm�F
		/// �����炭Managed�ő�
		/// </summary>
		/// <param name="s1"></param>
		/// <param name="s2"></param>
		/// <returns></returns>
		public int Compare_ManagedFast(string s1, string s2)
		{
			//���l����x�ϊ��������ɂ��Ȃ���r������B	
			//XP�ȍ~�̃\�[�g�����ɑΉ������͂��E�E�E


			//
			// ����K�w�Ȃ̂�1�������`�F�b�N���J�n����
			//
			int p1 = 0;		// s1���w���|�C���^
			int p2 = 0;		// s2���w���|�C���^
			long num1 = 0;	// s1�Ɋ܂܂�鐔�l�B�傫�Ȑ��l�ɑΉ������邽��long
			long num2 = 0;	// s2�Ɋ܂܂�鐔�l�B�傫�Ȑ��l�ɑΉ������邽��long
			char c1;	//��r�����P c1 = s1[p1];
			char c2;	//��r�����Q c2 = s2[p2];
			int s1Len = s1.Length;
			int s2Len = s2.Length;

			do
			{
				c1 = s1[p1];
				c2 = s2[p2];

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
						//if (p1 >= s1.Length)
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
						//if (p2 >= s2.Length)
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
			//while (p1 < s1.Length && p2 < s2.Length) ;

			//�ǂ��炩���I�[�ɒB�����B���Ƃ͒����������B
			return s1Len - s2Len;
			//return s1.Length - s2.Length;
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
		public int Compare_unsafeFast(string s1, string s2)
		{
			//���l����x�ϊ��������ɂ��Ȃ���r������B	
			//XP�ȍ~�̃\�[�g�����ɑΉ������͂��E�E�E


			//
			// 1�������`�F�b�N���J�n����
			//
			int p1 = 0;		// s1���w���|�C���^���Z�l
			int p2 = 0;		// s2���w���|�C���^���Z�l
			long num1 = 0;	// s1�Ɋ܂܂�鐔�l�B�傫�Ȑ��l�ɑΉ������邽��long
			long num2 = 0;	// s2�Ɋ܂܂�鐔�l�B�傫�Ȑ��l�ɑΉ������邽��long
			char c1;	//��r�����P c1 = s1[p1];
			char c2;	//��r�����Q c2 = s2[p2];
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


		/// <summary>
		/// �ق�̂�����Ƃ����œK��
		/// �C���N�������g�𒆂�
		/// while���[�v��do�`while��
		/// </summary>
		/// <param name="s1"></param>
		/// <param name="s2"></param>
		/// <returns></returns>
		public int Compare_unsafeFast2(string s1, string s2)
		{
			//���l����x�ϊ��������ɂ��Ȃ���r������B	
			//XP�ȍ~�̃\�[�g�����ɑΉ������͂��E�E�E


			//
			// 1�������`�F�b�N���J�n����
			//
			int p1 = 0;		// s1���w���|�C���^���Z�l
			int p2 = 0;		// s2���w���|�C���^���Z�l
			long num1 = 0;	// s1�Ɋ܂܂�鐔�l�B�傫�Ȑ��l�ɑΉ������邽��long
			long num2 = 0;	// s2�Ɋ܂܂�鐔�l�B�傫�Ȑ��l�ɑΉ������邽��long
			char c1;	//��r�����P c1 = s1[p1];
			char c2;	//��r�����Q c2 = s2[p2];
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
							do
							{
								num1 = num1 * 10 + c1 - '0';
								if (++p1 >= s1Len)
									break;
								c1 = s1[p1];
							}
							while (c1 >= '0' && c1 <= '9');

							//s2�n��̕����𐔒lnum2�ɕϊ�
							num2 = 0;
							do
							{
								num2 = num2 * 10 + c2 - '0';
								if (++p2 >= s2Len)
									break;
								c2 = s2[p2];
							}
							while (c2 >= '0' && c2 <= '9');

							//���l�Ƃ��Ĕ�r
							if (num1 != num2)
								return (int)(num1 - num2);
						}

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
					while (p1 < s1Len && p2 < s2Len);
				}//fixed
			}

			//�ǂ��炩���I�[�ɒB�����B���Ƃ͒����������B
			return s1Len - s2Len;
		}
	
	}



	public class DateCompare : IComparer<ImageInfo>
	{

		public int Compare(ImageInfo x, ImageInfo y)
		{
			return DateTime.Compare(x.createDate, y.createDate);
		}
	}



	public class CustomSortCompare : IComparer<ImageInfo>
	{
		public int Compare(ImageInfo x, ImageInfo y)
		{
			return x.nOrgIndex - y.nOrgIndex;
		}
	}


}
