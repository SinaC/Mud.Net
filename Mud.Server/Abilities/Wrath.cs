using System;
using Mud.Server.Constants;

namespace Mud.Server.Abilities
{
    public class Wrath : SingleTargetManaBase
    {
        public override string Name
        {
            get { return "Wrath"; }
        }

        public override int CostAmount
        {
            get { return 4; } // should be 3.5
        }

        public override int Duration
        {
            get { return 0; }
        }

        public override SchoolTypes School
        {
            get { return SchoolTypes.Nature; }
        }

        protected override bool Process(ICharacter source, ICharacter victim)
        {
            Damage(source, victim, 149*source.SpellPower/100);
            return true;
        }
    }
}
