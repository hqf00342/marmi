using System;
using System.Collections.Generic;
using System.Text;

namespace Marmi
{
	/// <summary>
	/// 追加専用のキャッシュ。
	/// Thumbnail Chacheで利用していたが
	/// ver1.55時点では利用していない
	/// 
	/// Dictionary<>を使った連想配列でデータに名前をつけて保存できる。
	/// 基本は追加と参照のみ。消すときは全部消す
	/// 
	/// サムネイルで高品質サムネイルを一時保持するために利用
	/// </summary>
	public class NamedBuffer<TKey, TValue>
	{
		// キャッシュを保存するDictionary
		static Dictionary<TKey, TValue> _cache;

		//コンストラクタ
		public NamedBuffer()
		{
			_cache = new Dictionary<TKey, TValue>();
		}

		public void Add(TKey key, TValue obj)
		{
			//キーの重複を避ける
			if (_cache.ContainsKey(key))
				_cache.Remove(key);

			_cache.Add(key, obj);
		}

		public void Delete(TKey key)
		{
			_cache.Remove(key);
		}


		/// <summary>
		/// 指定したキーのアイテムを返す
		/// </summary>
		/// <param name="key">アイテムを指定するキー</param>
		/// <returns>アイテムオブジェクト。消滅している場合はnullを返す</returns>
		public TValue this[TKey key]
		{
			get
			{
				try
				{
					TValue d = (TValue)_cache[key];
					return d;
				}
				catch
				{
					// キーが存在しない場合など
					return default(TValue);
				}
			}
		}

		public bool ContainsKey(TKey key)
		{
			return _cache.ContainsKey(key);
		}

		public void Clear()
		{
			_cache.Clear();
		}
	}
}
