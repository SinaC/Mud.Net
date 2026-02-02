using Mud.Common;
using Mud.Domain;
using Mud.Server.Class.Interfaces;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Race.Interfaces;

namespace Mud.Server.Commands.PlayableCharacter.Attribute;

[PlayableCharacterCommand("train", "Attribute")]
[Syntax(
        "[cmd]",
        "[cmd] <attribute>",
        "[cmd] <resource> (mana/psy)",
        "[cmd] hp")]
[Help(
@"[cmd] increases one of your attributes.  When you start the game, your
character has standard attributes based on your class, and several
initial training sessions.  You can increase your attributes by
using these sessions at a trainer (there are several in town).

It takes one training session to improve an attribute, or to increase
mana or hp by 10.  You receive one session per level.

The best attributes to train first are WIS and CON.  WIS gives you more
practice when you gain a level.  CON gives you more hit points.
In the long run, your character will be most powerful if you train
WIS and CON both to 18 before practising or training anything else.")]
public class Train : PlayableCharacterGameAction
{
    protected override IGuard<IPlayableCharacter>[] Guards => [new RequiresMinPosition(Positions.Resting)];

    private enum Actions
    {
        Display,
        Attribute,
        Resource,
        Hp
    }

    private static ResourceKinds[] TrainableResources { get; } = [ResourceKinds.Mana, ResourceKinds.Psy];

    private Actions Action { get; set; }
    private BasicAttributes? Attribute { get; set; }
    private ResourceKinds? ResourceKind { get; set; }
    private int Cost { get; set; }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var trainer = Actor.Room?.NonPlayableCharacters.FirstOrDefault(x => Actor.CanSee(x) && x.ActFlags.IsSet("Train"));
        if (trainer == null)
            return StringHelpers.CantDoThatHere;

        // display
        if (actionInput.Parameters.Length == 0)
        {
            Action = Actions.Display;
            return null;
        }
        // basic attribute
        var attributeFound = Enum.GetValues<BasicAttributes>().Select(x => new { attribute = x, name = x.ToString() }).FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.name, actionInput.Parameters[0].Value));
        if (attributeFound != null)
        {
            BasicAttributes attribute = attributeFound.attribute;
            var max = GetMaxAttributeValue(attribute, Actor.Race as IPlayableRace, Actor.Class);
            if (Actor.BaseAttribute((CharacterAttributes)attribute) >= max)
                return $"Your {attributeFound.name} is already at maximum.";
            Cost = 1;
            Attribute = attributeFound.attribute;
            Action = Actions.Attribute;
        }
        // resource
        var resourceFound = Actor.Class?.ResourceKinds.Where(x => TrainableResources.Contains(x))
            .Select(x => new { resource = x, name = x.ToString() })
            .FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.name, actionInput.Parameters[0].Value));
        if (resourceFound != null)
        {
            Cost = 1;
            ResourceKind = resourceFound.resource;
            Action = Actions.Resource;
        }
        // hp
        if (StringCompareHelpers.StringStartsWith("hp", actionInput.Parameters[0].Value))
        {
            Cost = 1;
            Action = Actions.Hp;
        }

        // no action found
        if (Cost <= 0)
            return BuildCommandSyntax();

        // check cost
        if (Cost > Actor.Trains)
            return "You don't have enough training sessions.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        switch (Action)
        {
            case Actions.Display:
                Actor.Send("You have {0} training sessions left.", Actor.Trains);
                return;
            case Actions.Attribute:
                {
                    if (Attribute.HasValue)
                    {
                        Actor.UpdateBaseAttribute((CharacterAttributes)Attribute.Value, 1);
                        Actor.UpdateTrainsAndPractices(-1, 0);
                        Actor.Act(ActOptions.ToAll, "{0:p} {1} increases!", Actor, Attribute.Value.ToString());
                        Actor.Recompute();
                    }

                    return;
                }
                case Actions.Resource:
                {
                    if (ResourceKind.HasValue)
                    {
                        Actor.UpdateBaseMaxResource(ResourceKind.Value, 10);
                        Actor.UpdateTrainsAndPractices(-1, 0);
                        Actor.Act(ActOptions.ToAll, "{0:p} power increases!", Actor);
                        Actor.Recompute();
                    }
                }
                return;
            case Actions.Hp:
                {
                    Actor.UpdateBaseMaxResource(ResourceKinds.HitPoints, 10);
                    Actor.UpdateTrainsAndPractices(-1, 0);
                    Actor.Act(ActOptions.ToAll, "{0:p} durability increases!", Actor);
                    Actor.Recompute();
                    return;
                }
        }
    }

    private static int GetMaxAttributeValue(BasicAttributes attribute, IPlayableRace? race, IClass? @class)
    {
        int value = race?.GetMaxAttribute(attribute) ?? 18;
        if (@class != null && attribute == @class.PrimeAttribute)
        {
            value += 2;
            if (race?.EnhancedPrimeAttribute == true)
                value++;
        }
        return value;
    }
}
