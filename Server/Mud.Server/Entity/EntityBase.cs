using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common;
using Mud.DataStructures.Flags;
using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Actor;
using Mud.Server.Common.Helpers;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using System.Diagnostics;
using System.Text;

namespace Mud.Server.Entity;

public abstract class EntityBase : ActorBase, IEntity
{
    private readonly List<IAura> _auras;

    protected ICommandParser CommandParser { get; }
    protected bool PrefixForwardedMessages { get; }
    protected bool ForwardSlaveMessages { get; }

    protected EntityBase(ILogger<EntityBase> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions)
        : base(logger, gameActionManager)
    {
        CommandParser = commandParser;
        PrefixForwardedMessages = messageForwardOptions.Value.PrefixForwardedMessages;
        ForwardSlaveMessages = messageForwardOptions.Value.ForwardSlaveMessages;

        _auras = [];
    }

    protected void Initialize(Guid guid, string name, string description)
    {
        if (guid == Guid.Empty)
            guid = Guid.NewGuid();
        Id = guid;
        Name = name;
        Keywords = name.Split([' '], StringSplitOptions.RemoveEmptyEntries);
        Description = description;

        Incarnatable = true; // TODO: test purpose

        IsValid = true;
    }

    #region IEntity

    #region IActor

    public override bool ProcessInput(string input)
    {
        // Extract command and parameters
        bool extractedSuccessfully = CommandParser.ExtractCommandAndParameters(input, out var command, out var parameters);
        if (!extractedSuccessfully)
        {
            Logger.LogWarning("Command and parameters not extracted successfully");
            Send("Invalid command or parameters");
            return false;
        }

        Logger.LogDebug("[{name}] executing [{input}]", DebugName, input);
        return ExecuteCommand(input, command, parameters);
    }

    public override void Send(string message, bool addTrailingNewLine)
    {
        Logger.LogDebug("SEND[{name}]: {message}", DebugName, message);

        if (IncarnatedBy != null)
        {
            if (PrefixForwardedMessages)
                message = "<INC|" + DisplayName + ">" + message;
            IncarnatedBy.Send(message, addTrailingNewLine);
        }
    }

    public override void Page(StringBuilder text)
    {
        IncarnatedBy?.Page(text);
    }

    #endregion

    public Guid Id { get; private set; }
    public bool IsValid { get; private set; }
    public string Name { get; private set; } = null!;
    public abstract string DisplayName { get; }
    public IEnumerable<string> Keywords { get; private set; } = Array.Empty<string>();
    public string Description { get; protected set; } = null!;
    public abstract string DebugName { get; }

    public bool Incarnatable { get; protected set; } // TODO: assign
    public IAdmin? IncarnatedBy { get; protected set; }

    //
    public void SetName(string name)
    {
        Name = name;
        Keywords = name.Split([' '], StringSplitOptions.RemoveEmptyEntries);
    }

    public void SetDescription(string description)
    {
        Description = description;
    }

    // Auras
    public IEnumerable<IAura> Auras => _auras;

    // Incarnation

    public virtual bool ChangeIncarnation(IAdmin? admin) // if non-null, start incarnation, else, stop incarnation
    {
        if (!IsValid)
        {
            Logger.LogWarning("IEntity.ChangeIncarnation: {name} is not valid anymore", DisplayName);
            IncarnatedBy = null;
            return false;
        }
        if (admin != null)
        {
            if (!Incarnatable)
            {
                Logger.LogWarning("IEntity.ChangeIncarnation: {name} cannot be incarnated", DebugName);
                return false;
            }
            if (IncarnatedBy != null)
            {
                Logger.LogWarning("IEntity.ChangeIncarnation: {name} is already incarnated by {incarnatedByName}", DebugName, IncarnatedBy.DisplayName);
                return false;
            }
        }
        IncarnatedBy = admin;
        return true;
    }

    // Recompute
    public virtual void Reset() 
    {
        if (!IsValid)
            Logger.LogWarning("IEntity.Reset: {name} is not valid anymore", DebugName);
    }

