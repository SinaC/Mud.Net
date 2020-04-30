using Mud.Domain;

namespace Mud.Server.Races
{
    public class Human : RaceBase
    {
        #region IRace

        public override string Name => "human";

        public override string ShortName => "Hum";

        public override int GetAttributeModifier(CharacterAttributes attribute)
        {
            return 0; // TODO: http://wow.gamepedia.com/Base_attributes#Racial_modifiers
        }

        #endregion
    }
}
