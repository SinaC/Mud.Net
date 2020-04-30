using Mud.Domain;

namespace Mud.Server.Races
{
    public class Gnome : RaceBase
    {
        #region IRace

        public override string Name => "gnome";

        public override string ShortName => "Gno";

        public override int GetAttributeModifier(CharacterAttributes attribute)
        {
            return 0; // TODO: http://wow.gamepedia.com/Base_attributes#Racial_modifiers
        }

        #endregion
    }
}