    public abstract void Recompute();

    // Auras
    public IAura? GetAura(string abilityName)
    {
        if (!IsValid)
        {
            Logger.LogWarning("IEntity.GetAura: {name} is not valid anymore", DebugName);
            return null;
        }

        return _auras.FirstOrDefault(x => StringCompareHelpers.StringEquals(x.AbilityName, abilityName));
    }

    public void AddAura(IAura aura, bool recompute)
    {
        if (!IsValid)
        {
            Logger.LogWarning("IEntity.AddAura: {name} is not valid anymore", DebugName);
            return;
        }
        Logger.LogInformation("IEntity.AddAura: Add: {name} {abilityName}| recompute: {recompute}", DebugName, aura.AbilityName ?? "<<??>>", recompute);
        _auras.Add(aura);
        if (recompute)
            Recompute();
    }

    public void RemoveAura(IAura aura, bool recompute, bool displayWearOffMessage)
    {
        Logger.LogInformation("IEntity.RemoveAura: {name} {abilityName} | recompute: {recompute}", DebugName, aura.AbilityName ?? "<<??>>", recompute);
        bool removed = _auras.Remove(aura);
        if (!removed)
            Logger.LogWarning("ICharacter.RemoveAura: Trying to remove unknown aura");
        else
            OnAuraRemoved(aura, displayWearOffMessage);
        aura.OnRemoved();
        if (recompute && removed)
            Recompute();
    }

    public void RemoveAuras(Func<IAura, bool> filterFunc, bool recompute, bool displayWearOffMessage)
    {
        Logger.LogInformation("IEntity.RemoveAuras: {name} | recompute: {recompute}", DebugName, recompute);
        var auras = Auras.Where(filterFunc).ToArray();
        foreach(IAura aura in auras)
            RemoveAura(aura, false, displayWearOffMessage);
        if (recompute)
            Recompute();
    }

    public virtual void OnAuraRemoved(IAura aura, bool displayWearOffMessage)
    {
        // NOP
    }

    public virtual string RelativeDisplayName(ICharacter beholder, bool capitalizeFirstLetter = false)
    {
        return DisplayName; // no behavior by default
    }

    public virtual string RelativeDescription(ICharacter beholder)
    {
        return Description; // no behavior by default
    }

    public void Act(IEnumerable<ICharacter> characters, string format, params object[] arguments)
    {
        foreach (ICharacter target in characters)
        {
            string phrase = FormatActOneLine(target, format, arguments);
            target.Send(phrase);
        }
    }

    public virtual void OnCleaned() // called when removing definitively an entity from the game
    {
    }

    #endregion

    protected abstract void ResetAttributesAndResourcesAndFlags();

    protected void OnRemoved() // called before removing an item from the game
    {
        IsValid = false;
        // TODO: warn IncarnatedBy about removing
        IncarnatedBy?.StopIncarnating();
        IncarnatedBy = null;
    }

    protected AuraData[] MapAuraData()
    {
        // don't save Shapeshift
        return Auras.Where(x => x.IsValid && !x.AuraFlags.HasFlag(AuraFlags.Shapeshift)).Select(x => x.MapAuraData()).ToArray();
    }

    protected static TFlags NewAndCopyAndSet<TFlags>(Func<TFlags> newFunc, TFlags? toCopy, TFlags? toSet)
        where TFlags : IFlags<string>
    {
        var newValue = newFunc();
        if (toCopy != null)
            newValue.Copy(toCopy);
        if (toSet != null)
            newValue.Set(toSet);
        return newValue;
    }

    #region Act

    // Recreate behaviour of String.Format with maximum 10 arguments
    // If an argument is ICharacter, IItem, IExit special formatting is applied (depending on who'll receive the message)
    protected enum ActParsingStates
    {
        Normal,
        OpeningBracketFound,
        ArgumentFound,
        FormatSeparatorFound,
    }

