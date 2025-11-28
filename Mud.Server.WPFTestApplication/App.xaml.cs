using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Network.Interfaces;
using Mud.Network.Telnet;
using Mud.Repository.Interfaces;
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
using Mud.Server.Options;
using Mud.Server.Random;
using Mud.Server.Rom24;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
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
        // Configure Serilog logger immediately on application start
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug() // Set the minimum level
            .WriteTo.Debug() // Add a sink to write to the debug output
            .WriteTo.Console() // Add a sink to write to the console
            .WriteTo.RichTextBoxSink() // add a sink to write to the RichTextBox
            .CreateLogger();

        var assemblyHelper = new AssemblyHelper();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Configure Logging to use once application start
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.RichTextBoxSink()
            .CreateLogger();
        services.AddLogging(builder => builder.AddSerilog(logger));
        //services.AddSingleton(provider => provider.GetRequiredService<ILoggerFactory>().CreateLogger("NonGenericILogger"));

        // Configure options
        ConfigureOptions(services, configuration);

        // Register Services
        services.AddSingleton<ITelnetNetworkServer, TelnetServer>();
        services.AddSingleton<IRandomManager>(new RandomManager()); // 2 ctors => injector can't decide which one to choose
        services.AddSingleton<IAssemblyHelper>(assemblyHelper);
        services.AddSingleton<ICommandParser, GameAction.CommandParser>();
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
        services.AddSingleton<IPulseManager, Server.PulseManager>();
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

        RegisterAllRegistrableTypes(services, assemblyHelper.AllReferencedAssemblies);
        RegisterFlagValues(services, assemblyHelper.AllReferencedAssemblies);

        //// Register ViewModels
        //services.AddSingleton<IMainViewModel, MainViewModel>();

        // Register Views
        services.AddSingleton<ServerWindow>();
    }

    private static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AvatarOptions>(options => configuration.GetSection(AvatarOptions.SectionName).Bind(options));
        services.Configure<AuraOptions>(options => configuration.GetSection(AuraOptions.SectionName).Bind(options));
        services.Configure<Repository.Filesystem.FileRepositoryOptions>(options => configuration.GetSection(Repository.Filesystem.FileRepositoryOptions.SectionName).Bind(options));
        services.Configure<ImportOptions>(options => configuration.GetSection(ImportOptions.SectionName).Bind(options));
        services.Configure<MessageForwardOptions>(options => configuration.GetSection(MessageForwardOptions.SectionName).Bind(options));
        services.Configure<Rom24Options>(options => configuration.GetSection(Rom24Options.SectionName).Bind(options));
        services.Configure<ServerOptions>(options => configuration.GetSection(ServerOptions.SectionName).Bind(options));
        services.Configure<TelnetOptions>(options => configuration.GetSection(TelnetOptions.SectionName).Bind(options));
        services.Configure<WorldOptions>(options => configuration.GetSection(WorldOptions.SectionName).Bind(options));
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        // Dispose of services if needed
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    internal void RegisterAllRegistrableTypes(IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        // register using ExportAttribute
        var exportAttributeType = typeof(ExportAttribute);
        foreach (var type in assemblies.SelectMany(x => x.GetTypes()).Where(t => t.CustomAttributes.Any(a => exportAttributeType.IsAssignableFrom(a.AttributeType))))
        {
            Log.Information("Registering type {0}.", type.FullName);
            ExportInspector.Register(services, type);
        }
        // register races
        var iRace = typeof(IRace);
        foreach (var race in assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iRace.IsAssignableFrom(t))))
        {
            Log.Information("Registering race type {0}.", race.FullName);
            services.AddSingleton(iRace, race);
        }
        // register classes
        var iClass = typeof(IClass);
        foreach (var cl in assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iClass.IsAssignableFrom(t))))
        {
            Log.Information("Registering class type {0}.", cl.FullName);
            services.AddSingleton(iClass, cl);
        }
        // register sanity checks
        var iSanityCheck = typeof(ISanityCheck);
        foreach (var sanityCheck in assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iSanityCheck.IsAssignableFrom(t))))
        {
            Log.Information("Registering sanity check type {0}.", sanityCheck.FullName);
            services.AddSingleton(iSanityCheck, sanityCheck);
        }
    }

    internal void RegisterFlagValues(IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
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
    }

    internal void RegisterFlagValues<TFlagValue>(IServiceCollection services, IEnumerable<Assembly> assemblies)
        where TFlagValue : IFlagValues<string>
    {
        var iFlagValuesType = typeof(TFlagValue);
        var concreteFlagValuesType = assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iFlagValuesType.IsAssignableFrom(t))).SingleOrDefault();
        if (concreteFlagValuesType == null)
            Log.Warning("Cannot find an implementation for {0}.", iFlagValuesType.FullName);
        else
        {
            Log.Information("Registering flag values type {0} for {1}.", concreteFlagValuesType.FullName, iFlagValuesType.FullName);
            services.AddTransient(iFlagValuesType, concreteFlagValuesType);
        }
    }

    internal class AssemblyHelper : IAssemblyHelper
    {
        public IEnumerable<Assembly> AllReferencedAssemblies =>
        [
            typeof(Server.Server).Assembly,
            typeof(Commands.Actor.Commands).Assembly,
            typeof(Affects.Character.CharacterAttributeAffect).Assembly,
            typeof(Quest.Quest).Assembly,
            typeof(Rom24.Spells.AcidBlast).Assembly,
            typeof(AdditionalAbilities.Crush).Assembly,
            typeof(POC.Classes.Druid).Assembly
        ];
    }
}
