using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Room;
using Mud.Server.Common.Helpers;
using Mud.Server.Entity;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using System.Diagnostics;
using System.Text;

namespace Mud.Server.Room;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Room : EntityBase, IRoom
{
    private ITimeManager TimeManager { get; }

    private readonly List<ICharacter> _people;
    private readonly List<IItem> _content;

    public Room(ILogger logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, ITimeManager timeManager, Guid guid, RoomBlueprint blueprint, IArea area)
        : base(logger, gameActionManager, commandParser, abilityManager, messageForwardOptions, guid, blueprint.Name, blueprint.Description)
    {
        TimeManager = timeManager;

        Blueprint = blueprint;
        _people = [];
        _content = [];
        Exits = new IExit[EnumHelpers.GetCount<ExitDirections>()];

        BaseRoomFlags = NewAndCopyAndSet<IRoomFlags, IRoomFlagValues>(() => new RoomFlags(serviceProvider), blueprint.RoomFlags, null);
        RoomFlags = NewAndCopyAndSet<IRoomFlags, IRoomFlagValues>(() => new RoomFlags(serviceProvider), BaseRoomFlags, null);
        SectorType = blueprint.SectorType;
        BaseHealRate = blueprint.HealRate;
        HealRate = BaseHealRate;
        BaseResourceRate = blueprint.ResourceRate;
        ResourceRate = BaseResourceRate;
        MaxSize = blueprint.MaxSize;

        Area = area;
        Area.AddRoom(this);
    }

    #region IRoom

    #region IEntity

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<Room>();

    #endregion

    public override string DisplayName => Name.UpperFirstLetter();

    public override string DebugName => $"{DisplayName}[{Blueprint.Id}]";

    // Recompute

    public override void ResetAttributes()
    {
        RoomFlags.Copy(BaseRoomFlags);
        HealRate = BaseHealRate;
        ResourceRate = BaseResourceRate;
    }

    public override void Recompute()
    {
        Logger.LogDebug("Room.Recompute: {name}", DebugName);

        // 0) Reset
        ResetAttributes();

        // 1) Apply own auras
        ApplyAuras(this);

        // 2) Apply people auras
        foreach (ICharacter character in People)
            ApplyAuras(character);

        // 3) Apply content auras
        foreach (IItem item in Content)
            ApplyAuras(item);
    }

    //
    public override void OnRemoved()
    {
        base.OnRemoved();
        Blueprint = null!;
        _people.Clear();
        for (int i = 0; i < Exits.Length; i++)
            Exits[i] = null!;
        _content.Clear();
    }

    #endregion

    #region IContainer

    public IEnumerable<IItem> Content => _content.Where(x => x.IsValid);

    public bool PutInContainer(IItem obj)
    {
        //if (obj.ContainedInto != null)
        //{
        //    Logger.LogError("PutInContainer: {0} is already in container {1}.", obj.DebugName, obj.ContainedInto.DebugName);
        //    return false;
        //}
        _content.Add(obj);
        return true;
    }

    public bool GetFromContainer(IItem obj)
    {
        bool removed = _content.Remove(obj);
        return removed;
    }

    #endregion

    public RoomBlueprint Blueprint { get; private set; }

    public ILookup<string, string> ExtraDescriptions => Blueprint.ExtraDescriptions;

    public IRoomFlags BaseRoomFlags { get; protected set; }
    public IRoomFlags RoomFlags { get; protected set; }

    public IArea Area { get; }

    public IEnumerable<ICharacter> People => _people.Where(x => x.IsValid);

    public IEnumerable<INonPlayableCharacter> NonPlayableCharacters => People.OfType<INonPlayableCharacter>();

    public IEnumerable<IPlayableCharacter> PlayableCharacters => People.OfType<IPlayableCharacter>();

    public IEnumerable<(INonPlayableCharacter character, TBlueprint blueprint)> GetNonPlayableCharacters<TBlueprint>()
        where TBlueprint : CharacterBlueprintBase
    {
        foreach (var character in NonPlayableCharacters.Where(x => x.Blueprint is TBlueprint))
            yield return (character, (character.Blueprint as TBlueprint)!);
    }

    public Sizes? MaxSize { get; }

    public int BaseHealRate { get; }
    public int HealRate { get; protected set; }

    public int BaseResourceRate { get; }
    public int ResourceRate { get; protected set; }

    public int Light { get; protected set; }

    public SectorTypes SectorType { get; }

