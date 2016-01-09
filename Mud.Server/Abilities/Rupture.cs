using Mud.Server.Constants;

namespace Mud.Server.Abilities
{
    public class Rupture : SingleTargetEnergyBase // http://www.wowhead.com/spell=1943/rupture
    {
        public override string Name
        {
            get { return "Rupture"; }
        }

        public override int CostAmount
        {
            get { return 25; }
        }

        public override int Duration
        {
            get { return 8; }
        }

        public override SchoolTypes School
        {
            get { return SchoolTypes.Physical; }
        }

        public override AbilityMechanics Mechanic
        {
            get { return AbilityMechanics.Bleeding; }
        }

        protected override bool Process(ICharacter source, ICharacter victim)
        {
            // Dot
            AddDot(source, victim, 685 * source.AttackPower / 10000, 2); // 6.85% AP every 2 seconds
            return true;
        }
    }
}
