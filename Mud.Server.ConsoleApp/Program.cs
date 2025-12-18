using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Repository.Filesystem.Json;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.World;
using Mud.Server.Options;
using Mud.Server.Rom24;
using Serilog;
using System.Reflection;

namespace Mud.Server.ConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var consoleServer = serviceProvider.GetRequiredService<ConsoleServer>();
        consoleServer.Create();
        consoleServer.Run();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Configure Serilog logger immediately on application start
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug() // Set the minimum level
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        Log.Information("Configuring services");

        var assemblyHelper = new AssemblyHelper(configuration);

        //  Configure Logging to use once application has started
        services.AddLogging(builder => builder.AddSerilog(Log.Logger));

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

        RegisterUsingExportAttribute(services, assemblyHelper.AllReferencedAssemblies);
    }

    private static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AvatarOptions>(options => configuration.GetSection(AvatarOptions.SectionName).Bind(options));
        services.Configure<AuraOptions>(options => configuration.GetSection(AuraOptions.SectionName).Bind(options));
        services.Configure<FileRepositoryOptions>(options => configuration.GetSection(FileRepositoryOptions.SectionName).Bind(options));
        services.Configure<ImportOptions>(options => configuration.GetSection(ImportOptions.SectionName).Bind(options));
        services.Configure<MessageForwardOptions>(options => configuration.GetSection(MessageForwardOptions.SectionName).Bind(options));
        services.Configure<Rom24Options>(options => configuration.GetSection(Rom24Options.SectionName).Bind(options));
        services.Configure<ServerOptions>(options => configuration.GetSection(ServerOptions.SectionName).Bind(options));
        services.Configure<Network.Telnet.TelnetOptions>(options => configuration.GetSection(Network.Telnet.TelnetOptions.SectionName).Bind(options));
        services.Configure<WorldOptions>(options => configuration.GetSection(WorldOptions.SectionName).Bind(options));
    }

    private static void RegisterUsingExportAttribute(IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        // register using ExportAttribute
        var exportAttributeType = typeof(ExportAttribute);
        foreach (var type in assemblies.SelectMany(x => x.GetTypes()).Where(t => t.CustomAttributes.Any(a => exportAttributeType.IsAssignableFrom(a.AttributeType))))
        {
            Log.Information("Registering type {0}.", type.FullName);
            ExportInspector.Register(services, type);
        }
    }

    internal class AssemblyHelper : IAssemblyHelper
    {
        private string[] ReferencedAssemblies { get; }

        private Assembly[] _assemblies = null!;
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
                foreach (var assemblyName in ReferencedAssemblies)
                {
                    var assembly = Assembly.Load(assemblyName);
                    if (null != assembly)
                    {
                        yield return assembly;
                    }
                }
            }
            else
            {
                var hash = new HashSet<string>();
                var stack = new Stack<Assembly>();

                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                    stack.Push(entryAssembly);
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
            public string[] Assemblies { get; set; } = null!;
        }
    }
}
