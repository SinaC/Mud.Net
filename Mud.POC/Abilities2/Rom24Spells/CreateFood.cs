using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Creation)]
    public class CreateFood : ItemCreationSpellBase
    {
        public const string SpellName = "Create Food";

        public CreateFood(IRandomManager randomManager, IItemManager itemManager, ISettings settings)
            : base(randomManager, itemManager, settings)
        {
        }

        protected override void Invoke()
        {
            IItemFood mushroom = ItemManager.AddItem(Guid.NewGuid(), Settings.MushroomBlueprintId, Caster.Room) as IItemFood;
            mushroom?.SetHours(Level / 2, Level);
            Caster.Act(ActOptions.ToAll, "{0} suddenly appears.", mushroom);
        }
    }
}
