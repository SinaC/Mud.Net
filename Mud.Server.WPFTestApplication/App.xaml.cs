using Microsoft.Extensions.DependencyInjection;
using Mud.DataStructures.Flags;
using Mud.Logger;
using Mud.Repository;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Table;
using Mud.Server.Interfaces.World;
using Mud.Server.Random;
using Mud.Settings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Mud.Server.WPFTestApplication;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IServiceProvider _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        _serviceProvider = serviceCollection.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<ServerWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        var settings = new Settings.ConfigurationManager.Settings();
        var assemblyHelper = new AssemblyHelper();

        // Initialize log
        Log.Default.Initialize(settings.LogPath, "server.log");

        // Configure Logging
        services.AddLogging();

        // Register Services
        services.AddSingleton<ISettings>(settings);
        RegisterAllTypes(services, assemblyHelper.AllReferencedAssemblies);

        RegisterFlagValues<ICharacterFlagValues>(services, assemblyHelper.AllReferencedAssemblies);
        RegisterFlagValues<IRoomFlagValues>(services, assemblyHelper.AllReferencedAssemblies);
        RegisterFlagValues<IItemFlagValues>(services, assemblyHelper.AllReferencedAssemblies);
        RegisterFlagValues<IWeaponFlagValues>(services, assemblyHelper.AllReferencedAssemblies);
        RegisterFlagValues<IActFlagValues>(services, assemblyHelper.AllReferencedAssemblies);
        RegisterFlagValues<IOffensiveFlagValues>(services, assemblyHelper.AllReferencedAssemblies);
        RegisterFlagValues<IAssistFlagValues>(services, assemblyHelper.AllReferencedAssemblies);
        RegisterFlagValues<IIRVFlagValues>(services, assemblyHelper.AllReferencedAssemblies);
        RegisterFlagValues<IBodyFormValues>(services, assemblyHelper.AllReferencedAssemblies);
        RegisterFlagValues<IBodyPartValues>(services, assemblyHelper.AllReferencedAssemblies);

        services.AddSingleton<IRandomManager>(new RandomManager()); // 2 ctors => injector can't decide which one to choose
        services.AddSingleton<IAssemblyHelper>(assemblyHelper);
        services.AddSingleton<IAbilityManager, Ability.AbilityManager>();
        services.AddSingleton<IGameActionManager, GameAction.GameActionManager>();
        services.AddSingleton<ITimeManager, Server.TimeManager>();
        services.AddSingleton<IQuestManager, Quest.QuestManager>();
        services.AddSingleton<IAuraManager, Aura.AuraManager>();
        services.AddSingleton<IItemManager, Item.ItemManager>();
        services.AddSingleton<ICharacterManager, Character.CharacterManager>();
        services.AddSingleton<IRoomManager, Room.RoomManager>();
        services.AddSingleton<IAreaManager, Area.AreaManager>();
        services.AddSingleton<IAdminManager, Admin.AdminManager>();
        services.AddSingleton<IWiznet, Server.Wiznet>();
        services.AddSingleton<IResetManager, Server.ResetManager>();
        services.AddSingleton<IServer, Server.Server>();
        services.AddSingleton<IWorld, Server.Server>(); // Server also implements IWorld
        services.AddSingleton<IPlayerManager, Server.Server>(); // Server also implements IPlayerManager
        services.AddSingleton<IServerAdminCommand, Server.Server>(); // Server also implements IServerAdminCommand
        services.AddSingleton<IServerPlayerCommand, Server.Server>(); // Server also implements IServerPlayerCommand
        services.AddSingleton<IClassManager, Class.ClassManager>();
        services.AddSingleton<IRaceManager, Race.RaceManager>();
        services.AddSingleton<IUniquenessManager, Server.UniquenessManager>();
        services.AddSingleton<ITableValues, Table.TableValues>();
        services.AddSingleton<IDispelManager, Aura.DispelManager>();
        services.AddSingleton<IEffectManager, Effects.EffectManager>();
        services.AddSingleton<IAffectManager, Affects.AffectManager>();
        services.AddSingleton<IWeaponEffectManager, Effects.WeaponEffectManager>();

        services.AddSingleton<ILoginRepository, Repository.Filesystem.LoginRepository>();
        services.AddSingleton<IPlayerRepository, Repository.Filesystem.PlayerRepository>();
        services.AddSingleton<IAdminRepository, Repository.Filesystem.AdminRepository>();

        services.AddAutoMapper(typeof(Repository.Filesystem.AutoMapperProfile).Assembly);

        //// Initialize mapping
        //var mapperConfiguration = new MapperConfiguration(cfg =>
        //{
        //    cfg.AllowNullCollections = true;
        //    cfg.AllowNullDestinationValues = true;

        //    cfg.AddProfile<Repository.Filesystem.AutoMapperProfile>();
        //});
        //services.AddSingleton(mapperConfiguration.CreateMapper());

        //// Register ViewModels
        //services.AddSingleton<IMainViewModel, MainViewModel>();

        // Register Views
        services.AddSingleton<ServerWindow>();
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        // Dispose of services if needed
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    internal void RegisterAllTypes(IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        var iRegistrable = typeof(IRegistrable);
        foreach (var registrable in assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iRegistrable.IsAssignableFrom(t))))
        {
            Log.Default.WriteLine(LogLevels.Info, "Registering type {0}.", registrable.FullName);
            services.AddTransient(registrable);
        }
        // register races
        var iRace = typeof(IRace);
        foreach (var race in assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iRace.IsAssignableFrom(t))))
        {
            Log.Default.WriteLine(LogLevels.Info, "Registering race type {0}.", race.FullName);
            services.AddSingleton(iRace, race);
        }
        // register classes
        var iClass = typeof(IClass);
        foreach (var cl in assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iClass.IsAssignableFrom(t))))
        {
            Log.Default.WriteLine(LogLevels.Info, "Registering class type {0}.", cl.FullName);
            services.AddSingleton(iClass, cl);
        }
    }

    internal void RegisterFlagValues<TFlagValue>(IServiceCollection services, IEnumerable<Assembly> assemblies)
        where TFlagValue : IFlagValues<string>
    {
        var iFlagValuesType = typeof(TFlagValue);
        var concreteFlagValuesType = assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iFlagValuesType.IsAssignableFrom(t))).SingleOrDefault();
        if (concreteFlagValuesType == null)
            Log.Default.WriteLine(LogLevels.Error, "Cannot find an implementation for {0}.", iFlagValuesType.FullName);
        else
        {
            Log.Default.WriteLine(LogLevels.Info, "Registering implementation type {0} for {1}.", concreteFlagValuesType.FullName, iFlagValuesType.FullName);
            services.AddTransient(iFlagValuesType, concreteFlagValuesType);
        }
    }

    internal class AssemblyHelper : IAssemblyHelper
    {
        public IEnumerable<Assembly> AllReferencedAssemblies =>
        [
            typeof(Server.Server).Assembly,
            typeof(Rom24.Spells.AcidBlast).Assembly,
            typeof(AdditionalAbilities.Crush).Assembly,
            typeof(POC.Classes.Druid).Assembly
        ];
    }
}
