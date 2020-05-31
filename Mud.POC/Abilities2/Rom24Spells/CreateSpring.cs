using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class CreateSpring : ItemCreationSpellBase
    {
        public override int Id => 21;
        public override string Name => "Create Spring";

        public CreateSpring(IRandomManager randomManager, IWiznet wiznet, IItemManager itemManager, ISettings settings)
            : base(randomManager, wiznet, itemManager, settings)
        {
        }

        public override void Action(ICharacter caster, int level)
        {
            IItemFountain fountain = ItemManager.AddItem(Guid.NewGuid(), Settings.SpringBlueprintId, caster.Room) as IItemFountain;
            int duration = level;
            fountain?.SetTimer(TimeSpan.FromMinutes(duration));
            caster.Act(ActOptions.ToAll, "{0} flows from the ground.", fountain);
        }
    }
}
