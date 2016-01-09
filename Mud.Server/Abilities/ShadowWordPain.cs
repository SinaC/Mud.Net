using Mud.Server.Constants;

namespace Mud.Server.Abilities
{
    public class ShadowWordPain : SingleTargetManaBase // http://www.wowhead.com/spell=589/shadow-word-pain
    {
        public override string Name
        {
            get { return "Shadow Word: Pain"; }
        }

        public override int CostAmount
        {
            get { return 1; } // TODO: should be 1.5
        }

        public override int Duration
        {
            get { return 18; }
        }

        public override SchoolTypes School
        {
            get { return SchoolTypes.Shadow; }
        }

        public override DispelTypes DispelType
        {
            get { return DispelTypes.Magic; }
        }

        protected override bool Process(ICharacter source, ICharacter victim)
        {
            // Direct damage
            Damage(source, victim, 475*source.SpellPower/1000);
            // Dot
            AddDot(source, victim, 475 * source.SpellPower / 1000, 3); // 47.5% SP every 3 seconds
            return true;
        }
    }
}
