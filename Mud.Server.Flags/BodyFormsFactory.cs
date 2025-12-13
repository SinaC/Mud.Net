using Microsoft.Extensions.DependencyInjection;
using Mud.Common.Attributes;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IFlagFactory<IBodyForms, IBodyFormValues>)), Shared]
public class BodyFormsFactory : IFlagFactory<IBodyForms, IBodyFormValues>
{
    private IServiceProvider ServiceProvider { get; }

    public BodyFormsFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public IBodyForms CreateInstance(string? flags)
    {
        var bodyFormFlags = ServiceProvider.GetRequiredService<IBodyForms>();
        if (flags != null) 
            bodyFormFlags.Set(flags);
        return bodyFormFlags;
    }

    public IBodyForms CreateInstance(params string[] flags)
    {
        var bodyFormFlags = ServiceProvider.GetRequiredService<IBodyForms>();
        bodyFormFlags.Set(flags);
        return bodyFormFlags;
    }
}
