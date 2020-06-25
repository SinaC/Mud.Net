using Mud.Domain;
using Mud.Logger;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races
{
    public class Giant : PlayableRaceBase
    {
        public Giant(IAbilityManager abilityManager)
            : base(abilityManager)
        {
            AddAbility("Bash");
            AddAbility("Fast healing");
        }

        #region IRace

        public override string Name => "giant";
        public override string ShortName => "Gia";

        public override Sizes Size => Sizes.Large;

        public override ICharacterFlags CharacterFlags => new CharacterFlags();

        public override IRVFlags Immunities => IRVFlags.None;
        public override IRVFlags Resistances => IRVFlags.Fire | IRVFlags.Cold;
        public override IRVFlags Vulnerabilities => IRVFlags.Mental | IRVFlags.Lightning;

        public override BodyForms BodyForms => BodyForms.Edible | BodyForms.Sentient | BodyForms.Biped | BodyForms.Mammal;
        public override BodyParts BodyParts => BodyParts.Head | BodyParts.Arms | BodyParts.Legs | BodyParts.Head | BodyParts.Brains | BodyParts.Guts | BodyParts.Hands | BodyParts.Feet | BodyParts.Fingers | BodyParts.Ear | BodyParts.Eye | BodyParts.Body;

        public override ActFlags ActFlags => ActFlags.None;
        public override OffensiveFlags OffensiveFlags => OffensiveFlags.None;
        public override AssistFlags AssistFlags => AssistFlags.None;

        public override int GetStartAttribute(CharacterAttributes attribute)
        {
            switch (attribute)
            {
                case CharacterAttributes.Strength: return 16;
                case CharacterAttributes.Intelligence: return 11;
                case CharacterAttributes.Wisdom: return 13;
                case CharacterAttributes.Dexterity: return 11;
                case CharacterAttributes.Constitution: return 14;
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
                case CharacterAttributes.Strength: return 22;
                case CharacterAttributes.Intelligence: return 15;
                case CharacterAttributes.Wisdom: return 18;
                case CharacterAttributes.Dexterity: return 15;
                case CharacterAttributes.Constitution: return 20;
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
                return 200;
            if (c is Classes.Cleric)
                return 150;
            if (c is Classes.Thief)
                return 150;
            if (c is Classes.Warrior)
                return 105;
            return 100;
        }

        #endregion
    }
}
