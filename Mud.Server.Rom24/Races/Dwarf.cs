using Mud.Domain;
using Mud.Logger;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
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

        public override ICharacterFlags CharacterFlags => new CharacterFlags("Infrared");

        public override IIRVFlags Immunities => new IRVFlags();
        public override IIRVFlags Resistances => new IRVFlags("Poison", "Disease");
        public override IIRVFlags Vulnerabilities => new IRVFlags("Drowning");

        public override IBodyForms BodyForms => new BodyForms("Edible", "Sentient", "Biped", "Mammal");
        public override IBodyParts BodyParts => new BodyParts("Head", "Arms", "Legs", "Head", "Brains", "Guts", "Hands", "Feet", "Fingers", "Ear", "Eye", "Body");

        public override IActFlags ActFlags => new ActFlags();
        public override IOffensiveFlags OffensiveFlags => new OffensiveFlags();
        public override IAssistFlags AssistFlags => new AssistFlags();

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
