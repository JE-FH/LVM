using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions
{
    public class OExtraArg : ITransition
    {
        public void Transfer(LState state, LStackFrame stackFrame)
        {
            throw new LException("Extra arg instruction was reached");
        }
    }
}
