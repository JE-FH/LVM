using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace LSharp.LTypes
{
	public enum TableEntryReference : int
	{
		Invalid = -1,
	};
	
	[DebuggerTypeProxy(typeof(LValueDictionary))]
	public class LValueDictionary
	{
		[DebuggerDisplay("{key} = {value}")]
		//TODO: possible bottleneck for cache lines
		internal struct Slot
		{
			public ILValue key;
			public ILValue value;
			public uint hashCode;
			public int next;
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

			var wantedSlots = _slots.Take(_nextSlotIndex).Where(slot => slot.value is not null).Select((slot, i) => (i, slot));

			foreach (var (slotIndex, slot) in wantedSlots)
			{
				var index = slot.hashCode % _buckets.Length;
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

			Slot slot = _slots[slotIndex];

			while (slot.value is not null || hash != slot.hashCode || !slot.key.LEqual(key))
			{
				if (slot.next == -1)
				{
					return (false, slotIndex);
				}
				slot = _slots[slot.next];
			}
			return (true, slotIndex);
		}

		private void UpsertValue(ILValue key, ILValue value)
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

		public void SetValue(ILValue key, ILValue value)
		{
			UpsertValue(key, value);
		}

		public TableEntryReference GetEntryReference(ILValue key)
		{
			var (found, slotIndex) = GetSlotIndexOrPreviousSlot(key);
			return found ? (TableEntryReference) slotIndex : TableEntryReference.Invalid;
		}

		public void UpdateValue(TableEntryReference ctx, ILValue value)
		{
			_slots[(int) ctx].value = value;
		}

		public ILValue GetValue(ILValue index)
		{
			var (found, slotIndex) = GetSlotIndexOrPreviousSlot(index);
			if (found)
			{
				return _slots[slotIndex].value;
			}
			return LNil.Instance;
		}

		public ILValue GetValue(TableEntryReference entryReference)
		{
			return _slots[(int)entryReference].value ?? LNil.Instance;
		}

		public void RemoveValue(TableEntryReference reference)
		{
			_slots[(int)reference].value = default;
			_slots[(int)reference].key = LNil.Instance;
		}

		public IEnumerable<(ILValue, ILValue)> values => _slots
			.Take(_nextSlotIndex)
			.Select(x => (x.key, x.value))
			.AsEnumerable();

		internal class LValueDictionaryDebugView
		{
			private LValueDictionary table;
			public LValueDictionaryDebugView(LValueDictionary table)
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
