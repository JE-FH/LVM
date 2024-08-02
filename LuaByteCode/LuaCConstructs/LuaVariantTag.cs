namespace LuaByteCode.LuaCConstructs;

public enum LuaVariantTag : byte
{
    LuaVNil = 0,
    LuaVFalse = LuaTypeTag.Boolean | 0 << 4,
    LuaVTrue = LuaTypeTag.Boolean | 1 << 4,
    LuaVNumInt = LuaTypeTag.Number | 0 << 4,
    LuaVNumFlt = LuaTypeTag.Number | 1 << 4,
    LuaVShrStr = LuaTypeTag.String | 0 << 4,
    LuaVLngStr = LuaTypeTag.String | 1 << 4,
}