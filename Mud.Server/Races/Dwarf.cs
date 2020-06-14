using Mud.Domain;

namespace Mud.Server.Races
{
    public class Dwarf : RaceBase
    {
        #region IRace

        public override string Name => "dwarf";

        public override string ShortName => "Dwa";

        public override int GetPrimaryAttributeModifier(PrimaryAttributeTypes primaryAttribute)
        {
            return 0; // TODO: http://wow.gamepedia.com/Base_attributes#Racial_modifiers
        }

        #endregion
    }
}
