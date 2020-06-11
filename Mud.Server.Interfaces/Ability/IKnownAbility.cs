using Mud.Domain;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Ability
{
    public interface IKnownAbility
    {
        IAbility Ability { get; set; }

        ResourceKinds? ResourceKind { get; set; }

        int CostAmount { get; set; }

        CostAmountOperators CostAmountOperator { get; set; }

        int Level { get; set; } // level at which ability can be learned

        int Learned { get; set; } // practice percentage, 0 means not learned, 100 mean fully learned

        int Rating { get; set; } // how difficult is it to improve/gain/practice

        bool CanBeGained(IPlayableCharacter playableCharacter);
        bool CanBePracticed(IPlayableCharacter playableCharacter);

        KnownAbilityData MapKnownAbilityData();
    }
}
