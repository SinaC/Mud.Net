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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
        base.OnStartup(e);

        SetupExceptionHandling();

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        _serviceProvider = serviceCollection.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<ServerWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
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
            //.WriteTo.Debug() // Add a sink to write to the debug output
            //.WriteTo.Console() // Add a sink to write to the console
            .WriteTo.RichTextBoxSink() // add a sink to write to the RichTextBox
            .CreateLogger();

        Log.Information("Configuring services");

        var assemblyHelper = new AssemblyHelper(configuration);

        //  Configure Logging to use once application has started
        //var logger = new LoggerConfiguration()
        //    .ReadFrom.Configuration(configuration)
        //    .WriteTo.RichTextBoxSink()
        //    .CreateLogger();
        // services.AddLogging(builder => builder.AddSerilog(logger));
        services.AddLogging(builder => builder.AddSerilog(Log.Logger));

        // Configure options
        ConfigureOptions(services, configuration);

        // Register Services
        services.AddSingleton<IAssemblyHelper>(assemblyHelper);
        RegisterUsingExportAttribute(services, assemblyHelper.AllReferencedAssemblies);

        //// Register ViewModels
        //services.AddSingleton<IMainViewModel, MainViewModel>();

        // Register Views
        services.AddSingleton<ServerWindow>();
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

    private void SetupExceptionHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            Log.Error((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

        DispatcherUnhandledException += (s, e) =>
        {
            Log.Error(e.Exception, "Application.Current.DispatcherUnhandledException");
            e.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            Log.Error(e.Exception, "TaskScheduler.UnobservedTaskException");
            e.SetObserved();
        };
    }
}
