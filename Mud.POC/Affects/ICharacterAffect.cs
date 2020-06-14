namespace Mud.POC.Affects
{
    public interface ICharacterAffect : IAffect
    {
        // Attributes

        void Apply(ICharacter character);
    }
}
