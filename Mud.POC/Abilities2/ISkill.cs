using Mud.Server.Input;
using Mud.POC.Abilities2.Interfaces;

namespace Mud.POC.Abilities2
{
    public interface ISkill : IAbility
    {
        UseResults Use(ICharacter user, IAbility ability, string rawParameters, params CommandParameter[] parameters);
    }

    public enum UseResults
    {
        Ok = 0,
        MissingParameter = 1,
        InvalidParameter = 2,
        InvalidTarget = 3,
        TargetNotFound = 4,
        CantUseRequiredResource = 5,
        NotEnoughResource = 6,
        Failed = 7,
        NotKnown = 8,
        InCooldown = 9,
        MustBeFighting = 10,
        Error = 11
    }
}
