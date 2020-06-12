using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;
using Mud.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.Server.Rom24.Spells
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
