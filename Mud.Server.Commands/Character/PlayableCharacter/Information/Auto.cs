using Mud.Common;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Commands.Character.PlayableCharacter.Information;

[PlayableCharacterCommand("auto", "Information")]
[Syntax(
    "[cmd]",
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

assit     : makes you help group members in combat
exit      : display room exits upon entering a room
sacrifice : sacrifice dead monsters (if autoloot is on, only empty corpes)
gold      : take all gold from dead mobiles
split     : split up spoils from combat among your group members
loot      : take all equipment from dead mobiles
affect    : display your affects when looking your score")]
public class Auto : PlayableCharacterGameAction
{
    private IWiznet Wiznet { get; }

    public Auto(IWiznet wiznet)
    {
        Wiznet = wiznet;
    }

    protected bool Display { get; set; }
    protected bool SetAll { get; set; }
    protected AutoFlags What { get; set; }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
        {
            Display = true;
            return null;
        }

        if (actionInput.Parameters[0].IsAll)
        {
            SetAll = true;
            return null;
        }

        bool found = EnumHelpers.TryFindByPrefix(actionInput.Parameters[0].Value, out var flag, AutoFlags.None);
        if (!found)
            return "This is not a valid auto.";

        What = flag;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (Display)
        {
            StringBuilder sb = new ();
            foreach (var autoFlag in Enum.GetValues<AutoFlags>().Where(x => x != AutoFlags.None).OrderBy(x => x.ToString()))
                sb.AppendFormatLine("{0}: {1}", autoFlag.PrettyPrint(), Actor.AutoFlags.HasFlag(autoFlag) ? "ON" : "OFF");

            Actor.Send(sb);
            return;
        }

        if (SetAll)
        {
            foreach (var autoFlag in Enum.GetValues<AutoFlags>().Where(x => x != AutoFlags.None).OrderBy(x => x.ToString()))
                Actor.AddAutoFlags(autoFlag);
            Actor.Send("Ok.");
            return;
        }

        if (Actor.AutoFlags.HasFlag(What))
        {
            Actor.RemoveAutoFlags(What);
            string msg = AutoRemovedMessage(What);
            Actor.Send(msg);
        }
        else
        {
            Actor.AddAutoFlags(What);
            string msg = AutoAddedMessage(What);
            Actor.Send(msg);
        }
    }

    private string AutoAddedMessage(AutoFlags flag)
    {
        switch (flag)
        {
            case AutoFlags.Assist: return "You will now assist when needed.";
            case AutoFlags.Exit: return "Exits will now be displayed.";
            case AutoFlags.Sacrifice: return "Automatic corpse sacrificing set.";
            case AutoFlags.Gold: return "Automatic gold looting set.";
            case AutoFlags.Loot: return "Automatic corpse looting set.";
            case AutoFlags.Split: return "Automatic gold splitting set.";
            case AutoFlags.Affect: return "Affects will now be displayed when looking your score.";
            default:
                Wiznet.Log($"AutoAddedMessage: invalid flag {flag}", WiznetFlags.Bugs, AdminLevels.Implementor);
                return "???";
        }
    }

    private string AutoRemovedMessage(AutoFlags flag)
    {
        switch (flag)
        {
            case AutoFlags.Assist: return "Autoassist removed.";
            case AutoFlags.Exit: return "Exits will no longer be displayed.";
            case AutoFlags.Sacrifice: return "Autosacrificing removed.";
            case AutoFlags.Gold: return "Autogold removed.";
            case AutoFlags.Loot: return "Autolooting removed.";
            case AutoFlags.Split: return "Autosplitting removed.";
            case AutoFlags.Affect: return "Autoaffect removed.";
            default:
                Wiznet.Log($"AutoRemovedMessage: invalid flag {flag}", WiznetFlags.Bugs, AdminLevels.Implementor);
                return "???";
        }
    }
}
