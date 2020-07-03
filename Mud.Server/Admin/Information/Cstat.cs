using Mud.Common;
using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using System.Linq;
using System.Text;

namespace Mud.Server.Admin.Information
{
    [AdminCommand("cstat", "Information")]
    [Alias("mstat")]
    [Syntax("[cmd] <character>")]
    public class Cstat : AdminGameAction
    {
        private ICharacterManager CharacterManager { get; }

        public ICharacter Whom { get; protected set; }

        public Cstat(ICharacterManager characterManager)
        {
            CharacterManager = characterManager;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if(actionInput.Parameters.Length == 0)
                return BuildCommandSyntax();

            Whom = Impersonating == null
                ? FindHelpers.FindByName(CharacterManager.Characters, actionInput.Parameters[0])
                : FindHelpers.FindChararacterInWorld(CharacterManager, Impersonating, actionInput.Parameters[0]);
            if (Whom == null)
                return StringHelpers.NotFound;
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            INonPlayableCharacter nonPlayableWhom = Whom as INonPlayableCharacter;
            IPlayableCharacter playableWhom = Whom as IPlayableCharacter;
            StringBuilder sb = new StringBuilder();
            if (nonPlayableWhom?.Blueprint != null)
            {
                sb.AppendFormatLine("Blueprint: {0}", nonPlayableWhom.Blueprint.Id);
                // TODO: display blueprint
                if (nonPlayableWhom.Blueprint is CharacterQuestorBlueprint characterQuestorBlueprint)
                {
                    sb.AppendLine($"Quest giver: {characterQuestorBlueprint.QuestBlueprints?.Length ?? 0}");
                    foreach (var questBlueprint in characterQuestorBlueprint.QuestBlueprints ?? Enumerable.Empty<QuestBlueprint>())
                    {
                        sb.AppendLine($"  Quest: {questBlueprint.Id}");
                        sb.AppendLine($"    Title: {questBlueprint.Title}");
                        sb.AppendLine($"    Level: {questBlueprint.Level}");
                        sb.AppendLine($"    Description: {questBlueprint.Description}");
                        sb.AppendLine($"    Experience: {questBlueprint.Experience}");
                        sb.AppendLine($"    Gold: {questBlueprint.Gold}");
                        sb.AppendLine($"    ShouldQuestItemBeDestroyed: {questBlueprint.ShouldQuestItemBeDestroyed}");
                        // TODO: display KillLootTable, ItemObjectives, KillObjectives, LocationObjectives
                    }
                }
                if (nonPlayableWhom.Blueprint is CharacterShopBlueprint characterShopBlueprint)
                {
                    sb.AppendLine("Shopkeeper:");
                    sb.AppendFormatLine("BuyTypes: {0}", string.Join(",", characterShopBlueprint.BuyBlueprintTypes.Select(x => x.ToString().AfterLast('.').Replace("Blueprint", string.Empty))));
                    sb.AppendFormatLine("Profit buy: {0}% sell: {1}%", characterShopBlueprint.ProfitBuy, characterShopBlueprint.ProfitSell);
                    sb.AppendFormatLine("Open hour: {0} Close hour: {1}", characterShopBlueprint.OpenHour, characterShopBlueprint.CloseHour);
                }
            }
            else
                sb.AppendLine("No blueprint");
            sb.AppendFormatLine("Name: {0} Keywords: {1}", Whom.Name, string.Join(",", Whom.Keywords));
            sb.AppendFormatLine("DisplayName: {0}", Whom.DisplayName);
            sb.AppendFormatLine("Description: {0}", Whom.Description);
            if (Whom.Leader != null)
                sb.AppendFormatLine("Leader: {0}", Whom.Leader.DisplayName);
            if (nonPlayableWhom?.Master != null)
                sb.AppendFormatLine("Master: {0}", nonPlayableWhom.Master.DisplayName);
            if (playableWhom?.Group != null)
                foreach (IPlayableCharacter member in playableWhom.Group.Members)
                    sb.AppendFormatLine("Group member: {0}", member.DisplayName);
            if (playableWhom?.Pets.Any() == true)
                foreach (INonPlayableCharacter pet in playableWhom.Pets)
                    sb.AppendFormatLine("Pet: {0}", pet.DisplayName);
            if (Whom.IncarnatedBy != null)
                sb.AppendFormatLine("Incarnated by {0}", Whom.IncarnatedBy.DisplayName);
            else
                sb.AppendFormatLine("Incarnatable: {0}", Whom.Incarnatable);
            if (playableWhom?.ImpersonatedBy != null)
                sb.AppendFormatLine("Impersonated by {0}", playableWhom.ImpersonatedBy.DisplayName);
            if (Whom.Fighting != null)
                sb.AppendFormatLine("Fighting: {0}", Whom.Fighting.DisplayName);
            sb.AppendFormatLine("Position: {0} Stunned: {1}", Whom.Position, Whom.Stunned);
            sb.AppendFormatLine("Furniture: {0}", Whom.Furniture?.DisplayName ?? "(none)");
            sb.AppendFormatLine("Room: {0} [vnum: {1}]", Whom.Room.DisplayName, Whom.Room.Blueprint?.Id ?? -1);
            sb.AppendFormatLine("Race: {0} Class: {1}", Whom.Race?.DisplayName ?? "(none)", Whom.Class?.DisplayName ?? "(none)");
            sb.AppendFormatLine("Sex: {0} (base: {1})", Whom.Sex, Whom.BaseSex);
            sb.AppendFormatLine("Size: {0} (base: {1})", Whom.Size, Whom.BaseSize);
            sb.AppendFormatLine("Silver: {0} Gold: {1}", Whom.SilverCoins, Whom.GoldCoins);
            sb.AppendFormatLine("Carry: {0}/{1} Weight: {2}/{3}", Whom.CarryNumber, Whom.MaxCarryNumber, Whom.CarryWeight, Whom.MaxCarryWeight);
            if (playableWhom != null)
                sb.AppendFormatLine("Level: {0} Experience: {1} NextLevel: {2}", playableWhom.Level, playableWhom.Experience, playableWhom.ExperienceToLevel);
            else
                sb.AppendFormatLine("Level: {0}", Whom.Level);
            if (nonPlayableWhom != null)
                sb.AppendFormatLine("Damage: {0}d{1}+{2} {3} {4}", nonPlayableWhom.DamageDiceCount, nonPlayableWhom.DamageDiceValue, nonPlayableWhom.DamageDiceBonus, nonPlayableWhom.DamageType, nonPlayableWhom.DamageNoun);
            sb.AppendFormatLine("Hitpoints: Current: {0} Max: {1}", Whom.HitPoints, Whom.MaxHitPoints);
            sb.AppendFormatLine("Movepoints: Current: {0} Max: {1}", Whom.MovePoints, Whom.MaxMovePoints);
            sb.AppendFormatLine("Flags: {0} (base: {1})", Whom.CharacterFlags, Whom.BaseCharacterFlags);
            sb.AppendFormatLine("Immunites: {0} (base: {1})", Whom.Immunities, Whom.BaseImmunities);
            sb.AppendFormatLine("Resistances: {0} (base: {1})", Whom.Resistances, Whom.BaseResistances);
            sb.AppendFormatLine("Vulnerabilities: {0} (base: {1})", Whom.Vulnerabilities, Whom.BaseVulnerabilities);
            sb.AppendFormatLine("Forms: {0} (base: {1})", Whom.BodyForms, Whom.BaseBodyForms);
            sb.AppendFormatLine("Parts: {0} (base: {1})", Whom.BodyParts, Whom.BaseBodyParts);
            sb.AppendFormatLine("Alignment: {0}", Whom.Alignment);
            sb.AppendLine("Attributes:");
            foreach (CharacterAttributes attribute in EnumHelpers.GetValues<CharacterAttributes>())
                sb.AppendFormatLine("{0}: {1} (base: {2})", attribute, Whom[attribute], Whom.BaseAttribute(attribute));
            foreach (ResourceKinds resourceKind in EnumHelpers.GetValues<ResourceKinds>())
                sb.AppendFormatLine("{0}: {1} Max: {2}", resourceKind, Whom[resourceKind], Whom.MaxResource(resourceKind));
            if (nonPlayableWhom != null)
            {
                sb.AppendFormatLine("Act: {0}", nonPlayableWhom.ActFlags);
                sb.AppendFormatLine("Offensive: {0}", nonPlayableWhom.OffensiveFlags);
                sb.AppendFormatLine("Assist: {0}", nonPlayableWhom.AssistFlags);
            }
            if (playableWhom != null)
            {
                sb.Append("Conditions: ");
                sb.AppendLine(string.Join(" ", EnumHelpers.GetValues<Conditions>().Select(x => $"{x}: {playableWhom[x]}")));
            }
            foreach (IAura aura in Whom.Auras)
                aura.Append(sb);
            if (Whom.Equipments.Any(x => x.Item != null))
            {
                sb.AppendLine("Equipments:");
                foreach (IEquippedItem equippedItem in Whom.Equipments.Where(x => x.Item != null))
                {
                    sb.Append(equippedItem.EquipmentSlotsToString());
                    sb.AppendLine($"{equippedItem.Item.DisplayName} [id: {equippedItem.Item.Blueprint?.Id.ToString() ?? " ??? "}]");
                }
            }

            if (Whom.Inventory.Any())
            {
                sb.AppendLine("Inventory:");
                foreach (IItem item in Whom.Inventory)
                    sb.AppendLine($"{item.DisplayName} [id: {item.Blueprint?.Id.ToString() ?? " ??? "}]");
            }
            //
            Actor.Send(sb);
        }
    }
}
