namespace Mud.POC.Affects
{
    public class CharacterIRVAffect : FlagAffectBase<IRVFlags>, ICharacterAffect
    {
        public IRVAffectLocations Location { get; }

        protected override string Target => Location.ToString();

        public void Apply(ICharacter character)
        {
            character.ApplyAffect(this);
        }
    }
}
