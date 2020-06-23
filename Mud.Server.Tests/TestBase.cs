using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.World;
using Mud.Server.Random;
using Mud.Server.Rom24.Spells;
using Mud.Server.Tests.Mocking;
using Mud.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mud.Server.Tests
{
    [TestClass]
    public abstract class TestBase
    {
        protected IWorld World => Container.DependencyContainer.Current.GetInstance<IWorld>();
        protected IRoomManager RoomManager => Container.DependencyContainer.Current.GetInstance<IRoomManager>();
        protected IItemManager ItemManager => Container.DependencyContainer.Current.GetInstance<IItemManager>();
        protected ICharacterManager CharacterManager => Container.DependencyContainer.Current.GetInstance<ICharacterManager>();
        protected IQuestManager QuestManager => Container.DependencyContainer.Current.GetInstance<IQuestManager>();

        [TestInitialize]
        public void TestInitialize()
        {
            (Container.DependencyContainer.Current.GetInstance<IWorld>() as WorldMock).Clear();
        }

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            Container.DependencyContainer.Current.Options.EnableAutoVerification = false;

            Container.DependencyContainer.Current.RegisterInstance<ISettings>(new SettingsMock());
            Container.DependencyContainer.Current.RegisterInstance<ITimeManager>(new TimeManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IRaceManager>(new RaceManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IClassManager>(new ClassManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IAbilityManager>(new AbilityManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IWorld>(new WorldMock());
            Container.DependencyContainer.Current.RegisterInstance<IWiznet>(new WiznetMock());
            Container.DependencyContainer.Current.RegisterInstance<IRoomManager>(new RoomManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IAreaManager>(new AreaManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<ICharacterManager>(new CharacterManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IItemManager>(new ItemManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IQuestManager>(new QuestManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IRandomManager>(new RandomManager());
            Container.DependencyContainer.Current.RegisterInstance<IGameActionManager>(new GameActionManager(new AssemblyHelper()));
            //Container.DependencyContainer.Current.RegisterInstance<IPlayerManager>(new PlayerManagerMock());
            //Container.DependencyContainer.Current.RegisterInstance<IAdminManager>(new AdminManagerMock());
            //Container.DependencyContainer.Current.RegisterInstance<IServerPlayerCommand>(new ServerPlayerCommandMock());
            //Container.DependencyContainer.Current.RegisterInstance<IServerAdminCommand>(new ServerAdminCommandMock());
            //Container.DependencyContainer.Current.RegisterInstance<ILoginRepository>(new LoginRepositoryMock());
            //Container.DependencyContainer.Current.RegisterInstance<IPlayerRepository>(new PlayerRepositoryMock());
            //Container.DependencyContainer.Current.RegisterInstance<IAdminRepository>(new AdminRepositoryMock());
            //Container.DependencyContainer.Current.RegisterInstance<ITableValues>(new TableValuesMock());
            Container.DependencyContainer.Current.RegisterInstance<IAuraManager>(new AuraManagerMock());

            IAssemblyHelper assemblyHelper = new AssemblyHelper();
            Type iRegistrable = typeof(IRegistrable);
            foreach (var registrable in assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iRegistrable.IsAssignableFrom(t))))
                Container.DependencyContainer.Current.Register(registrable);
        }

        internal class AssemblyHelper : IAssemblyHelper
        {
            public IEnumerable<Assembly> AllReferencedAssemblies => new[] { typeof(Server.Server).Assembly, typeof(AcidBlast).Assembly };
        }

    }
}
