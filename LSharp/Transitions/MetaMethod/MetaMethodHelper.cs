using LSharp.Helpers;
using LSharp.LTypes;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

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

		public static void TableSet(
			LState state,
			LStackFrame stackFrame,
			Func<LTable> getTable,
			Func<ILValue> getKey,
			Func<ILValue> getVal
		) {
			if (stackFrame.MetaMethodStalled) {
				stackFrame.PC += 1;
				return;
			}

			var table = getTable();
			var key = getKey();

			var ctx = table.HasValueMaybeUpdate(key);

			if (ctx.HasValue) {
				table.UpdateValue(ctx, getVal());
				stackFrame.PC += 1;
				return;
			}

			CallMetaMethod(state, stackFrame, [table, key, getVal()], MetaMethodTag.NewIndex);
		}

		private static ILValue PopMMReturn(LState state, LStackFrame stackFrame) {
			stackFrame.MetaMethodStalled = false;
			var result = state.Stack[stackFrame.FrameTop - 1];
			stackFrame.FrameTop -= 1;
			state.Stack.SetTop(stackFrame.FrameTop);
			stackFrame.PC += 1;
			return result;
		}

		public static void UnaryMM<T>(
			LState state,
			LStackFrame stackFrame,
			MetaMethodTag tag,
			Func<T, ILValue?> attempt,
			Func<T> getVal,
			Action<ILValue> setResult
		) where T : ILValue {
			if (stackFrame.MetaMethodStalled) {
				setResult(PopMMReturn(state, stackFrame));
				return;
			}
			var val = getVal();
			if (attempt(val) is ILValue newVal) {
				setResult(val);
				stackFrame.PC += 1;
				return;
			}
			CallMetaMethod(state, stackFrame, [val], tag);
		}

		public static void EqualityMM<T1, T2>(
			LState state,
			LStackFrame stackFrame,
			bool k,
			Func<T1, T2, bool> comparison,
			Func<(T1, T2)> getVals
		) where T1 : ILValue where T2 : ILValue {
			if (stackFrame.MetaMethodStalled) {
				if (ArithmeticHelper.IsTruthy(PopMMReturn(state, stackFrame)) != k)
					stackFrame.PC += 2;
				else
					stackFrame.PC += 1;
				return;
			}
			var (lhs, rhs) = getVals();

			if (lhs is LTable && rhs is LTable && !lhs.LEqual(rhs)) {
				CallMetaMethod(state, stackFrame, [lhs, rhs], MetaMethodTag.Eq);
				return;
			}

			if (comparison(lhs, rhs) != k)
				stackFrame.PC += 2;
			else
				stackFrame.PC += 1;
		}

		public static void OrderMM<T1, T2>(
			LState state,
			LStackFrame stackFrame,
			bool k,
			MetaMethodTag tag,
			Func<T1, T2, TernaryBool> comparison,
			Func<(T1, T2)> getVals
		) where T1 : ILValue where T2 : ILValue {
			if (stackFrame.MetaMethodStalled) {
				if (ArithmeticHelper.IsTruthy(PopMMReturn(state, stackFrame)) != k)
					stackFrame.PC += 2;
				else
					stackFrame.PC += 1;
				return;
			}
			var (lhs, rhs) = getVals();
			var res = comparison(lhs, rhs);
			if (res == TernaryBool.Unknown) {
				CallMetaMethod(state, stackFrame, [lhs, rhs], tag);
				return;
			}

			if (res != (k ? TernaryBool.True : TernaryBool.False))
				stackFrame.PC += 2;
			else
				stackFrame.PC += 1;
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
				return;
			}

			stackFrame.PC += Comparison(lhs, rhs) == k ? 2 : 1;
		}

		protected abstract bool Comparison(T1 lhs, T2 rhs);

		protected abstract (T1, T2) GetComparedValues(LState state, LStackFrame stackFrame);
	}
}
