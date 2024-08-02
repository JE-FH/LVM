namespace LuaByteCode.LuaCConstructs.Types;

public class BoolConstant(bool value) : ILuaConstant
{
    public bool Value => value;
}