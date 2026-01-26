using Mud.Common;
using Mud.Flags.Interfaces;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Flags;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Commands.Character.PlayableCharacter.Information;

[PlayableCharacterCommand("auto", "Information")]
[Syntax(
    "[cmd]",
    "[cmd] all",
    "[cmd] assist",
    "[cmd] exit",
    "[cmd] sacrifice",
    "[cmd] gold",
    "[cmd] split",
    "[cmd] loot",
    "[cmd] affect")]
[Help(
@"To ease the boredom of always splitting gold and sacrificing corpses.
The commands are as follows:

all       : active every 'auto'
assist    : makes you help group members in combat
exit      : display room exits upon entering a room
sacrifice : sacrifice dead monsters (if autoloot is on, only empty corpes)
gold      : take all gold from dead mobiles
split     : split up spoils from combat among your group members
loot      : take all equipment from dead mobiles
affect    : display your affects when looking your score")]
public class Auto : PlayableCharacterGameAction
{
    private IGameActionManager GameActionManager { get; }
    private IFlagsManager FlagsManager { get; }

    public Auto(IGameActionManager gameActionManager, IFlagsManager flagsManager)
    {
        GameActionManager = gameActionManager;
        FlagsManager = flagsManager;
    }

    protected enum Actions
    {
        Display,
        SubCommand
    }

    protected (string Flag, Type? GameActionType)[] ActionTable { get; } =
[
        ("Assist", typeof(AutoAssist)),
        ("Exit", typeof(AutoExit)),
        ("Sacrifice", typeof(AutoSacrifice)),
        ("Gold", typeof(AutoGold)),
        ("Split", typeof(AutoSplit)),
        ("Loot", typeof(AutoLoot)),
        ("Affect", typeof(AutoAffect)),
    ];

    protected Actions Action { get; set; }
    protected string? What { get; set; }
    protected Type? GameActionType { get; set; }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
        {
            Action = Actions.Display;
            return null;
        }

        if (actionInput.Parameters[0].IsAll)
        {
            Action = Actions.SubCommand;
            GameActionType = typeof(AutoAll);
            return null;
        }

        // search in action table
        var actionEntry = ActionTable.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Flag, actionInput.Parameters[0].Value));
        if (actionEntry != default)
        {
            Action = Actions.SubCommand;
            What = actionEntry.Flag;
            GameActionType = actionEntry.GameActionType;
            return null;
        }

        return "This is not a valid auto.";
    }

    public override void Execute(IActionInput actionInput)
    {
        switch (Action)
        {
            case Actions.Display:
                StringBuilder sb = new();
                foreach (var autoFlag in FlagsManager.AvailableValues<IAutoFlags>().OrderBy(x => x))
                    sb.AppendFormatLine("{0}: {1}", autoFlag, Actor.AutoFlags.IsSet(autoFlag) ? "%g%ON%x%" : "%r%OFF%x%");
                Actor.Send(sb);
                return;

            case Actions.SubCommand:
                var executionResults = GameActionManager.Execute(GameActionType!, Actor, null);
                if (executionResults != null)
                    Actor.Send(executionResults);
                return;
        }
    }
}
