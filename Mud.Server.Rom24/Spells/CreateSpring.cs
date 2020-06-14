using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Settings;
using System;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Creation)]
    public class CreateSpring : ItemCreationSpellBase
    {
        public const string SpellName = "Create Spring";

        public CreateSpring(IRandomManager randomManager, IItemManager itemManager, ISettings settings)
            : base(randomManager, itemManager, settings)
        {
        }

        protected override void Invoke()
        {
            IItemFountain fountain = ItemManager.AddItem(Guid.NewGuid(), Settings.SpringBlueprintId, Caster.Room) as IItemFountain;
            int duration = Level;
            fountain?.SetTimer(TimeSpan.FromMinutes(duration));
            Caster.Act(ActOptions.ToAll, "{0} flows from the ground.", fountain);
        }
    }
}
