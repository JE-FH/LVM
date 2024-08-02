namespace LuaByteCode.LuaCConstructs.Types;

public class IntegerConstant(long value) : ILuaConstant
{
    public long Value => value;
}