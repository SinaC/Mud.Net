using System.Collections.Generic;
using Mud.Server.Constants;

namespace Mud.Server.Abilities
{
    // TODO: delete whole class
    public class ToDeleteAbilities
    {
        public List<ToRemoveAbility> AbilityList = new List<ToRemoveAbility>
        {
            new ToRemoveAbility // http://www.wowhead.com/spell=589/shadow-word-pain
            {
                Name = "Shadow Word: Pain",

                ResourceKind = ResourceKinds.Mana,
                CostType = AmountOperators.Percentage,
                CostAmount = 1, // should be 0.25

                GlobalCooldown = 1,
                Cooldown = 0,
                Duration = 18,

                School = SchoolTypes.Shadow,
                Mechanic = AbilityMechanics.None,
                DispelType = DispelTypes.Magic,

                Flags = AbilityFlags.CannotBeUsedWhileShapeshifted

                //EFFECTS
                //Effect #1	School Damage (Shadow)
                //Value: 1 (SP mod: 0.475)
                //Effect #2	Apply Aura: Periodic Damage
                //Value: 1 every 3 seconds (SP mod: 0.475)
            },
            new ToRemoveAbility // http://www.wowhead.com/spell=1943/rupture
            {
                Name = "Rupture",

                ResourceKind = ResourceKinds.Energy,
                CostType = AmountOperators.Fixed,
                CostAmount = 25, // should be 0.25

                GlobalCooldown = 1,
                Cooldown = 0,
                Duration = 4,

                School = SchoolTypes.Physical,
                Mechanic = AbilityMechanics.Bleeding,
                DispelType = DispelTypes.None,

                Flags = AbilityFlags.CannotBeUsedWhileShapeshifted | AbilityFlags.RequiresMainHand | AbilityFlags.RequiresComboPoints

                //EFFECTS
                //Effect #1	Apply Aura: Periodic Damage
                //Value: 1 every 2 seconds (AP mod: 0.0685)
                //Mechanic: Bleeding
                //Effect #2	Apply Aura: Periodic Dummy
                //Value: 1 every 2 seconds
                //Mechanic: Bleeding
            },
            new ToRemoveAbility // http://www.wowhead.com/spell=1943/rupture
            {
                Name = "Battle Shout",

                ResourceKind = ResourceKinds.None,
                CostType = AmountOperators.Fixed,
                CostAmount = 0, // should be 0.25

                GlobalCooldown = 1,
                Cooldown = 0,
                Duration = 1*60*60,

                School = SchoolTypes.Physical,
                Mechanic = AbilityMechanics.None,
                DispelType = DispelTypes.None,

                Flags = AbilityFlags.CannotBeUsedWhileShapeshifted

                //EFFECTS
                //Effect #1	Apply Aura: Mod Melee Attack Power - %
                //Value: 10%
                //Radius: 100 yards
                //Effect #2	Apply Aura: Mod Ranged Attack Power - % (1)
                //Value: 10%
                //Radius: 100 yards
            },
            new ToRemoveAbility // http://www.wowhead.com/spell=528/dispel-magic
            {
                Name = "Dispel Magic",

                ResourceKind = ResourceKinds.Mana,
                CostType = AmountOperators.Percentage,
                CostAmount = 3, // should be 2.6

                GlobalCooldown = 1,
                Cooldown = 0,
                Duration = 0,

                School = SchoolTypes.Holy,
                Mechanic = AbilityMechanics.None,
                DispelType = DispelTypes.Magic,

                Flags = AbilityFlags.CannotBeUsedWhileShapeshifted

                //EFFECTS
                //Effect	Dispel (Magic)
                //Value: 1
            },
            new ToRemoveAbility // http://www.wowhead.com/spell=32375/mass-dispel
            {
                Name = "Mass Dispel",

                ResourceKind = ResourceKinds.Mana,
                CostType = AmountOperators.Percentage,
                CostAmount = 13,

                GlobalCooldown = 1,
                Cooldown = 15,
                Duration = 0,

                School = SchoolTypes.Holy,
                Mechanic = AbilityMechanics.None,
                DispelType = DispelTypes.Magic,

                Flags = AbilityFlags.CannotBeUsedWhileShapeshifted

                //EFFECTS
                //Effect	Dispel (Magic)
                //Value: 100
            },
            new ToRemoveAbility // http://www.wowhead.com/spell=47541/death-coil
            {
                Name = "Death Coil",

                ResourceKind = ResourceKinds.Runic,
                CostType = AmountOperators.Fixed,
                CostAmount = 30,

                GlobalCooldown = 1,
                Cooldown = 0,
                Duration = 0,

                School = SchoolTypes.Shadow,
                Mechanic = AbilityMechanics.None,
                DispelType = DispelTypes.None,

                Flags = AbilityFlags.None

                //EFFECTS
                //Effect	Dummy
                //Value: 167
                //Server-side script
            },
            new ToRemoveAbility // http://www.wowhead.com/spell=26297/berserking
            {
                Name = "Berserking",

                ResourceKind = ResourceKinds.None,
                CostType = AmountOperators.Fixed,
                CostAmount = 0,

                GlobalCooldown = 1,
                Cooldown = 3*60,
                Duration = 10,

                School = SchoolTypes.Physical,
                Mechanic = AbilityMechanics.None,
                DispelType = DispelTypes.None,

                Flags = AbilityFlags.None

                //EFFECTS
                //Effect	Apply Aura: Decrease Attack Speed %
                //Value: 15%
            },
            new ToRemoveAbility // http://www.wowhead.com/spell=139/renew
            {
                Name = "Renew",

                ResourceKind = ResourceKinds.Mana,
                CostType = AmountOperators.Percentage,
                CostAmount = 2, // should be 1.5

                GlobalCooldown = 1,
                Cooldown = 0,
                Duration = 12,

                School = SchoolTypes.Holy,
                Mechanic = AbilityMechanics.None,
                DispelType = DispelTypes.Magic,

                Flags = AbilityFlags.CannotBeUsedWhileShapeshifted

                //EFFECTS
                //Effect #1	Apply Aura: Periodic Heal
                //Value: 1 every 3 seconds (SP mod: 0.44)
                //Effect #2	Heal
                //Value: 1 (SP mod: 0.22)
            },
        };
    }
}
