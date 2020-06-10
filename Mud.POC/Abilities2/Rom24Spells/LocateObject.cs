using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Helpers;
using Mud.Server.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Detection, PulseWaitTime = 18)]
    public class LocateObject : SpellBase
    {
        public const string SpellName = "Locate Object";

        private IItemManager ItemManager { get; }

        protected string ItemName { get; set;  }

        public LocateObject(IRandomManager randomManager, IItemManager itemManager) 
            : base(randomManager)
        {
            ItemManager = itemManager;
        }

        protected override void Invoke()
        {
            StringBuilder sb = new StringBuilder();
            int maxFound = (Caster as IPlayableCharacter)?.IsImmortal == true
                ? 200
                : Level * 2;
            int number = 0;
            IEnumerable<IItem> foundItems = FindHelpers.FindAllByName(ItemManager.Items.Where(x => Caster.CanSee(x) && !x.ItemFlags.HasFlag(ItemFlags.NoLocate) && x.Level <= Caster.Level && RandomManager.Range(1, 100) <= 2 * Level), ItemName);
            foreach (IItem item in foundItems)
            {
                IItem outOfItemContainer = item;
                // Get container until container is not an item anymore (room or character)
                int maxDepth = 500;
                while (outOfItemContainer.ContainedInto is IItem container)
                {
                    outOfItemContainer = container;
                    maxDepth--;
                    if (maxDepth <= 0) // avoid infinite loop if something goes wrong in container
                        break;
                }

                if (item.ContainedInto is IRoom room)
                {
                    if ((Caster as IPlayableCharacter)?.IsImmortal == true)
                        sb.AppendFormatLine("One is in {0} (room {1})", room.DisplayName, room.Blueprint?.Id.ToString() ?? "???");
                    else
                        sb.AppendFormatLine("One is in {0}", room.DisplayName);
                }
                else if (item.ContainedInto is ICharacter character && Caster.CanSee(character))
                    sb.AppendFormatLine("One is carried by {0}", character.DisplayName);
                else if (item.EquippedBy != null && Caster.CanSee(item.EquippedBy))
                    sb.AppendFormatLine("One is carried by {0}", item.EquippedBy.DisplayName);

                number++;
                if (number >= maxFound)
                    break;
            }
            if (number == 0)
                Caster.Send("Nothing like that in heaven or earth.");
            else
                Caster.Page(sb);
        }

        protected override string SetTargets(SpellActionInput spellActionInput)
        {
            if (string.IsNullOrWhiteSpace(spellActionInput.RawParameters))
                return "Locate what?";
            ItemName = spellActionInput.RawParameters;
            return null;
        }
    }
}
