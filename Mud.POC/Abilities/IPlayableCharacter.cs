using Mud.Server.Input;

namespace Mud.POC.Abilities
{
    public interface IPlayableCharacter : ICharacter
    {
        long Experience { get; }

        bool CheckAbilityImprove(IAbility ability, bool abilityUsedSuccessfully, int multiplier);
        bool CheckAbilityImprove(KnownAbility ability, bool abilityUsedSuccessfully, int multiplier);

        void GainExperience(long experience); // add/substract experience

        bool ExecuteCommand(string command, string rawParameters, CommandParameter[] parameters);
    }
}
