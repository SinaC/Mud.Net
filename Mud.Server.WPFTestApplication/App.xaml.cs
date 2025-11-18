using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
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
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using static Mud.Server.WPFTestApplication.App;

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
        Logger.Log.Default.Initialize(settings.LogPath, "server.log");

        // Configure Logging
        //services.AddLogging();
        //services.AddLogging(builder =>
        //{
        //    //builder.AddConfiguration();
        //    builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, CustomLoggerProvider>());
        //});
        // Configure Logging
        Serilog.Log.Logger = new LoggerConfiguration()
            .WriteTo.RichTextBoxSink()//outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"))
            .WriteTo.File(@"c:\temp\server.test.serilog.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        services.AddLogging(builder => builder.AddSerilog(Serilog.Log.Logger));

        // Register Services
        services.AddSingleton<ISettings>(settings);

        RegisterAllTypes(services, assemblyHelper.AllReferencedAssemblies);

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
        services.AddSingleton<Server.Server>();
        services.AddSingleton<IServer>(x => x.GetRequiredService<Server.Server>());
        services.AddSingleton<IWorld>(x => x.GetRequiredService<Server.Server>()); // Server also implements IWorld
        services.AddSingleton<IPlayerManager>(x => x.GetRequiredService<Server.Server>()); // Server also implements IPlayerManager
        services.AddSingleton<IServerAdminCommand>(x => x.GetRequiredService<Server.Server>()); // Server also implements IServerAdminCommand
        services.AddSingleton<IServerPlayerCommand>(x => x.GetRequiredService<Server.Server>()); // Server also implements IServerPlayerCommand
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

        //// Register ViewModels
        //services.AddSingleton<IMainViewModel, MainViewModel>();

        // Register Views
        services.AddSingleton<ServerWindow>();
    }

    //public sealed class CustomLogger(string scopeName) : ILogger
    //{
    //    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    //    public bool IsEnabled(LogLevel logLevel) =>
    //        true;

    //    public void Log<TState>(
    //        LogLevel logLevel,
    //        EventId eventId,
    //        TState state,
    //        Exception? exception,
    //        Func<TState, Exception?, string> formatter)
    //    {
    //        if (!IsEnabled(logLevel))
    //        {
    //            return;
    //        }

    //        ServerWindow.LogScopedMethod(logLevel.ToString(), scopeName, formatter(state, exception));
    //    }
    //}

    //private class CustomLoggerProvider : ILoggerProvider
    //{
    //    public ILogger CreateLogger(string categoryName)
    //        => new CustomLogger(categoryName);

    //    public void Dispose()
    //    {
    //    }
    //}

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
        // register commands and abilities
        var iRegistrable = typeof(IRegistrable);
        foreach (var registrable in assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iRegistrable.IsAssignableFrom(t))))
        {
            Logger.Log.Default.WriteLine(LogLevels.Info, "Registering type {0}.", registrable.FullName);
            services.AddTransient(registrable);
        }
        // register races
        var iRace = typeof(IRace);
        foreach (var race in assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iRace.IsAssignableFrom(t))))
        {
            Logger.Log.Default.WriteLine(LogLevels.Info, "Registering race type {0}.", race.FullName);
            services.AddSingleton(iRace, race);
        }
        // register classes
        var iClass = typeof(IClass);
        foreach (var cl in assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iClass.IsAssignableFrom(t))))
        {
            Logger.Log.Default.WriteLine(LogLevels.Info, "Registering class type {0}.", cl.FullName);
            services.AddSingleton(iClass, cl);
        }
        // register flag
        RegisterFlagValues<ICharacterFlagValues>(services, assemblies);
        RegisterFlagValues<IRoomFlagValues>(services, assemblies);
        RegisterFlagValues<IItemFlagValues>(services, assemblies);
        RegisterFlagValues<IWeaponFlagValues>(services, assemblies);
        RegisterFlagValues<IActFlagValues>(services, assemblies);
        RegisterFlagValues<IOffensiveFlagValues>(services, assemblies);
        RegisterFlagValues<IAssistFlagValues>(services, assemblies);
        RegisterFlagValues<IIRVFlagValues>(services, assemblies);
        RegisterFlagValues<IBodyFormValues>(services, assemblies);
        RegisterFlagValues<IBodyPartValues>(services, assemblies);

        // register group
        services.AddTransient<IGroup, Group.Group>();
    }

    internal void RegisterFlagValues<TFlagValue>(IServiceCollection services, IEnumerable<Assembly> assemblies)
        where TFlagValue : IFlagValues<string>
    {
        var iFlagValuesType = typeof(TFlagValue);
        var concreteFlagValuesType = assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iFlagValuesType.IsAssignableFrom(t))).SingleOrDefault();
        if (concreteFlagValuesType == null)
            Logger.Log.Default.WriteLine(LogLevels.Error, "Cannot find an implementation for {0}.", iFlagValuesType.FullName);
        else
        {
            Logger.Log.Default.WriteLine(LogLevels.Info, "Registering flag values type {0} for {1}.", concreteFlagValuesType.FullName, iFlagValuesType.FullName);
            services.AddTransient(iFlagValuesType, concreteFlagValuesType);
        }
    }

    internal class AssemblyHelper : IAssemblyHelper
    {
        public IEnumerable<Assembly> AllReferencedAssemblies =>
        [
            typeof(Server.Server).Assembly,
            typeof(Commands.Actor.Commands).Assembly,
            typeof(Rom24.Spells.AcidBlast).Assembly,
            typeof(AdditionalAbilities.Crush).Assembly,
            typeof(POC.Classes.Druid).Assembly
        ];
    }
}
