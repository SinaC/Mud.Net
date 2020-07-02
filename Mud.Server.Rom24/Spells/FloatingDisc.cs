using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Settings.Interfaces;
using System;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Creation, PulseWaitTime = 24)]
    public class FloatingDisc : ItemCreationSpellBase
    {
        public const string SpellName = "Floating Disc";

        private IWiznet Wiznet { get; }

        public FloatingDisc(IRandomManager randomManager, IWiznet wiznet, IItemManager itemManager, ISettings settings)
            : base(randomManager, itemManager, settings)
        {
            Wiznet = wiznet;
        }

        protected override void Invoke()
        {
            // TODO: using data is kindy hacky to perform a custom level item
            IItem item = ItemManager.AddItem(Guid.NewGuid(), Settings.FloatingDiscBlueprintId, Caster);
            if (!(item is IItemContainer floatingDisc))
            {
                Caster.Send("Somehing went wrong.");
                Wiznet.Wiznet($"SpellFloatingDisc: blueprint {Settings.FloatingDiscBlueprintId} is not a container.", WiznetFlags.Bugs, AdminLevels.Implementor);
                ItemManager.RemoveItem(item); // destroy it if invalid
                return;
            }
            int maxWeight = Level * 10;
            int maxWeightPerItem = Level * 5;
            int duration = Level * 2 - RandomManager.Range(0, Level / 2);
            floatingDisc.SetTimer(TimeSpan.FromMinutes(duration));
            floatingDisc.SetCustomValues(Level, maxWeight, maxWeightPerItem);

            Caster.Act(ActOptions.ToGroup, "{0:N} has created a floating black disc.", Caster);
            Caster.Send("You create a floating disc.");
            // TODO: Try to equip it ?
        }
    }
}
