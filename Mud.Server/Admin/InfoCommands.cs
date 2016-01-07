using System.Security.Cryptography;
using System.Text;
using Mud.Server.Constants;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [Command("mstat")]
        protected virtual bool DoMstat(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("mstat whom?");
            else
            {
                ICharacter character = FindHelpers.FindByName(World.World.Instance.GetCharacters(), parameters[0]);
                if (character == null)
                    Send(StringHelpers.NotFound);
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

        [Command("ostat")]
        protected virtual bool DoOstat(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("ostat what?");
            else
            {
                IItem item = FindHelpers.FindByName(World.World.Instance.GetItems(), parameters[0]);
                if (item == null)
                    Send(StringHelpers.NotFound);
                else
                {
                    StringBuilder sb = new StringBuilder();
                    if (item.Blueprint != null)
                        sb.AppendFormatLine("Blueprint: {0}", item.Blueprint.Id);
                    // TODO: display blueprint
                    else
                        sb.AppendLine("No blueprint");
                    sb.AppendFormatLine("Name: {0}", item.Name);
                    sb.AppendFormatLine("DisplayName: {0}", item.DisplayName);
                    if (item.ContainedInto != null)
                        sb.AppendFormatLine("Contained in {0}", item.ContainedInto.Name);
                    IEquipable equipable = item as IEquipable;
                    if (equipable != null)
                        sb.AppendFormatLine("Equiped by {0}", equipable.EquipedBy == null ? "(none)" : equipable.EquipedBy.Name);
                    else
                        sb.AppendLine("Cannot be equiped");
                    sb.AppendFormatLine("Cost: {0} Weight: {1}", item.Cost, item.Weight);
                    sb.AppendFormatLine("Type: {0}", item.GetType().Name);
                    IItemArmor armor = item as IItemArmor;
                    if (armor != null)
                        sb.AppendFormatLine("Armor type: {0} Armor value: {1}", armor.ArmorKind, armor.Armor);
                    else
                    {
                        IItemContainer container = item as IItemContainer;
                        if (container != null)
                            sb.AppendFormatLine("Item count: {0} Weight multiplier: {1}", container.ItemCount, container.WeightMultiplier);
                        else
                        {
                            IItemCorpse corpse = item as IItemCorpse;
                            if (corpse != null)
                                ; // TODO: additional info for IItemCorpse
                            else
                            {
                                IItemLight light = item as IItemLight;
                                if (light != null)
                                    sb.AppendFormatLine("Time left: {0}", light.TimeLeft);
                                else
                                {
                                    IItemWeapon weapon = item as IItemWeapon;
                                    if (weapon != null)
                                        sb.AppendFormatLine("Weapon type: {0}  {1}d{2} {3}", weapon.Type, weapon.DiceCount, weapon.DiceValue, weapon.DamageType);
                                    else
                                        sb.AppendLine("UNHANDLED ITEM TYPE");
                                }
                            }
                        }
                    }
                    Send(sb);
                }
            }
            return true;
        }
    }
}
