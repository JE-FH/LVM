namespace LuaByteCode.LuaCConstructs.Types;

public class FloatConstant(double value) : ILuaConstant
{
    public double Value => value;
}