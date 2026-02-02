using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Random;
using Mud.Server.Ability.Spell;
using Mud.Server.Ability.Spell.Interfaces;
using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.SpellGuards;
using Mud.Server.Interfaces;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Environment)]
[Syntax(
    "cast [spell] better",
    "cast [spell] worse")]
[Help(
@"This spell makes the weather either better or worse.")]
[OneLineHelp("changes the weather in the manner desired by the caster")]
public class ControlWeather : NoTargetSpellBase
{
    private const string SpellName = "Control Weather";

    protected override ISpellGuard[] Guards => [new CannotBeInCombat()];

    private ITimeManager TimeManager { get; }
    private ICommandParser CommandParser { get; }

    public ControlWeather(ILogger<ControlWeather> logger, IRandomManager randomManager, ITimeManager timeManager, ICommandParser commandParser)
        : base(logger, randomManager)
    {
        TimeManager = timeManager;
        CommandParser = commandParser;
    }

    private bool IsBetterRequired { get; set; }

    public override string? Setup(ISpellActionInput spellActionInput)
    {
        var baseSetup = base.Setup(spellActionInput);
        if (baseSetup != null)
            return baseSetup;

        var what = CommandParser.JoinParameters(spellActionInput.Parameters);
        if (StringCompareHelpers.StringEquals(what, "better"))
        {
            IsBetterRequired = true;
            return null;
        }
        if (StringCompareHelpers.StringEquals(what, "worse"))
        {
            IsBetterRequired = false;
            return null;
        }
        return "Do you want it to get better or worse?";
    }

    protected override void Invoke()
    {
        int value = RandomManager.Dice(Level / 3, 4) * (IsBetterRequired ? 1 : -1);
        TimeManager.ChangePressure(value);
        Caster.Send("Ok.");
    }
}
