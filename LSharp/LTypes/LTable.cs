using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LSharp.LTypes
{
	[DebuggerTypeProxy(typeof(LuaTableDebugView))]
	public class LTable : ILValue
	{
		[DebuggerDisplay("{key} = {value}")]
		internal struct Slot
		{
			public int next;
			public uint hashCode;
			public ILValue key;
			public ILValue value;
		}

		int[] _buckets;
		Slot[] _slots;
		private int _nextSlotIndex = 0;
		private ILValue?[] _metaMethods = Array.Empty<ILValue>();

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

		public LTable()
		{
			_buckets = [];
			_slots = [];
		}

		public ILValue this[ILValue value]
		{
			get => GetValue(value);
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

		public void SetValue(ILValue index, ILValue value)
		{
			UpsertValue(index, value);
		}
		
		public MaybeSetContext HasValueMaybeUpdate(ILValue key)
		{
			var (found, slotIndex) = GetSlotIndexOrPreviousSlot(key);
			return found ? new MaybeSetContext(slotIndex) : new MaybeSetContext();
		}

		public void UpdateValue(MaybeSetContext ctx, ILValue value)
		{
			_slots[ctx.SlotIndex].value = value;
		}

		public ILValue? GetMetaMethod(MetaMethodTag tag)
		{
			if (_metaMethods.Length >= (int) tag)
			{
				return _metaMethods[(int)tag];
			}

			return null;
		}

		public long GetLength()
		{
			return 0;
		}

		public LTable? MetaTable { get; set; }

		public bool LEqual(ILValue other) =>
			ReferenceEquals(this, other);

		public uint LHash => unchecked((uint)RuntimeHelpers.GetHashCode(this));

		public string Type => "table";

		internal class LuaTableDebugView
		{
			private LTable table;
			public const string TestString = "This should appear in the debug window.";
			public LuaTableDebugView(LTable table)
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

		public readonly struct MaybeSetContext
		{
			internal readonly int SlotIndex;
			public bool HasValue => SlotIndex > -1;
			internal MaybeSetContext(int _slotIndex)
			{
				SlotIndex = _slotIndex;
			}

			public MaybeSetContext()
			{
				SlotIndex = -1;
			}

		}
	}
}
