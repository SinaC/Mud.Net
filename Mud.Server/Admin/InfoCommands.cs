using System.Text;
using Mud.Server.Constants;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [Command("cstat")]
        protected virtual bool DoStat(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("mstat whom?");
            else
            {
                ICharacter character = FindHelpers.FindByName(World.World.Instance.GetCharacters(), parameters[0]);
                if (character == null)
                    Send(StringHelpers.CharacterNotFound);
                else
                {
                    StringBuilder sb = new StringBuilder();
                    if (character.Blueprint != null)
                        sb.AppendFormatLine("Blueprint: {0}", character.Blueprint.Id);
                        // TODO: display blueprint
                    else
                        sb.AppendLine("No blueprint");
                    sb.AppendFormatLine("Name: {0}", character.Name);
                    sb.AppendFormatLine("DisplayName: {0}", character.DisplayName);
                    if (character.Slave != null)
                        sb.AppendFormatLine("Slave: {0}", character.Slave.Name);
                    if (character.ImpersonatedBy != null)
                        sb.AppendFormatLine("Impersonated by {0}", character.ImpersonatedBy.Name);
                    if (character.ControlledBy != null)
                        sb.AppendFormatLine("Controlled by {0}", character.ControlledBy.Name);
                    if (character.Fighting != null)
                        sb.AppendFormatLine("Fighting: {0}", character.Fighting.Name);
                    sb.AppendFormatLine("Room: {0} [vnum: {1}]", character.Room.Name, character.Room.Blueprint == null ? -1 : character.Room.Blueprint.Id);
                    sb.AppendFormatLine("Level: {0} Sex: {1}", character.Level, character.Sex);
                    sb.AppendFormatLine("Hitpoints: Current: {0} Max: {1}", character.HitPoints, character.MaxHitPoints);
                    sb.AppendLine("Attributes:");
                    foreach (AttributeTypes attributeType in EnumHelpers.GetValues<AttributeTypes>())
                        sb.AppendFormatLine("{0}: Current: {1} Base: {2}", attributeType, character.CurrentAttribute(attributeType), character.BaseAttribute(attributeType));
                    Send(sb);
                }
            }
            return true;
        }
    }
}
