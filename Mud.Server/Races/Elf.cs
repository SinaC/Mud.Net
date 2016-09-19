using Mud.Server.Constants;

namespace Mud.Server.Races
{
    public class Elf : RaceBase
    {
        #region IRace

        public override string Name => "elf";

        public override string ShortName => "Elf";

        public override int GetPrimaryAttributeModifier(PrimaryAttributeTypes primaryAttribute)
        {
            return 0; // TODO: http://wow.gamepedia.com/Base_attributes#Racial_modifiers
        }

        #endregion
    }
}
