using Mud.Datas;

namespace Mud.Server
{
    public static class Repository
    {
        public static IServer Server => Mud.Server.Server.Server.Instance;

        public static IWorld World => Mud.Server.World.World.Instance;

        public static IAbilityManager AbilityManager => Abilities.AbilityManager.Instance;

        public static IClassManager ClassManager => Classes.ClassManager.Instance;

        public static IRaceManager RaceManager => Races.RaceManager.Instance;

        public static ILoginManager LoginManager => Datas.Filesystem.LoginManager.Instance;

        public static IPlayerManager PlayerManager => Datas.Filesystem.PlayerManager.Instance;

        public static IAdminManager AdminManager => Datas.Filesystem.AdminManager.Instance;
    }
}
