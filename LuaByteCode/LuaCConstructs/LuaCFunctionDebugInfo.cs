namespace LuaByteCode.LuaCConstructs;

public struct LuaCFunctionDebugInfo
{
    public required byte[] LineInfo;
    public required LuaCAbsLineInfo[] AbsLineInfo;
    public required LuaCLocVar[] LocVars;
    public required byte[][] UpValueNames;
}