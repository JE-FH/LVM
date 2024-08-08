using LSharp.Helpers;
using LSharp.LTypes;

namespace LSharp.Transitions.MetaMethod
{
	internal static class MetaMethodHelper
	{
		public static void CallMetaMethod(LState state, LStackFrame stackFrame, ILValue[] args, MetaMethodTag operation, int outputIndex = -1)
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

		public static void MoveResultBack(LState state, LStackFrame stackFrame, int originalA)
		{
			state.Stack[stackFrame.FrameBase + originalA] = GetResult(state, stackFrame);
		}

		public static ILValue GetResult(LState state, LStackFrame stackFrame)
		{
			stackFrame.MetaMethodStalled = false;
			var result = state.Stack[stackFrame.FrameTop - 1];
			stackFrame.FrameTop -= 1;
			state.Stack.SetTop(stackFrame.FrameTop);
			stackFrame.PC += 1;
			return result;
		}
	}

	public abstract class MetaMethodTransition(int outputIndex) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame)
		{
			if (stackFrame.MetaMethodStalled)
			{
				MetaMethodHelper.MoveResultBack(state, stackFrame, outputIndex);
				stackFrame.PC += 1;
				return;
			}

			stackFrame.MetaMethodStalled = NormalOrMetaAccess(state, stackFrame);
			if (!stackFrame.MetaMethodStalled)
			{
				stackFrame.PC += 1;
			}
		}

		public abstract bool NormalOrMetaAccess(LState state, LStackFrame stackFrame);

		protected bool CallMetaMethod(LState state, LStackFrame stackFrame, ILValue[] args, MetaMethodTag operation)
		{
			MetaMethodHelper.CallMetaMethod(state, stackFrame, args, operation, outputIndex);
			return outputIndex == -1;
		}

		protected bool UpsertTable(LState state, LStackFrame stackFrame, LTable table, ILValue key, ILValue val)
		{
			var updateCtx = table.HasValueMaybeUpdate(key);

			if (!updateCtx.HasValue && table.GetMetaMethod(MetaMethodTag.NewIndex) != null)
				return CallMetaMethod(state, stackFrame, [table, key, val], MetaMethodTag.NewIndex);

			if (updateCtx.HasValue)
				table.UpdateValue(updateCtx, val);
			
			table.SetValue(key, val);

			return false;
		}
	}

	public abstract class EqualityTransition<T1, T2>(bool k) : ITransition
		where T1 : ILValue 
		where T2 : ILValue
	{
		public void Transfer(LState state, LStackFrame stackFrame)
		{
			if (stackFrame.MetaMethodStalled)
			{
				var res = MetaMethodHelper.GetResult(state, stackFrame);
				stackFrame.PC += ArithmeticHelper.IsTruthy(res) == k ? 2 : 1;

				return;
			}

			var (lhs, rhs) = GetComparedValues(state, stackFrame);

			if (lhs is LTable && rhs is LTable && !lhs.LEqual(rhs))
			{
				MetaMethodHelper.CallMetaMethod(state, stackFrame, [lhs, rhs], MetaMethodTag.Eq);
				stackFrame.MetaMethodStalled = true;

				return;
			}

			stackFrame.PC += Comparison(lhs, rhs) == k ? 2 : 1;
		}

		protected abstract bool Comparison(T1 lhs, T2 rhs);

		protected abstract (T1, T2) GetComparedValues(LState state, LStackFrame stackFrame);
	}
}
