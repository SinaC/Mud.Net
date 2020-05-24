using Mud.Server.Input;
using Mud.POC.Abilities2.Interfaces;

namespace Mud.POC.Abilities2
{
    public interface ISpell : IAbility
    {
        CastResults Cast(ICharacter caster, KnownAbility knownAbility, string rawParameters, params CommandParameter[] parameters);
    }

    public enum CastResults
    {
        Ok = 0,
        MissingParameter = 1,
        InvalidParameter = 2,
        InvalidTarget = 3,
        TargetNotFound = 4,
        CantUseRequiredResource = 5,
        NotEnoughResource = 6,
        InCooldown = 7,
        Failed = 8,
        Error = 9
    }
}
