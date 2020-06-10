using System.Linq;
using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System.Collections.Generic;

namespace Mud.POC.Abilities2
{
    public abstract class OptionalItemInventorySpellBase : SpellBase
    {
        protected IItem Item { get; set; }

        protected OptionalItemInventorySpellBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        public override IEnumerable<IEntity> AvailableTargets(ICharacter caster) => Enumerable.Empty<IEntity>();

        protected override string SetTargets(SpellActionInput spellActionInput)
        {
            Item = null;
            if (spellActionInput.Parameters.Length >= 1)
            {
                Item = FindHelpers.FindByName(Caster.Inventory.Where(Caster.CanSee), spellActionInput.Parameters[0]); // TODO: equipments ?
                if (Item == null)
                    return "You are not carrying that.";
            }
            return null;
        }
    }
}
