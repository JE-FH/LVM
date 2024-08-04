﻿using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions
{
	public class OReturn0() : ITransition
	{
		public void Transfer(LuaState state, LStackFrame stackFrame)
		{
			state.CallStack.RemoveAt(state.CallStack.Count - 1);
			if (state.CallStack.Count == 0)
			{
				return;
			}
			

			var lowerStackFrame = state.CallStack[^1];

			state.Stack.SetTop(lowerStackFrame.FrameTop);

			if (lowerStackFrame is LStackFrame)
			{
				for (int i = 0; i < lowerStackFrame.MandatoryReturnCount; i++)
				{
					state.Stack[stackFrame.FrameBase + i] = LNil.Instance;
				}
			}
		}
	}

	public class OReturn1(byte A) : ITransition
	{
		public void Transfer(LuaState state, LStackFrame stackFrame)
		{
			state.CallStack.RemoveAt(state.CallStack.Count - 1);
			if (state.CallStack.Count == 0)
			{
				return;
			}

			var lowerStackFrame = state.CallStack[^1];
			if (lowerStackFrame is LStackFrame)
			{
				state.Stack[stackFrame.FrameBase] = state.Stack[stackFrame.FrameBase + A];

				state.Stack.SetTop(lowerStackFrame.FrameTop);
				
				for (int i = 1; i < lowerStackFrame.MandatoryReturnCount; i++)
				{
					state.Stack[stackFrame.FrameBase + i] = LNil.Instance;
				}
			}
			else if (lowerStackFrame is CSStackFrame lowerCSStackFrame)
			{
				lowerCSStackFrame.ActionGenerator.Current.Return = [state.Stack[stackFrame.FrameBase]];
				state.Stack.SetTop(lowerCSStackFrame.FrameTop);
			}
		}
	}

	//C and k are not needed for this implementation
	public class OReturn(byte a, byte b) : ITransition
	{
		public void Transfer(LuaState state, LStackFrame stackFrame)
		{
			state.CallStack.RemoveAt(state.CallStack.Count - 1);
			if (state.CallStack.Count == 0)
			{
				return;
			}

			int returnCount = b == 0 ? stackFrame.FrameTop - a : b - 1;

			var lowerStackFrame = state.CallStack[^1];
			if (lowerStackFrame is LStackFrame lowerLStackFrame)
			{
				var actualReturnCount = stackFrame.MandatoryReturnCount == -1 ? returnCount : stackFrame.MandatoryReturnCount;
				for (int i = 0; i < actualReturnCount; i++)
				{
					state.Stack[stackFrame.FrameBase + i] = i < returnCount
						? state.Stack[stackFrame.FrameBase + a + i]
						: LNil.Instance;
				}

				if (stackFrame.MandatoryReturnCount == -1) {
					lowerLStackFrame.FrameTop = Math.Max(stackFrame.FrameBase + actualReturnCount, lowerLStackFrame.FrameTop);
				}
				state.Stack.SetTop(lowerLStackFrame.FrameTop);
			}
			else if (lowerStackFrame is CSStackFrame lowerCSStackFrame)
			{
				List<ILValue> returnValues = new();
				for (int i = 0; i < returnCount; i++)
				{
					returnValues.Add(state.Stack[stackFrame.FrameBase + i + a]);
				}
				lowerCSStackFrame.ActionGenerator.Current.Return = [..returnValues];
				state.Stack.SetTop(lowerCSStackFrame.FrameTop);
			}
			else
			{
				throw new NotImplementedException();
			}
		}
	}
}
