﻿using Mud.Domain;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Affect
{
    public class CharacterIRVAffect : FlagAffectBase<IRVFlags>, ICharacterIRVAffect
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
