using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Commands.Character.Movement;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.World;
using Mud.Server.Options;
using Mud.Server.Rom24;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Ribbon;

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

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var assemblyHelper = new AssemblyHelper(configuration);

        // Configure Logging to use once application start
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.RichTextBoxSink()
            .CreateLogger();
        services.AddLogging(builder => builder.AddSerilog(logger));

        // Configure options
        ConfigureOptions(services, configuration);

        // Register Services
        services.AddSingleton<IAssemblyHelper>(assemblyHelper);
        // how can be configured to use multiple implementations?
        services.AddSingleton<Server.Server>();
        services.AddSingleton<IServer>(x => x.GetRequiredService<Server.Server>());
        services.AddSingleton<IWorld>(x => x.GetRequiredService<Server.Server>()); // Server also implements IWorld
        services.AddSingleton<IPlayerManager>(x => x.GetRequiredService<Server.Server>()); // Server also implements IPlayerManager
        services.AddSingleton<IServerAdminCommand>(x => x.GetRequiredService<Server.Server>()); // Server also implements IServerAdminCommand
        services.AddSingleton<IServerPlayerCommand>(x => x.GetRequiredService<Server.Server>()); // Server also implements IServerPlayerCommand

        services.AddAutoMapper(typeof(Repository.Filesystem.AutoMapperProfile).Assembly);

        RegisterUsingExportAttribute(services, assemblyHelper.AllReferencedAssemblies);
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
        services.Configure<Network.Telnet.TelnetOptions>(options => configuration.GetSection(Network.Telnet.TelnetOptions.SectionName).Bind(options));
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

    private void RegisterUsingExportAttribute(IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        // register using ExportAttribute
        var exportAttributeType = typeof(ExportAttribute);
        foreach (var type in assemblies.SelectMany(x => x.GetTypes()).Where(t => t.CustomAttributes.Any(a => exportAttributeType.IsAssignableFrom(a.AttributeType))))
        {
            Log.Information("Registering type {0}.", type.FullName);
            ExportInspector.Register(services, type);
        }
    }

    private void RegisterFlagValues(IServiceCollection services, IEnumerable<Assembly> assemblies)
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

    private void RegisterFlagValues<TFlagValue>(IServiceCollection services, IEnumerable<Assembly> assemblies)
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
        private string[] ReferencedAssemblies { get; }

        private Assembly[] _assemblies;
        public IEnumerable<Assembly> AllReferencedAssemblies => _assemblies ??= GetAssemblies().ToArray();

        public AssemblyHelper(IConfiguration configuration)
        {
            var dependency = new Dependency();
            configuration.GetSection("Application.Dependency").Bind(dependency);
            ReferencedAssemblies = dependency.Assemblies;
        }

        private IEnumerable<Assembly> GetAssemblies()
        {
            if (ReferencedAssemblies?.Length > 0)
            {
                foreach (var assembly in ReferencedAssemblies)
                {
                    var a = Assembly.Load(assembly);
                    if (null != a)
                    {
                        yield return a;
                    }
                }
            }
            else
            {
                var hash = new HashSet<string>();
                var stack = new Stack<Assembly>();

                stack.Push(Assembly.GetEntryAssembly());
                while (stack.Count > 0)
                {
                    var assembly = stack.Pop();

                    yield return assembly;

                    foreach (var reference in assembly.GetReferencedAssemblies().Where(x => x.FullName?.StartsWith("Mud") == true))
                    {
                        if (!hash.Contains(reference.FullName))
                        {
                            stack.Push(Assembly.Load(reference));
                            hash.Add(reference.FullName);
                        }
                    }
                }
            }
        }

        private class Dependency
        {
            public string[] Assemblies { get; set; }
        }
    }
}
