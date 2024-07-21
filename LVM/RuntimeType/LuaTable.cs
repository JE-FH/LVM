using LVM.Utils;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;

namespace LVM.RuntimeType
{
	public class LuaTable : IRuntimeValue
	{
		struct Slot
		{
			public int next;
			public uint hashCode;
			public IRuntimeValue key;
			public IRuntimeValue value;
		}

		int[] _buckets;
		Slot[] _slots;
		private int _nextSlotIndex = 0;

		private void RecalculateBuckets() {

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

		private (bool, int) GetSlotIndexOrPreviousSlot(IRuntimeValue key)
		{
			if (_buckets.Length == 0)
				return (false, -1);
			var hash = key.LuaHash;
			var slotIndex = _buckets[hash % _buckets.Length];
			if (slotIndex == -1)
			{
				return (false, -1);
			}
			while (hash != _slots[slotIndex].hashCode || !_slots[slotIndex].key.LuaEqual(key))
			{
				if (_slots[slotIndex].next == -1)
				{
					return (false, slotIndex);
				}
				slotIndex = _slots[slotIndex].next;
			}
			return (true, slotIndex);
		}

		private void UpsertValue(IRuntimeValue key, IRuntimeValue value)
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

			var hash = key.LuaHash;

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

		public LuaTable()
		{
			_buckets = [];
			_slots = [];
		}

		public IRuntimeValue GetValue(IRuntimeValue index)
		{
			var (found, slotIndex) = GetSlotIndexOrPreviousSlot(index);
			if (found)
			{
				return _slots[slotIndex].value;
			}
			return new LuaNil();
		}

		//TODO: optimize this
		public IRuntimeValue GetValue(int index)
		{
			return GetValue(new LuaInteger(index));
		}

		//TODO: optimize this
		public IRuntimeValue GetValue(byte[] index)
		{
			return GetValue(new LuaString(index));
		}

		public void SetValue(IRuntimeValue index, IRuntimeValue value)
		{
			UpsertValue(index, value);
		}

		public void SetValue(int index, IRuntimeValue value)
		{
			//TODO: Optimize this
			UpsertValue(new LuaInteger(index), value);
		}

		public void SetValue(byte[] index, IRuntimeValue value)
		{
			UpsertValue(new LuaString(index), value);
		}

		public long GetLength()
		{
			return 0;
		}

		public LuaType TypeName => LuaType.Table;

		public bool LuaEqual(IRuntimeValue other) =>
			ReferenceEquals(this, other);

		public uint LuaHash => unchecked((uint)RuntimeHelpers.GetHashCode(this));
	}
}
