using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LSharp.LTypes
{
	[DebuggerTypeProxy(typeof(LuaTableDebugView))]
	public class LTable : ILValue
	{
		LValueDictionary<ILValue> _dictionary = new();

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
						var val = _dictionary.GetValue(new LInteger(i));
						if (val == null)
							break;

						_consecutiveValues.Add(val);
					}
					return;
				}
			}
			_dictionary.SetValue(key, value);
		}

		public TableKeyReference HasValueMaybeUpdate(ILValue key)
		{
			if (key is LInteger i && i.Value > 0 && _consecutiveValues.Count >= i.Value)
			{
				return (TableKeyReference) (-i.Value - 1);
			}
			return _dictionary.HasValueMaybeUpdate(key);
		}

		public void UpdateValue(TableKeyReference ctx, ILValue value)
		{
			if (ctx < 0) {
				_consecutiveValues[-((int)ctx) - 2] = value;
				return;
			}
			_dictionary.UpdateValue(ctx, value);
		}

		public ILValue GetValue(ILValue key)
		{
			if (key is LInteger i && i.Value > 0 && _consecutiveValues.Count >= i.Value)
			{
				return _consecutiveValues[(int) i.Value - 1];
			}
			return _dictionary.GetValue(key) ?? LNil.Instance;
		}

		public ILValue? GetMetaMethod(MetaMethodTag tag)
		{
			if (MetaTable is null) {
				return null;
			} else {
				var mm = MetaTable.GetActualMetaMethod(tag);
				return mm;
			}
			
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
