using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class CreateWater : ItemInventorySpellBase<IItemDrinkContainer>
    {

        public override int Id => 22;
        public override string Name => "Create Water";
        public override AbilityEffects Effects => AbilityEffects.Creation;

        private ITimeManager TimeManager { get; }
        public CreateWater(IRandomManager randomManager, IWiznet wiznet, ITimeManager timeManager)
            : base(randomManager, wiznet)
        {
            TimeManager = timeManager;
        }

        protected override string InvalidItemTypeMsg => "It is unable to hold water.";

        public override void Action(ICharacter caster, int level, IItemDrinkContainer item)
        {
            if (item.LiquidName != "water" && !item.IsEmpty)
            {
                caster.Send("It contains some other liquid.");
                return;
            }

            int multiplier = TimeManager.SkyState == SkyStates.Raining
                ? 4
                : 2;
            int water = Math.Min(level * multiplier, item.MaxLiquid - item.LiquidLeft);
            if (water > 0)
            {
                item.Fill("water", water);
                caster.Act(ActOptions.ToCharacter, "{0:N} is filled.", item);
            }
        }
    }
}
