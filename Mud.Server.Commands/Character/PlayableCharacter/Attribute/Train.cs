using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Race;

namespace Mud.Server.Commands.Character.PlayableCharacter.Attribute;

[PlayableCharacterCommand("train", "Attribute", MinPosition = Positions.Resting)]
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
    protected enum Actions
    {
        Display,
        Attribute,
        Resource,
        Hp
    }

    protected static ResourceKinds[] TrainableResources = [ResourceKinds.Mana, ResourceKinds.Psy];

    protected Actions Action { get; set; }
    protected BasicAttributes? Attribute { get; set; }
    protected ResourceKinds? ResourceKind { get; set; }
    protected int Cost { get; set; }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var trainer = Actor.Room?.NonPlayableCharacters.FirstOrDefault(x => Actor.CanSee(x) && x.ActFlags.IsSet("Train"));
        if (trainer == null)
            return "You can't do that here.";

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
                        Actor.UpdateMaxResource(ResourceKind.Value, 10);
                        Actor.UpdateTrainsAndPractices(-1, 0);
                        Actor.Act(ActOptions.ToAll, "{0:p} power increases!", Actor);
                        Actor.Recompute();
                    }
                }
                return;
            case Actions.Hp:
                {
                    Actor.UpdateBaseAttribute(CharacterAttributes.MaxHitPoints, 10);
                    Actor.UpdateHitPoints(10);
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
