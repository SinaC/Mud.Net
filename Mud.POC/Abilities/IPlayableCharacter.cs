namespace Mud.POC.Abilities
{
    public interface IPlayableCharacter : ICharacter
    {
        long Experience { get; }

        bool CheckAbilityImprove(KnownAbility ability, bool abilityUsedSuccessfully, int multiplier);

        void GainExperience(long experience); // add/substract experience
    }
}