    protected string FormatActOneLine(ICharacter target, string format, params object[] arguments)
    {
        StringBuilder result = new();

        ActParsingStates state = ActParsingStates.Normal;
        object currentArgument = null!;
        StringBuilder argumentFormat = null!;
        foreach (char c in format)
        {
            switch (state)
            {
                case ActParsingStates.Normal: // searching for {
                    if (c == '{')
                    {
                        state = ActParsingStates.OpeningBracketFound;
                        currentArgument = null!;
                        argumentFormat = new StringBuilder();
                    }
                    else
                        result.Append(c);
                    break;
                case ActParsingStates.OpeningBracketFound: // searching for a number
                    if (c == '{') // {{ -> {
                    {
                        result.Append('{');
                        state = ActParsingStates.Normal;
                    }
                    else if (c == '}') // {} -> nothing
                    {
                        state = ActParsingStates.Normal;
                    }
                    else if (c >= '0' && c <= '9') // {x -> argument found
                    {
                        var argumentIndex = c - '0';
                        if (argumentIndex >= arguments.Length)
                        {
                            Logger.LogError("FormatActOneLine for format {format} and target {target} tries to access argument {argumentIndex} but {argumentCount} were provided.", format, target.DebugName, argumentIndex, arguments.Length);
                            return "%R%!!Error!!%x%";
                        }
                        currentArgument = arguments[argumentIndex];
                        state = ActParsingStates.ArgumentFound;
                    }
                    break;
                case ActParsingStates.ArgumentFound: // searching for } or :
                    if (c == '}')
                    {
                        FormatActOneArgument(target, result, null, currentArgument);
                        state = ActParsingStates.Normal;
                    }
                    else if (c == ':')
                        state = ActParsingStates.FormatSeparatorFound;
                    break;
                case ActParsingStates.FormatSeparatorFound: // searching for }
                    if (c == '}')
                    {
                        Debug.Assert(argumentFormat != null);
                        FormatActOneArgument(target, result, argumentFormat.ToString(), currentArgument);
                        state = ActParsingStates.Normal;
                    }
                    else
                    {
                        // argumentFormat cannot be null
                        Debug.Assert(argumentFormat != null);
                        argumentFormat.Append(c);
                    }
                    break;
            }
        }
        if (result.Length > 0)
            result[0] = char.ToUpperInvariant(result[0]);
        return result.ToString();
    }

