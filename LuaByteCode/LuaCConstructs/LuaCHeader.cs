namespace LuaByteCode.LuaCConstructs;

public struct LuaCHeader
{
    public required byte LuaVersion;
    public required byte LuaFormat;
    public required byte InstructionSize;
    public required byte IntegerSize;
    public required byte NumberSize;
}