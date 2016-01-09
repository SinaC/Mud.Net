using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mud.Server.Constants;

namespace Mud.Server.Abilities
{
    public class Trash : AllAbilityBase // http://www.wowhead.com/spell=106830/thrash (or http://www.wowhead.com/spell=77758/thrash)
    {
        public override string Name
        {
            get { return "Trash"; }
        }

        public override ResourceKinds ResourceKind
        {
            get { return ResourceKinds.Energy; }
        }

        public override AmountOperators CostType
        {
            get { return AmountOperators.Fixed; }
        }

        public override int CostAmount
        {
            get { return 50; }
        }

        public override int Duration
        {
            get { return 15; }
        }

        public override SchoolTypes School
        {
            get { return SchoolTypes.Physical; }
        }

        public override AbilityMechanics Mechanic
        {
            get { return AbilityMechanics.Bleeding; }
        }

        protected override bool Process(ICharacter source, IReadOnlyCollection<ICharacter> victims)
        {
            foreach (ICharacter victim in victims)
            {
                // Direct damage
                Damage(source, victim, 513*source.AttackPower/100);
                // Dot
                AddDot(source, victim, 365*source.AttackPower/100, 3);
            }
            return true;
        }
    }
}
