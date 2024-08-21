using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LSharp.LTypes
{
	[DebuggerTypeProxy(typeof(LuaTableDebugView))]
	public class LTable : ILValue
	{
		LValueDictionary _dictionary = new();

		private ILValue?[] _metaMethods = Array.Empty<ILValue>();
		private List<ILValue> _consecutiveValues = new();

		public void SetValue(ILValue key, ILValue value)
		{

			if (key is LInteger iKey && iKey.Value > 0)
			{
				if (_consecutiveValues.Count >= iKey.Value)
				{
					_consecutiveValues[(int)iKey.Value - 1] = value;
					return;
				}
				else if (_consecutiveValues.Count + 1 >= iKey.Value)
				{
					_consecutiveValues.Add(value);
					for (int i = (int)iKey.Value + 1; ;i++) {
						var reference = _dictionary.GetEntryReference(new LInteger(i));
						if (reference == TableEntryReference.Invalid)
							break;

						_consecutiveValues.Add(_dictionary.GetValue(reference));
						_dictionary.RemoveValue(reference);
					}
					return;
				}
			}
			_dictionary.SetValue(key, value);
		}

		public TableEntryReference HasValueMaybeUpdate(ILValue key)
		{
			if (key is LInteger i && i.Value > 0 && _consecutiveValues.Count >= i.Value)
			{
				return (TableEntryReference) (-i.Value - 1);
			}
			return _dictionary.GetEntryReference(key);
		}

		public void UpdateValue(TableEntryReference ctx, ILValue value)
		{
			if (ctx < 0) {
				_consecutiveValues[-((int)ctx) - 2] = value;
				return;
			}

			if (value is LNil)
			{
				_dictionary.RemoveValue(ctx);
			}
			else
			{
				_dictionary.UpdateValue(ctx, value);
			}
		}

		public ILValue GetValue(ILValue key)
		{
			if (key is LInteger i && i.Value > 0 && _consecutiveValues.Count >= i.Value)
			{
				return _consecutiveValues[(int) i.Value - 1];
			}
			return _dictionary.GetValue(key);
		}

		public ILValue? GetMetaMethod(MetaMethodTag tag)
		{
			return MetaTable?.GetActualMetaMethod(tag);
		}

		private ILValue? GetActualMetaMethod(MetaMethodTag tag) {
			if (_metaMethods.Length >= (int)tag) {
				return _metaMethods[(int)tag];
			}

			return null;
		}

		public long GetLength()
		{
			return _consecutiveValues.Count;
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
			public (ILValue, ILValue)[] keys =>
				table._consecutiveValues
					.Select((x, i) => ((ILValue) new LInteger(i + 1), x))
					.Concat(table._dictionary.values)
					.ToArray();
		}
	}
}
