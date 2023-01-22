using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Marmi
{
	class WeakRefCache<TKey, TValue>
	{
		private static Dictionary<TKey, WeakReference> _cache;

		WeakRefCache()
		{
			_cache = new Dictionary<TKey, WeakReference>();
		}

		~WeakRefCache()
		{
			_cache.Clear();
		}


		public void Add(TKey key, Bitmap TValue, bool isLongCache)
		{
			_cache.Add(
				key,
				new WeakReference(TValue, false)
				);
		}


		public int Count
		{
			get
			{
				return _cache.Count;
			}
		}


		public TValue this[TKey key]
		{
			get
			{
				try
				{
					//Target�����݂��Ă���΂���𗘗p����
					//�܂������Ă��邩�ǂ����̃`�F�b�N��IsAlive���Target��
					//null�`�F�b�N�̕����ǂ��炵��
					TValue val = (TValue)_cache[key].Target;
					return val;
				}
				catch
				{
					// �L�[�����݂��Ȃ��ꍇnull��Ԃ�
					return default(TValue);
				}
			}
		}

		public bool ContainsKey(TKey key)
		{
			try
			{
				//Target�����݂��Ă��邩�m�F
				TValue val = (TValue)_cache[key].Target;
				if (val != null)
					return true;
				else
					return false;
			}
			catch
			{
				// �L�[�����݂��Ȃ�
				return false;
			}
		}
	}
}
