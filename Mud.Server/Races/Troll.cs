namespace Mud.Server.Races
{
    public class Troll : RaceBase
    {
        public override string Name => "Troll";

        public override string ShortName => "Tro";

        public Troll()
        {
            AddAbility(1, "berserking");
            AddAbility(5, "test");
            AddAbility(5, "renew");
            AddAbility(50, "trash");
            AddAbility(20, "smite");
            AddAbility(20, "swiftmend");
        }
    }
}
