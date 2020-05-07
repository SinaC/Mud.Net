using Mud.Domain;
using Mud.Logger;

namespace Mud.Server.Races
{
    public class Elf : RaceBase
    {
        public Elf()
        {
            AddAbility("Sneak");
            AddAbility("Hide");
        }

        #region IRace

        public override string Name => "elf";

        public override string ShortName => "Elf";

        public override IRVFlags Immunities => IRVFlags.None;
        public override IRVFlags Resistances => IRVFlags.Charm;
        public override IRVFlags Vulnerabilities => IRVFlags.Iron;

        public override int GetStartAttribute(CharacterAttributes attribute)
        {
            switch (attribute)
            {
                case CharacterAttributes.Strength: return 12;
                case CharacterAttributes.Intelligence: return 14;
                case CharacterAttributes.Wisdom: return 13;
                case CharacterAttributes.Dexterity: return 15;
                case CharacterAttributes.Constitution: return 11;
                case CharacterAttributes.MaxHitPoints: return 100;
                case CharacterAttributes.SavingThrow: return 0;
                case CharacterAttributes.HitRoll: return 0;
                case CharacterAttributes.DamRoll: return 0;
                case CharacterAttributes.MaxMovePoints: return 100;
                case CharacterAttributes.ArmorBash: return 100;
                case CharacterAttributes.ArmorPierce: return 100;
                case CharacterAttributes.ArmorSlash: return 100;
                case CharacterAttributes.ArmorMagic: return 100;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Unexpected attribute {0} for Elf", attribute);
                    return 0;
            }
        }

        public override int GetMaxAttribute(CharacterAttributes attribute)
        {
            switch (attribute)
            {
                case CharacterAttributes.Strength: return 16;
                case CharacterAttributes.Intelligence: return 20;
                case CharacterAttributes.Wisdom: return 18;
                case CharacterAttributes.Dexterity: return 21;
                case CharacterAttributes.Constitution: return 15;
                case CharacterAttributes.MaxHitPoints: return 100;
                case CharacterAttributes.SavingThrow: return 0;
                case CharacterAttributes.HitRoll: return 0;
                case CharacterAttributes.DamRoll: return 0;
                case CharacterAttributes.MaxMovePoints: return 100;
                case CharacterAttributes.ArmorBash: return 100;
                case CharacterAttributes.ArmorPierce: return 100;
                case CharacterAttributes.ArmorSlash: return 100;
                case CharacterAttributes.ArmorMagic: return 100;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Unexpected attribute {0} for Elf", attribute);
                    return 0;
            }
        }

        public override int ClassExperiencePercentageMultiplier(IClass c) => c is Classes.Priest ? 125 : 100;

        #endregion
    }
}
