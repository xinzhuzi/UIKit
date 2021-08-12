using System;
using System.Collections.Generic;

namespace UIKit
{
	public class DoubleMap<TKey, TValue>
	{
		private readonly Dictionary<TKey, TValue> kv = new Dictionary<TKey, TValue>();
		private readonly Dictionary<TValue, TKey> vk = new Dictionary<TValue, TKey>();

		public DoubleMap() { }

		public DoubleMap(int capacity)
		{
			kv = new Dictionary<TKey, TValue>(capacity);
			vk = new Dictionary<TValue, TKey>(capacity);
		}

		public int Count => kv.Count;

		public void ForEach(Action<TKey, TValue> action)
		{
			if (action == null)
			{
				return;
			}
			Dictionary<TKey, TValue>.KeyCollection keys = kv.Keys;
			foreach (TKey key in keys)
			{
				action(key, kv[key]);
			}
		}
		
		public Dictionary<TKey, TValue>.ValueCollection ForEachValues()
		{
			return kv.Values;
		}
		
		public Dictionary<TKey, TValue>.KeyCollection ForEachKeys()
		{
			return kv.Keys;
		}

		public List<TKey> Keys => new List<TKey>(kv.Keys);

		public List<TValue> Values => new List<TValue>(vk.Keys);

		public void Add(TKey key, TValue value)
		{
			if (key == null || value == null || kv.ContainsKey(key) || vk.ContainsKey(value))
			{
				return;
			}
			kv.Add(key, value);
			vk.Add(value, key);
		}

		public TValue GetValueByKey(TKey key)
		{
			if (key != null && kv.ContainsKey(key))
			{
				return kv[key];
			}
			return default(TValue);
		}

		public TKey GetKeyByValue(TValue value)
		{
			if (value != null && vk.ContainsKey(value))
			{
				return vk[value];
			}
			return default(TKey);
		}

		public bool RemoveByKey(TKey key)
		{
			if (key == null)
			{
				return false;
			}
			if (!kv.TryGetValue(key, out TValue value))
			{
				return false;
			}
			return kv.Remove(key) && vk.Remove(value);
		}

		public bool RemoveByValue(TValue value)
		{
			if (value == null)
			{
				return false;
			}

			if (!vk.TryGetValue(value, out TKey key))
			{
				return false;
			}
			return kv.Remove(key) && vk.Remove(value);
		}

		public void Clear()
		{
			kv.Clear();
			vk.Clear();
		}

		public bool ContainsKey(TKey key)
		{
			return key != null && kv.ContainsKey(key);
		}

		public bool ContainsValue(TValue value)
		{
			return value != null && vk.ContainsKey(value);
		}

		public bool Contains(TKey key, TValue value)
		{
			if (key == null || value == null)
			{
				return false;
			}
			return kv.ContainsKey(key) && vk.ContainsKey(value);
		}
	}
}