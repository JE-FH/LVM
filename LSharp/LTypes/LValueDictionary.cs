using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.LTypes
{
	public enum TableKeyReference : int
	{
		Invalid = -1,
	};
	
	[DebuggerTypeProxy(typeof(LValueDictionary<dynamic>))]
	public class LValueDictionary<T>
	{
		[DebuggerDisplay("{key} = {value}")]
		internal struct Slot
		{
			public int next;
			public uint hashCode;
			public ILValue key;
			public T value;
		}

		int[] _buckets = [];
		Slot[] _slots = [];
		private int _nextSlotIndex = 0;

		private void RecalculateBuckets()
		{
			_buckets = Enumerable.Range(0, HashUtils.GetPrime(_slots.Length))
				.Select(_ => -1)
				.ToArray();
			for (int i = 0; i < _slots.Length; i++)
			{
				_slots[i].next = -1;
			}
			for (int slotIndex = 0; slotIndex < _nextSlotIndex; slotIndex++)
			{
				var index = _slots[slotIndex].hashCode % _buckets.Length;
				if (_buckets[index] >= 0)
				{
					var lastSlotIndex = _buckets[index];
					while (_slots[lastSlotIndex].next > 0)
					{
						lastSlotIndex = _slots[lastSlotIndex].next;
					}
					_slots[lastSlotIndex].next = slotIndex;
				}
				else
				{
					_buckets[index] = slotIndex;
				}
			}
		}

		private (bool, int) GetSlotIndexOrPreviousSlot(ILValue key)
		{
			if (_buckets.Length == 0)
				return (false, -1);
			var hash = key.LHash;
			var slotIndex = _buckets[hash % _buckets.Length];
			if (slotIndex == -1)
			{
				return (false, -1);
			}
			while (hash != _slots[slotIndex].hashCode || !_slots[slotIndex].key.LEqual(key))
			{
				if (_slots[slotIndex].next == -1)
				{
					return (false, slotIndex);
				}
				slotIndex = _slots[slotIndex].next;
			}
			return (true, slotIndex);
		}

		private void UpsertValue(ILValue key, T value)
		{
			var (found, slotIndex) = GetSlotIndexOrPreviousSlot(key);
			if (found)
			{
				_slots[slotIndex].value = value;
				return;
			}

			if (_nextSlotIndex + 1 >= _buckets.Length)
			{
				RecalculateBuckets();
				(_, slotIndex) = GetSlotIndexOrPreviousSlot(key);
			}

			var hash = key.LHash;

			if (_nextSlotIndex >= _slots.Length)
			{
				Array.Resize(ref _slots, Math.Max(_slots.Length * 2, 2));
			}

			_slots[_nextSlotIndex] = new Slot
			{
				hashCode = hash,
				next = -1,
				key = key,
				value = value
			};

			if (slotIndex < 0)
			{
				_buckets[hash % _buckets.Length] = _nextSlotIndex;
			}
			else
			{
				_slots[slotIndex].next = _nextSlotIndex;
			}

			_nextSlotIndex += 1;
		}

		public void SetValue(ILValue key, T value)
		{
			UpsertValue(key, value);
		}

		public TableKeyReference HasValueMaybeUpdate(ILValue key)
		{
			var (found, slotIndex) = GetSlotIndexOrPreviousSlot(key);
			return found ? (TableKeyReference) slotIndex : TableKeyReference.Invalid;
		}

		public void UpdateValue(TableKeyReference ctx, T value)
		{
			_slots[(int) ctx].value = value;
		}

		public T? GetValue(ILValue index)
		{
			var (found, slotIndex) = GetSlotIndexOrPreviousSlot(index);
			if (found)
			{
				return _slots[slotIndex].value;
			}
			return default;
		}

		public IEnumerable<(ILValue, T)> values => _slots
			.Take(_nextSlotIndex)
			.Select(x => (x.key, x.value))
			.AsEnumerable();

		internal class LValueDictionaryDebugView
		{
			private LValueDictionary<T> table;
			public LValueDictionaryDebugView(LValueDictionary<T> table)
			{
				this.table = table;
			}

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public Slot[] slots
			{
				get
				{
					return table._slots
						.Take(table._nextSlotIndex)
						.ToArray();
				}
			}
		}
	}
}
