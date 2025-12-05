using Mud.DataStructures.Flags;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Domain.SerializationData;
using Mud.Server.Interfaces.Affect;
using System.Text;

namespace Mud.Server.Affects;

public abstract class FlagsAffectBase<TFlag, TFlagValues> : IFlagsAffect<TFlag, TFlagValues>
    where TFlag : IFlags<string, TFlagValues>
    where TFlagValues : IFlagValues<string>
{
    public AffectOperators Operator { get; set; } // Add and Or are identical
    public TFlag Modifier { get; set; } = default!;

    protected abstract string Target { get; }

    public void Append(StringBuilder sb)
    {
        sb.AppendFormat("%c%modifies %y%{0} %c%{1} %y%{2}%x%", Target, Operator.PrettyPrint(), Modifier.ToString());
    }

    public abstract AffectDataBase MapAffectData();
}