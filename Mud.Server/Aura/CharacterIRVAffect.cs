using Mud.Domain;

namespace Mud.Server.Aura
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
