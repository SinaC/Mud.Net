using Microsoft.Extensions.Logging;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Parser.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mob", "MobProgram", Hidden = true)]
[Help("Lets the mobile use mob program command.")]
[Syntax("mob [command] {parameters}")]
public class Mob : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastOneArgument()];

    private ILogger<Mob> Logger { get; }
    private IParser Parser { get; }

    public Mob(ILogger<Mob> logger, IParser parser)
    {
        Logger = logger;
        Parser = parser;
    }

    public override void Execute(IActionInput actionInput)
    {
        var mobInstruction = "mp" + actionInput.RawParameters.Trim(); // prefix command with 'mp' to avoid issue with command existing for character and mob program such as kill
        var mobInstructionParseResult = Parser.Parse(mobInstruction);

        if (mobInstructionParseResult is null)
            Logger.LogError("MOBPROGRAM: {debugName} cannot parse mob instruction {mobInstruction}", Actor.DebugName, mobInstruction);
        else
        {
            var mobInstructionExecuted = Actor.ExecuteCommand(mobInstruction, mobInstructionParseResult);
            if (!mobInstructionExecuted)
                Logger.LogError("MOBPROGRAM: {debugName} cannot execute mob instruction: CMD={command} RAWPARAM={rawParameters}", Actor.DebugName, mobInstructionParseResult.Command, mobInstructionParseResult.RawParameters);
        }
    }
}
