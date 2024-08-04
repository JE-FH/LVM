using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp
{
	public class LStack()
	{
		private readonly List<UpValue> _stack = [];
		private int _top = 0;
		public int Top => _top;

		public void SetTop(int newTop)
		{
			if (newTop > _stack.Count)
			{
				for (int i = _stack.Count; i < newTop; i++)
				{
					_stack.Add(new UpValue(LNil.Instance));
				}
			}
			
			if (newTop > _top)
			{
				for (int i = _top; i < newTop - 1; i++)
				{
					_stack[i].Value = LNil.Instance;
				}
			}
			
			_top = newTop;
		}

		public ILValue this[int key]
		{
			get => _stack[key].Value;
			set => _stack[key].Value = value;
		}

		public UpValue GetAsUpValue(int key)
		{
			return _stack[key];
		}
	}
}
