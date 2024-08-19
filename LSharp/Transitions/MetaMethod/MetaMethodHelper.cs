using LSharp.Helpers;
using LSharp.LTypes;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

namespace LSharp.Transitions.MetaMethod
{
	internal static class MetaMethodHelper
	{
		public static void CallMetaMethod(LState state, LStackFrame stackFrame, ILValue[] args, IClosure metaMethod, bool saveResult) {
			if (saveResult) {
				state.Stack.SetTop(stackFrame.FrameTop + 1);
				stackFrame.FrameTop += 1;
				state.CallAt(stackFrame.FrameTop - 1, metaMethod, 1, args);
			} else {
				state.Call(metaMethod, args);
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

		public static void TableGetMM(
			LState state,
			LStackFrame stackFrame,
			Func<LTable> getTable,
			Func<ILValue> getKey,
			Action<ILValue> setResult
		) {
			if (stackFrame.MetaMethodStalled) {
				setResult(PopMMReturn(state, stackFrame));
				stackFrame.PC += 1;
				return;
			}

			TableGetRec(state, stackFrame, getTable(), getKey(), setResult);
		}

		private static void TableGetRec(
			LState state,
			LStackFrame stackFrame,
			LTable table,
			ILValue key,
			Action<ILValue> setResult
		) {
			var val = table.GetValue(key);
			if (val is not LNil) {
				stackFrame.PC += 1;
				setResult(val);
				return;
			}

			var metaMethod = table.GetMetaMethod(MetaMethodTag.Index);
			if (metaMethod is not null) {
				if (metaMethod is LTable innerTable) {
					TableGetRec(state, stackFrame, innerTable, key, setResult);
				} else if (metaMethod is IClosure mmClosure) {
					CallMetaMethod(state, stackFrame, [table, key], mmClosure, true);
				} else {
					throw new LException($"attempt to index a {metaMethod.Type} value");
				}
			} else {
				stackFrame.PC += 1;
				setResult(LNil.Instance);
			}
		}

		public static void TableSetMM(
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

			TableSetRec(state, stackFrame, getTable(), getKey(), getVal);
		}

		private static void TableSetRec(
			LState state,
			LStackFrame stackFrame,
			LTable table,
			ILValue key,
			Func<ILValue> getVal
		) {
			var ctx = table.HasValueMaybeUpdate(key);

			if (ctx == TableKeyReference.Invalid) {
				table.UpdateValue(ctx, getVal());
				stackFrame.PC += 1;
				return;
			}
			ILValue? metaMethod = ArithmeticHelper.GetMetaMethod(table, key, MetaMethodTag.NewIndex);
			if (metaMethod is not null) {
				if (metaMethod is LTable mmTable) {
					TableSetRec(state, stackFrame, mmTable, key, getVal);
				} else if (metaMethod is IClosure mmClosure) {
					CallMetaMethod(state, stackFrame, [table, key, getVal()], mmClosure, false);
				} else {
					throw new LException($"attempt to index a {metaMethod.Type} value");
				}
			} else {
				stackFrame.PC += 1;
				table.SetValue(key, getVal());
			}
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
				setResult(newVal);
				stackFrame.PC += 1;
				return;
			}
			ILValue? metaMethod = ArithmeticHelper.GetMetaMethod(val, tag);
			if (metaMethod is IClosure mmClosure) {
				CallMetaMethod(state, stackFrame, [val], mmClosure, true);
			} else if (metaMethod is null) {
				throw new LException("attempted to perform arithmetic operation on a table value");
			} else {
				throw new LException($"attempt to call a {(metaMethod ?? LNil.Instance).Type} value");
			}
		}

		public static void EqualityMM<T1, T2>(
			LState state,
			LStackFrame stackFrame,
			bool k,
			Func<T1, T2, bool> comparison,
			Func<(T1, T2)> getValues
		) where T1 : ILValue where T2 : ILValue {
			if (stackFrame.MetaMethodStalled) {
				if (ArithmeticHelper.IsTruthy(PopMMReturn(state, stackFrame)) != k)
					stackFrame.PC += 2;
				else
					stackFrame.PC += 1;
				return;
			}
			var (lhs, rhs) = getValues();

			if (lhs is LTable && rhs is LTable && !lhs.LEqual(rhs)) {
				var mm = ArithmeticHelper.GetMetaMethod(lhs, rhs, MetaMethodTag.Eq);
				if (mm is IClosure mmClosure) {
					CallMetaMethod(state, stackFrame, [lhs, rhs], mmClosure, true);
					return;
				} else if (mm is not null) {
					throw new LException($"attempt to call a {mm.Type} value (metamethod 'eq')");
				}
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
			Func<(T1, T2)> getValues
		) where T1 : ILValue where T2 : ILValue {
			if (stackFrame.MetaMethodStalled) {
				if (ArithmeticHelper.IsTruthy(PopMMReturn(state, stackFrame)) != k)
					stackFrame.PC += 2;
				else
					stackFrame.PC += 1;
				return;
			}
			var (lhs, rhs) = getValues();
			var res = comparison(lhs, rhs);
			if (res == TernaryBool.Unknown) {
				var metaMethod = ArithmeticHelper.GetMetaMethod(lhs, rhs, MetaMethodTag.Eq);
				if (metaMethod is IClosure mmClosure) {
					CallMetaMethod(state, stackFrame, [lhs, rhs], mmClosure, true);
				} else if (metaMethod is null) {
					throw new LException($"attempted to compare {lhs.Type} with a {rhs.Type}");
				} else {
					throw new LException($"attempt to call a {(metaMethod ?? LNil.Instance).Type} value");
				}
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
			var metaMethod = ArithmeticHelper.GetMetaMethod(args[0], args[1], operation);
			if (metaMethod is IClosure mmClosure) {
				MetaMethodHelper.CallMetaMethod(state, stackFrame, args, mmClosure, outputIndex != -1);
			} else if (metaMethod is null) {
				throw new LException($"attempted to compare {args[0].Type} with a {args[1].Type}");
			} else {
				throw new LException($"attempt to call a {(metaMethod ?? LNil.Instance).Type} value");
			}
			return outputIndex == -1;
		}
	}
}
