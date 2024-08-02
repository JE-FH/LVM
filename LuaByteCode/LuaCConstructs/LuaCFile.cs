namespace LuaByteCode.LuaCConstructs;

public struct LuaCFile
{
    public required LuaCHeader Header;
    public required byte UpValueSize;
    public required LuaCPrototype TopLevelFunction;
}