using System.Text;
using Mud.DataStructures.Flags;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.Interfaces.Affect;

namespace Mud.Server.Affects
{
    public abstract class FlagsAffectBase : IFlagsAffect
    {
        public AffectOperators Operator { get; set; } // Add and Or are identical
        public Flags Modifier { get; set; }

        protected abstract string Target { get; }

        public void Append(StringBuilder sb)
        {
            sb.AppendFormat("%c%modifies %y%{0} %c%{1} %y%{2}%x%", Target, Operator.PrettyPrint(), Modifier.ToString());
        }

        public abstract AffectDataBase MapAffectData();
    }
}