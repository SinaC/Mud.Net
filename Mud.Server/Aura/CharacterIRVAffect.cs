using Mud.Domain;

namespace Mud.Server.Aura
{
    public class CharacterIRVAffect : FlagAffectBase<IRVFlags>, ICharacterAffect
    {
        public IRVAffectLocations Location { get; set; }

        protected override string Target => Location.ToString();

        public CharacterIRVAffect()
        {
        }

        public CharacterIRVAffect(CharacterIRVAffectData data)
        {
            Location = data.Location;
            Operator = data.Operator;
            Modifier = data.Modifier;
        }

        public void Apply(ICharacter character)
        {
            character.ApplyAffect(this);
        }

        public override AffectDataBase MapAffectData()
        {
            return new CharacterIRVAffectData
            {
                Location = Location,
                Operator = Operator,
                Modifier = Modifier
            };
        }
    }
}
