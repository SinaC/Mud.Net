namespace Mud.POC.Abilities2.Interfaces
{
    public interface IPlayableCharacter : ICharacter
    {
        IPlayer ImpersonatedBy { get; }
        bool IsImmortal { get; }

        void CheckAbilityImprove(KnownAbility knownAbility, bool success, int multiplier);
    }
}
