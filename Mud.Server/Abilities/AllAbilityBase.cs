using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.Server.Input;

namespace Mud.Server.Abilities
{
    public abstract class AllAbilityBase : AbilityBase
    {
        public override bool Process(ICharacter source, params CommandParameter[] parameters)
        {
            // TODO: only enemy (non-impersonated, not in group, no slave)
            IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(source.Room.People.Where(x => x != source).ToList());
            if (clone.Any())
                return Process(source, clone);
            return true;
        }

        protected abstract bool Process(ICharacter source, IReadOnlyCollection<ICharacter> victims);
    }
}
