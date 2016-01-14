namespace Mud.Server
{
    public static class Repository
    {
        public static IServer Server
        {
            get { return Mud.Server.Server.Server.Instance; }
        }

        public static IWorld World
        {
            get { return Mud.Server.World.World.Instance; }
        }

        public static IAbilityManager AbilityManager
        {
            get { return Mud.Server.Abilities.AbilityManager.Instance; }
        }
    }
}
