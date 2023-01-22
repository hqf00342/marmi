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
					//Targetが存在していればそれを利用する
					//まだ生きているかどうかのチェックはIsAliveよりTargetの
					//nullチェックの方が良いらしい
					TValue val = (TValue)_cache[key].Target;
					return val;
				}
				catch
				{
					// キーが存在しない場合nullを返す
					return default(TValue);
				}
			}
		}

		public bool ContainsKey(TKey key)
		{
			try
			{
				//Targetが存在しているか確認
				TValue val = (TValue)_cache[key].Target;
				if (val != null)
					return true;
				else
					return false;
			}
			catch
			{
				// キーが存在しない
				return false;
			}
		}
	}
}
