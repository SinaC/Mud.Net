﻿using Mud.Datas;

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
            get { return Abilities.AbilityManager.Instance; }
        }

        public static IClassManager ClassManager
        {
            get { return Classes.ClassManager.Instance; }
        }

        public static IRaceManager RaceManager
        {
            get { return Races.RaceManager.Instance; }
        }

        public static ILoginManager LoginManager
        {
            get { return Datas.Filesystem.LoginManager.Instance; }
            //get { return new Mud.Datas.Mongo.LoginManager(); }
        }
    }
}
