using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class CreateFood : ItemCreationSpellBase
    {
        public override int Id => 19;
        public override string Name => "Create Food";

        public CreateFood(IRandomManager randomManager, IWiznet wiznet, IItemManager itemManager, ISettings settings)
            : base(randomManager, wiznet, itemManager, settings)
        {
        }

        public override void Action(ICharacter caster, int level, string rawParameters, params CommandParameter[] parameters)
        {
            IItemFood mushroom = ItemManager.AddItem(Guid.NewGuid(), Settings.MushroomBlueprintId, caster.Room) as IItemFood;
            mushroom?.SetHours(level / 2, level);
            caster.Act(ActOptions.ToAll, "{0} suddenly appears.", mushroom);
        }
    }
}
