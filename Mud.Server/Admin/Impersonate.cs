using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Room;
using Mud.Settings;

namespace Mud.Server.Admin
{
    [AdminCommand("impersonate", "Avatar", Priority = 0)]
    [Syntax(
        "[cmd]",
        "[cmd] <character>")]
    public class Impersonate : AdminGameAction
    {
        private IRoomManager RoomManager { get; }
        private ICharacterManager CharacterManager { get; }
        private ISettings Settings { get; }
        private IWiznet Wiznet { get; }

        public Impersonate(IRoomManager roomManager, ICharacterManager characterManager, ISettings settings, IWiznet wiznet)
        {
            RoomManager = roomManager;
            CharacterManager = characterManager;
            Settings = settings;
            Wiznet = wiznet;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (Actor.Incarnating != null)
                return $"You are already incarnating {Actor.Incarnating.DisplayName}.";

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            Player.Avatar.Impersonate impersonate = new Player.Avatar.Impersonate(RoomManager, CharacterManager, Settings, Wiznet);

            string guards = impersonate.Guards(actionInput);

            if (guards != null)
                Actor.Send(guards);
            else
                impersonate.Execute(actionInput);
        }
    }
}