    public bool IsPrivate
    {
        get
        {
            // TODO: ownership
            int count = People.Count();
            if (RoomFlags.IsSet("Private") && count >= 2)
                return true;
            if (RoomFlags.IsSet("Solitary") && count >= 1)
                return true;
            if (RoomFlags.IsSet("ImpOnly"))
                return true;
            return false;
        }
    }

    public bool IsDark
    {
        get
        {
            if (Light > 0)
                return false;
            if (RoomFlags.IsSet("Dark"))
                return true;
            if (SectorType == SectorTypes.Inside
                || SectorType == SectorTypes.City
                || RoomFlags.IsSet("Indoors"))
                return false;
            if (TimeManager.SunPhase == SunPhases.Set
                || TimeManager.SunPhase == SunPhases.Dark)
                return true;
            return false;
        }
    }

    public IExit[] Exits { get; }

    public IExit? this[ExitDirections direction]
        => Exits[(int) direction];

    public IRoom? GetRoom(ExitDirections direction)
    {
        var exit = this[direction];
        return exit?.Destination;
    }

    public bool Enter(ICharacter character)
    {
        if (_people.Contains(character))
            Logger.LogError("IRoom.Enter: Character {characterName} is already in Room {roomName}", character.DebugName, character.Room.DebugName);
        else
            _people.Add(character);
        // Update light
        var light = character.GetEquipment<IItemLight>(EquipmentSlots.Light);
        if (light != null
            && light.IsLighten)
            Light++;
        // Update location quest
        if (character is IPlayableCharacter playableCharacter)
        {
            foreach(IQuest quest in playableCharacter.Quests)
                quest.Update(this);
        }
        return true;
    }

    public bool Leave(ICharacter character)
    {
        // Update light
        var light = character.GetEquipment<IItemLight>(EquipmentSlots.Light);
        if (light != null
            && light.IsLighten
            && Light > 0)
            Light--;

        // TODO: check if in room
        bool removed = _people.Remove(character);
        return removed;
    }

    public void IncreaseLight()
    {
        Light++;
    }

    public void DecreaseLight()
    {
        Light = Math.Max(0, Light - 1);
    }

    public StringBuilder Append(StringBuilder sb, ICharacter viewer)
    {
        var playableCharacter = viewer as IPlayableCharacter;
        // Room name
        if (playableCharacter?.IsImmortal == true)
            sb.AppendFormatLine($"%c%{DisplayName} [{Blueprint?.Id.ToString() ?? "???"}]%x%");
        else
            sb.AppendFormatLine("%c%{0}%x%", DisplayName);
        // Room description
        sb.Append(Description);
        // Exits
        if (playableCharacter != null && playableCharacter.AutoFlags.HasFlag(AutoFlags.Exit))
            AppendExits(sb, viewer, true);
        ItemsHelpers.AppendItems(sb, Content.Where(viewer.CanSee), viewer, false, false);
        AppendCharacters(sb, viewer);
        return sb;
    }

    public StringBuilder AppendExits(StringBuilder sb, ICharacter viewer, bool compact)
    {
        if (compact)
            sb.Append("[Exits:");
        else if (viewer is IPlayableCharacter playableCharacter && playableCharacter.IsImmortal)
            sb.AppendFormatLine($"Obvious exits from room {Blueprint?.Id.ToString() ?? "???"}:");
        else
            sb.AppendLine("Obvious exits:");
        var exitFound = false;
        var isImmortal = viewer is IPlayableCharacter { IsImmortal: true };
        foreach (var direction in EnumHelpers.GetValues<ExitDirections>())
        {
            var exit = this[direction];
            var destination = exit?.Destination;
            if (exit != null && destination != null && viewer.CanSee(exit))
            {
                if (compact)
                {
                    sb.Append(' ');
                    if (exit.IsHidden)
                        sb.Append('[');
                    if (exit.IsClosed)
                        sb.Append('(');
                    sb.AppendFormat("{0}", direction.ToString().ToLowerInvariant());
                    if (exit.IsClosed)
                        sb.Append(')');
                    if (exit.IsHidden)
                        sb.Append(']');
                }
                else
                {
                    sb.Append(direction.DisplayName());
                    sb.Append(" - ");
                    if (exit.IsClosed)
                        sb.Append("A closed door");
                    else if (destination.IsDark && !isImmortal)
                        sb.Append("Too dark to tell");
                    else
                        sb.Append(exit.Destination.DisplayName);
                    if (exit.IsClosed)
                        sb.Append(" (CLOSED)");
                    if (exit.IsHidden)
                        sb.Append(" [HIDDEN]");
                    if (isImmortal)
                        sb.Append($" (room {destination.Blueprint?.Id.ToString() ?? "???"})");
                    sb.AppendLine();
                }
                exitFound = true;
            }
        }
        if (!exitFound)
        {
            if (compact)
                sb.AppendLine(" none");
            else
                sb.AppendLine("None.");
        }
        if (compact)
            sb.AppendLine("]");
        return sb;
    }

