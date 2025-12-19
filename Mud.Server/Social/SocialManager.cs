using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Commands.Character.Social;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Social;

namespace Mud.Server.Social;

[Export(typeof(ISocialManager)), Shared]
public partial class SocialManager : ISocialManager
{
    private readonly IReadOnlyDictionary<string, SocialDefinition> _socialDefinitionByName;
    private readonly IGameActionInfo[] _socialGameActions;
    private ILogger<SocialManager> Logger { get; }

    public SocialManager(ILogger<SocialManager> logger, IEnumerable<ISocialDefinitionGenerator> socialDefinitionGenerators)
    {
        Logger = logger;

        var socialDefinitions = socialDefinitionGenerators.SelectMany(x => x.SocialDefinitions).ToList();
        var duplicates = socialDefinitions.GroupBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase).Where(x => x.Count() > 1).ToArray();
        if (duplicates.Length > 0)
        {
            foreach (var duplicateSocialDefinition in duplicates)
                Logger.LogError("SocialManager: social {name} has been found more than once -> remove from socials", duplicateSocialDefinition.Key);
            // remove all duplicates, dont try to chose which one to keep
            socialDefinitions.RemoveAll(x => duplicates.Any(y => StringCompareHelpers.StringEquals(y.Key, x.Name)));
        }

        _socialDefinitionByName = socialDefinitions.ToDictionary(x => x.Name);
        _socialGameActions = socialDefinitions.Select(GenerateGameAction).ToArray();
    }

    public IEnumerable<IGameActionInfo> GetGameActions()
        => _socialGameActions;

    public IReadOnlyDictionary<string, SocialDefinition> SocialDefinitionByName => _socialDefinitionByName;

    private IGameActionInfo GenerateGameAction(SocialDefinition socialDefinition)
    {
        var characterCommandAttribute = new CharacterCommandAttribute(socialDefinition.Name, ["Socials"])
        {
            Priority = 999, // low priority
            Hidden = true, // should not appear in command list
            NoShortcut = false,
            AddCommandInParameters = false,
            MinPosition = Positions.Standing,
            NotInCombat = false,
        };

        var gai = new CharacterGameActionInfo(typeof(DynamicSocialCommand), characterCommandAttribute, GameActionInfo.DefaultSyntaxCommandAttribute, [], null);
        return gai;
    }
}
