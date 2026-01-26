using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Character;
using Mud.Common;
using Mud.DataStructures;
using Mud.Domain;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Affects.Character;
using Mud.Server.Common;
using Mud.Server.Common.Extensions;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.Entity;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Combat;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Flags;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Loot;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Table;
using Mud.Server.Options;
using System.Text;

namespace Mud.Server.Character;

public abstract class CharacterBase : EntityBase, ICharacter
{
    private const int MaxRecompute = 5;
    private const int MinAlignment = -1000;
    private const int MaxAlignment = 1000;

    private readonly List<IItem> _inventory;
    private readonly List<IEquippedItem> _equipments;
    private readonly ArrayByEnum<int, CharacterAttributes> _baseAttributes;
    private readonly ArrayByEnum<int, CharacterAttributes> _currentAttributes;
    private readonly ArrayByEnum<int, ResourceKinds> _baseMaxResources;
    private readonly ArrayByEnum<int, ResourceKinds> _currentMaxResources;
    private readonly ArrayByEnum<decimal, ResourceKinds> _currentResources;
    private readonly Dictionary<string, int> _cooldownsPulseLeft;
    private readonly Dictionary<string, IAbilityLearned> _learnedAbilities;

    protected IAbilityManager AbilityManager { get; }
    protected IRandomManager RandomManager { get; }
    protected ITableValues TableValues { get; }
    protected IRoomManager RoomManager { get; }
    protected IItemManager ItemManager { get; }
    protected ICharacterManager CharacterManager { get; }
    protected IAuraManager AuraManager { get; }
    protected IResistanceCalculator ResistanceCalculator { get; }
    protected IRageGenerator RageGenerator { get; }
    protected IWeaponEffectManager WeaponEffectManager { get; }
    protected IAffectManager AffectManager { get; }
    protected IFlagsManager FlagsManager { get; }
    protected IWiznet Wiznet { get; }
    protected ILootManager LootManager { get; }
    protected IAggroManager AggroManager { get; }

    protected CharacterBase(ILogger<CharacterBase> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions, IAbilityManager abilityManager, IRandomManager randomManager, ITableValues tableValues, IRoomManager roomManager, IItemManager itemManager, ICharacterManager characterManager, IAuraManager auraManager, IResistanceCalculator resistanceCalculator, IRageGenerator rageGenerator, IWeaponEffectManager weaponEffectManager, IAffectManager affectManager, IFlagsManager flagsManager, IWiznet wiznet, ILootManager lootManager, IAggroManager aggroManager)
        : base(logger, gameActionManager, commandParser, messageForwardOptions)
    {
        AbilityManager = abilityManager;
        RandomManager = randomManager;
        TableValues = tableValues;
        RoomManager = roomManager;
        ItemManager = itemManager;
        CharacterManager = characterManager;
        AuraManager = auraManager;
        ResistanceCalculator = resistanceCalculator;
        RageGenerator = rageGenerator;
        WeaponEffectManager = weaponEffectManager;
        AffectManager = affectManager;
        FlagsManager = flagsManager;
        Wiznet = wiznet;
        LootManager = lootManager;
        AggroManager = aggroManager;

        _inventory = [];
        _equipments = [];
        _baseAttributes = new ();
        _currentAttributes = new ();
        _baseMaxResources = new ();
        _currentMaxResources = new ();
        _currentResources = new ();
        _cooldownsPulseLeft = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        _learnedAbilities = new Dictionary<string, IAbilityLearned>(StringComparer.InvariantCultureIgnoreCase); // handled by RecomputeKnownAbilities

        Position = Positions.Standing;
        Shape = Shapes.Normal;

        Class = null!;
        Race = null!;
        Room = null!;
        CurrentResourceKinds = null!;

        BaseCharacterFlags = new CharacterFlags();
        CharacterFlags = new CharacterFlags();
        BaseBodyParts = new BodyParts();
        BodyParts = new BodyParts();
        BaseBodyForms = new BodyForms();
        BodyForms = new BodyForms();
        BaseImmunities = new IRVFlags();
        Immunities = new IRVFlags();
        BaseResistances = new IRVFlags();
        Resistances = new IRVFlags();
        BaseVulnerabilities = new IRVFlags();
        Vulnerabilities = new IRVFlags();
        BaseShieldFlags = new ShieldFlags();
        ShieldFlags = new ShieldFlags();
    }

    #region ICharacter

    #region IEntity

    // TODO: override RelativeDescription ?

    public override void OnAuraRemoved(IAura aura, bool displayWearOffMessage)
    {
        base.OnAuraRemoved(aura, displayWearOffMessage);
        if (displayWearOffMessage)
        {
            if (aura.AbilityDefinition != null && aura.AbilityDefinition.HasCharacterWearOffMessage)
                Send(aura.AbilityDefinition.CharacterWearOffMessage!);
        }
    }

    public override bool ChangeIncarnation(IAdmin? admin)
    {
        bool result = base.ChangeIncarnation(admin);
        if (result)
        {
            RecomputeKnownAbilities();
            Recompute();
        }
        return result;
    }

    #endregion

    #region IContainer

    public IEnumerable<IItem> Content => _inventory.Where(x => x.IsValid);

    public bool PutInContainer(IItem obj)
    {
        //if (obj.ContainedInto != null)
        //{
        //    Logger.LogError("PutInContainer: {0} is already in container {1}.", obj.DebugName, obj.ContainedInto.DebugName);
        //    return false;
        //}
        _inventory.Insert(0, obj);
        return true;
    }

    public bool GetFromContainer(IItem obj)
    {
        bool removed = _inventory.Remove(obj);
        return removed;
    }

    #endregion

    public IRoom Room { get; protected set; }

    public ICharacter? Fighting { get; protected set; }

    public abstract ImmortalModeFlags ImmortalMode { get; }

    public IEnumerable<IEquippedItem> Equipments => _equipments;
    public IEnumerable<IItem> Inventory => Content;
    public virtual int MaxCarryWeight => TableValues.CarryBonus(this) * 10 + Level * 25;
    public virtual int MaxCarryNumber => Equipments.Count() + 2 * this[BasicAttributes.Dexterity] + Level;
    public int CarryWeight => Inventory.Sum(x => x.Weight) + Equipments.Where(x => x.Item != null).Sum(x => x.Item!.Weight);
    public int CarryNumber => Inventory.Sum(x => x.CarryCount) + Equipments.Where(x => x.Item != null).Sum(x => x.Item!.CarryCount);

    // GCD (aka WAIT_STATE in rom24)
    public int GlobalCooldown { get; private set; } // delay (in Pulse) before next action    check WAIT_STATE

    public void DecreaseGlobalCooldown() // decrease one by one
    {
        GlobalCooldown = Math.Max(GlobalCooldown - 1, 0);
    }

    public void SetGlobalCooldown(int pulseCount) // set global cooldown delay (in pulse), can only increase
    {
        if (pulseCount > GlobalCooldown)
        {
            Logger.LogTrace("SETTING GCD to {pulseCount} for {name}", pulseCount, DebugName);
            GlobalCooldown = pulseCount;
        }
    }

    // Daze
    public int Daze { get; private set; }

    public void DecreaseDaze() // decrease one by one
    {
        Daze = Math.Max(Daze - 1, 0);
    }

    public void SetDaze(int pulseCount) // set daze delay (in pulse), can only increase
    {
        if (pulseCount > Daze)
        {
            Logger.LogTrace("SETTING DAZE to {pulseCount} for {name}", pulseCount, DebugName);
            Daze = pulseCount;
        }
    }

    // Stun
    public int Stun { get; protected set; }
    
    public bool IsStunned => Stun > 0;

    public void DecreaseStun() // decrease one by one
    {
        Stun = Math.Max(Stun - 1, 0);
    }

    public void SetStun(int pulseCount) // set daze delay (in pulse), can only increase
    {
        if (pulseCount > Stun)
        {
            Logger.LogTrace("SETTING STUN to {pulseCount} for {name}", pulseCount, DebugName);
            Stun = pulseCount;
        }
    }

    // Money
    public long SilverCoins { get; protected set; }
    public long GoldCoins { get; protected set; }

    public virtual (long silverSpent, long goldSpent) DeductCost(long cost)
    {
        long silver = Math.Min(SilverCoins, cost);
        long gold = 0;

        if (silver < cost)
        {
            gold = ((cost - silver + 99) / 100);
            silver = cost - 100 * gold;
        }

        SilverCoins -= silver;
        GoldCoins -= gold;

        if (GoldCoins < 0)
        {
            Logger.LogError("DeductCost: gold {gold} < 0", GoldCoins);
            GoldCoins = 0;
        }
        if (SilverCoins < 0)
        {
            Logger.LogError("DeductCost: silver {silver} < 0", SilverCoins);
            SilverCoins = 0;
        }

        return (silver, gold);
    }

    public virtual void UpdateMoney(long silverCoins, long goldCoins)
    {
        SilverCoins = Math.Max(0, SilverCoins + silverCoins);
        GoldCoins = Math.Max(0, GoldCoins + goldCoins);
    }

    public virtual (long stolenSilver, long stolenGold) StealMoney(long silverCoins, long goldCoins)
    {
        if (silverCoins > SilverCoins)
            silverCoins = SilverCoins;
        if (goldCoins > GoldCoins)
            goldCoins = GoldCoins;
        SilverCoins -= silverCoins;
        GoldCoins -= goldCoins;
        return (silverCoins, goldCoins);
    }

    // Furniture (sleep/sit/stand)
    public IItemFurniture? Furniture { get; protected set; }

    // Position
    public Positions Position { get; protected set; }

    // Class/Race
    public IClass Class { get; protected set; }
    public IRace Race { get; protected set; }

    // Attributes
    public int Level { get; protected set; }

    public ICharacterFlags BaseCharacterFlags { get; protected set; }
    public ICharacterFlags CharacterFlags { get; protected set; }

    public IIRVFlags BaseImmunities { get; protected set; }
    public IIRVFlags Immunities { get; protected set; }
    public IIRVFlags BaseResistances { get; protected set; }
    public IIRVFlags Resistances { get; protected set; }
    public IIRVFlags BaseVulnerabilities { get; protected set; }
    public IIRVFlags Vulnerabilities { get; protected set; }

    public IShieldFlags BaseShieldFlags { get; protected set; }
    public IShieldFlags ShieldFlags { get; protected set; }

    public Sex BaseSex { get; protected set; }
    public Sex Sex { get; protected set; }

    public Sizes BaseSize { get; protected set; }
    public Sizes Size { get; protected set; }

    public int Alignment { get; protected set; }
    public bool IsEvil => Alignment <= -350;
    public bool IsGood => Alignment >= 350;
    public bool IsNeutral => !IsEvil && !IsGood;

    public int this[CharacterAttributes attribute]
    {
        get => _currentAttributes[attribute];
        protected set => _currentAttributes[attribute] = value;
    }

    public int this[BasicAttributes attribute] => this[(CharacterAttributes)attribute];
    public int this[Armors armor] => this[(CharacterAttributes)armor] + (Position > Positions.Sleeping ? TableValues.DefensiveBonus(this) : 0);
    public int HitRoll => this[CharacterAttributes.HitRoll] + TableValues.HitBonus(this);
    public int DamRoll => this[CharacterAttributes.DamRoll] + TableValues.DamBonus(this);

    public int this[ResourceKinds resource]
    {
        get => decimal.ToInt32(_currentResources[resource]);
    }

    public IEnumerable<ResourceKinds> CurrentResourceKinds { get; private set; }

    public IBodyForms BaseBodyForms { get; protected set; }
    public IBodyForms BodyForms { get; protected set; }
    public IBodyParts BaseBodyParts { get; protected set; }
    public IBodyParts BodyParts { get; protected set; }

    // Shape
    public Shapes BaseShape { get; protected set; }
    public Shapes Shape { get; protected set; }


    // Abilities
    public virtual IEnumerable<IAbilityLearned> LearnedAbilities => _learnedAbilities.Values;

    // Followers
    public ICharacter? Leader { get; protected set; }

    public void AddFollower(ICharacter character)
    {
        if (character.Leader == this)
            return;
        // check if A->B->C->A
        var next = Leader;
        while (next != null)
        {
            if (next == character)
                return; // found a cycle
            next = next.Leader;
        }

        character.Leader?.RemoveFollower(character);
        character.ChangeLeader(this);
        Act(ActOptions.ToCharacter, "{0:N} starts following you.", character);
        character.Act(ActOptions.ToCharacter, "You start following {0:N}.", this);
    }

    public void RemoveFollower(ICharacter character)
    {
        if (character.Leader != this)
            return;
        Act(ActOptions.ToCharacter, "{0:N} stops following you.", character);
        character.Act(ActOptions.ToCharacter, "You stop following {0:N}.", this);
        character.ChangeLeader(null);
        if (character is INonPlayableCharacter npc)
        {
            npc.RemoveBaseCharacterFlags(true, "Charm");
            npc.RemoveAuras(x => StringCompareHelpers.StringEquals(x.AbilityName, "Charm Person"), true, false); // TODO: do this differently
            npc.ChangeMaster(null);
        }
    }

    public void ChangeLeader(ICharacter? character)
    {
        Leader = character;
    }

    // Group
    public virtual bool IsSameGroupOrPet(ICharacter character)
    {
        var pcCh1 = this as IPlayableCharacter;
        var pcCh2 = character as IPlayableCharacter;
        var npcCh1 = this as INonPlayableCharacter;
        var npcCh2 = character as INonPlayableCharacter;
        return (pcCh1 != null && pcCh1.IsSameGroupOrPet(character)) || (pcCh2 != null && pcCh2.IsSameGroupOrPet(this)) || (npcCh1 != null && npcCh2 != null && npcCh1.Master != null && npcCh2.Master != null && npcCh1.Master == npcCh2.Master);
    }

