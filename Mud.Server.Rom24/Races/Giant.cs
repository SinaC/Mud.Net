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

        public override IIRVFlags Immunities => new IRVFlags();
        public override IIRVFlags Resistances => new IRVFlags("Fire", "Cold");
        public override IIRVFlags Vulnerabilities => new IRVFlags("Mental", "Lightning");

        public override IBodyForms BodyForms => new BodyForms("Edible", "Sentient", "Biped", "Mammal");
        public override IBodyParts BodyParts => new BodyParts("Head", "Arms", "Legs", "Head", "Brains", "Guts", "Hands", "Feet", "Fingers", "Ear", "Eye", "Body");

        public override IActFlags ActFlags => new ActFlags();
        public override IOffensiveFlags OffensiveFlags => new OffensiveFlags();
        public override IAssistFlags AssistFlags => new AssistFlags();

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
