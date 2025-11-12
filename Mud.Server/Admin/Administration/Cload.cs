using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using System;
using Mud.Server.Interfaces;

namespace Mud.Server.Admin.Administration
{
    [AdminCommand("cload", "Admin", MustBeImpersonated = true)]
    [Alias("mload")]
    [Syntax("[cmd] <id>")]
    public class Cload : AdminGameAction
    {
        private ICharacterManager CharacterManager { get; }
        private IWiznet Wiznet { get; }

        public int BlueprintId { get; protected set; }
        public CharacterBlueprintBase CharacterBlueprint { get; protected set; }

        public Cload(ICharacterManager characterManager, IWiznet wiznet)
        {
            CharacterManager = characterManager;
            Wiznet = wiznet;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0 || !actionInput.Parameters[0].IsNumber)
                return BuildCommandSyntax();

            BlueprintId = actionInput.Parameters[0].AsNumber;

            CharacterBlueprint = CharacterManager.GetCharacterBlueprint(BlueprintId);
            if (CharacterBlueprint == null)
                return "No character with that id.";

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            INonPlayableCharacter character = CharacterManager.AddNonPlayableCharacter(Guid.NewGuid(), CharacterBlueprint, Impersonating.Room);
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
