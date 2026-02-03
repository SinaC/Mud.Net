using Mud.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.PlayableCharacter.Information;

[PlayableCharacterCommand("worth", "Information")]
public class Worth : PlayableCharacterGameAction
{
    protected override IGuard<IPlayableCharacter>[] Guards => [];

    public override void Execute(IActionInput actionInput)
    {
        Actor.Send("You have {0} gold coins, {1} silver coins, and {2} experience ({3} exp to level).", Actor.GoldCoins, Actor.SilverCoins, Actor.Experience, Actor.ExperienceToLevel);
        var currencies = new List<string>();
        foreach (var currency in Enum.GetValues<Currencies>())
            currencies.Add($"{currency}: {Actor[currency]}");
        Actor.Send(string.Join(",", currencies));
    }
}
