using Mud.Domain;
using System.Collections.Generic;

namespace Mud.POC.Abilities
{
    public static class Passives
    {
        public static List<IAbility> Abilities = new List<IAbility>
        {
            // Weapons
            Passive(2000, "Axe"),
            Passive(2001, "Dagger"),
            Passive(2002, "Flail"),
            Passive(2003, "Mace"),
            Passive(2004, "Polearm"),
            Passive(2005, "Spear"),
            Passive(2006, "Staves"),
            Passive(2007, "Sword"),
            Passive(2008, "Whip"),
            Passive(2009, "Hand to hand"),

            // Combat
            Passive(2100, "Parry"),
            Passive(2101, "Dodge"),
            Passive(2102, "Shield block"),
            Passive(2103, "Enhanced damage"),
            Passive(2104, "Second attack"),
            Passive(2105, "Third attack"),

            // Non-combat
            Passive(2200, "Fast healing"),
            Passive(2201, "Meditation"),
            Passive(2202, "Peek"),
            Passive(2203, "Haggle"),
        };

        private static IAbility Passive(int id, string name, AbilityFlags flags = AbilityFlags.None) => new Ability(AbilityKinds.Passive, id, name, AbilityTargets.None, 0, flags, null, null);
    }
}