    // Formatting
    //  ICharacter
    //      default: same as n, N
    //      p, P: same as n but you is replaced with your
    //      n, N: argument.name if visible by target, someone otherwise
    //      e, E: you/he/she/it, depending on argument.sex
    //      m, M: you/him/her/it, depending on argument.sex
    //      s, S: your/his/her/its, depending on argument.sex
    //      f, F: yourself/himself/herself/itself
    //      b, B: are/is
    //      h, H: have/has
    //      v, V: add 's' at the end of a verb if argument is different than target (take care of verb ending with y/o/h
    // IItem
    //      argument.Name if visible by target, something otherwise
    // IExit
    //      exit name
    // IAbility
    //      ability name
    protected void FormatActOneArgument(ICharacter target, StringBuilder result, string? format, object argument)
    {
        // Character ?
        if (argument is ICharacter character)
        {
            char letter = format?[0] ?? 'n'; // if no format, n
            switch (letter)
            {
                // your/name
                case 'p':
                    if (target == character)
                        result.Append("your");
                    else
                    {
                        result.Append(character.RelativeDisplayName(target));
                        if (result[^1] == 's')
                            result.Append('\'');
                        else
                            result.Append("'s");
                    }
                    break;
                case 'P':
                    if (target == character)
                        result.Append("Your");
                    else
                    {
                        result.Append(character.RelativeDisplayName(target));
                        if (result[^1] == 's')
                            result.Append('\'');
                        else
                            result.Append("'s");
                    }
                    break;
                // you/name
                case 'n':
                    if (target == character)
                        result.Append("you");
                    else
                        result.Append(character.RelativeDisplayName(target));
                    break;
                case 'N':
                    if (target == character)
                        result.Append("You");
                    else
                        result.Append(character.RelativeDisplayName(target));
                    break;
                // you/he/she/it
                case 'e':
                    if (target == character)
                        result.Append("you");
                    else
                        result.Append(StringHelpers.Subjects[character.Sex]);
                    break;
                case 'E':
                    if (target == character)
                        result.Append("You");
                    else
                        result.Append(StringHelpers.Subjects[character.Sex].UpperFirstLetter());
                    break;
                // you/him/her/it
                case 'm':
                    if (target == character)
                        result.Append("you");
                    else
                        result.Append(StringHelpers.Objectives[character.Sex]);
                    break;
                case 'M':
                    if (target == character)
                        result.Append("You");
                    else
                        result.Append(StringHelpers.Objectives[character.Sex].UpperFirstLetter());
                    break;
                // your/his/her/its
                case 's':
                    if (target == character)
                        result.Append("your");
                    else
                        result.Append(StringHelpers.Possessives[character.Sex]);
                    break;
                case 'S':
                    if (target == character)
                        result.Append("Your");
                    else
                        result.Append(StringHelpers.Possessives[character.Sex].UpperFirstLetter());
                    break;
                // yourself/himself/herself/itself (almost same as 'm' + self)
                case 'f':
                    if (target == character)
                        result.Append("your");
                    else
                        result.Append(StringHelpers.Objectives[character.Sex]);
                    result.Append("self");
                    break;
                case 'F':
                    if (target == character)
                        result.Append("your");
                    else
                        result.Append(StringHelpers.Objectives[character.Sex].UpperFirstLetter());
                    result.Append("self");
                    break;
                // is/are
                case 'b':
                    if (target == character)
                        result.Append("are");
                    else
                        result.Append("is");
                    break;
                case 'B':
                    if (target == character)
                        result.Append("Are");
                    else
                        result.Append("Is");
                    break;
                // has/have
                case 'h':
                    if (target == character)
                        result.Append("have");
                    else
                        result.Append("has");
                    break;
                case 'H':
                    if (target == character)
                        result.Append("Have");
                    else
                        result.Append("Has");
                    break;
                // verb
                case 'v':
                case 'V':
                    // nothing to do if target is actor
                    if (target != character)
                    {
                        //http://www.grammar.cl/Present/Verbs_Third_Person.htm
                        if (result.Length > 0)
                        {
                            char verbLastLetter = result[^1];
                            char verbBeforeLastLetter = result.Length > 1 ? result[^2] : ' ';
                            if (verbLastLetter == 'y' && verbBeforeLastLetter != ' ' && verbBeforeLastLetter != 'a' && verbBeforeLastLetter != 'e' && verbBeforeLastLetter != 'i' && verbBeforeLastLetter != 'o' && verbBeforeLastLetter != 'u')
                            {
                                result.Remove(result.Length - 1, 1);
                                result.Append("ies");
                            }
                            else if (verbLastLetter == 'o' || verbLastLetter == 'h' || verbLastLetter == 's' || verbLastLetter == 'x')
                                result.Append("es");
                            else
                                result.Append('s');
                        }
                        else
                            Logger.LogError("Act: v-format on an empty string");
                    }
                    break;
                default:
                    Logger.LogError("Act: invalid format {format} for ICharacter", format ?? "???");
                    result.Append("<???>");
                    break;
            }
            return;
        }
        // Item ?
        if (argument is IItem item)
        {
            // no specific format
            result.Append(item.RelativeDisplayName(target));
            return;
        }
        // Exit ?
        if (argument is IExit exit)
        {
            // TODO: can see destination room ?
            // no specific format
            result.Append(exit.Keywords.FirstOrDefault() ?? "door");
            return;
        }
        if (argument is IAbilityLearned abilityLearned)
        {
            result.Append(abilityLearned.Name);
            return;
        }
        // Other (int, string, ...)
        if (format == null)
            result.Append(argument);
        else
        {
            if (argument is IFormattable formattable)
                result.Append(formattable.ToString(format, null));
            else
                result.Append(argument);
        }
    }

    #endregion
}
