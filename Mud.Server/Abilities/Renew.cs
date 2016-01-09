using Mud.Server.Constants;

namespace Mud.Server.Abilities
{
    public class Renew : SingleTargetManaBase // http://www.wowhead.com/spell=139/renew
    {
        public override string Name
        {
            get { return "Renew"; }
        }

        public override int CostAmount
        {
            get { return 2; } // TODO: should be 1.5
        }

        public override int Duration
        {
            get { return 12; }
        }

        public override SchoolTypes School
        {
            get { return SchoolTypes.Holy; }
        }

        public override DispelTypes DispelType
        {
            get { return DispelTypes.Magic; }
        }

        protected override bool Process(ICharacter source, ICharacter victim)
        {
            // Direct heal
            Heal(source, victim, 22*source.SpellPower/100); // TODO: only if level 64 and spec as Holy
            // Hot
            AddHot(source, victim, 44 * source.SpellPower/100, 3);
            return true;
        }
    }
}
