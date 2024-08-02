using LuaByteCode.LuaCConstructs.Types;

namespace LuaByteCode.LuaCConstructs;

public struct LuaCPrototype
{
    public required byte[] Source;
    public required int LineDefined;
    public required int LastLineDefined;
    public required byte NumParams;
    public required bool IsVarArg;
    public required byte MaxStackSize;
    public required LuaInstruction[] Code;
    public required ILuaConstant[] Constants;
    public required LuaCUpValue[] UpValues;
    public required LuaCPrototype[] Prototypes;
    public required LuaCFunctionDebugInfo DebugInfo;


}