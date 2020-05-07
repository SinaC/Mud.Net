using System;
using System.Text;

namespace Mud.POC.Affects
{
    public abstract class FlagAffectBase<T> : IAffect
        where T:Enum
    {
        public AffectOperators Operator { get; set; } // Add and Or are identical
        public T Modifier { get; set; }

        protected abstract string Target { get; }

        public void Append(StringBuilder sb)
        {
            sb.AppendFormat("%c%modifies %y%{0} %c%{1} %y%{2}", Target, Operator.PrettyPrint(), Modifier);
        }
    }
}