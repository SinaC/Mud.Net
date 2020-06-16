﻿using Mud.Domain;
using Mud.Logger;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races
{
    public class Dwarf : PlayableRaceBase
    {
        public Dwarf(IAbilityManager abilityManager)
            : base(abilityManager)
        {
            AddAbility("Berserk");
        }

        #region IRace

        public override string Name => "dwarf";
        public override string ShortName => "Dwa";

        public override Sizes Size => Sizes.Small;

        public override CharacterFlags CharacterFlags => CharacterFlags.Infrared;

        public override IRVFlags Immunities => IRVFlags.None;
        public override IRVFlags Resistances => IRVFlags.Poison | IRVFlags.Disease;
        public override IRVFlags Vulnerabilities => IRVFlags.Drowning;

        public override BodyForms BodyForms => BodyForms.Edible | BodyForms.Sentient | BodyForms.Biped | BodyForms.Mammal;
        public override BodyParts BodyParts => BodyParts.Head | BodyParts.Arms | BodyParts.Legs | BodyParts.Head | BodyParts.Brains | BodyParts.Guts | BodyParts.Hands | BodyParts.Feet | BodyParts.Fingers | BodyParts.Ear | BodyParts.Eye | BodyParts.Body;

        public override ActFlags ActFlags => ActFlags.None;
        public override OffensiveFlags OffensiveFlags => OffensiveFlags.None;
        public override AssistFlags AssistFlags => AssistFlags.None;

        public override int GetStartAttribute(CharacterAttributes attribute)
        {
            switch (attribute)
            {
                case CharacterAttributes.Strength: return 14;
                case CharacterAttributes.Intelligence: return 12;
                case CharacterAttributes.Wisdom: return 14;
                case CharacterAttributes.Dexterity: return 10;
                case CharacterAttributes.Constitution: return 15;
                case CharacterAttributes.MaxHitPoints: return 100;
                case CharacterAttributes.SavingThrow: return 0;
                case CharacterAttributes.HitRoll: return 0;
                case CharacterAttributes.DamRoll: return 0;
                case CharacterAttributes.MaxMovePoints: return 100;
                case CharacterAttributes.ArmorBash: return 100;
                case CharacterAttributes.ArmorPierce: return 100;
                case CharacterAttributes.ArmorSlash: return 100;
                case CharacterAttributes.ArmorExotic: return 100;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Unexpected start attribute {0} for {1}", attribute, Name);
                    return 0;
            }
        }

        public override int GetMaxAttribute(CharacterAttributes attribute)
        {
            switch (attribute)
            {
                case CharacterAttributes.Strength: return 20;
                case CharacterAttributes.Intelligence: return 16;
                case CharacterAttributes.Wisdom: return 19;
                case CharacterAttributes.Dexterity: return 14;
                case CharacterAttributes.Constitution: return 21;
                case CharacterAttributes.MaxHitPoints: return 100;
                case CharacterAttributes.SavingThrow: return 0;
                case CharacterAttributes.HitRoll: return 0;
                case CharacterAttributes.DamRoll: return 0;
                case CharacterAttributes.MaxMovePoints: return 100;
                case CharacterAttributes.ArmorBash: return 100;
                case CharacterAttributes.ArmorPierce: return 100;
                case CharacterAttributes.ArmorSlash: return 100;
                case CharacterAttributes.ArmorExotic: return 100;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Unexpected max attribute {0} for {1}", attribute, Name);
                    return 0;
            }
        }

        public override int ClassExperiencePercentageMultiplier(IClass c) 
        {
            if (c is Classes.Mage)
                return 150;
            if (c is Classes.Cleric)
                return 125;
            return 100;
        }

        #endregion
    }
}