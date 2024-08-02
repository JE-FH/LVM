namespace LuaByteCode.LuaCConstructs;

public enum LuaTypeTag : byte
{
    Nil = 0,
    Boolean = 1,
    LightUserData = 2,
    Number = 3,
    String = 4,
    Table = 5,
    Function = 6,
    UserData = 7,
    Thread = 8,
    None = 0xFF
}