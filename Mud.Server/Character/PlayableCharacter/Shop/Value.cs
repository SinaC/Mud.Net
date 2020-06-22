using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using System.Linq;

namespace Mud.Server.Character.PlayableCharacter.Shop
{
    [PlayableCharacterCommand("value", "Shop")]
    [Syntax("[cmd] <item>")]
    public class Value : ShopPlayableCharacterGameActionBase
    {
        public IItem What { get; protected set; }
        public long Cost { get; protected set; }

        public Value(ITimeManager timeManager)
            : base(timeManager)
        {
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Value what?";


            What = FindHelpers.FindByName(Actor.Inventory.Where(x => Actor.CanSee(x)), actionInput.Parameters[0]);
            if (What == null)
                return Actor.ActPhrase("{0:N} tells you 'You don't have that item'.", Keeper.shopKeeper);

            if (!Keeper.shopKeeper.CanSee(What))
                return Actor.ActPhrase("{0:N} doesn't see what you are offering.", Keeper.shopKeeper);

            if (What.ItemFlags.HasFlag(ItemFlags.NoDrop))
                return "You can't let go of it.";

            Cost = GetSellCost(Keeper.shopKeeper, Keeper.shopBlueprint, What);
            if (Cost <= 0 || What.DecayPulseLeft > 0)
                return Actor.ActPhrase("{0:N} looks uninterested in {1}.", Keeper.shopKeeper, What);
 
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            long silver = Cost % 100;
            long gold = Cost / 100;

            Actor.Act(ActOptions.ToCharacter, "{0:N} tells you 'I'll give you {1} silver and {2} gold coins for {3}'.", Keeper.shopKeeper, silver, gold, What);
        }
    }
}
