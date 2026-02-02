using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Social;

namespace Mud.Server.Commands.Character.Social;

[DynamicCommand]
public class DynamicSocialCommand : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Standing)];

    private ILogger<DynamicSocialCommand> Logger { get; }
    private ISocialManager SocialManager { get; }

    public DynamicSocialCommand(ILogger<DynamicSocialCommand> logger, ISocialManager socialManager)
    {
        Logger = logger;
        SocialManager = socialManager;
    }

    private SocialDefinition SocialDefinition { get; set; } = null!;
    private bool UseCharacterNotFound { get; set; }
    private ICharacter? Victim { get; set; }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards is not null)
            return baseGuards;

        var socialName = actionInput.Command;
        if (!SocialManager.SocialDefinitionByName.TryGetValue(socialName, out var socialDefinition))
        {
            Logger.LogError("DynamicSocialCommand: social {name} not found", socialName);
            return "Huh ?";
        }
        SocialDefinition = socialDefinition;

        if (actionInput.Parameters.Length > 0)
        {
            var victim = FindHelpers.FindByName(Actor.Room.People.Where(Actor.CanSee), actionInput.Parameters[0]);
            if (victim is null)
            {
                if (SocialDefinition.CharacterNotFound is null)
                    return StringHelpers.CharacterNotFound;
                else
                {
                    if (SocialDefinition.CharacterNotFound.Contains("{0"))
                    {
                        Logger.LogError("Social {name} CharacterNotFound phrase contains arguments.", socialName);
                        return StringHelpers.CharacterNotFound;
                    }
                    UseCharacterNotFound = true;
                }
            }
            Victim = victim;
        }

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (UseCharacterNotFound)
            Actor.Act(ActOptions.ToCharacter, SocialDefinition.CharacterNotFound!);
        else if (Victim is null)
        {
            if (SocialDefinition.CharacterNoArg is not null)
                Actor.Act(ActOptions.ToCharacter, SocialDefinition.CharacterNoArg, Actor);
            if (SocialDefinition.OthersNoArg is not null)
                Actor.ActToNotVictim(Actor, SocialDefinition.OthersNoArg, Actor);
        }
        else if (Victim == Actor)
        {
            if (SocialDefinition.CharacterAuto is not null)
                Actor.Act(ActOptions.ToCharacter, SocialDefinition.CharacterAuto, Actor);
            if (SocialDefinition.OthersAuto is not null)
                Actor.ActToNotVictim(Actor, SocialDefinition.OthersAuto, Actor);
        }
        else
        {
            if (SocialDefinition.CharacterFound is not null)
                Actor.Act(ActOptions.ToCharacter, SocialDefinition.CharacterFound, Actor, Victim);
            if (SocialDefinition.VictimFound is not null)
                Victim.Act(ActOptions.ToCharacter, SocialDefinition.VictimFound, Actor, Victim);
            if (SocialDefinition.OthersFound is not null)
                Actor.ActToNotVictim(Victim, SocialDefinition.OthersFound, Actor, Victim);
        }
        // TODO: slaps ?
    }
}
