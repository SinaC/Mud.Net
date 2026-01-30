using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Interfaces.GameAction;

namespace Mud.POC.DynamicCommand
{
    public class SocialManager : ISocialManager
    {
        private SocialDefinition[] SocialDefinitions { get; } =
        [
            new SocialDefinition("kiss", "Isn't there someone you want to kiss?", null, "You kiss $M.", "$n kisses $N.", "$n kisses you.", "Never around when required.", "All the lonely people :(", null),
            new SocialDefinition("smile", "You smile happily.", "$n smiles happily.", "You smile at $M.", "$n beams a smile at $N.", "$n smiles at you.", "There's no one by that name around.", "You smile at yourself.", "$n smiles at $mself"),
        ];

        private IGameActionInfo[] SocialGameActions { get; }

        public SocialManager()
        {
            SocialGameActions = SocialDefinitions.Select(GenerateGameAction).ToArray();
        }

        public IEnumerable<IGameActionInfo> GetGameActions()
            => SocialGameActions;

        private IGameActionInfo GenerateGameAction(SocialDefinition socialDefinition)
        {
            var characterCommandAttribute = new CharacterCommandAttribute(socialDefinition.Name, ["Socials"])
            {
                Priority = 999, // low priority
                Hidden = true, // should not appear in command list
                NoShortcut = false,
                AddCommandInParameters = true, // the generic social command will extract the command and parameters
            };

            var gai = new CharacterGameActionInfo(typeof(GenericSocialCommand), characterCommandAttribute, GameActionInfo.DefaultSyntaxCommandAttribute, [], null, [], [new MinPositionGuard(Positions.Standing)]);
            return gai;
        }

        private record SocialDefinition(string Name, string? CharacterNoArg, string? OthersNoArg, string? CharacterFound, string? OthersFound, string? VictimFound, string? CharacterNotFound, string? CharacterAuto, string? OthersAuto);
    }
}
