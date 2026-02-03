using Mud.Common;
using Mud.Server.Ability.Interfaces;
using Mud.Server.AbilityGroup.Interfaces;
using Mud.Server.Class.Interfaces;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Effects.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Special;
using Mud.Server.Race.Interfaces;
using System.Text;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("stat", "Information")]
public class Stat : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [];

    private IAreaManager AreaManager { get; }
    private IRoomManager RoomManager { get; }
    private ICharacterManager CharacterManager { get; }
    private IItemManager ItemManager { get; }
    private IQuestManager QuestManager { get; }
    private IAdminManager AdminManager { get; }
    private IPlayerManager PlayerManager { get; }
    private IGameActionManager GameActionManager { get; }
    private IAffectManager AffectManager { get; }
    private IEffectManager EffectManager { get; }
    private IAbilityManager AbilityManager { get; }
    private IAbilityGroupManager AbilityGroupManager { get; }
    private IRaceManager RaceManager { get; }
    private IClassManager ClassManager { get; }
    private IWeaponEffectManager WeaponEffectManager { get; }
    private ISpecialBehaviorManager SpecialBehaviorManager { get; }

    public Stat(IAreaManager areaManager, IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager, IQuestManager questManager, IAdminManager adminManager, IPlayerManager playerManager, IGameActionManager gameActionManager, IAffectManager affectManager, IEffectManager effectManager, IAbilityManager abilityManager, IAbilityGroupManager abilityGroupManager, IRaceManager raceManager, IClassManager classManager, IWeaponEffectManager weaponEffectManager, ISpecialBehaviorManager specialBehaviorManager)
    {
        AreaManager = areaManager;
        RoomManager = roomManager;
        CharacterManager = characterManager;
        ItemManager = itemManager;
        QuestManager = questManager;
        AdminManager = adminManager;
        PlayerManager = playerManager;
        GameActionManager = gameActionManager;
        AffectManager = affectManager;
        EffectManager = effectManager;
        AbilityManager = abilityManager;
        AbilityGroupManager = abilityGroupManager;
        RaceManager = raceManager;
        ClassManager = classManager;
        WeaponEffectManager = weaponEffectManager;
        SpecialBehaviorManager = specialBehaviorManager;
    }

    public override void Execute(IActionInput actionInput)
    {

        StringBuilder sb = new();
        sb.AppendFormatLine("#Admins: {0}", AdminManager.Admins.Count());
        sb.AppendFormatLine("#Players: {0}", PlayerManager.Players.Count());
        sb.AppendFormatLine("#Commands: {0}", GameActionManager.GameActions.Count());
        sb.AppendFormatLine("#Classes: {0}", ClassManager.Classes.Count());
        sb.AppendFormatLine("#Races: {0}", RaceManager.PlayableRaces.Count());
        sb.AppendFormatLine("#Affects: {0}", AffectManager.Count);
        sb.AppendFormatLine("#Effects: {0}", EffectManager.Count);
        sb.AppendFormatLine("#WeaponEffects: {0}", WeaponEffectManager.Count);
        sb.AppendFormatLine("#Specials: {0}", SpecialBehaviorManager.Count);
        sb.AppendFormatLine("#AbilityGroups: {0}", AbilityGroupManager.AbilityGroups.Count());
        sb.AppendLine("Abilities");
        sb.AppendFormatLine("   #Weapons: {0}", AbilityManager.Abilities.Count(x => x.Type == AbilityTypes.Weapon));
        sb.AppendFormatLine("   #Passives: {0}", AbilityManager.Abilities.Count(x => x.Type == AbilityTypes.Passive));
        sb.AppendFormatLine("   #Spells: {0}", AbilityManager.Abilities.Count(x => x.Type == AbilityTypes.Spell));
        sb.AppendFormatLine("   #Skills: {0}", AbilityManager.Abilities.Count(x => x.Type == AbilityTypes.Skill));
        sb.AppendLine("Blueprints:");
        sb.AppendFormatLine("   #Areas: {0}", AreaManager.AreaBlueprints.Count);
        sb.AppendFormatLine("   #Rooms: {0}", RoomManager.RoomBlueprints.Count);
        sb.AppendFormatLine("   #Characters: {0}", CharacterManager.CharacterBlueprints.Count);
        sb.AppendFormatLine("   #Items: {0}", ItemManager.ItemBlueprints.Count);
        sb.AppendFormatLine("   #Quests: {0}", QuestManager.QuestBlueprints.Count);
        sb.AppendLine("Entities:");
        sb.AppendFormatLine("   #Areas: {0}", AreaManager.Areas.Count());
        sb.AppendFormatLine("   #Rooms: {0}", RoomManager.Rooms.Count());
        sb.AppendFormatLine("   #Characters: {0}", CharacterManager.Characters.Count());
        sb.AppendFormatLine("   #NPC: {0}", CharacterManager.NonPlayableCharacters.Count());
        sb.AppendFormatLine("   #PC: {0}", CharacterManager.PlayableCharacters.Count());
        sb.AppendFormatLine("   #Items: {0}", ItemManager.Items.Count());

        Actor.Send(sb);
    }
}
