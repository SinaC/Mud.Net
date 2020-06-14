using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IPlayableCharacter : ICharacter
    {
        IPlayer ImpersonatedBy { get; }
        bool IsImmortal { get; }

        IRoom RecallRoom { get; }

        void GainCondition(Conditions condition, int amount);

        void CheckAbilityImprove(IAbilityLearned abilityLearned, bool success, int multiplier);
    }
}
