using Mud.Common;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Race;
using System.Linq;

namespace Mud.Server.Character.PlayableCharacter.Attribute
{
    [PlayableCharacterCommand("train", "Attributes", MinPosition = Positions.Resting)]
    [Syntax(
            "[cmd]",
            "[cmd] <attribute>",
            "[cmd] <resource> (mana/psy)",
            "[cmd] hp")]
    public class Train : PlayableCharacterGameAction
    {
        public enum Actions
        {
            Display,
            Attribute,
            Resource,
            Hp
        }

        public Actions Action { get; protected set; }
        public CharacterAttributes? Attribute { get; protected set; }
        public ResourceKinds? ResourceKind { get; protected set; }
        public int Cost { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            INonPlayableCharacter trainer = Actor.Room.NonPlayableCharacters.FirstOrDefault(x => Actor.CanSee(x) && x.ActFlags.HasFlag(ActFlags.Train));
            if (trainer == null)
                return "You can't do that here.";

            // display
            if (actionInput.Parameters.Length == 0)
            {
                Action = Actions.Display;
                return null;
            }
            // basic attribute
            var attributeFound = EnumHelpers.GetValues<BasicAttributes>().Select(x => new { attribute = (CharacterAttributes)x, name = x.ToString() }).FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.name, actionInput.Parameters[0].Value));
            if (attributeFound != null)
            {
                CharacterAttributes attribute = attributeFound.attribute;
                int max = GetMaxAttributeValue(attribute, Actor.Race as IPlayableRace, Actor.Class);
                if (Actor.BaseAttribute(attribute) >= max)
                    return $"Your {attributeFound.name} is already at maximum.";
                Cost = 1;
                Attribute = attributeFound.attribute;
                Action = Actions.Attribute;
            }
            // resource
            var resourceFound = Actor.Class?.ResourceKinds.Select(x => new { resource = x, name = x.ToString() }).FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.name, actionInput.Parameters[0].Value));
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
                        Actor.UpdateBaseAttribute(Attribute.Value, 1);
                        Actor.Act(ActOptions.ToAll, "{0:p} {1} increases!", Actor, Attribute.Value.ToString());
                        return;
                    }
                    case Actions.Resource:
                    {
                        Actor.UpdateMaxResource(ResourceKind.Value, 10);
                        Actor.Act(ActOptions.ToAll, "{0:p} power increases!", Actor);
                    }
                    return;
                case Actions.Hp:
                    {
                        Actor.UpdateBaseAttribute(CharacterAttributes.MaxHitPoints, 10);
                        Actor.UpdateHitPoints(10);
                        Actor.Act(ActOptions.ToAll, "{0:p} durability increases!", Actor);
                        return;
                    }
            }
        }

        private int GetMaxAttributeValue(CharacterAttributes attribute, IPlayableRace race, IClass @class)
        {
            int value = race?.GetStartAttribute(attribute) ?? 18;
            if (@class != null && (int)attribute == (int)@class.PrimeAttribute)
            {
                value += 2;
                if (race?.Name == "Human")
                    value++;
            }
            return value;
        }
    }
}
