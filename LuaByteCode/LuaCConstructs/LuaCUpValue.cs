namespace LuaByteCode.LuaCConstructs;

public struct LuaCUpValue
{
    public required bool InStack;
    public required byte Index;
    public required byte Kind;
}