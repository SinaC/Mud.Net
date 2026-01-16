using Mud.Server.Domain;

namespace Mud.Server.Interfaces.Social;

public interface ISocialDefinitionGenerator
{
    IEnumerable<SocialDefinition> SocialDefinitions { get; }
}