    public (IExit? exit, ExitDirections exitDirection) VerboseFindDoor(ICharacter character, ICommandParameter parameter)
    {
        bool found = FindDoor(character, parameter, out var exitDirection, out var wasAskingForDirection);
        if (!found)
        {
            //  if open north -> I see no door north here.
            //  if open black door -> I see no black door here.
            if (wasAskingForDirection)
                character.Send($"I see no door {parameter.Value} here.");
            else
                character.Send($"I see no {parameter.Value} here.");
            return (null, ExitDirections.North);
        }
        var exit = this[exitDirection];
        if (exit == null)
            return (null, ExitDirections.North);
        if (!exit.IsDoor)
        {
            character.Send("You can't do that.");
            return (null, ExitDirections.North);
        }
        return (exit, exitDirection);
    }

    public void ApplyAffect(IRoomFlagsAffect affect)
    {
        switch (affect.Operator)
        {
            case AffectOperators.Add:
            case AffectOperators.Or:
                RoomFlags.Set(affect.Modifier);
                break;
            case AffectOperators.Assign:
                RoomFlags.Copy(affect.Modifier);
                break;
            case AffectOperators.Nor:
                RoomFlags.Unset(affect.Modifier);
                break;
        }
    }

    public void ApplyAffect(IRoomHealRateAffect affect)
    {
        switch (affect.Operator)
        {
            case AffectOperators.Add:
                HealRate += affect.Modifier;
                break;
            case AffectOperators.Assign:
                HealRate = affect.Modifier;
                break;
            default:
                Logger.LogWarning("Room.ApplyAffect(IRoomHealRateAffect): wrong operator {operator}.", affect.Operator);
                break;
        }
    }

    public void ApplyAffect(IRoomResourceRateAffect affect)
    {
        switch (affect.Operator)
        {
            case AffectOperators.Add:
                ResourceRate += affect.Modifier;
                break;
            case AffectOperators.Assign:
                ResourceRate = affect.Modifier;
                break;
            default:
                Logger.LogWarning("Room.ApplyAffect(IRoomResourceRateAffect): wrong operator {operator}.", affect.Operator);
                break;
        }
    }

    #endregion

    protected void ApplyAuras(IEntity entity)
    {
        if (!entity.IsValid)
            return;
        foreach (var aura in entity.Auras.Where(x => x.IsValid))
        {
            foreach (var affect in aura.Affects.OfType<IRoomAffect>())
            {
                affect.Apply(this);
            }
        }
    }

    protected StringBuilder AppendCharacters(StringBuilder sb, ICharacter viewer)
    {
        foreach (var victim in People.Where(x => x != viewer))
        {
            //  (see act_info.C:714 show_char_to_char)
            if (viewer.CanSee(victim)) // see act_info.C:375 show_char_to_char_0)
                victim.AppendInRoom(sb, viewer);
            else if (IsDark && victim.CharacterFlags.IsSet("Infrared"))
                sb.AppendLine("You see glowing red eyes watching YOU!");
        }

        return sb;
    }

    protected bool FindDoor(ICharacter character, ICommandParameter parameter, out ExitDirections exitDirection, out bool wasAskingForDirection)
    {
        if (ExitDirectionsExtensions.TryFindDirection(parameter.Value, out exitDirection))
        {
            wasAskingForDirection = true;
            return true;
        }
        wasAskingForDirection = false;
        //exit = Room.Exits.FirstOrDefault(x => x?.Destination != null && x.IsDoor && x.Keywords.Any(k => FindHelpers.StringStartsWith(k, parameter.Value)));
        foreach (var direction in EnumHelpers.GetValues<ExitDirections>())
        {
            var exit = this[direction];
            if (exit?.Destination != null && exit.IsDoor && exit.Keywords.Any(k => StringCompareHelpers.StringStartsWith(k, parameter.Value)))
            {
                exitDirection = direction;
                return true;
            }
        }
        return false;
    }

    //
    private string DebuggerDisplay => $"R {Name} INC:{IncarnatedBy?.Name} BId:{Blueprint?.Id}";
}
