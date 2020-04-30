using Mud.Domain;

namespace Mud.Server.Races
{
    public class Troll : RaceBase
    {
        #region IRace

        public override string Name => "troll";

        public override string ShortName => "Tro";

        public override int GetAttributeModifier(CharacterAttributes attribute)
        {
            return 0; // TODO: http://wow.gamepedia.com/Base_attributes#Racial_modifiers
        }

        #endregion

        public Troll()
        {
            AddAbility(1, "berserking");
            AddAbility(5, "test");
            AddAbility(5, "renew");
            AddAbility(20, "smite");
            AddAbility(60, "Power Word: Shield");
        }
    }
}
