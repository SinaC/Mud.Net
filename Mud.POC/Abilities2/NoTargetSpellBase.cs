using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System.Collections.Generic;
using System.Linq;

namespace Mud.POC.Abilities2
{
    public abstract class NoTargetSpellBase : SpellBase
    {
        protected NoTargetSpellBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        public override IEnumerable<IEntity> AvailableTargets(ICharacter caster) => Enumerable.Empty<IEntity>();

        protected override string SetTargets(SpellActionInput spellActionInput) => null;
    }
}
