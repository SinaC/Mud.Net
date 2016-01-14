using Mud.Server.Abilities;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class Character
    {
        [Command("use", Priority = 2)]
        [Command("cast", Priority = 2)]
        protected virtual bool DoCast(string rawParameters, params CommandParameter[] parameters)
        {
            AbilityManager manager = new AbilityManager();
            manager.Process(this, parameters);
            return true;
        }
    }
}
