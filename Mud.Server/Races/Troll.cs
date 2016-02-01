namespace Mud.Server.Races
{
    public class Troll : RaceBase
    {
        public override string Name
        {
            get { return "Troll"; }
        }

        public override string ShortName
        {
            get { return "Tro"; }
        }

        public Troll()
        {
            AddAbility(1, "berserking");
        }
    }
}
