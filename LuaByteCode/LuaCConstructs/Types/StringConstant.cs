namespace LuaByteCode.LuaCConstructs.Types;

public class StringConstant(byte[] data) : ILuaConstant
{
    public byte[] Data => data;
}