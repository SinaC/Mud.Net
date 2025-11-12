using System.Linq;
using System.Text;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Character.Information
{
    [CharacterCommand("examine", "Information", MinPosition = Positions.Resting)]
    [Syntax(
        "[cmd] item",
        "[cmd] <container>",
        "[cmd] <corpse>")]
    public class Examine : CharacterGameAction
    {
        public IEntity Target { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
               return "Examine what or whom?";

            // Search character
            Target = FindHelpers.FindByName(Actor.Room.People, actionInput.Parameters[0]);
            if (Target != null)
                return null;

            // Search item
            Target = FindHelpers.FindItemHere(Actor, actionInput.Parameters[0]);
            if (Target != null)
                return null;

            return $"You don't see any {actionInput.Parameters[0].Value}.";
        }

        public override void Execute(IActionInput actionInput)
        {
            if (Target is ICharacter victim)
                Execute(victim);
            else if (Target is IItem item)
                Execute(item);
        }

        private void Execute(ICharacter victim)
        {
            Actor.Act(ActOptions.ToAll, "{0:N} examine{0:v} {1}.", Actor, victim);
            StringBuilder sb = new StringBuilder();
            victim.Append(sb, Actor, true);
            // TODO: display race and size
            Actor.Send(sb);
        }

        private void Execute(IItem item)
        {
            Actor.Act(ActOptions.ToAll, "{0:N} examine{0:v} {1}.", Actor, item);
            StringBuilder sb = new StringBuilder();
            switch (item)
            {
                case IContainer container:
                    if (container is IItemContainer itemContainer && itemContainer.IsClosed)
                        sb.AppendLine("It's closed.");
                    else
                    {
                        sb.AppendFormatLine("{0} holds:", container.RelativeDisplayName(Actor));
                        ItemsHelpers.AppendItems(sb, container.Content.Where(x => Actor.CanSee(x)), Actor, true, true);
                    }
                    break;
                case IItemMoney money:
                    if (money.SilverCoins == 0 && money.GoldCoins == 0)
                        sb.AppendLine("Odd...there's no coins in the pile.");
                    else if (money.SilverCoins == 0 && money.GoldCoins > 0)
                    {
                        if (money.GoldCoins == 1)
                            sb.AppendLine("Wow. one gold coin.");
                        else
                            sb.AppendFormatLine("There are {0} gold coins in the pile.", money.GoldCoins);
                    }
                    else if (money.SilverCoins > 0 && money.GoldCoins == 0)
                    {
                        if (money.SilverCoins == 1)
                            sb.AppendLine("Wow. one silver coin.");
                        else
                            sb.AppendFormatLine("There are {0} silver coins in the pile.", money.SilverCoins);
                    }
                    else
                        sb.AppendFormatLine("There are {0} gold and {1} silver coins in the pile.", money.SilverCoins, money.GoldCoins);
                    break;
                default:
                    item.Append(sb, Actor, true);
                    sb.AppendLine();
                    break;
            }

            Actor.Send(sb);
        }
    }
}