    public abstract IEnumerable<IPlayableCharacter> GetPlayableCharactersImpactedByKill();

    // Act
    // IFormattable cannot be used because formatting depends on who'll receive the message (CanSee check)
    public void Act(ActOptions option, string format, params object[] arguments)
        => Act(option, Positions.Resting, format, arguments);

    public void Act(ActOptions option, Positions minPosition, string format, params object[] arguments)
    {
        //
        IEnumerable<ICharacter> targets = GetActTargets(option, minPosition);
        //
        foreach (var target in targets)
        {
            var phrase = FormatActOneLine(target, format, arguments);
            target.Send(phrase);
        }
    }

    public void ActToNotVictim(ICharacter victim, string format, params object[] arguments) // to everyone except this and victim
        => ActToNotVictim(victim, Positions.Resting, format, arguments);

    public void ActToNotVictim(ICharacter victim, Positions minPosition, string format, params object[] arguments) // to everyone except this and victim
    {
        foreach (var to in Room.People.Where(x => x != this && x != victim && x.Position >= minPosition))
        {
            var phrase = FormatActOneLine(to, format, arguments);
            to.Send(phrase);
        }
    }

    public string ActPhrase(string format, params object[] arguments)
    {
        return FormatActOneLine(this, format, arguments);
    }

    // Equipments
    public bool Unequip(IItem item)
    {
        if (item is IItemLight itemLight && itemLight.IsLighten)
            Room.DecreaseLight();
        foreach (IEquippedItem equipmentSlot in _equipments.Where(x => x.Item == item))
            equipmentSlot.Item = null;
        return true;
    }

    public bool Equip(IItem item)
    {
        if (item is IItemLight itemLight && itemLight.IsLighten)
            Room.IncreaseLight();
        return true;
    }

    // Furniture
    public bool ChangeFurniture(IItemFurniture? furniture)
    {
        Furniture = furniture;
        return true;
    }

    // Position
    public bool StandUpInCombatIfPossible()
    {
        //if (ch->timer > 4)
        //    return FALSE;
        if (Daze > 0 || GlobalCooldown > 0)
            return false;
        if (Fighting == null)
            return false;
        if (Position != Positions.Standing)
        {
            Logger.LogInformation("*** changing position to standing for {name} from {position}", DebugName, Position);

            DisplayChangePositionMessage(Position, Positions.Standing, Furniture);
            ChangePosition(Positions.Standing);
        }
        return true;
    }

    public bool ChangePosition(Positions position)
    {
        Position = position;
        return true;
    }

