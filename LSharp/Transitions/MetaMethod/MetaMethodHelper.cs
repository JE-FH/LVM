using LSharp.Helpers;
using LSharp.LTypes;

namespace LSharp.Transitions.MetaMethod
{
	internal static class MetaMethodHelper
	{
		public static void CallMetaMethod(LuaState state, LStackFrame stackFrame, ILValue[] args, MetaMethodTag operation, int outputIndex = -1)
		{
			ILValue metaMethod = ArithmeticHelper.GetMetaMethod(args[0], args[1], operation);

			//Do something like this for newindex
			if (operation == MetaMethodTag.Index && args[0] is LTable && metaMethod is LTable mTable)
			{
				var val = mTable.GetValue(args[1]);
				if (val is LNil)
				{
					CallMetaMethod(state, stackFrame, [mTable, args[1]], operation, outputIndex);
				}
				else
				{
					state.Stack[stackFrame.FrameBase + outputIndex] = mTable.GetValue(args[1]) ?? LNil.Instance;
					stackFrame.PC += 1;
				}
				return;
			}

			if (metaMethod is IClosure metaMethodClosure)
			{
				if (outputIndex == -1)
				{
					state.Stack.SetTop(stackFrame.FrameTop + 1);
					stackFrame.FrameTop += 1;
					state.CallAt(stackFrame.FrameTop - 1, metaMethodClosure, 1, args);
				}
				else
				{
					state.CallAt(stackFrame.FrameTop - 1, metaMethodClosure, 0, args);
				}
			}
			else
			{
				throw new LException($"Tried to call {metaMethod.Type} meta method");
			}
			stackFrame.MetaMethodStalled = true;
		}

		public static void MoveResultBack(LuaState state, LStackFrame stackFrame, int originalA)
		{
			stackFrame.MetaMethodStalled = false;
			state.Stack[stackFrame.FrameBase + originalA] =
				state.Stack[stackFrame.FrameTop - 1];
			stackFrame.FrameTop -= 1;
			state.Stack.SetTop(stackFrame.FrameTop);
			stackFrame.PC += 1;
		}
	}

	public abstract class MetaMethodTransition(int outputIndex) : ITransition
	{
		public void Transfer(LuaState state, LStackFrame stackFrame)
		{
			if (stackFrame.MetaMethodStalled)
			{
				MetaMethodHelper.MoveResultBack(state, stackFrame, outputIndex);
				stackFrame.PC += 1;
				return;
			}

			stackFrame.MetaMethodStalled = NormalOrMetaAccess(state, stackFrame);
			if (stackFrame.MetaMethodStalled)
			{
				stackFrame.PC += 1;
			}
		}

		public abstract bool NormalOrMetaAccess(LuaState state, LStackFrame stackFrame);

		protected bool CallMetaMethod(LuaState state, LStackFrame stackFrame, ILValue[] args, MetaMethodTag operation)
		{
			MetaMethodHelper.CallMetaMethod(state, stackFrame, args, operation, outputIndex);
			return outputIndex == -1;
		}
	}
}
