using Mud.Server.Domain;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Interfaces.Social;

public interface ISocialManager
{
    IEnumerable<IGameActionInfo> GetGameActions();
    IReadOnlyDictionary<string, SocialDefinition> SocialDefinitionByName { get; }
}