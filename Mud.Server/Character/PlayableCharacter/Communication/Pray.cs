using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.PlayableCharacter.Communication
{
    [PlayableCharacterCommand("pray", "Communication", MinPosition = Positions.Dead)]
    [Syntax("[cmd] <msg>")]
    public class Pray : PlayableCharacterGameAction
    {
        private IAdminManager AdminManager { get; }

        public string What { get; protected set; }

        public Pray(IAdminManager adminManager)
        {
            AdminManager = adminManager;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Pray what?";

            What = actionInput.RawParameters;
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            string phrase = $"%g%{Actor.DisplayName} has prayed '%x%{What}%g%'%x%";
            foreach (IAdmin admin in AdminManager.Admins)
                admin.Send(phrase);
        }
    }
}
