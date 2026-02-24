using Mud.Common;
using Mud.Random;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.MobProgram;
using Mud.Server.MobProgram.Interfaces;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpcall", "MobProgram", Hidden = true)]
[Syntax("mob call [vnum] [victim|'null'] [object1|'null'] [object2|'null']")]
[Help(
@"Lets the mobile to call another MOBprogram withing a MOBprogram.
This is a crude way to implement subroutines/functions. Beware of
nested loops and unwanted triggerings... Stack usage might be a problem.
Characters and objects referred to must be in the same room with the
mobile.")]
public class Call : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastOneArgument()];

    private IMobProgramManager MobProgramManager { get; }
    private IMobProgramEvaluator MobProgramEvaluator { get; }
    private IRandomManager RandomManager { get; }
    private ITimeManager TimeManager { get; }
    private ICharacterManager CharacterManager { get; }

    public Call(IMobProgramManager mobProgramManager, IMobProgramEvaluator mobProgramEvaluator, IRandomManager randomManager, ITimeManager timeManager, ICharacterManager characterManager)
    {
        MobProgramManager = mobProgramManager;
        MobProgramEvaluator = mobProgramEvaluator;
        RandomManager = randomManager;
        TimeManager = timeManager;
        CharacterManager = characterManager;
    }

    private IMobProgram MobProgram { get; set; } = null!;
    private IMobProgramExecutionContext Context { get; set; } = null!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        // mob program
        if (!actionInput.Parameters[0].IsNumber)
            return BuildCommandSyntax();
        var id = actionInput.Parameters[0].AsNumber;
        var mobProgram = MobProgramManager.GetMobProgram(id);
        if (mobProgram == null)
            return "Unknown mob program";
        MobProgram = mobProgram;

        var context = new MobProgramExecutionContext
        {
            Self = Actor,

            MobProgramId = id,
            RandomManager = RandomManager,
            TimeManager = TimeManager,
            CharacterManager = CharacterManager,
        };

        // param[1] as char or 'null' or empty
        // param[2] as obj or 'null' or empty
        // param[3] as char or obj or 'null' or empty
        if (actionInput.Parameters.Length > 1)
        {
            if (!StringCompareHelpers.StringEquals("null", actionInput.Parameters[1].Value))
            {
                var whom = FindHelpers.FindByName(Actor.Room.People, actionInput.Parameters[1]);
                if (whom is not null)
                    context.Triggerer = whom;
            }
        }
        if (actionInput.Parameters.Length > 2)
        {
            if (!StringCompareHelpers.StringEquals("null", actionInput.Parameters[2].Value))
            {
                var what = FindHelpers.FindItemHere(Actor, actionInput.Parameters[2]);
                if (what is not null)
                    context.PrimaryObject = what;
            }
        }
        if (actionInput.Parameters.Length > 3)
        {
            if (!StringCompareHelpers.StringEquals("null", actionInput.Parameters[3].Value))
            {
                var whom = FindHelpers.FindByName(Actor.Room.People, actionInput.Parameters[3]);
                if (whom is not null)
                    context.Secondary = whom;
                else
                {
                    var what = FindHelpers.FindItemHere(Actor, actionInput.Parameters[3]);
                    if (what is not null)
                        context.SecondaryObject = what;
                }
            }
        }

        Context = context;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        MobProgramEvaluator.Evaluate(MobProgram, Context);
    }
}
