﻿using Mud.Domain;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Affects
{
    public class CharacterSizeAffect : ICharacterSizeAffect
    {
        public Sizes Value { get; set; }

        public CharacterSizeAffect()
        {
        }

        public CharacterSizeAffect(CharacterSizeAffectData data)
        {
            Value = data.Value;
        }

        public void Append(StringBuilder sb)
        {
            sb.AppendFormat("%c%modifies %y%size %c%by setting to %y%{0}%x%", Value);
        }

        public void Apply(ICharacter character)
        {
            character.ApplyAffect(this);
        }

        public AffectDataBase MapAffectData()
        {
            return new CharacterSizeAffectData
            {
                Value = Value
            };
        }
    }
}
