using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Admin.Quest;

[AdminCommand("questreset", "Quest")]
[Syntax("[cmd] <character>")]
public class QuestReset : AdminGameAction
{
    private ICharacterManager CharacterManager { get; }

    public QuestReset(ICharacterManager characterManager)
    {
        CharacterManager = characterManager;
    }

    protected IPlayableCharacter Whom { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return BuildCommandSyntax();

        Whom = FindHelpers.FindByName(CharacterManager.PlayableCharacters.Where(x => x.ImpersonatedBy != null), actionInput.Parameters[0])!;
        if (Whom == null)
            return StringHelpers.CharacterNotFound;

        if (!Whom.Quests.Any())
            return $"No quest to reset on {Whom.DisplayName}";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        foreach (var quest in Whom.Quests)
        {
            Actor.Send($"Resetting quest '{quest.Title}' for '{Whom.DisplayName}'");
            Whom.Send($"%y%The quest ''{quest.Title}' has been reset.%x%");
            quest.Reset();
        }
    }
}
