using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Create Water", AbilityEffects.Creation)]
    public class CreateWater : ItemInventorySpellBase<IItemDrinkContainer>
    {
        private ITimeManager TimeManager { get; }
        public CreateWater(IRandomManager randomManager, IWiznet wiznet, ITimeManager timeManager)
            : base(randomManager, wiznet)
        {
            TimeManager = timeManager;
        }

        protected override string InvalidItemTypeMsg => "It is unable to hold water.";

        protected override void Invoke()
        {
            int multiplier = TimeManager.SkyState == SkyStates.Raining
                ? 4
                : 2;
            int water = Math.Min(Level * multiplier, Item.MaxLiquid - Item.LiquidLeft);
            if (water > 0)
            {
                Item.Fill("water", water);
                Caster.Act(ActOptions.ToCharacter, "{0:N} is filled.", Item);
            }
        }

        public override string Setup(AbilityActionInput abilityActionInput)
        {
            string baseGuards = base.Setup(abilityActionInput);
            if (baseGuards != null)
                return baseGuards;
            if (Item.LiquidName != "water" && !Item.IsEmpty)
                return "It contains some other liquid.";
            return null;
        }
    }
}
