using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LVM
{
	public enum InstructionEnum : byte
	{
		Move,/*      A B     R[A] := R[B]                                    */
		LoadI,/*     A sBx   R[A] := sBx                                     */
		LoadF,/*     A sBx   R[A] := (lua_Number)sBx                         */
		LoadK,/*     A Bx    R[A] := K[Bx]                                   */
		LoadKX,/*    A       R[A] := K[extra arg]                            */
		LoadFalse,/* A       R[A] := false                                   */
		LFalseSkip,/*A       R[A] := false; pc++     (*)                     */
		LoadTrue,/*  A       R[A] := true                                    */
		LoadNil,/*   A B     R[A], R[A+1], ..., R[A+B] := nil                */
		GetUpVal,/*  A B     R[A] := UpValue[B]                              */
		SetUpVal,/*  A B     UpValue[B] := R[A]                              */

		GetTabUp,/*  A B C   R[A] := UpValue[B][K[C]:shortstring]            */
		GetTable,/*  A B C   R[A] := R[B][R[C]]                              */
		GetI,/*      A B C   R[A] := R[B][C]                                 */
		GetField,/*  A B C   R[A] := R[B][K[C]:shortstring]                  */

		SetTabUp,/*  A B C   UpValue[A][K[B]:shortstring] := RK(C)           */
		SetTable,/*  A B C   R[A][R[B]] := RK(C)                             */
		SetI,/*      A B C   R[A][B] := RK(C)                                */
		SetField,/*  A B C   R[A][K[B]:shortstring] := RK(C)                 */

		NewTable,/*  A B C k R[A] := {}                                      */

		Self,/*      A B C   R[A+1] := R[B]; R[A] := R[B][RK(C):string]      */

		AddI,/*      A B sC  R[A] := R[B] + sC                               */

		AddK,/*      A B C   R[A] := R[B] + K[C]:number                      */
		SubK,/*      A B C   R[A] := R[B] - K[C]:number                      */
		MulK,/*      A B C   R[A] := R[B] * K[C]:number                      */
		ModK,/*      A B C   R[A] := R[B] % K[C]:number                      */
		PowK,/*      A B C   R[A] := R[B] ^ K[C]:number                      */
		DivK,/*      A B C   R[A] := R[B] / K[C]:number                      */
		IDivK,/*     A B C   R[A] := R[B] // K[C]:number                     */

		BAndK,/*     A B C   R[A] := R[B] & K[C]:integer                     */
		BOrK,/*      A B C   R[A] := R[B] | K[C]:integer                     */
		BXorK,/*     A B C   R[A] := R[B] ~ K[C]:integer                     */

		ShrI,/*      A B sC  R[A] := R[B] >> sC                              */
		ShlI,/*      A B sC  R[A] := sC << R[B]                              */

		Add,/*       A B C   R[A] := R[B] + R[C]                             */
		Sub,/*       A B C   R[A] := R[B] - R[C]                             */
		Mul,/*       A B C   R[A] := R[B] * R[C]                             */
		Mod,/*       A B C   R[A] := R[B] % R[C]                             */
		Pow,/*       A B C   R[A] := R[B] ^ R[C]                             */
		Div,/*       A B C   R[A] := R[B] / R[C]                             */
		IDiv,/*      A B C   R[A] := R[B] // R[C]                            */

		BAnd,/*      A B C   R[A] := R[B] & R[C]                             */
		BOr,/*       A B C   R[A] := R[B] | R[C]                             */
		BXor,/*      A B C   R[A] := R[B] ~ R[C]                             */
		Shl,/*       A B C   R[A] := R[B] << R[C]                            */
		Shr,/*       A B C   R[A] := R[B] >> R[C]                            */

		MMBIN,/*     A B C   call C metamethod over R[A] and R[B]    (*)     */
		MMBINI,/*    A sB C k        call C metamethod over R[A] and sB      */
		MMBINK,/*    A B C k         call C metamethod over R[A] and K[B]    */

		UNM,/*       A B     R[A] := -R[B]                                   */
		BNot,/*      A B     R[A] := ~R[B]                                   */
		Not,/*       A B     R[A] := not R[B]                                */
		Len,/*       A B     R[A] := #R[B] (length operator)                 */

		Concat,/*    A B     R[A] := R[A].. ... ..R[A + B - 1]               */

		Close,/*     A       close all upvalues >= R[A]                      */
		TBC,/*       A       mark variable A "to be closed"                  */
		Jmp,/*       sJ      pc += sJ                                        */
		Eq,/*        A B k   if ((R[A] == R[B]) ~= k) then pc++              */
		Lt,/*        A B k   if ((R[A] <  R[B]) ~= k) then pc++              */
		Le,/*        A B k   if ((R[A] <= R[B]) ~= k) then pc++              */

		EqK,/*       A B k   if ((R[A] == K[B]) ~= k) then pc++              */
		EqI,/*       A sB k  if ((R[A] == sB) ~= k) then pc++                */
		LtI,/*       A sB k  if ((R[A] < sB) ~= k) then pc++                 */
		LeI,/*       A sB k  if ((R[A] <= sB) ~= k) then pc++                */
		GtI,/*       A sB k  if ((R[A] > sB) ~= k) then pc++                 */
		GeI,/*       A sB k  if ((R[A] >= sB) ~= k) then pc++                */

		Test,/*      A k     if (not R[A] == k) then pc++                    */
		TestSet,/*   A B k   if (not R[B] == k) then pc++ else R[A] := R[B] (*) */

		Call,/*      A B C   R[A], ... ,R[A+C-2] := R[A](R[A+1], ... ,R[A+B-1]) */
		TailCall,/*  A B C k return R[A](R[A+1], ... ,R[A+B-1])              */

		Return,/*    A B C k return R[A], ... ,R[A+B-2]      (see note)      */
		Return0,/*           return                                          */
		Return1,/*   A       return R[A]                                     */

		ForLoop,/*   A Bx    update counters; if loop continues then pc-=Bx; */
		ForPrep,/*   A Bx    <check values and prepare counters>;
                        if not to run then pc+=Bx+1;                    */

		TForPrep,/*  A Bx    create upvalue for R[A + 3]; pc+=Bx             */
		TForCall,/*  A C     R[A+4], ... ,R[A+3+C] := R[A](R[A+1], R[A+2]);  */
		TForLoop,/*  A Bx    if R[A+2] ~= nil then { R[A]=R[A+2]; pc -= Bx } */

		SetList,/*   A B C k R[A][C+i] := R[A+i], 1 <= i <= B                */

		Closure,/*   A Bx    R[A] := closure(KPROTO[Bx])                     */

		VarArg,/*    A C     R[A], R[A+1], ..., R[A+C-2] = vararg            */

		VarArgPrep,/*A       (adjust vararg parameters)                      */

		ExtraArg/*   Ax      extra (larger) argument for previous opcode     */
	}

	public class LuaInstruction(Span<byte> bytes)
	{
		public readonly byte a = bytes[0];
		public readonly byte b = bytes[1];
		public readonly byte c = bytes[2];
		public readonly byte d = bytes[3];

		public InstructionEnum DecodeOpCode()
		{
			return (InstructionEnum)(a & 0b0111_1111u);
		}

		public byte DecodeA()
		{
			return unchecked((byte)(
				((a & 0b1000_0000u) >> 7) | ((b & 0b0111_1111u) << 1)
			));
		}

		public byte DecodeB()
		{
			return c;
		}

		public byte DecodeC()
		{
			return d;
		}

		public uint DecodeBx()
		{
			return ((b & 0b1000_0000u) >> 7) |
					(((uint)c) << 1) |
					(((uint)d) << 9);
		}

		public int DecodeSBx()
		{
			return unchecked((int)(
				((b & 0b1000_0000u) >> 7) |
				(((uint)c) << 1) |
				(((uint)d) << 9)
			) + 1) - (1 << 16);
		}

		public uint DecodeAx()
		{
			return
				((a & 0b1000_0000u) >> 7) |
				((b & 0b1111_1111u) << 1) |
				(((uint)c) << 9) |
				(((uint)d) << 17);
		}

		public int DecodeSJ()
		{
			return unchecked((int)(
				((a & 0b1000_0000u) >> 7) |
				((b & 0b1111_1111u) << 1) |
				(((uint)c) << 9) |
				(((uint)d) << 17)
			) + 1) - (1 << 24);
		}
	}

	public struct LuaIABCInstruction
	{
		public required byte A;
		public required bool k;
		public required byte B;
		public required byte C;
	}

	public struct LuaIABx
	{
		public required byte A;
		public required uint Bx;
	}

	public struct LuaIAsBx
	{
		public required byte A;
		public required int sBx;
	}

	public struct LuaIAx
	{
		public required uint Ax;
	}

	public struct LuaIsJ
	{
		public required int sJ;
	}

	public static class InstructionDecoder
	{
		public static InstructionEnum GetOpcode(LuaInstruction instruction)
		{
			return (InstructionEnum)(instruction.a & 0b0111_1111u);
		}

		public static LuaIABCInstruction DecodeIABC(LuaInstruction instruction)
		{
			return new LuaIABCInstruction
			{
				A = unchecked((byte)(
					((instruction.a & 0b1000_0000u) >> 7) | ((instruction.b & 0b0111_1111u) << 1)
				)),
				k = (instruction.b & 0b1000_0000u) != 0,
				B = instruction.c,
				C = instruction.d
			};
		}

		public static LuaIABx DecodeIABx(LuaInstruction instruction)
		{
			return new LuaIABx
			{
				A = unchecked((byte)(
					((instruction.a & 0b1000_0000u) >> 7) | ((instruction.b & 0b0111_1111u) << 1)
				)),
				Bx = ((instruction.b & 0b1000_0000u) >> 7) |
					(((uint)instruction.c) << 1) |
					(((uint)instruction.d) << 9)
			};
		}

		public static LuaIAsBx DecodeIAsBx(LuaInstruction instruction)
		{
			return new LuaIAsBx
			{
				A = unchecked((byte)(
					((instruction.a & 0b1000_0000u) >> 7) | ((instruction.b & 0b0111_1111u) << 1)
				)),
				sBx = unchecked((int)(
					((instruction.b & 0b1000_0000u) >> 7) |
					(((uint)instruction.c) << 1) |
					(((uint)instruction.d) << 9)
				) + 1) - (1 << 16)
			};
		}

		public static LuaIAx DecodeAx(LuaInstruction instruction)
		{
			return new LuaIAx
			{
				Ax = unchecked((uint)(
					((instruction.a & 0b1000_0000u) >> 7) |
					((instruction.b & 0b1111_1111u) << 1) |
					(((uint)instruction.c) << 9) |
					(((uint)instruction.d) << 17)
				))
			};
		}

		public static LuaIsJ DecodeIsJ(LuaInstruction instruction)
		{
			return new LuaIsJ
			{
				sJ = unchecked((int)(
					((instruction.a & 0b1000_0000u) >> 7) |
					((instruction.b & 0b1111_1111u) << 1) |
					(((uint)instruction.c) << 9) |
					(((uint)instruction.d) << 17)
				) + 1) - (1 << 24)
			};
		}
	}
}
