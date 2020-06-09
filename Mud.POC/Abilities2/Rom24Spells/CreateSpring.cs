using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
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
