using Mud.Common;
using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Extensions;
using Mud.Server.GameAction;
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

    public Auto(IGameActionManager gameActionManager)
    {
        GameActionManager = gameActionManager;
    }

    protected enum Actions
    {
        Display,
        SubCommand
    }

    protected (string? parameter, AutoFlags Flag, Type? GameActionType)[] ActionTable { get; } =
[
        ("assist", AutoFlags.Assist, typeof(AutoAssist)),
        ("exit", AutoFlags.Exit, typeof(AutoExit)),
        ("sacrifice", AutoFlags.Sacrifice, typeof(AutoSacrifice)),
        ("gold", AutoFlags.Gold, typeof(AutoGold)),
        ("split", AutoFlags.Split, typeof(AutoSplit)),
        ("loot", AutoFlags.Loot, typeof(AutoLoot)),
        ("affect", AutoFlags.Affect, typeof(AutoAffect)),
    ];

    protected Actions Action { get; set; }
    protected AutoFlags? What { get; set; }
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
        foreach (var actionTableEntry in ActionTable.Where(x => x.parameter is not null && x.GameActionType is not null))
        {
            if (actionTableEntry.parameter!.StartsWith(actionInput.Parameters[0].Value))
            {
                Action = Actions.SubCommand;
                What = actionTableEntry.Flag;
                GameActionType = actionTableEntry.GameActionType;
                return null;
            }
        }

        return "This is not a valid auto.";
    }

    public override void Execute(IActionInput actionInput)
    {
        switch (Action)
        {
            case Actions.Display:
                StringBuilder sb = new();
                foreach (var autoFlag in Enum.GetValues<AutoFlags>().Where(x => x != AutoFlags.None).OrderBy(x => x.ToString()))
                    sb.AppendFormatLine("{0}: {1}", autoFlag.PrettyPrint(), Actor.AutoFlags.HasFlag(autoFlag) ? "ON" : "OFF");
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
