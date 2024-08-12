using LSharp.Transitions;
using LuaByteCode.LuaCConstructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp
{
    public class LPrototype(ITransition[] transitions, int paramCount, int maxStackSize, LuaCUpValue[] upValues, LuaCPrototype source)
    {
        public ITransition[] Transitions => transitions;
		public int ParamCount => paramCount;
        public int MinStackSize => maxStackSize;
        public LuaCUpValue[] UpValues => upValues;
        public LuaCPrototype Source => source;
    }
}
