namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IPlayableCharacter : ICharacter
    {
        IPlayer ImpersonatedBy { get; }
        bool IsImmortal { get; }

        void CheckAbilityImprove(AbilityLearned abilityLearned, bool success, int multiplier);
    }
}