    public void DisplayChangePositionMessage(Positions oldPosition, Positions newPosition, IItemFurniture? furniture)
    {
        string phrase = string.Empty;

        switch (newPosition)
        {
            case Positions.Sleeping:
                if (furniture == null) phrase = "{0:N} go{0:v} to sleep.";
                else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.At) phrase = "{0:N} go{0:v} sleep at {1}.";
                else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.On) phrase = "{0:N} go{0:v} sleep on {1}.";
                else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.In) phrase = "{0:N} go{0:v} sleep in {1}.";
                break;
            case Positions.Resting:
                switch (oldPosition)
                {
                    case Positions.Sleeping:
                        if (furniture == null) phrase = "{0:N} wake{0:v} and start{0:v} resting.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.At) phrase = "{0:N} wake{0:v} and rest{0:v} at {1}.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.On) phrase = "{0:N} wake{0:v} and rest{0:v} on {1}.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.In) phrase = "{0:N} wake{0:v} and rest{0:v} in {1}.";
                        break;
                    case Positions.Sitting:
                        if (furniture == null) phrase = "{0;N} rest{0:v}.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.At) phrase = "{0:N} rest{0:v} at {1}.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.On) phrase = "{0:N} rest{0:v} on {1}.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.In) phrase = "{0:N} rest{0:v} in {1}.";
                        break;
                    case Positions.Standing:
                        if (furniture == null) phrase = "{0:N} sit{0:v} down and rest{0:v}.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.At) phrase = "{0:N} sit{0:v} down at {1} and rest{0:v}.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.On) phrase = "{0:N} sit{0:v} on {1} and rest{0:v}.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.In) phrase = "{0:N} rest{0:v} in {1}.";
                        break;
                }
                break;
            case Positions.Sitting:
                switch (oldPosition)
                {
                    case Positions.Sleeping: // we have to use Send because this is sleeping and ActToNotVictim
                        if (furniture == null) phrase = "{0:N} wake{0:v} and sit{0:v} up.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.At) phrase = "{0:N} wake{0:v} and sit{0:v} at {1}.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.On) phrase = "{0:N} wake{0:v} and sit{0:v} on {1}.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.In) phrase = "{0:N} wake{0:v} and sit{0:v} in {1}.";
                        break;
                    case Positions.Resting:
                        if (furniture == null) phrase = "You stop resting.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.At) phrase = "{0:N} sit{0:v} at {1}.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.On) phrase = "{0:N} sit{0:v} on {1}.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.In) phrase = "{0:N} sit{0:v} in {1}.";
                        break;
                    case Positions.Standing:
                        if (furniture == null) phrase = "{0:N} sit{0:v} down.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.At) phrase = "{0:N} sit{0:v} down at {1}.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.On) phrase = "{0:N} sit{0:v} on {1}.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.In) phrase = "{0:N} sit{0:v} down in {1}.";
                        break;
                }
                break;
            case Positions.Standing:
                switch (oldPosition)
                {
                    case Positions.Sleeping:
                        if (furniture == null) phrase = "{0:N} wake{0:v} up and stand{0:v} up.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.At) phrase = "{0:N} wake{0:v} up and stand{0:v} at {1}.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.On) phrase = "{0:N} wake{0:v} up and stand{0:v} on {1}.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.In) phrase = "{0:N} wake{0:v} up and stand{0:v} in {1}.";
                        break;
                    case Positions.Resting:
                    case Positions.Sitting:
                        if (furniture == null) phrase = "{0:N} stand{0:v} up.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.At) phrase = "{0:N} stand{0:v} at {1}.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.On) phrase = "{0:N} stand{0:v} on {1}.";
                        else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.In) phrase = "{0:N} stand{0:v} in {1}.";
                        break;
                }
                break;
        }
        if (oldPosition == Positions.Sleeping)
        {
            // we cannot use Act(ToAll) because 'this' is sleeping and will not receive message (min position is resting by default)
            // we cannot use Act(ToAll, Positions.Sleeping) because other people could be sleeping and should not receive the message
            if (furniture == null)
            {
                Act(ActOptions.ToCharacter, Positions.Sleeping, phrase, this);
                ActToNotVictim(this, phrase, this);
            }
            else
            {
                Act(ActOptions.ToCharacter, Positions.Sleeping, phrase, this, furniture);
                ActToNotVictim(this, phrase, this, furniture);
            }
        }
        else
        {
            if (furniture == null)
                Act(ActOptions.ToAll, phrase, this);
            else
                Act(ActOptions.ToAll, phrase, this, furniture);
        }
    }

    // Visibility
    public virtual bool CanSee(ICharacter? victim)
    {
        if (victim == null)
            return false;
        if (victim == this)
            return true;
        // blind
        if (CharacterFlags.IsSet("Blind"))
            return false;
        // infrared + dark
        if (!CharacterFlags.IsSet("Infrared") && victim.Room?.IsDark == true)
            return false;
        // invis
        if (victim.CharacterFlags.IsSet("Invisible")
            && !CharacterFlags.IsSet("DetectInvis"))
            return false;
        // sneaking
        if (victim.CharacterFlags.IsSet("Sneak")
            && !CharacterFlags.IsSet("DetectHidden")
            && victim.Fighting == null)
        {
            var (percentage, _) = victim.GetAbilityLearnedAndPercentage("Sneak");
            int chance = percentage;
            chance += (3 * victim[BasicAttributes.Dexterity]) / 2;
            chance -= this[BasicAttributes.Intelligence] * 2;
            chance -= Level - (3* victim.Level)/ 2;

            if (!RandomManager.Chance(chance))
                return false;
        }
        // hide
        if (victim.CharacterFlags.IsSet("Hide")
            && !CharacterFlags.IsSet("DetectHidden")
            && victim.Fighting == null)
            return false;
        //
        return true;
    }

    public virtual bool CanSee(IItem? item)
    {
        if (item == null)
            return false;

        // visible death
        if (item.ItemFlags.IsSet("VisibleDeath"))
            return false;

        // blind except if potion
        if (CharacterFlags.IsSet("Blind") && item is not IItemPotion)
            return false;

        // Light
        if (item is IItemLight light && light.IsLighten)
            return true;

        // invis
        if (item.ItemFlags.IsSet("Invis")
            && !CharacterFlags.IsSet("DetectInvis"))
            return false;

        // glow
        if (item.ItemFlags.IsSet("Glowing"))
            return true;

        // room dark
        if (Room.IsDark && !CharacterFlags.IsSet("Infrared"))
            return false;

        return true;
    }

    public virtual bool CanSee(IExit? exit)
    {
        if (exit == null)
            return false;
        if (exit.ExitFlags.HasFlag(ExitFlags.Hidden))
        {
            if (CharacterFlags.IsSet("DetectHidden"))
                return true;
            return false;
        }
        return true;
    }

    public virtual bool CanSee(IRoom? room)
    {
        if (room == null)
            return false;
        // not needed
        //// infrared + dark
        //if (room.IsDark && !CharacterFlags.IsSet("Infrared"))
        //    return false;

        if (room.RoomFlags.IsSet("ImpOnly") || room.RoomFlags.IsSet("GodsOnly") || room.RoomFlags.IsSet("HeroesOnly"))
            return false;

        if (room.RoomFlags.IsSet("NewbiesOnly") && Level > 5)
            return false;

        // TODO: clan
        //        if (!IS_IMMORTAL(ch) && pRoomIndex->clan && ch->clan != pRoomIndex->clan)
        //            return FALSE;
        //        return TRUE;

        return true;
    }

    public virtual bool CanLoot(IItem? target)
        => CanSee(target);

    // Attributes
    public int BaseAttribute(CharacterAttributes attribute)
        => _baseAttributes[attribute];

    public void UpdateBaseAttribute(CharacterAttributes attribute, int amount)
    {
        _baseAttributes[attribute] += amount;
        _currentAttributes[attribute] = Math.Min(_currentAttributes[attribute], _baseAttributes[attribute]);
    }

    // Resources
    public int MaxResource(ResourceKinds resourceKind)
        => decimal.ToInt32(_currentMaxResources[resourceKind]);

    public int BaseMaxResource(ResourceKinds resourceKind)
        => decimal.ToInt32(_baseMaxResources[resourceKind]);

    public void SetBaseMaxResource(ResourceKinds resourceKind, int value)
    {
        _baseMaxResources[resourceKind] = value;
    }

    public void UpdateBaseMaxResource(ResourceKinds resourceKind, int amount)
    {
        _baseMaxResources[resourceKind] += amount;
    }

    public void SetResource(ResourceKinds resourceKind, int value)
    {
        _currentResources[resourceKind] = Math.Clamp(value, 0, _currentMaxResources[resourceKind]);
    }

    public void UpdateResource(ResourceKinds resourceKind, decimal amount)
    {
        _currentResources[resourceKind] = Math.Clamp(_currentResources[resourceKind] + amount, 0, _currentMaxResources[resourceKind]);
    }

    public void Regen(int pulseCount)
    {
        // hp/move/mana/psy
        if (this[ResourceKinds.HitPoints] != MaxResource(ResourceKinds.HitPoints) || this[ResourceKinds.MovePoints] != MaxResource(ResourceKinds.MovePoints) || this[ResourceKinds.Mana] != MaxResource(ResourceKinds.Mana) || this[ResourceKinds.Psy] != MaxResource(ResourceKinds.Psy))
        {
            var (hitGain, moveGain, manaGain, psyGain) = CalculateResourcesDeltaByMinute();

            hitGain = hitGain * Room.HealRate / 100;
            manaGain = manaGain * Room.ResourceRate / 100;
            psyGain = psyGain * Room.ResourceRate / 100;

            if (Furniture != null && Furniture.HealBonus != 0)
            {
                hitGain = (hitGain * Furniture.HealBonus) / 100;
                moveGain = (moveGain * Furniture.HealBonus) / 100;
            }

            if (Furniture != null && Furniture.ResourceBonus != 0)
            {
                manaGain = (manaGain * Furniture.ResourceBonus) / 100;
                psyGain = (psyGain * Furniture.ResourceBonus) / 100;
            }

            if (CharacterFlags.IsSet("Poison"))
            {
                hitGain /= 4;
                moveGain /= 4;
                manaGain /= 4;
                psyGain /= 4;
            }
            if (CharacterFlags.IsSet("Plague"))
            {
                hitGain /= 8;
                moveGain /= 8;
                manaGain /= 8;
                psyGain /= 8;
            }
            if (CharacterFlags.IsSet("Haste") || CharacterFlags.IsSet("Slow"))
            {
                hitGain /= 2;
                moveGain /= 2;
                manaGain /= 2;
                psyGain /= 2;
            }
            //
            var byMinuteDivisor = Pulse.ToMinutes(pulseCount);
            hitGain /= byMinuteDivisor;
            moveGain /= byMinuteDivisor;
            manaGain /= byMinuteDivisor;
            psyGain /= byMinuteDivisor;

            UpdateResource(ResourceKinds.HitPoints, hitGain);
            UpdateResource(ResourceKinds.MovePoints, moveGain);
            UpdateResource(ResourceKinds.Mana, manaGain);
            UpdateResource(ResourceKinds.Psy, psyGain);
        }

        // energy/rage
        if (this[ResourceKinds.Energy] != MaxResource(ResourceKinds.Energy) // energy increase
            || this[ResourceKinds.Rage] != 0) // rage decrease
        {
            var (energyGain, rageGain) = CalculateResourcesDeltaBySecond();
            var bySecondDivisor = Pulse.ToSeconds(pulseCount);
            energyGain /= bySecondDivisor;
            rageGain /= bySecondDivisor;

            UpdateResource(ResourceKinds.Energy, energyGain);
            UpdateResource(ResourceKinds.Rage, rageGain);
        }
    }

    public void Heal(ICharacter source, int amount)
    {
        UpdateResource(ResourceKinds.HitPoints, amount);
        AggroManager.OnHeal(source, this, amount);
    }

    // Alignment
    public void UpdateAlignment(int amount)
    {
        Alignment = Math.Clamp(Alignment + amount, MinAlignment, MaxAlignment);
        // impact on equipment
        var recomputeNeeded = false;
        foreach (var item in Equipments.Where(x => x.Item != null).Select(x => x.Item))
        {
            recomputeNeeded |= ZapWornItemIfNeeded(item!);
        }

        if (recomputeNeeded)
        {
            Recompute();
            Room.Recompute();
        }
    }

    public bool ZapWornItemIfNeeded(IItem item)
    {
        if ((IsEvil && item.ItemFlags.IsSet("AntiEvil"))
            || (IsGood && item.ItemFlags.IsSet("AntiGood"))
            || (IsNeutral && item.ItemFlags.IsSet("AntiNeutral")))
        {
            Act(ActOptions.ToAll, "%R%{0:N} {0:b} %Y%zapped %R%by {1}.%x%", this, item);
            item.ChangeEquippedBy(null, false);
            item.ChangeContainer(Room);
            return true;
        }
        return false;
    }

    // Character flags
    public void AddBaseCharacterFlags(bool recompute, params string[] characterFlags)
    {
        BaseCharacterFlags.Set(characterFlags);
        if (recompute)
            Recompute();
    }

    public void RemoveBaseCharacterFlags(bool recompute, params string[] characterFlags)
    {
        BaseCharacterFlags.Unset(characterFlags);
        if (recompute)
            Recompute();
    }

    // Shape
    public bool ChangeShape(Shapes shape)
    {
        if (shape == Shape)
            return false;

        if (shape == Shapes.Normal)
            Send("%W%You regain your normal form%x%");

        // remove any existing Shape aura
        if (Shape != Shapes.Normal)
            RemoveAuras(x => x.AuraFlags.HasFlag(AuraFlags.Shapeshift), false, true);

        Shape = shape;

        RecomputeKnownAbilities();
        RecomputeCurrentResourceKinds();
        Recompute();

        return true;
    }

    // Recompute
    public override void Recompute()
    {
        Logger.LogDebug("CharacterBase.Recompute: {name}", DebugName);

        var recomputeCount = 0;
        var additionalRecomputeNeeded = false;

        while (true)
        {
            // Reset current attributes
            ResetAttributesAndResourcesAndFlags();

            // 1) Apply room auras
            if (Room != null)
                ApplyAuras(Room);

            // 2) Apply equipment auras
            foreach (var equipment in Equipments.Where(x => x.Item != null))
                ApplyAuras(equipment.Item!);

            // 3) Apply equipment armor
            foreach (var equippedItem in Equipments.Where(x => x.Item is IItemArmor || x.Item is IItemShield))
            {
                if (equippedItem.Item is IItemArmor armor)
                {
                    int equipmentSlotMultiplier = TableValues.EquipmentSlotMultiplier(equippedItem.Slot);
                    this[CharacterAttributes.ArmorBash] -= armor.Bash * equipmentSlotMultiplier;
                    this[CharacterAttributes.ArmorPierce] -= armor.Pierce * equipmentSlotMultiplier;
                    this[CharacterAttributes.ArmorSlash] -= armor.Slash * equipmentSlotMultiplier;
                    this[CharacterAttributes.ArmorExotic] -= armor.Exotic * equipmentSlotMultiplier;
                }
                if (equippedItem.Item is IItemShield shield)
                {
                    int equipmentSlotMultiplier = TableValues.EquipmentSlotMultiplier(equippedItem.Slot);
                    this[CharacterAttributes.ArmorBash] -= shield.Armor * equipmentSlotMultiplier;
                    this[CharacterAttributes.ArmorPierce] -= shield.Armor * equipmentSlotMultiplier;
                    this[CharacterAttributes.ArmorSlash] -= shield.Armor * equipmentSlotMultiplier;
                    this[CharacterAttributes.ArmorExotic] -= shield.Armor * equipmentSlotMultiplier;
                }
            }

            // 4) Apply own auras
            ApplyAuras(this);

            // 5) Check some equipped items need to be unequipped which could imply an additional recompute
            additionalRecomputeNeeded |= CheckEquippedItemsDuringRecompute();

            // 6) Keep resources in valid range
            foreach (var resourceKind in Enum.GetValues<ResourceKinds>())
                _currentResources[resourceKind] = Math.Min(_currentResources[resourceKind], _currentMaxResources[resourceKind]);
            // 7) keep basic attributes in valid range
            //  3->25 for NPC
            //  3->MIN(25, max)
            //      where max = max race + 2 if prime attribute + 1 if enhanced prime attribute
            foreach (var basicAttribute in Enum.GetValues<BasicAttributes>())
            {
                var maxAllowed = MaxAllowedBasicAttribute(basicAttribute);
                var characterAttributeIndex = (CharacterAttributes)basicAttribute;
                _currentAttributes[characterAttributeIndex] = Math.Clamp(_currentAttributes[characterAttributeIndex], 3, maxAllowed);
            }

            // 8) additional recompute needed ?
            if (!additionalRecomputeNeeded || recomputeCount >= MaxRecompute)
                break;
            recomputeCount++;
        }
    }

    // Move
    public bool Move(ExitDirections direction, bool following, bool forceFollowers)
    {
        var fromRoom = Room;

        //TODO exit flags such as climb, ...

        if (!CanMove)
            return false;

        if (Daze > 5)
        {
            Send("You're too dazed to move...");
            return false;
        }
        if (GlobalCooldown > 0)
        {
            Send("You haven't quite recovered yet..."); // this is only be for NPC because PC input processing is blocked until GlobalCooldown is 0
            return false;
        }

        // Under certain circumstances, direction can be modified (Drunk anyone?)
        direction = ChangeDirectionBeforeMove(direction, fromRoom);

        // Get exit and destination room
        var exit = fromRoom[direction];
        var toRoom = exit?.Destination;

        var passThru = ImmortalMode.HasFlag(ImmortalModeFlags.PassThru);

        // Check if existing exit
        if (exit == null || toRoom == null)
        {
            Send("You almost goes {0}, but suddenly realize that there's no exit there.", direction.DisplayNameLowerCase());
            Act(ActOptions.ToRoom, "{0} looks like {0:e}'s about to go {1}, but suddenly stops short and looks confused.", this, direction.DisplayNameLowerCase());
            return false;
        }
        // Closed ?
        if ((exit.IsClosed && (!CharacterFlags.IsSet("PassDoor") || exit.ExitFlags.HasFlag(ExitFlags.NoPass)))
             && !passThru)
        {
            Act(ActOptions.ToCharacter, "The {0} is closed.", exit);
            return false;
        }
        // Private ?
        if (toRoom.IsPrivate) // even if pass-thru, we cannot go in private room
        {
            Send("That room is private right now.");
            return false;
        }

        // Size
        if (toRoom.MaxSize.HasValue && toRoom.MaxSize.Value < Size
            && !passThru)
        {
            Send("You're too huge to go that direction.");
            return false;
        }
        // Flying
        if ((fromRoom.SectorType == SectorTypes.Air || toRoom.SectorType == SectorTypes.Air)
            && !CharacterFlags.IsSet("Flying")
            && !passThru)
        {
            Send("You can't fly.");
            return false;
        }
        // Water
        var hasBoatOrMasterHasBoat = HasBoat;
        if ((fromRoom.SectorType == SectorTypes.WaterSwim || toRoom.SectorType == SectorTypes.WaterSwim)
            && !passThru
            && !CharacterFlags.IsSet("Swim")
            && !CharacterFlags.IsSet("Flying")
            && !hasBoatOrMasterHasBoat) // TODO: WalkOnWater
        {
            Send("You need a boat to go there, or be swimming, flying or walking on water.");
            return false;
        }
        // Water no swim or underwater
        if ((fromRoom.SectorType == SectorTypes.WaterNoSwim || toRoom.SectorType == SectorTypes.WaterNoSwim)
            && !passThru
            && !CharacterFlags.IsSet("Flying")
            && !hasBoatOrMasterHasBoat)// TODO: WalkOnWater
        {
            Send("You need a boat to go there, flying or walking on water.");
            return false;
        }

        // Check move points left or drunk special phrase
        bool beforeMove = BeforeMove(direction, fromRoom, toRoom);
        if (!beforeMove)
            return false;

        //
        if (!CharacterFlags.IsSet("Sneak"))
            Act(ActOptions.ToRoom, "{0} leaves {1}.", this, direction.DisplayNameLowerCase());

        ChangeRoom(toRoom, false);

        // Display special phrase after entering room
        AfterMove(direction, fromRoom, toRoom);

        //
        if (!CharacterFlags.IsSet("Sneak"))
            Act(ActOptions.ToRoom, "{0} has arrived.", this);

        // Followers: no circular follows
        if (forceFollowers && fromRoom != toRoom)
            MoveFollow(fromRoom, toRoom, direction);

        // Recompute both rooms
        if (!following)
        {
            fromRoom.Recompute();
            toRoom.Recompute();
        }

        return true;
    }

    public bool Enter(IItemPortal portal, bool following, bool forceFollowers)
    {
        if (portal == null)
            return false;

        var passThru = ImmortalMode.HasFlag(ImmortalModeFlags.PassThru);

        if (portal.PortalFlags.HasFlag(PortalFlags.Closed) && !passThru)
        {
            Send("You can't seem to find a way in.");
            return false;
        }

        if (((portal.PortalFlags.HasFlag(PortalFlags.NoCurse) && CharacterFlags.IsSet("Curse")) || Room.RoomFlags.IsSet("NoRecall"))
            && !passThru)
        {
            Send("Something prevents you from leaving...");
            return false;
        }

        // Default destination is portal stored destination
        IRoom? destination = portal.Destination;
        // Random portal will fix it's destination once used
        if (portal.PortalFlags.HasFlag(PortalFlags.Random) && portal.Destination == null)
        {
            destination = RoomManager.GetRandomRoom(this);
            portal.ChangeDestination(destination);
        }
        // Buggy portal has a low chance to lead somewhere else
        if (portal.PortalFlags.HasFlag(PortalFlags.Buggy) && RandomManager.Chance(5))
            destination = RoomManager.GetRandomRoom(this);

        if (destination == null
            || destination == Room
            || (!CanSee(destination) && !passThru)
            || destination.IsPrivate) // even if pass-thru, we cannot go in private room
        {
            Act(ActOptions.ToCharacter, "{0:N} doesn't seem to go anywhere.", portal);
            return false;
        }

        if (!IsAllowedToEnterTo(destination))
        {
            Send("Something prevents you from leaving...");
            return false;
        }

        if (destination.MaxSize.HasValue && destination.MaxSize.Value < Size)
        {
            Send("You're too huge to enter that.");
            return false;
        }

        var wasRoom = Room;

        Act(ActOptions.ToRoom, "{0:N} steps into {1}.", this, portal);
        Act(ActOptions.ToCharacter, "You walk through {0} and find yourself somewhere else...", portal);

        (this as IPlayableCharacter)?.IncrementStatistics(AvatarStatisticTypes.PortalUsed);

        ChangeRoom(destination, false);

        // take portal along
        if (portal.PortalFlags.HasFlag(PortalFlags.GoWith) && portal.ContainedInto is IRoom)
        {
            portal.ChangeContainer(Room);
        }

        if (AutomaticallyDisplayRoom)
        {
            StringBuilder sb = new ();
            Room.Append(sb, this);
            Send(sb);
        }

        Act(ActOptions.ToRoom, "{0:N} arrived through {1}.", this, portal);

        // decrease charge left
        portal.Use();

        // if no charge left, destroy portal and no follow
        if (!portal.HasChargeLeft())
        {
            Act(ActOptions.ToCharacter, "{0:N} fades out of existence.", portal);
            if (portal.ContainedInto is IRoom portalInRoom && portalInRoom == Room)
                Act(ActOptions.ToRoom, "{0:N} fades out of existence.", portal);
            if (wasRoom.People.Any())
                Act(ActOptions.ToAll, "{0:N} fades out of existence.", portal);
            ItemManager.RemoveItem(portal);
        }
        else
        {
            // Followers: no circular follows
            if (forceFollowers && wasRoom != destination)
                EnterFollow(wasRoom, destination, portal);
        }

        if (!following)
        {
            wasRoom.Recompute();
            destination.Recompute();
        }

        return true;
    }

    public void ChangeRoom(IRoom destination, bool recompute)
    {
        if (!IsValid)
        {
            Logger.LogError("ICharacter.ChangeRoom: {name} is not valid anymore", DebugName);
            return;
        }

        if (destination == null)
        {
            Logger.LogError("ICharacter.ChangeRoom: {name} from: {room} to null", DebugName, Room == null ? "<<no room>>" : Room.DebugName);
            Wiznet.Log($"Null destination room for character {DebugName}!!", WiznetFlags.Bugs, AdminLevels.Implementor);
        }

        Logger.LogDebug("ICharacter.ChangeRoom: {name} from: {fromRoom} to {toRoom}", DebugName, Room == null ? "<<no room>>" : Room.DebugName, destination == null ? "<<no room>>" : destination.DebugName);
        Room?.Leave(this);
        if (recompute)
            Room?.Recompute();
        Room = destination!;
        destination?.Enter(this);
        if (recompute)
            destination?.Recompute();
    }

    // Combat
    public bool StartFighting(ICharacter victim) // equivalent to set_fighting in fight.C:3441
    {
        if (!IsValid)
        {
            Logger.LogError("StartFighting: {name} is not valid anymore", DebugName);
            return false;
        }

        // TODO: remove sleep affect if any
        if (victim.Fighting == null && victim.Daze == 0 && victim.Position < Positions.Standing)
        {
            int? dazeMultiplier = null;
            switch (victim.Position)
            {
                case Positions.Sleeping:
                    victim.Send("You were caught sleeping!");
                    ActToNotVictim(victim, "{0:N} was caught sleeping!", victim);
                    dazeMultiplier = 3;
                    break;
                case Positions.Resting:
                    victim.Send("You were caught resting!");
                    ActToNotVictim(victim, "{0:N} was caught resting!", victim);
                    dazeMultiplier = 2;
                    break;
                case Positions.Sitting:
                    victim.Send("You were caught sitting down!");
                    ActToNotVictim(victim, "{0:N} was caught sitting down!", victim);
                    dazeMultiplier = 1;
                    break;
            }
            if (dazeMultiplier != null)
            {
                //do_function (victim, &do_sit, ""); TODO ?
                victim.SetDaze(dazeMultiplier.Value * Pulse.PulseViolence + Pulse.PulseViolence / 2);
            }
        }

        Logger.LogDebug("{name} starts fighting {victimName}", DebugName, victim.DebugName);

        Fighting = victim;

        // generate small amount of aggro when entering in combat
        AggroManager.OnStartFight(this, victim);

        StandUpInCombatIfPossible();
        return true;
    }

    public bool StopFighting(bool both) // equivalent to stop_fighting in fight.C:3441
    {
        Logger.LogDebug("{name} stops fighting {victimName} [{both}]", DebugName, Fighting?.DebugName ?? "<<no victim>>", both);

        // remove 'this' aggro table
        if (both)
            AggroManager.Clear(this);
        //else if (Fighting != null)
        //    AggroManager.OnStopFight(this, Fighting);

        Fighting = null;
        Stun = 0;
        ChangePosition(DefaultPosition);
        if (both)
        {
            var fighters = CharacterManager.Characters.Where(x => x.Fighting == this).ToArray();
            foreach (var victim in fighters)
                victim.StopFighting(false);
        }
        return true;
    }

    public void MultiHit(ICharacter? victim) // 'this' starts a combat with 'victim'
    {
        MultiHit(victim, null);
    }

    public abstract void MultiHit(ICharacter? victim, IMultiHitModifier? multiHitModifier); // 'this' starts a combat with 'victim' and has been initiated by an ability

    public DamageResults AbilityDamage(ICharacter source, int damage, SchoolTypes damageType, string? damageNoun, bool display) // 'this' is dealt damage by 'source' using an ability
    {
        var damageResults = Damage(source, damage, damageType, DamageSources.Ability, damageNoun, display);
        if (damageResults == DamageResults.Done && source != this)
            HandleWimpy();
        return damageResults;
    }

    public DamageResults HitDamage(ICharacter source, IItemWeapon? wield, int damage, SchoolTypes damageType, string damageNoun, bool display) // 'this' is dealt damage by 'source' using a weapon
    {
        var damageResults = Damage(source, damage, damageType, DamageSources.Hit, damageNoun, display);
        if (damageResults == DamageResults.Done && source != this)
            HandleWimpy();
        return damageResults;
    }

    public abstract void HandleAutoGold(IItemCorpse corpse);
    public abstract void HandleAutoLoot(IItemCorpse corpse);
    public abstract void HandleAutoSacrifice(IItemCorpse corpse);

    public IItemCorpse? RawKilled(ICharacter? killer, bool payoff)
    {
        if (!IsValid)
        {
            Logger.LogWarning("RawKilled: {name} is not valid anymore", DebugName);
            return null;
        }

        Send("%R%You have been KILLED!!%x%", true);
        Act(ActOptions.ToRoom, "{0:N} is dead.", this);

        Wiznet.Log($"{DebugName} got toasted by {killer?.DebugName ?? "???"} at {Room?.DebugName ?? "???"}", DeathWiznetFlags);

        // remove 'this' from every fight
        StopFighting(true);

        // Clear 'this' generated aggro in every aggro table
        AggroManager.OnDeath(this);

        // Death cry
        ActToNotVictim(this, "You hear {0}'s death cry.", this); // TODO: custom death cry + body part creation

        // Get each character that will be impacted by death (group member, pet master, ...)
        var playableCharactersImpactedByKill = killer?.GetPlayableCharactersImpactedByKill().ToArray();

        // Create corpse if needed
        var corpse = CreateCorpseOnDeath
            ? ItemManager.AddItemCorpse(Guid.NewGuid(), this, $"RawKilled[{killer?.DebugName ?? "/"}]", Room!)
           : null;

        // Generate loots
        if (playableCharactersImpactedByKill != null)
            LootManager.GenerateLoots(corpse, this, playableCharactersImpactedByKill);

        // handle death here to avoid showing autoloot/sac/... message to victim
        HandleDeath();

        // xp/reputation/quests for each member of groups + auto loot/gold/sac for killer
        if (payoff)
        {
            if (playableCharactersImpactedByKill != null && playableCharactersImpactedByKill.Length > 0)
            {
                // xp/reputation/quests/...
                var groupLevelSum = playableCharactersImpactedByKill.Sum(x => x.Level);
                foreach (var playableCharacterImpactedByKill in playableCharactersImpactedByKill)
                    playableCharacterImpactedByKill?.KillingPayoff(this, groupLevelSum);

                // autogold by killer
                if (corpse != null && killer != null)
                    killer.HandleAutoGold(corpse);

                // autoloot by each PC impacted by kill (this will enable quest item to be retrieved by master and group members) starting by killer
                if (corpse != null)
                {
                    foreach (var playableCharacterImpactedByKill in playableCharactersImpactedByKill.OrderBy(x => x == killer ? 0 : 1))
                        playableCharacterImpactedByKill.HandleAutoLoot(corpse);
                }

                // autosac by killer
                if (corpse != null && killer != null)
                    killer.HandleAutoSacrifice(corpse);
            }

            //
            DeathPayoff(killer);
        }

        return corpse;
    }

    public bool SavesSpell(int level, SchoolTypes damageType)
    {
        var victim = this;
        var save = 50 + (victim.Level - level) * 5 - victim[CharacterAttributes.SavingThrow] * 2;
        if (victim.CharacterFlags.IsSet("Berserk"))
            save += victim.Level / 2;
        var resistanceResult = ResistanceCalculator.CheckResistance(victim, damageType);
        switch (resistanceResult)
        {
            case ResistanceLevels.Immune:
                return true;
            case ResistanceLevels.Resistant:
                save += 2;
                break;
            case ResistanceLevels.Vulnerable:
                save -= 2;
                break;
        }
        if (victim.Class?.CurrentResourceKinds(victim.Shape).Contains(ResourceKinds.Mana) == true)
            save = (save * 9) / 10;
        save = Math.Clamp(save, 5, 95);
        return RandomManager.Chance(save);
    }

    public bool IsSafeSpell(ICharacter caster, bool area)
    {
        var victim = this;
        if (!victim.IsValid || victim.Room == null || !caster.IsValid || caster.Room == null)
            return true;
        if (area && caster == victim)
            return true;
        if (victim.Fighting == caster || victim == caster)
            return false;
        if (!area && ImmortalMode.HasFlag(ImmortalModeFlags.AlwaysSafe))
            return false;
        // safe room
        if (victim.Room.RoomFlags.IsSet("Safe"))
            return true;
        // Killing npc
        if (victim is INonPlayableCharacter npcVictim)
        {
            if (npcVictim.ActFlags.HasAny("Train", "Gain", "Practice", "IsHealer")
                || npcVictim.Blueprint is CharacterQuestorBlueprint
                || npcVictim.Blueprint is CharacterShopBlueprintBase)
                return true;
            // Npc doing the killing
            if (caster is INonPlayableCharacter)
            {
                // no pets
                if (npcVictim.ActFlags.IsSet("Pet"))
                    return true;
                // no charmed creatures unless owner
                if (victim.CharacterFlags.IsSet("Charm") && (area || caster != npcVictim.Master))
                    return true;
                // legal kill? -- cannot hit mob fighting non-group member
                if (victim.Fighting != null && !caster.IsSameGroupOrPet(victim.Fighting))
                    return true;
            }
            // Player doing the killing
            else
            {
                // area effect spells do not hit other mobs
                if (area && caster.Fighting != null && !victim.IsSameGroupOrPet(caster.Fighting))
                    return true;
            }
        }
        // Killing players
        else
        {
            if (area && victim.ImmortalMode.HasFlag(ImmortalModeFlags.AlwaysSafe))
                return true;
            // Npc doing the killing
            if (caster is INonPlayableCharacter npcCaster)
            {
                // charmed mobs and pets cannot attack players while owned
                if (caster.CharacterFlags.IsSet("Charm") && npcCaster.Master!= null && npcCaster.Master.Fighting != victim)
                    return true;
                // legal kill? -- mobs only hit players grouped with opponent
                if (caster.Fighting != null && !caster.Fighting.IsSameGroupOrPet(victim))
                    return true;
            }
            // Player doing the killing
            else
            {
                // TODO: PK
                //if (!is_clan(ch))
                //    return true;

                //if (IS_SET(victim->act, PLR_KILLER) || IS_SET(victim->act, PLR_THIEF))
                //    return FALSE;

                //if (!is_clan(victim))
                //    return true;

                if (Level > victim.Level + 8)
                    return true;
            }
        }
        return false;
    }

    public string? IsSafe(ICharacter aggressor)
    {
        var victim = this;
        if (!victim.IsValid || victim.Room == null || !aggressor.IsValid || aggressor.Room == null)
            return "Invalid target!";
        if (victim.Fighting == aggressor || victim == aggressor)
            return null;
        if (victim.ImmortalMode.HasFlag(ImmortalModeFlags.AlwaysSafe))
            return null;
        if (victim.Room.RoomFlags.IsSet("Safe"))
            return "Not in this room.";
        // Killing npc
        if (victim is INonPlayableCharacter npcVictim)
        {
            if (npcVictim.Blueprint is CharacterShopBlueprintBase)
                return "The shopkeeper wouldn't like that.";

            if (npcVictim.ActFlags.HasAny("Train", "Gain", "Practice", "IsHealer")
                || npcVictim.Blueprint is CharacterQuestorBlueprint)
                return "I don't think Mota would approve.";

            // Player doing the killing
            if (aggressor is IPlayableCharacter)
            {
                // no pets
                if (npcVictim.ActFlags.IsSet("Pet"))
                    return aggressor.ActPhrase("But {0} looks so cute and cuddly...", victim);
                // no charmed creatures unless owner
                if (victim.CharacterFlags.IsSet("Charm") && aggressor != npcVictim.Master)
                    return "You don't own that monster.";
            }
        }
        // Killing player
        else
        {
            // Npc doing the killing
            if (aggressor is INonPlayableCharacter npcAggressor)
            {
                // charmed mobs and pets cannot attack players while owned
                if (aggressor.CharacterFlags.IsSet("Charm") && npcAggressor.Master != null && npcAggressor.Master.Fighting != victim)
                    return "Players are your friends!";
            }
            // Player doing the killing
            else
            {
                //if (!is_clan(ch))
                //{
                //    send_to_char("Join a clan if you want to kill players.\n\r", ch);
                //    return true;
                //}

                //if (IS_SET(victim->act, PLR_KILLER) || IS_SET(victim->act, PLR_THIEF))
                //    return FALSE;

                //if (!is_clan(victim))
                //{
                //    send_to_char("They aren't in a clan, leave them alone.\n\r", ch);
                //    return true;
                //}

                if (Level > victim.Level + 8)
                    return "Pick on someone your own size.";
            }
        }
        return null;
    }

    public bool Flee()
    {
        if (Fighting == null)
        {
            Send("You aren't fightning anyone.");
            return false;
        }

        if (Daze > 5) // totally dazed, can't flee
        {
            Send("You're too dazed to get your bearings!");
            return false;
        }

        var wasInRoom = Room;
        var pc = this as IPlayableCharacter;
        var npc = this is INonPlayableCharacter;

        // Try 6 times to find an exit
        for (int attempt = 0; attempt < 6; attempt++)
        {
            var randomExit = RandomManager.Random<ExitDirections>() ?? ExitDirections.North;
            var exit = Room.Exits[(int) randomExit];
            var destination = exit?.Destination;
            if (destination != null && exit?.IsClosed == false
                && RandomManager.OneOutOf(Daze) // partially dazed, has some chance to flee
                && IsAllowedToEnterTo(destination))
            {
                // Try to move without checking if in combat or not
                Move(randomExit, false, false); // followers will not follow
                if (Room != wasInRoom) // successful only if effectively moved away
                {
                    //
                    StopFighting(true);
                    //
                    Send("You flee from combat!");
                    Act(wasInRoom.People, "{0} has fled!", this);

                    if (pc != null)
                        // TODO
                        //if ((ch->class == CLASS_THIEF) && (number_percent() < 3 * (ch->level / 2)))
                        //send_to_char("You snuck away safely.\n\r", ch);
                        // else
                    {
                        Send("You lost 10 exp.");
                        pc.GainExperience(-10);
                    }

                    // decrease aggro when fleeing
                    AggroManager.OnFlee(this, wasInRoom);

                    return true;
                }
            }
        }

        Send("PANIC! You couldn't escape!");
        return false;
    }

    public void OnDamagePerformed(int damage, DamageSources damageSource)
    {
        RageGenerator.GenerateRageFromOutgoingDamage(this, damage, damageSource);
    }

    // Abilities
    public abstract (int percentage, IAbilityLearned? abilityLearned) GetWeaponLearnedAndPercentage(IItemWeapon? weapon);

    public abstract (int percentage, IAbilityLearned? abilityLearned) GetAbilityLearnedAndPercentage(string abilityName);

    public IDictionary<string, int> AbilitiesInCooldown => _cooldownsPulseLeft;

    public bool HasAbilitiesInCooldown => _cooldownsPulseLeft.Count != 0;

    public int CooldownPulseLeft(string abilityName)
    {
        if (_cooldownsPulseLeft.TryGetValue(abilityName, out int pulseLeft))
            return pulseLeft;
        return int.MinValue;
    }

    public void SetCooldown(string abilityName, TimeSpan timeSpan)
    {
        _cooldownsPulseLeft[abilityName] = Pulse.FromTimeSpan(timeSpan);
    }

    public bool DecreaseCooldown(string abilityName, int pulseCount)
    {
        if (_cooldownsPulseLeft.TryGetValue(abilityName, out int pulseLeft))
        {
            pulseLeft = Math.Max(0, pulseLeft - pulseCount);
            _cooldownsPulseLeft[abilityName] = pulseLeft;
            return pulseLeft == 0;
        }
        return false;
    }

    public void ResetCooldown(string abilityName, bool verbose)
    {
        _cooldownsPulseLeft.Remove(abilityName);
        if (verbose)
            Send("%b%{0}%x% is available.", abilityName);
    }

    // Equipment
    public IItem? GetEquipment(EquipmentSlots slot)
        => Equipments.FirstOrDefault(x => x.Slot == slot && x.Item != null)?.Item;

    public T? GetEquipment<T>(EquipmentSlots slot)
        where T : IItem => Equipments.Where(x => x.Slot == slot && x.Item is T).Select(x => x.Item).OfType<T>().FirstOrDefault();

    public (IEquippedItem? equippedItem, SearchEquipmentSlotResults result) SearchEquipmentSlot(IItem item, bool replace)
        => item.WearLocation switch
        {
            WearLocations.None => (null, SearchEquipmentSlotResults.NoWearLocation),
            WearLocations.Light => SearchEquipmentSlot(EquipmentSlots.Light, replace),
            WearLocations.Head => SearchEquipmentSlot(EquipmentSlots.Head, replace),
            WearLocations.Amulet => SearchEquipmentSlot(EquipmentSlots.Amulet, replace),
            WearLocations.Chest => SearchEquipmentSlot(EquipmentSlots.Chest, replace),
            WearLocations.Cloak => SearchEquipmentSlot(EquipmentSlots.Cloak, replace),
            WearLocations.Waist => SearchEquipmentSlot(EquipmentSlots.Waist, replace),
            WearLocations.Wrists => SearchEquipmentSlot(EquipmentSlots.Wrists, replace),
            WearLocations.Arms => SearchEquipmentSlot(EquipmentSlots.Arms, replace),
            WearLocations.Hands => SearchEquipmentSlot(EquipmentSlots.Hands, replace),
            WearLocations.Ring => SearchEquipmentSlot(EquipmentSlots.Ring, replace),
            WearLocations.Legs => SearchEquipmentSlot(EquipmentSlots.Legs, replace),
            WearLocations.Feet => SearchEquipmentSlot(EquipmentSlots.Feet, replace),
            WearLocations.Wield => SearchOneHandedWeaponEquipmentSlot(replace),// Search empty mainhand, then empty offhand
            WearLocations.Hold => SearchOffhandEquipmentSlot(replace),// only if mainhand is not wielding a 2H
            WearLocations.Shield => SearchOffhandEquipmentSlot(replace),// only if mainhand is not wielding a 2H
            WearLocations.Wield2H => SearchTwoHandedWeaponEquipmentSlot(replace),// Search empty mainhand + empty offhand
            WearLocations.Float => SearchEquipmentSlot(EquipmentSlots.Float, replace),
            _ => (null, SearchEquipmentSlotResults.UnknownWearLocation),
        };

    // Misc
    public virtual bool GetItem(IItem item, IContainer container) // equivalent to get_obj in act_obj.C:211
    {
        //
        if (item.NoTake || !CanLoot(item))
        {
            Send("You can't take that.");
            return false;
        }
        if (CarryNumber + item.CarryCount > MaxCarryNumber)
        {
            Act(ActOptions.ToCharacter, "{0:N}: you can't carry that many items.", item);
            return false;
        }
        if (CarryWeight + item.TotalWeight > MaxCarryWeight)
        {
            Act(ActOptions.ToCharacter, "{0:N}: you can't carry that much weight.", item);
            return false;
        }

        // TODO: from pit ?
        if (container != null)
            Act(ActOptions.ToAll, "{0:N} get{0:v} {1} from {2}.", this, item, container);
        else
            Act(ActOptions.ToAll, "{0:N} get{0:v} {1}.", this, item);

        if (item is IItemMoney money)
        {
            UpdateMoney(money.SilverCoins, money.GoldCoins);
            ItemManager.RemoveItem(money);
        }
        else
            item.ChangeContainer(this);
        return true;
    }

    // Display
    public StringBuilder Append(StringBuilder sb, ICharacter viewer, bool peekInventory) // equivalent to act_info.C:show_char_to_char_1
    {
        // description
        sb.Append(RelativeDescription(viewer)); // description always ends with CRLF

        // position
        sb.AppendLine($"{StringHelpers.Subjects[Sex].UpperFirstLetter()} is {Position.ToString().ToLowerInvariant()}");

        // display name + condition
        var condition = "is here.";
        var maxHitPoints = MaxResource(ResourceKinds.HitPoints);
        if (maxHitPoints > 0)
        {
            int percent = (100 * this[ResourceKinds.HitPoints]) / maxHitPoints;
            if (percent >= 100)
                condition = "is in excellent condition.";
            else if (percent >= 90)
                condition = "has a few scratches.";
            else if (percent >= 75)
                condition = "has some small wounds and bruises.";
            else if (percent >= 50)
                condition = "has quite a few wounds.";
            else if (percent >= 30)
                condition = "has some big nasty wounds and scratches.";
            else if (percent >= 15)
                condition = "looks pretty hurt.";
            else if (percent >= 0)
                condition = "is in awful condition.";
            else
                condition = "is bleeding to death.";
        }
        sb.AppendLine($"{RelativeDisplayName(viewer, true)} {condition}");

        // equipments
        if (Equipments.Any(x => x.Item != null))
        {
            sb.AppendLine($"{RelativeDisplayName(viewer, true)} is using:");
            foreach (var equippedItem in Equipments.Where(x => x.Item != null))
            {
                sb.Append(equippedItem.EquipmentSlotsToString(Size));
                equippedItem.Item!.Append(sb, viewer, true);
                sb.AppendLine();
            }
        }

        // inventory
        if (peekInventory)
        {
            sb.AppendLine("You peek at the inventory:");
            IEnumerable<IItem> items = viewer == this
                ? Inventory
                : Inventory.Where(viewer.CanSee); // don't display 'invisible item' when inspecting someone else
            ItemsHelpers.AppendItems(sb, items, this, true, true);
        }

        return sb;
    }

    public StringBuilder AppendInRoom(StringBuilder sb, ICharacter viewer)
    {
        var npc = this as INonPlayableCharacter;
        // quest ?
        if (viewer is IPlayableCharacter pcViewer && npc != null && npc.IsQuestObjective(pcViewer, true))
            sb.Append(StringHelpers.QuestPrefix);
        // display flags
        FlagsManager.Append(sb, CharacterFlags, false);
        if (viewer.CharacterFlags.IsSet("DetectEvil") && IsEvil)
            sb.Append("%r%(Red Aura)%x%");
        if (viewer.CharacterFlags.IsSet("DetectGood") && IsGood)
            sb.Append("%Y%(Golden Aura)%x%");
        FlagsManager.Append(sb, ShieldFlags, false);

        // TODO: killer/thief

        if (npc != null && npc.Blueprint.StartPosition == Position && !string.IsNullOrWhiteSpace(npc.Blueprint.LongDescription) && npc.Fighting == null)
        {
            sb.Append(npc.Blueprint.LongDescription); // long description always includes CRLF
            return sb;
        }

        sb.Append(RelativeDisplayName(viewer));
        // TODO: title ?
        if (IsStunned)
            sb.Append(" is lying here stunned.");
        else if (Fighting != null)
        {
            sb.Append(" is here, fighting ");
            if (Fighting == viewer)
                sb.Append("YOU!");
            else if (Room == Fighting.Room)
                sb.AppendFormat("{0}.", Fighting.RelativeDisplayName(viewer));
            else
            {
                Logger.LogWarning("{name} is fighting {fightingName} in a different room.", DebugName, Fighting.DebugName);
                sb.Append("someone who left??");
            }
        }
        else
        {
            switch (Position)
            {
                case Positions.Sleeping:
                    AppendPositionFurniture(sb, "sleeping", Furniture);
                    break;
                case Positions.Resting:
                    AppendPositionFurniture(sb, "resting", Furniture);
                    break;
                case Positions.Sitting:
                    AppendPositionFurniture(sb, "sitting", Furniture);
                    break;
                case Positions.Standing:
                    if (Furniture != null)
                        AppendPositionFurniture(sb, "standing", Furniture);
                    else
                        sb.Append(" is here");
                    break;
            }
        }
        sb.AppendLine();
        return sb;
    }

    // Affects
    public void ApplyAffect(ICharacterFlagsAffect affect)
    {
        switch (affect.Operator)
        {
            case AffectOperators.Add:
            case AffectOperators.Or:
                CharacterFlags.Set(affect.Modifier);
                break;
            case AffectOperators.Assign:
                CharacterFlags.Copy(affect.Modifier);
                break;
            case AffectOperators.Nor:
                CharacterFlags.Unset(affect.Modifier);
                break;
        }
    }

    public void ApplyAffect(ICharacterIRVAffect affect)
    {
        switch (affect.Location)
        {
            case IRVAffectLocations.Immunities:
                switch (affect.Operator)
                {
                    case AffectOperators.Add:
                    case AffectOperators.Or:
                        Immunities.Set(affect.Modifier);
                        break;
                    case AffectOperators.Assign:
                        Immunities.Copy(affect.Modifier);
                        break;
                    case AffectOperators.Nor:
                        Immunities.Unset(affect.Modifier);
                        break;
                }
                break;
            case IRVAffectLocations.Resistances:
                switch (affect.Operator)
                {
                    case AffectOperators.Add:
                    case AffectOperators.Or:
                        Resistances.Set(affect.Modifier);
                        break;
                    case AffectOperators.Assign:
                        Resistances.Copy(affect.Modifier);
                        break;
                    case AffectOperators.Nor:
                        Resistances.Unset(affect.Modifier);
                        break;
                }
                break;
            case IRVAffectLocations.Vulnerabilities:
                switch (affect.Operator)
                {
                    case AffectOperators.Add:
                    case AffectOperators.Or:
                        Resistances.Set(affect.Modifier);
                        break;
                    case AffectOperators.Assign:
                        Resistances.Copy(affect.Modifier);
                        break;
                    case AffectOperators.Nor:
                        Resistances.Unset(affect.Modifier);
                        break;
                }
                break;
        }
    }

    public void ApplyAffect(ICharacterShieldFlagsAffect affect)
    {
        switch (affect.Operator)
        {
            case AffectOperators.Add:
            case AffectOperators.Or:
                
                ShieldFlags.Set(affect.Modifier);
                break;
            case AffectOperators.Assign:
                ShieldFlags.Copy(affect.Modifier);
                break;
            case AffectOperators.Nor:
                ShieldFlags.Unset(affect.Modifier);
                break;
        }
    }

    public void ApplyAffect(ICharacterAttributeAffect affect)
    {
        if (affect.Location == CharacterAttributeAffectLocations.None)
            return;
        if (affect.Location == CharacterAttributeAffectLocations.Characteristics)
        {
            switch (affect.Operator)
            {
                case AffectOperators.Add:
                    _currentAttributes[CharacterAttributes.Strength] += affect.Modifier;
                    _currentAttributes[CharacterAttributes.Intelligence] += affect.Modifier;
                    _currentAttributes[CharacterAttributes.Wisdom] += affect.Modifier;
                    _currentAttributes[CharacterAttributes.Dexterity] += affect.Modifier;
                    _currentAttributes[CharacterAttributes.Constitution] += affect.Modifier;
                    break;
                case AffectOperators.Assign:
                    _currentAttributes[CharacterAttributes.Strength] = affect.Modifier;
                    _currentAttributes[CharacterAttributes.Intelligence] = affect.Modifier;
                    _currentAttributes[CharacterAttributes.Wisdom] = affect.Modifier;
                    _currentAttributes[CharacterAttributes.Dexterity] = affect.Modifier;
                    _currentAttributes[CharacterAttributes.Constitution] = affect.Modifier;
                    break;
                case AffectOperators.Or:
                case AffectOperators.Nor:
                    Logger.LogError("Invalid AffectOperators {operator} for CharacterAttributeAffect Characteristics", affect.Operator);
                    break;
            }
            return;
        }
        if (affect.Location == CharacterAttributeAffectLocations.AllArmor)
        {
            switch (affect.Operator)
            {
                case AffectOperators.Add:
                    _currentAttributes[CharacterAttributes.ArmorBash] += affect.Modifier;
                    _currentAttributes[CharacterAttributes.ArmorPierce] += affect.Modifier;
                    _currentAttributes[CharacterAttributes.ArmorSlash] += affect.Modifier;
                    _currentAttributes[CharacterAttributes.ArmorExotic] += affect.Modifier;
                    break;
                case AffectOperators.Assign:
                    _currentAttributes[CharacterAttributes.ArmorBash] = affect.Modifier;
                    _currentAttributes[CharacterAttributes.ArmorPierce] = affect.Modifier;
                    _currentAttributes[CharacterAttributes.ArmorSlash] = affect.Modifier;
                    _currentAttributes[CharacterAttributes.ArmorExotic] = affect.Modifier;
                    break;
                case AffectOperators.Or:
                case AffectOperators.Nor:
                    Logger.LogError("Invalid AffectOperators {operator} for CharacterAttributeAffect AllArmor", affect.Operator);
                    break;
            }
            return;
        }
        CharacterAttributes attribute;
        switch (affect.Location)
        {
            case CharacterAttributeAffectLocations.Strength: attribute = CharacterAttributes.Strength; break;
            case CharacterAttributeAffectLocations.Intelligence: attribute = CharacterAttributes.Intelligence; break;
            case CharacterAttributeAffectLocations.Wisdom: attribute = CharacterAttributes.Wisdom; break;
            case CharacterAttributeAffectLocations.Dexterity: attribute = CharacterAttributes.Dexterity; break;
            case CharacterAttributeAffectLocations.Constitution: attribute = CharacterAttributes.Constitution; break;
            case CharacterAttributeAffectLocations.SavingThrow: attribute = CharacterAttributes.SavingThrow; break;
            case CharacterAttributeAffectLocations.HitRoll: attribute = CharacterAttributes.HitRoll; break;
            case CharacterAttributeAffectLocations.DamRoll: attribute = CharacterAttributes.DamRoll; break;
            case CharacterAttributeAffectLocations.ArmorBash: attribute = CharacterAttributes.ArmorBash; break;
            case CharacterAttributeAffectLocations.ArmorPierce: attribute = CharacterAttributes.ArmorPierce; break;
            case CharacterAttributeAffectLocations.ArmorSlash: attribute = CharacterAttributes.ArmorSlash; break;
            case CharacterAttributeAffectLocations.ArmorMagic: attribute = CharacterAttributes.ArmorExotic; break;
            default:
                Logger.LogError("CharacterBase.ApplyAffect: Unexpected CharacterAttributeAffectLocations {location}", affect.Location);
                return;
        }
        switch (affect.Operator)
        {
            case AffectOperators.Add:
                _currentAttributes[attribute] += affect.Modifier;
                break;
            case AffectOperators.Assign:
                _currentAttributes[attribute] = affect.Modifier;
                break;
            case AffectOperators.Or:
            case AffectOperators.Nor:
                Logger.LogError("Invalid AffectOperators {operator} for CharacterAttributeAffect {location}", affect.Operator, affect.Location);
                break;
        }
    }

    public void ApplyAffect(ICharacterSexAffect affect)
    {
        Sex = affect.Value;
    }

    public void ApplyAffect(ICharacterSizeAffect affect)
    {
        Size = affect.Value;
    }

    public void ApplyAffect(ICharacterResourceAffect affect)
    {
        switch (affect.Operator)
        {
            case AffectOperators.Add:
                _currentMaxResources[affect.Location] = Math.Max(0, _currentMaxResources[affect.Location] + affect.Modifier);
                break;
            case AffectOperators.Assign:
                _currentMaxResources[affect.Location] = Math.Max(0, affect.Modifier);
                break;
            case AffectOperators.Or:
            case AffectOperators.Nor:
                Logger.LogError("Invalid AffectOperators {operator} for CharacterResourceAffect {location}", affect.Operator, affect.Location);
                break;
        }
    }

    public virtual void OnRemoved(IRoom nullRoom) // called before removing a character from the game
    {
        base.OnRemoved();

        StopFighting(true);

        AggroManager.Clear(this);

        // Leave follower
        Leader?.RemoveFollower(this);

        // Release followers
        foreach (var follower in CharacterManager.Characters.Where(x => x.Leader == this))
            RemoveFollower(follower);

        ResetCooldowns();
        DeleteInventory();
        DeleteEquipments();
        Room = nullRoom; // this will avoid a lot of problem, will be set to null in Cleanup phase
    }

    #endregion

    protected abstract WiznetFlags DeathWiznetFlags { get; }

    protected abstract bool CreateCorpseOnDeath { get; }

    protected abstract int CharacterTypeSpecificDamageModifier(int damage);

    protected abstract bool CanMove { get; }

    protected abstract bool IsAllowedToEnterTo(IRoom destination);

    protected abstract bool HasBoat { get; }

    protected abstract ExitDirections ChangeDirectionBeforeMove(ExitDirections direction, IRoom fromRoom);

    protected abstract bool BeforeMove(ExitDirections direction, IRoom fromRoom, IRoom toRoom);

    protected abstract void AfterMove(ExitDirections direction, IRoom fromRoom, IRoom toRoom);

    protected abstract bool CheckEquippedItemsDuringRecompute();

    protected abstract int MaxAllowedBasicAttribute(BasicAttributes basicAttributes);

    protected abstract Positions DefaultPosition { get; }

    protected abstract bool CannotDie { get; }

    protected abstract (decimal hit, decimal move, decimal mana, decimal psy) CalculateResourcesDeltaByMinute();

    protected abstract (decimal energy, decimal rage) CalculateResourcesDeltaBySecond();

    protected virtual bool AutomaticallyDisplayRoom
        => IncarnatedBy != null;

    protected virtual void MoveFollow(IRoom fromRoom, IRoom toRoom, ExitDirections direction)
    {
        if (fromRoom != toRoom)
        {
            var followers = fromRoom.People.Where(x => x.IsValid && x.Leader == this && x.Position == Positions.Standing && x.CanSee(toRoom)).ToArray(); // clone because Move will modify fromRoom.People
            foreach (var follower in followers)
            {
                follower.Send("You follow {0}.", DisplayName);
                follower.Move(direction, true, true);
            }
        }
    }

    protected virtual void EnterFollow(IRoom wasRoom, IRoom destination, IItemPortal portal)
    {
        // Followers will not automatically enter portal
    }

    protected virtual IEnumerable<ICharacter> GetActTargets(ActOptions option, Positions minPosition)
    {
        switch (option)
        {
            case ActOptions.ToAll:
                return Room.People.Where(x => x.Position >= minPosition);
            case ActOptions.ToRoom:
                return Room.People.Where(x => x != this && x.Position >= minPosition);
            case ActOptions.ToGroup:
                Logger.LogWarning("Act with option ToGroup used on generic CharacterBase");
                return []; // defined only for PlayableCharacter
            case ActOptions.ToCharacter:
                return Position >= minPosition ? [this] : [];
            default:
                Logger.LogError("Act with invalid option: {option}", option);
                return [];
        }
    }

    protected abstract void HandleDeath();

    protected abstract void HandleWimpy();

    protected abstract (int thac0_00, int thac0_32) GetThac0();

    protected abstract SchoolTypes NoWeaponDamageType { get; }

    protected abstract int NoWeaponBaseDamage { get; }

    protected abstract string NoWeaponDamageNoun { get; }

    protected int GetWeaponBaseDamage(IItemWeapon weapon, ICharacter victim, int weaponLearned)
    {
        int damage = RandomManager.Dice(weapon.DiceCount, weapon.DiceValue) * weaponLearned / 100;
        if (GetEquipment<IItemShield>(EquipmentSlots.OffHand) == null) // no shield -> more damage
            damage = 11 * damage / 10;
        foreach (var damageModifierWeaponEffect in WeaponEffectManager.WeaponEffectsByType<IDamageModifierWeaponEffect>(weapon))
        {
            var effect = WeaponEffectManager.CreateInstance<IDamageModifierWeaponEffect>(damageModifierWeaponEffect);
            if (effect != null)
                damage += effect.DamageModifier(this, victim, weapon, weaponLearned, damage);
        }

        return damage;
    }

    protected void OneHit(ICharacter? victim, IItemWeapon? wield, IHitModifier? hitModifier) // 'this' hits 'victim' using hitModifier (optional, used only for backstab)
    {
        if (victim == this || victim == null)
            return;
        // can't beat a dead char!
        // guard against weird room-leavings.
        if (victim.Room != Room)
            return;
        var damageType = wield?.DamageType ?? NoWeaponDamageType;
        // get weapon skill
        var (percentage, abilityLearned) = GetWeaponLearnedAndPercentage(wield);
        var learned = 20 + percentage;
        // Calculate to-hit-armor-class-0 versus armor.
        (int thac0_00, int thac0_32) = GetThac0();
        int thac0 = IntExtensions.Lerp(thac0_00, thac0_32, Level, 32);
        if (thac0 < 0)
            thac0 /= 2;
        if (thac0 < -5)
            thac0 = -5 + (thac0 + 5) / 2;
        thac0 -= HitRoll * learned / 100;
        thac0 += 5 * (100 - learned) / 100;
        if (hitModifier != null)
            thac0 = hitModifier.Thac0Modifier(thac0);
        var victimAc = damageType switch
        {
            SchoolTypes.Bash => victim[Armors.Bash] / 10,
            SchoolTypes.Pierce => victim[Armors.Pierce] / 10,
            SchoolTypes.Slash => victim[Armors.Slash] / 10,
            _ => victim[Armors.Exotic] / 10,
        };
        if (victimAc < -15)
            victimAc =  (victimAc + 15) / 5 - 15;
        if (!CanSee(victim))
            victimAc -= 4;
        if (victim.Position < Positions.Standing)
            victimAc += 1;
        else if (victim.Position < Positions.Sitting)
            victimAc += 2;
        else if (victim.Position < Positions.Resting)
            victimAc += 3;
        else
            victimAc += 4;
        // miss ?  1d20 -> 1:miss  20: success
        var diceroll = RandomManager.Dice(1, 20); //  while ((diceroll = number_bits (5)) >= 20) /*NOP*/;   in original code which is equivalent to a roll on a D20
        if (diceroll == 1
            || (diceroll != 20 && diceroll < thac0 - victimAc))
        {
            if (hitModifier?.AbilityName != null)
                victim.AbilityDamage(this, 0, damageType, hitModifier.DamageNoun ?? "hit", true); // miss
            else
            {
                var damageNoun = wield == null ? NoWeaponDamageNoun : wield.DamageNoun;
                victim.HitDamage(this, wield, 0, damageType, damageNoun ?? "hit", true);
            }

            return;
        }

        // avoidance
        foreach (var avoidanceAbilityDefinition in AbilityManager.SearchAbilitiesByExecutionType<IHitAvoidancePassive>())
        {
            // check victim learned percentage
            var (avoidancePercentage, _) = victim.GetAbilityLearnedAndPercentage(avoidanceAbilityDefinition.Name);
            if (avoidancePercentage > 0)
            {
                // check if avoidance is triggered
                var avoidanceAbility = AbilityManager.CreateInstance<IHitAvoidancePassive>(avoidanceAbilityDefinition);
                if (avoidanceAbility != null)
                {
                    bool success = avoidanceAbility.Avoid(victim, this, damageType);
                    if (success)
                    {
                        StartFightAndFightBack(victim);
                        return; // stops here -> no damage
                    }
                }
            }
        }

        // instant-death ?
        // must stop here because victim is dead, corpse has been created (an eventually looted/sacced)
        if (wield != null)
        {
            foreach (var instantDeathWeaponEffect in WeaponEffectManager.WeaponEffectsByType<IInstantDeathWeaponEffect>(wield))
            {
                var effect = WeaponEffectManager.CreateInstance<IInstantDeathWeaponEffect>(instantDeathWeaponEffect);
                if (effect != null)
                {
                    bool isTriggered = effect.Trigger(this, victim, wield, damageType);
                    if (isTriggered)
                    {
                        victim.RawKilled(this, true);
                        return; // must stop here because victim is dead, corpse has been created (an eventually looted/sacced)
                    }
                }
            }
        }

        // base weapon damage
        int damage = wield == null
            ? NoWeaponBaseDamage
            : GetWeaponBaseDamage(wield, victim, learned);
        if (abilityLearned != null)
        {
            //var weaponAbility = AbilityManager.CreateInstance<IPassive>(abilityLearned.Name);
            //weaponAbility?.IsTriggered(this, victim, true, out _, out _); // TODO: maybe we should test return value (imagine a big bad boss which add CD to every skill)
            // we don't need to call IsTriggered because GetWeaponBaseDamage already used learned %age to mitigate the damage
            // the only thing we need in IsTriggered is to call CheckAbilityImprove
            var pc = this as IPlayableCharacter;
            pc?.CheckAbilityImprove(abilityLearned.Name, true, abilityLearned.AbilityUsage.AbilityDefinition.LearnDifficultyMultiplier);
        }

        // bonus damage such as EnhancedDamage
        foreach (var enhancementAbility in AbilityManager.SearchAbilitiesByExecutionType<IHitEnhancementPassive>())
        {
            var ability = AbilityManager.CreateInstance<IHitEnhancementPassive>(enhancementAbility);
            if (ability != null)
                damage += ability.DamageModifier(this, victim, damageType, damage);
        }

        // bonus damage from affects
        var recompute = false;
        var aurasWithCharacterDamageIncreaseModifier = Auras.Where(x => x.IsValid).Where(x => x.Affects.OfType<ICharacterHitDamageModifierAffect>().Any()).ToArray();
        foreach (var aura in aurasWithCharacterDamageIncreaseModifier)
        {
            foreach (var characterDamageIncreaseModifierAffect in aura.Affects.OfType<ICharacterHitDamageModifierAffect>())
            {
                (damage, var wearOff) = characterDamageIncreaseModifierAffect.ModifyDamage(this, damageType, damage);
                if (wearOff)
                    recompute = true;
            }
        }
        if (recompute)
            Recompute();

        // other modifiers
        if (victim.Position <= Positions.Sleeping)
            damage *= 2;
        if (victim.Position <= Positions.Resting)
            damage = (damage * 3) / 2;
        if (hitModifier != null)
            damage = hitModifier.DamageModifier(wield, Level, damage);
        damage += DamRoll * learned / 100;
        if (damage <= 0)
            damage = 1; // at least one damage :)

        // perform damage
        DamageResults damageResult;
        if (hitModifier?.AbilityName != null)
            damageResult = victim.AbilityDamage(this, damage, damageType, hitModifier.DamageNoun ?? "hit", true);
        else
        {
            var damageNoun = wield == null ? NoWeaponDamageNoun : wield.DamageNoun;
            damageResult = victim.HitDamage(this, wield, damage, damageType, damageNoun ?? "hit", true);
        }

        if (Fighting != victim)
            return;

        // funky weapon ?
        if (damageResult == DamageResults.Done && wield != null)
        {
            var weaponEffects = WeaponEffectManager.WeaponEffectsByType<IPostHitDamageWeaponEffect>(wield).ToArray(); // some weapon effect can worn off when used -> wield.WeaponEffect collection could be modified while applying effects
            foreach (var postHitDamageWeaponEffect in weaponEffects)
            {
                var effect = WeaponEffectManager.CreateInstance<IPostHitDamageWeaponEffect>(postHitDamageWeaponEffect);
                effect?.Apply(this, victim, wield);

                if (Fighting != victim) // stop if not anymore fighting
                    return;
            }
        }

        // after hit affects ?
        if (damageResult == DamageResults.Done)
        {
            // any damage modifier on affects on victim ?
            var characterAfterHitAffects = victim.Auras.Where(x => x.IsValid).SelectMany(x => x.Affects.OfType<ICharacterAfterHitAffect>()).ToArray();
            foreach (var characterAfterHitAffect in characterAfterHitAffects)
            {
                characterAfterHitAffect.AfterHit(this, victim);
                if (Fighting != victim) // stop if not anymore fighting
                    return;
            }
        }
    }

    protected abstract void DeathPayoff(ICharacter? killer);

    protected void ResetCooldowns()
    {
        _cooldownsPulseLeft.Clear();
    }

    protected void DeleteInventory()
    {
        _inventory.Clear();
    }

    protected void DeleteEquipments()
    {
        _equipments.Clear();
    }

    protected void BuildEquipmentSlots()
    {
        // TODO: depend also on affects+...
        if (Race != null)
        {
            // TODO: take care of existing equipment (add only new slot, if slot is removed put equipment in inventory)
            foreach (var item in _equipments.Where(x => x.Item != null).Select(x => x.Item))
            {
                item!.ChangeEquippedBy(null, false);
                item!.ChangeContainer(this);
            }

            _equipments.Clear();
            _equipments.AddRange(Race.EquipmentSlots.Select(x => new EquippedItem(Logger, x)));
            //Recompute();
        }
        else
        {
            _equipments.Add(new EquippedItem(Logger, EquipmentSlots.Light));
            _equipments.Add(new EquippedItem(Logger, EquipmentSlots.Head));
            _equipments.Add(new EquippedItem(Logger, EquipmentSlots.Amulet)); // 2 amulets
            _equipments.Add(new EquippedItem(Logger, EquipmentSlots.Amulet));
            _equipments.Add(new EquippedItem(Logger, EquipmentSlots.Chest));
            _equipments.Add(new EquippedItem(Logger, EquipmentSlots.Cloak));
            _equipments.Add(new EquippedItem(Logger, EquipmentSlots.Waist));
            _equipments.Add(new EquippedItem(Logger, EquipmentSlots.Wrists)); // 2 wrists
            _equipments.Add(new EquippedItem(Logger, EquipmentSlots.Wrists));
            _equipments.Add(new EquippedItem(Logger, EquipmentSlots.Arms));
            _equipments.Add(new EquippedItem(Logger, EquipmentSlots.Hands));
            _equipments.Add(new EquippedItem(Logger, EquipmentSlots.Ring)); // 2 rings
            _equipments.Add(new EquippedItem(Logger, EquipmentSlots.Ring));
            _equipments.Add(new EquippedItem(Logger, EquipmentSlots.Legs));
            _equipments.Add(new EquippedItem(Logger, EquipmentSlots.Feet));
            _equipments.Add(new EquippedItem(Logger, EquipmentSlots.MainHand)); // 2 hands
            _equipments.Add(new EquippedItem(Logger, EquipmentSlots.OffHand));
            _equipments.Add(new EquippedItem(Logger, EquipmentSlots.Float));
        }
    }

    protected (IEquippedItem? equippedItem, SearchEquipmentSlotResults result) SearchEquipmentSlot(EquipmentSlots equipmentSlot, bool replace)
    {
        if (replace) // search empty slot, if not found, return first matching slot
        {
            var equippedItem = Equipments.FirstOrDefault(x => x.Slot == equipmentSlot && x.Item == null) ?? Equipments.FirstOrDefault(x => x.Slot == equipmentSlot);
            return (equippedItem, equippedItem != null ? SearchEquipmentSlotResults.Found : SearchEquipmentSlotResults.NotFound);
        }
        else
        {
            var equippedItem = Equipments.FirstOrDefault(x => x.Slot == equipmentSlot && x.Item == null);
            return (equippedItem, equippedItem != null ? SearchEquipmentSlotResults.Found : SearchEquipmentSlotResults.NotEmpty);
        }
    }

    protected (IEquippedItem? equippedItem, SearchEquipmentSlotResults result) SearchTwoHandedWeaponEquipmentSlot(bool replace)
    {
        if (replace)
        {
            // If size is giant, one mainhand is enough
            if (Size >= Sizes.Giant)
            {
                // search empty main hand
                var searchEmptyMainHandResult = SearchEquipmentSlot(EquipmentSlots.MainHand, false);
                if (searchEmptyMainHandResult.equippedItem != null)
                    return searchEmptyMainHandResult;
                // search main hand to replace
                var searchMainHandResult = SearchEquipmentSlot(EquipmentSlots.MainHand, true);
                if (searchMainHandResult.equippedItem != null)
                    return searchMainHandResult;
                return (null, SearchEquipmentSlotResults.NoFreeMainHand);
            }
            // mainhand + offhand (no replace)
            else
            {
                // search empty mainhand + offhand
                var searchEmptyMainHandResult = SearchEquipmentSlot(EquipmentSlots.MainHand, false);
                var searchEmptyOffhandResult = SearchOffhandEquipmentSlot(false);
                if (searchEmptyMainHandResult.equippedItem != null && searchEmptyOffhandResult.equippedItem != null)
                    return searchEmptyMainHandResult;
                // no autoreplace if no empty mainhand + offhand
                return (null, SearchEquipmentSlotResults.NoFreeMainAndOffHand);
            }
        }
        else
        {
            // If size is giant, one mainhand is enough
            if (Size >= Sizes.Giant)
            {
                var searchEmptyMainHandResult = SearchEquipmentSlot(EquipmentSlots.MainHand, false);
                if (searchEmptyMainHandResult.equippedItem != null)
                    return searchEmptyMainHandResult;
                return (null, SearchEquipmentSlotResults.NoFreeMainHand);
            }
            // Search empty mainhand + empty offhand
            else
            {
                var searchEmptyMainHandResult = SearchEquipmentSlot(EquipmentSlots.MainHand, false);
                var searchEmptyOffhandResult = SearchOffhandEquipmentSlot(false);
                if (searchEmptyMainHandResult.equippedItem != null && searchEmptyOffhandResult.equippedItem != null)
                    return searchEmptyMainHandResult;
                return (null, SearchEquipmentSlotResults.NoFreeMainAndOffHand);
            }
        }
    }

    protected (IEquippedItem? equippedItem, SearchEquipmentSlotResults result) SearchOneHandedWeaponEquipmentSlot(bool replace)
    {
        // Search empty mainhand, then empty offhand only if mainhand is not wielding a 2H
        if (replace)
        {
            // Search empty mainhand
            var searchEmptyMainHandResult = SearchEquipmentSlot(EquipmentSlots.MainHand, false);
            if (searchEmptyMainHandResult.equippedItem != null)
                return searchEmptyMainHandResult;
            // Search empty offhand
            var searchEmptyOffhandResult = SearchOffhandEquipmentSlot(false);
            if (searchEmptyOffhandResult.equippedItem != null)
                return searchEmptyOffhandResult;
            // If not empty mainhand/offhand, search a slot to replace
            var searchMainHandResult = SearchEquipmentSlot(EquipmentSlots.MainHand, true);
            if (searchMainHandResult.equippedItem != null)
                return searchMainHandResult;
            var searchOffHandResult = SearchOffhandEquipmentSlot(true);
            if (searchMainHandResult.equippedItem != null)
                return searchOffHandResult;
            return (null, SearchEquipmentSlotResults.NoFreeMainOrOffHand);
        }
        else
        {
            // Search empty mainhand
            var searchEmptyMainHandResult = SearchEquipmentSlot(EquipmentSlots.MainHand, false);
            if (searchEmptyMainHandResult.equippedItem != null)
                return searchEmptyMainHandResult;
            // If no empty mainhand found, search empty offhand
            return SearchOffhandEquipmentSlot(false);
        }
    }

    protected (IEquippedItem? equippedItem, SearchEquipmentSlotResults result) SearchOffhandEquipmentSlot(bool replace)
    {
        // 2H weapon take mainhand + offhand if non giant size, we have to reduce number of available offhand by the number of 2H weapon
        var countMainhand2HIfNonGiant = Size >= Sizes.Giant
            ? 0
            : Equipments.Count(x => x.Slot == EquipmentSlots.MainHand && x.Item?.WearLocation == WearLocations.Wield2H);
        if (replace)
        {
            var equippedItem = Equipments.Where(x => x.Slot == EquipmentSlots.OffHand && x.Item == null).ElementAtOrDefault(countMainhand2HIfNonGiant) ?? Equipments.Where(x => x.Slot == EquipmentSlots.OffHand).ElementAtOrDefault(countMainhand2HIfNonGiant);
            return (equippedItem, equippedItem != null ? SearchEquipmentSlotResults.Found : SearchEquipmentSlotResults.NoFreeOffHand);
        }
        else
        {
            var equippedItem = Equipments.Where(x => x.Slot == EquipmentSlots.OffHand && x.Item == null).ElementAtOrDefault(countMainhand2HIfNonGiant);
            return (equippedItem, equippedItem != null ? SearchEquipmentSlotResults.Found : SearchEquipmentSlotResults.NoFreeOffHand);
        }
    }

    protected abstract void RecomputeKnownAbilities();

    protected virtual void RecomputeCurrentResourceKinds()
    {
        // Get current resource kind from class if any, default resources otherwisse
        CurrentResourceKinds = ImmortalMode.HasFlag(ImmortalModeFlags.Infinite)
            ? Enum.GetValues<ResourceKinds>().ToList()
            : (Class?.CurrentResourceKinds(Shape) ?? ResourceKindsExtensions.DefaultAvailableResources).Union(ResourceKindsExtensions.MandatoryAvailableResources).ToList();
    }

    protected void SetBaseMaxResource(ResourceKinds resourceKind, int value, bool checkCurrent)
    {
        _baseMaxResources[resourceKind] = value;
        if (checkCurrent)
            _currentResources[resourceKind] = Math.Min(_currentResources[resourceKind], _baseMaxResources[resourceKind]);
    }

    protected void SetCurrentMaxResource(ResourceKinds resourceKinds, int value)
    {
        _currentMaxResources[resourceKinds] = value;
    }


    protected void SetBaseAttributes(CharacterAttributes attribute, int value, bool checkCurrent)
    {
        _baseAttributes[attribute] = value;
        if (checkCurrent)
            _currentAttributes[attribute] = Math.Min(_currentAttributes[attribute], _baseAttributes[attribute]);
    }

    protected void AddLearnedAbility(IAbilityUsage abilityUsage, bool naturalBorn)
    {
        if (!_learnedAbilities.ContainsKey(abilityUsage.Name))
        {
            var abilityLearned = new AbilityLearned(abilityUsage);
            _learnedAbilities.Add(abilityLearned.Name, abilityLearned);
            if (naturalBorn)
                abilityLearned.Update(1, 1, 100);
            else
            {
                var minLearned = abilityUsage.Level <= Level ? 1 : 0; // force minimum to be 1 if we are higher level than ability required level
                abilityLearned.SetLearned(minLearned);
            }
        }
    }

    protected void AddLearnedAbility(IAbilityLearned abilityLearned)
    {
        if (!_learnedAbilities.ContainsKey(abilityLearned.Name))
            _learnedAbilities.Add(abilityLearned.Name, abilityLearned);
    }

    protected void ApplyAuras(IEntity entity)
    {
        if (!entity.IsValid)
            return;
        foreach (IAura aura in entity.Auras.Where(x => x.IsValid))
        {
            foreach (ICharacterAffect affect in aura.Affects.OfType<ICharacterAffect>())
            {
                affect.Apply(this);
            }
        }
    }

    protected override void ResetAttributesAndResourcesAndFlags()
    {
        foreach(var characterAtttribute in Enum.GetValues<CharacterAttributes>())
            _currentAttributes[characterAtttribute] = _baseAttributes[characterAtttribute];
        foreach(var resourceKind in Enum.GetValues<ResourceKinds>())
            _currentMaxResources[resourceKind] = _baseMaxResources[resourceKind];
        Sex = BaseSex;
        Size = BaseSize;
        //Shape = BaseShape; TODO: uncomment when shape will be handled using aura
        CharacterFlags.Copy(BaseCharacterFlags);
        Immunities.Copy(BaseImmunities);
        Resistances.Copy(BaseResistances);
        Vulnerabilities.Copy(BaseVulnerabilities);
        ShieldFlags.Copy(BaseShieldFlags);
        BodyForms.Copy(BaseBodyForms);
        BodyParts.Copy(BaseBodyParts);
    }

    protected virtual void AddAurasFromBaseFlags()
    {
        // shields
        if (BaseShieldFlags.IsSet("Sanctuary") && !Auras.HasAffect<ICharacterShieldFlagsAffect>(x => x.Modifier.IsSet("Sanctuary")))
        {
            // TODO: code copied from sanctuary spell (except duration and aura flags) use effect ??
            var sanctuaryAbilityDefinition = AbilityManager["Sanctuary"];
            AuraManager.AddAura(this, sanctuaryAbilityDefinition?.Name ?? "sanctuary", this, Level, AuraFlags.Permanent, false,
                new CharacterShieldFlagsAffect { Modifier = new ShieldFlags("Sanctuary"), Operator = AffectOperators.Or },
                AffectManager.CreateInstance("Sanctuary"));
        }
        if (BaseShieldFlags.IsSet("ProtectGood") && !Auras.HasAffect<ICharacterShieldFlagsAffect>(x => x.Modifier.IsSet("Protection Good")))
        {
            // TODO: code copied from protection good spell (except duration and aura flags) use effect ??
            var sanctuaryAbilityDefinition = AbilityManager["Protection Good"];
            AuraManager.AddAura(this, sanctuaryAbilityDefinition?.Name ?? "protection good", this, Level, AuraFlags.Permanent, false,
                new CharacterShieldFlagsAffect { Modifier = new ShieldFlags("ProtectGood"), Operator = AffectOperators.Or },
                AffectManager.CreateInstance("ProtectGood"));
        }
        if (BaseShieldFlags.IsSet("ProtectEvil") && !Auras.HasAffect<ICharacterShieldFlagsAffect>(x => x.Modifier.IsSet("Protection Evil")))
        {
            // TODO: code copied from protection evil spell (except duration and aura flags) use effect ??
            var sanctuaryAbilityDefinition = AbilityManager["Protection Evil"];
            AuraManager.AddAura(this, sanctuaryAbilityDefinition?.Name ?? "Protection Evil", this, Level, AuraFlags.Permanent, false,
                new CharacterShieldFlagsAffect { Modifier = new ShieldFlags("ProtectEvil"), Operator = AffectOperators.Or },
                AffectManager.CreateInstance("ProtectEvil"));
        }
        // TODO: other shields like FireShield, IceShield, LightningShield
        if (BaseCharacterFlags.IsSet("Haste") && !Auras.HasAffect<ICharacterFlagsAffect>(x => x.Modifier.IsSet("Haste")))
        {
            // TODO: code copied from haste spell (except duration and aura flags) use effect ??
            var hasteAbilityDefinition = AbilityManager["Haste"];
            var modifier = 1 + (Level >= 18 ? 1 : 0) + (Level >= 25 ? 1 : 0) + (Level >= 32 ? 1 : 0);
            AuraManager.AddAura(this, hasteAbilityDefinition?.Name ?? "Haste", this, Level, AuraFlags.Permanent, false,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Dexterity, Modifier = modifier, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = new CharacterFlags("Haste"), Operator = AffectOperators.Or },
                new CharacterAdditionalHitAffect { AdditionalHitCount = 1 });
        }
    }

    protected StringBuilder AppendPositionFurniture(StringBuilder sb, string verb, IItemFurniture? furniture)
    {
        if (furniture == null)
            sb.AppendFormat(" is {0} here.", verb);
        else
            furniture.AppendPosition(sb, verb);
        return sb;
    }

    protected void MergeAbilities(IEnumerable<IAbilityUsage> abilities, bool naturalBorn)
    {
        // If multiple identical abilities, keep only one with lowest level
        foreach (var abilityUsage in abilities)
        {
            MergeAbility(abilityUsage, naturalBorn);
        }
    }

    protected void MergeAbility(IAbilityUsage abilityUsage, bool naturalBorn)
    {
        var (_, abilityLearned) = GetAbilityLearnedAndPercentage(abilityUsage.Name);
        if (abilityLearned != null)
        {
            var abilityLevel = Math.Min(abilityUsage.Level, abilityLearned.Level);
            var abilityRating = Math.Min(abilityUsage.Rating, abilityLearned.Rating);
            var minLearned = abilityLevel <= Level ? 1 : 0; // force minimum to be 1 if we are higher level than ability required level
            var learned = Math.Max(minLearned, Math.Max(abilityUsage.MinLearned, abilityLearned.Learned));
            //Logger.LogDebug("Merging KnownAbility with AbilityUsage for {0} Ability {1}", DebugName, abilityUsage.Ability.Name);
            abilityLearned.Update(abilityLevel, abilityRating, learned);
            // TODO: what should be we if multiple resource kind or operator ?
        }
        else
        {
            Logger.LogDebug("Adding AbilityLearned from AbilityUsage for {name} Ability {abilityUsageName}", DebugName, abilityUsage.Name);
            AddLearnedAbility(abilityUsage, naturalBorn);
        }
    }

    protected DamageResults Damage(ICharacter source, int damage, SchoolTypes damageType, DamageSources damageSource, string? damageNoun, bool display) // 'this' is dealt damage by 'source'
    {
        if (this[ResourceKinds.HitPoints] <= 0)
            return DamageResults.AlreadyDead;

        // check safe + start fight
        if (this != source)
        {
            // Certain attacks are forbidden.
            // Most other attacks are returned.
            var safeResult = IsSafe(source);
            if (safeResult != null)
            {
                source.Send(safeResult);
                return DamageResults.Safe;
            }
            // TODO: check_killer

            // start fight between this and source
            StartFightAndFightBack(source);

            // more charm stuff
            if (this is INonPlayableCharacter npcVictim && npcVictim.Master == source) // TODO: no more cast like this
                npcVictim.ChangeMaster(null);
        }

        // inviso attack
        if (source.CharacterFlags.IsSet("Invisible"))
        {
            source.RemoveBaseCharacterFlags(false, "Invisible");
            source.RemoveAuras(x => StringCompareHelpers.StringEquals(x.AbilityName, "Invisibility"), false, true); // TODO: do this differently
            source.Recompute(); // force a recompute to check if there is something special that gives invis
            // if not anymore invis
            if (!source.CharacterFlags.IsSet("Invisible"))
                source.Act(ActOptions.ToRoom, "{0:N} fades into existence.", source);
        }

        // damage modifiers

        // damage reduction
        if (damage > 35)
            damage = (damage - 35) / 2 + 35;
        if (damage > 80)
            damage = (damage - 80) / 2 + 80;

        // drunk reduction
        if (damage > 1)
            damage = CharacterTypeSpecificDamageModifier(damage);

        if (damage > 1)
        {
            // any damage modifier on victim (this) affects ?
            var characterDamageDecreaseModifierAffects = Auras.Where(x => x.IsValid).SelectMany(x => x.Affects.OfType<ICharacterDamageModifierAffect>()).ToArray();
            foreach (var characterDamageDecreaseModifierAffect in characterDamageDecreaseModifierAffects)
            {
                damage = characterDamageDecreaseModifierAffect.ModifyDamage(source, this, damageType, damageSource, damage);
            }
        }

        // apply resistances
        var resistanceLevel = ResistanceCalculator.CheckResistance(this, damageType);
        switch (resistanceLevel)
        {
            case ResistanceLevels.Immune:
                damage = 0;
                break;
            case ResistanceLevels.Resistant:
                damage = 2 * damage / 3;
                break;
            case ResistanceLevels.Vulnerable:
                damage =  3 * damage / 2;
                break;
        }

        // display
        if (display)
        {
            string phraseOther; // {0}: source {1}: victim {2}: damage display {3}: damage noun {4}: damage value
            string phraseSource; // {0}: victim {1}: damage display {2}: damage noun {3}: damage value
            string phraseVictim = string.Empty; // {0}: source {1}: damage display {2}: damage noun {3}: damage value
            // build phrases
            if (string.IsNullOrWhiteSpace(damageNoun))
            {
                if (this == source)
                {
                    phraseOther = "{0:N} {2} {0:f}.[{4}]";
                    phraseSource = "You {1} yourself.[{3}]";
                }
                else
                {
                    phraseOther = "{0:N} {2} {1}.[{4}]";
                    phraseSource = "You {1} {0}.[{3}]";
                    phraseVictim = "{0:N} {1} you.[{3}]";
                }
            }
            else
            {
                if (resistanceLevel == ResistanceLevels.Immune)
                {
                    if (this == source)
                    {
                        phraseOther = "{0:N} is unaffected by {0:s} own {2}.";
                        phraseSource = "Luckily, you are immune to that.";
                    }
                    else
                    {
                        phraseOther = "{1:N} is unaffected by {0:p} {2}!";
                        phraseSource = "{0} is unaffected by your {1}!";
                        phraseVictim = "{0:p} {1} is powerless against you.";
                    }
                }
                else
                {
                    if (this == source)
                    {
                        phraseOther = "{0:P} {3} {2} {0:m}.[{4}]";
                        phraseSource = "Your {2} {1} you.[{3}]";
                    }
                    else
                    {
                        phraseOther = "{0:P} {3} {2} {1}.[{4}]";
                        phraseSource = "Your {2} {1} {0}.[{3}]";
                        phraseVictim = "{0:P} {2} {1} you.[{3}]";
                    }
                }
            }

            // display phrases
            var damagePhraseSelf = StringHelpers.DamagePhraseSelf(damage);
            var damagePhraseOther = StringHelpers.DamagePhraseOther(damage);
            if (this == source)
            {
                source.Act(ActOptions.ToRoom, phraseOther, source, this, damagePhraseOther, damageNoun ?? "", damage);
                source.Act(ActOptions.ToCharacter, phraseSource, this, damagePhraseSelf, damageNoun ?? "", damage);
            }
            else
            {
                source.ActToNotVictim(this, phraseOther, source, this, damagePhraseOther, damageNoun ?? "", damage);
                source.Act(ActOptions.ToCharacter, phraseSource, this, damagePhraseOther, damageNoun ?? "", damage);
                Act(ActOptions.ToCharacter, phraseVictim, source, damagePhraseOther, damageNoun ?? "", damage);
            }
        }

        // no damage done, stops here
        if (damage <= 0)
            return DamageResults.NoDamage;

        // generate aggro amount depending on received damage
        AggroManager.OnReceiveDamage(source, this, damage);

        // hurt the victim
        _currentResources[(int)ResourceKinds.HitPoints] -= damage; // don't use UpdateHitPoints because value will not be allowed to go below 0

        // check if can die
        if (CannotDie
            && this[ResourceKinds.HitPoints] < 1)
            _currentResources[(int)ResourceKinds.HitPoints] = 1;

        // TODO: in original code, position is updating depending on hitpoints and a specific message depending on position is displayed (check update_pos)
        var isDead = this[ResourceKinds.HitPoints] <= 0;

        // handle dead people
        if (isDead)
        {
            StopFighting(true); // StopFighting will set position to standing
            RawKilled(source, true); // group group_gain + dying penalty + raw_kill

            return DamageResults.Killed;
        }
        else
        {
            if (damage > MaxResource(ResourceKinds.HitPoints) / 4)
                Send("That really did HURT!");
            if (this[ResourceKinds.HitPoints] < MaxResource(ResourceKinds.HitPoints) / 4)
                Send("You sure are BLEEDING!");
        }

        if (Position == Positions.Sleeping)
            StopFighting(false); // StopFighting will set position to standing

        // rage generation
        OnDamageReceived(damage, damageSource);
        source.OnDamagePerformed(damage, damageSource);

        if (this == source)
            return DamageResults.Done;

        // TODO: take care of link-dead people

        return DamageResults.Done;
    }

    protected void StartFightAndFightBack(ICharacter victim)
    {
        // Make sure we're fighting the victim.
        if (Position >= Positions.Sleeping && !IsStunned)
        {
            if (Fighting == null)
                StartFighting(victim);
            StandUpInCombatIfPossible();
        }
        // Tell the victim to fight back!
        if (victim.Position >= Positions.Sleeping && !victim.IsStunned)
        {
            // TODO: if victim.Timer <= 4 -> victim.Position = Positions.Fighting
            if (victim.Fighting == null)
                victim.StartFighting(this);
            StandUpInCombatIfPossible();
        }
    }

    private void OnDamageReceived(int damage, DamageSources damageSource)
    {
        RageGenerator.GenerateRageFromIncomingDamage(this, damage, damageSource);
    }

    //
    protected virtual IAbilityLearned? GetAbilityLearned(string abilityName)
        => _learnedAbilities.GetValueOrDefault(abilityName);
}
