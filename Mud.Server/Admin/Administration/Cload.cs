using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.World;
using System;

namespace Mud.Server.Admin.Administration
{
    [AdminCommand("cload", "Admin", Priority = 10, MustBeImpersonated = true)]
    [AdminCommand("mload", "Admin", Priority = 10, MustBeImpersonated = true)]
    [Syntax("[cmd] <id>")]
    public class Cload : AdminGameAction
    {
        private IWorld World { get; }
        private IWiznet Wiznet { get; }

        public int BlueprintId { get; protected set; }
        public CharacterBlueprintBase CharacterBlueprint { get; protected set; }

        public Cload(IWorld world, IWiznet wiznet)
        {
            World = world;
            Wiznet = wiznet;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return null;

            if (actionInput.Parameters.Length == 0 || !actionInput.Parameters[0].IsNumber)
                return BuildCommandSyntax();

            BlueprintId = actionInput.Parameters[0].AsNumber;

            CharacterBlueprint = World.GetCharacterBlueprint(BlueprintId);
            if (CharacterBlueprint == null)
                return "No character with that id.";

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            INonPlayableCharacter character = World.AddNonPlayableCharacter(Guid.NewGuid(), CharacterBlueprint, Impersonating.Room);
            if (character == null)
            {
                Wiznet.Wiznet($"DoCload: character with id {BlueprintId} cannot be created", WiznetFlags.Bugs, AdminLevels.Implementor);
                Actor.Send("Character cannot be created.");
                return;
            }

            Wiznet.Wiznet($"{Actor.DisplayName} loads {character.DebugName}.", WiznetFlags.Load);

            Impersonating.Act(ActOptions.ToAll, "{0:N} {0:h} created {1:n}!", Actor.Impersonating, character);
            Actor.Send("Ok.");
        }
    }
}
