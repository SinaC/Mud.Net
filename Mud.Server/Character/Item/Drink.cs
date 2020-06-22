using Mud.Domain;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Table;
using Mud.Server.Random;
using Mud.Server.Rom24.Affects;
using System;
using System.Linq;

namespace Mud.Server.Character.Item
{
    [CharacterCommand("drink", "Drink", MinPosition = Positions.Resting)]
    [Syntax(
    "[cmd]",
    "[cmd] <container>")]
    public class Drink : CharacterGameAction
    {
        private ITableValues TableValues { get; }
        private IRandomManager RandomManager { get; }
        private IAuraManager AuraManager { get; }
        private IWiznet Wiznet { get; }

        public IItemDrinkable Drinkable { get; protected set; }
        public (string name, string color, int proof, int full, int thirst, int food, int servingsize) LiquidInfo { get; protected set; }

        public Drink(ITableValues tableValues, IRandomManager randomManager, IAuraManager auraManager, IWiznet wiznet)
        {
            TableValues = tableValues;
            RandomManager = randomManager;
            AuraManager = auraManager;
            Wiznet = wiznet;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            // fountain in room
            if (actionInput.Parameters.Length == 0)
            {
                Drinkable = Actor.Room.Content.OfType<IItemDrinkable>().FirstOrDefault();
                if (Drinkable == null)
                    return "Drink what?";
            }
            // search in room/inventory/equipment
            else
            {
                Drinkable = FindHelpers.FindItemHere<IItemDrinkable>(Actor, actionInput.Parameters[0]);
                if (Drinkable == null)
                    return StringHelpers.CantFindIt;
            }
            // from here, we are sure we have a drinkable item
            IPlayableCharacter pc = Actor as IPlayableCharacter;
            // drunk ?
            if (pc?[Conditions.Drunk] > 10)
                return "You fail to reach your mouth.  *Hic*";

            // get liquid info
            LiquidInfo = TableValues.LiquidInfo(Drinkable.LiquidName);
            if (LiquidInfo == default)
            {
                Wiznet.Wiznet($"Invalid liquid name {Drinkable.LiquidName} item {Drinkable.DebugName}", WiznetFlags.Bugs, AdminLevels.Implementor);
                return "You can't drink from that.";
            }
            // empty
            if (Drinkable.IsEmpty)
                return "It is already empty.";
            // full ?
            if (pc?[Conditions.Full] > 45 && pc?.IsImmortal != true)
                return "You're too full to drink more.";

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            IPlayableCharacter pc = Actor as IPlayableCharacter;
            // compute amount (limited to liquid left)
            int amount = Math.Min(Drinkable.LiquidLeft, LiquidInfo.servingsize * Drinkable.LiquidAmountMultiplier);
            // drink
            Drinkable.Drink(amount);
            Actor.Act(ActOptions.ToAll, "{0:N} drink{0:v} {1} from {2}.", this, LiquidInfo.name, Drinkable);
            // drunk/thirst/food/full
            if (pc != null)
            {
                pc.GainCondition(Conditions.Drunk, (amount * LiquidInfo.proof) / 36);
                pc.GainCondition(Conditions.Full, (amount * LiquidInfo.full) / 4);
                pc.GainCondition(Conditions.Thirst, (amount * LiquidInfo.thirst) / 10);
                pc.GainCondition(Conditions.Hunger, (amount * LiquidInfo.food) / 2);

                if (pc[Conditions.Drunk] > 10)
                    Actor.Send("You feel drunk.");
                if (pc[Conditions.Full] > 40)
                    Actor.Send("You are full.");
                if (pc[Conditions.Thirst] > 40)
                    Actor.Send("Your thirst is quenched.");
            }
            // poisoned?
            if (Drinkable is IItemPoisonable poisonable && poisonable.IsPoisoned)
            {
                Actor.Act(ActOptions.ToAll, "{0:N} choke{0:v} and gag{0:v}.", this);
                // search poison affect
                IAura poisonAura = Actor.GetAura("Poison");
                int duration = amount * 3;
                int level = RandomManager.Fuzzy(amount);
                if (poisonAura != null)
                    poisonAura.Update(level, TimeSpan.FromMinutes(duration));
                else
                    AuraManager.AddAura(Actor, "Poison", Drinkable, level, TimeSpan.FromMinutes(duration), AuraFlags.None, false,
                        new CharacterFlagsAffect { Modifier = CharacterFlags.Poison, Operator = AffectOperators.Or },
                        new PoisonDamageAffect());
                Actor.Recompute();
            }
        }
    }
}
