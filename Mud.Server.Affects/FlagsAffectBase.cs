using Mud.Common;
using Mud.DataStructures.Flags;
using Mud.Domain.SerializationData;
using Mud.Server.Common.Extensions;
using Mud.Server.Domain;
using Mud.Server.Interfaces.Affect;
using System.Text;

namespace Mud.Server.Affects;

public abstract class FlagsAffectBase<TFlag> : IFlagsAffect<TFlag>
    where TFlag : IFlags<string>
{
    public AffectOperators Operator { get; set; } // Add and Or are identical
    public TFlag Modifier { get; set; } = default!;

    protected abstract string Target { get; }

    public void Append(StringBuilder sb)
    {
        sb.AppendFormat("%c%modifies %y%{0} %c%{1} %y%{2}%x%", Target.ToPascalCase(), Operator.PrettyPrint(), Modifier.ToString());
    }

    public abstract AffectDataBase MapAffectData();
}