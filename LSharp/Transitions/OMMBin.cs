using LSharp.Helpers;
using LSharp.LTypes;

namespace LSharp.Transitions
{
	internal static class MetaMethodHelper {
		public static void CallMetaMethod(LuaState state, LStackFrame stackFrame, ILValue lhs, ILValue rhs, MetaMethodTag operation, int originalA)
		{
			ILValue metaMethod = ArithmeticHelper.GetMetaMethod(lhs, rhs, operation);

			if (operation == MetaMethodTag.Index && lhs is LTable && metaMethod is LTable mmTable)
			{
				state.Stack[stackFrame.FrameBase + originalA] = mmTable.GetValue(rhs);
				stackFrame.PC += 1;
				return;
			}

			ILValue[] args = [
				lhs,
				rhs
			];

			state.Stack.SetTop(stackFrame.FrameTop + 1);
			stackFrame.FrameTop += 1;
			if (metaMethod is IClosure metaMethodClosure)
			{
				state.CallAt(stackFrame.FrameTop - 1, metaMethodClosure, 1, args);
			}
			else
			{
				throw new LException($"Tried to call {metaMethod.Type} meta method");
			}
			stackFrame.MetaMethodCalled = true;
		}

		public static void MoveResultBack(LuaState state, LStackFrame stackFrame, int originalA)
		{
			stackFrame.MetaMethodCalled = false;
			state.Stack[stackFrame.FrameBase + originalA] =
				state.Stack[stackFrame.FrameTop - 1];
			stackFrame.FrameTop -= 1;
			state.Stack.SetTop(stackFrame.FrameTop);
		}
	}

	public class OMMBin(byte a, byte b, MetaMethodTag c, byte originalA) : ITransition
	{
		public void Transfer(LuaState state, LStackFrame stackFrame)
		{
			if (!stackFrame.MetaMethodCalled)
			{
				var lhs = state.Stack[stackFrame.FrameBase + a];
				var rhs = state.Stack[stackFrame.FrameBase + b];
				MetaMethodHelper.CallMetaMethod(state, stackFrame, lhs, rhs, c, originalA);
			}
			else
			{
				MetaMethodHelper.MoveResultBack(state, stackFrame, originalA);
			}
		}
	}

	public class OMMBinK(byte a, ILValue b, MetaMethodTag c, byte originalA) : ITransition
	{
		public void Transfer(LuaState state, LStackFrame stackFrame)
		{
			if (!stackFrame.MetaMethodCalled)
			{
				var lhs = state.Stack[stackFrame.FrameBase + a];
				MetaMethodHelper.CallMetaMethod(state, stackFrame, lhs, b, c, originalA);
			}
			else
			{
				MetaMethodHelper.MoveResultBack(state, stackFrame, originalA);
			}
		}
	}

	public class OMMBinKk(byte a, ILValue b, MetaMethodTag c, byte originalA) : ITransition
	{
		public void Transfer(LuaState state, LStackFrame stackFrame)
		{
			if (!stackFrame.MetaMethodCalled)
			{
				var lhs = state.Stack[stackFrame.FrameBase + a];
				MetaMethodHelper.CallMetaMethod(state, stackFrame, b, lhs, c, originalA);
			}
			else
			{
				MetaMethodHelper.MoveResultBack(state, stackFrame, originalA);
			}
		}
	}

	public class OMMBinI(byte a, LInteger b, MetaMethodTag c, byte originalA) : ITransition
	{
		public void Transfer(LuaState state, LStackFrame stackFrame)
		{
			if (!stackFrame.MetaMethodCalled)
			{
				var lhs = state.Stack[stackFrame.FrameBase + a];
				MetaMethodHelper.CallMetaMethod(state, stackFrame, lhs, b, c, originalA);
			}
			else
			{
				MetaMethodHelper.MoveResultBack(state, stackFrame, originalA);
			}
		}
	}

	public class OMMBinIk(byte a, LInteger b, MetaMethodTag c, byte originalA) : ITransition
	{
		public void Transfer(LuaState state, LStackFrame stackFrame)
		{
			if (!stackFrame.MetaMethodCalled)
			{
				var lhs = state.Stack[stackFrame.FrameBase + a];
				MetaMethodHelper.CallMetaMethod(state, stackFrame, b, lhs, c, originalA);
			}
			else
			{
				MetaMethodHelper.MoveResultBack(state, stackFrame, originalA);
			}
		}
	}
}
