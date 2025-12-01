using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;
using System.Text;

namespace Mud.Server.Flags;

[Export(typeof(ICharacterFlags))]
public class CharacterFlags : Flags<ICharacterFlagValues>, ICharacterFlags
{
    public CharacterFlags(ICharacterFlagValues flagValues)
        : base(flagValues)
    {
    }

    public StringBuilder Append(StringBuilder sb, bool shortDisplay)
    {
        foreach (var flag in Values)
        {
            var flagPrettyPrint = FlagValues.PrettyPrint(flag, shortDisplay);
            if (!string.IsNullOrWhiteSpace(flagPrettyPrint))
                sb.Append(flagPrettyPrint);
        }
        return sb;
    }
}
