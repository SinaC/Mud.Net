namespace Mud.POC.Abilities
{
    public interface INonPlayableCharacter : ICharacter
    {
        ActFlags ActFlags { get; }

        OffensiveFlags OffensiveFlags { get; }
    }
}
