using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mud.Common;
using Mud.Container;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Ability;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Common;
using Mud.Server.Entity;
using Mud.Server.Helpers;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Table;
using Mud.Server.Random;
using Mud.Server.Flags;

namespace Mud.Server.Character
{
    public abstract class CharacterBase : EntityBase, ICharacter
    {
        private const int MinAlignment = -1000;
        private const int MaxAlignment = 1000;

        private readonly List<IItem> _inventory;
        private readonly List<IEquippedItem> _equipments;
        private readonly int[] _baseAttributes;
        private readonly int[] _currentAttributes;
        private readonly int[] _maxResources;
        private readonly int[] _currentResources;
        private readonly Dictionary<string, int> _cooldownsPulseLeft;
        private readonly Dictionary<string, IAbilityLearned> _learnedAbilities;

        protected IRandomManager RandomManager => DependencyContainer.Current.GetInstance<IRandomManager>();
        protected ITableValues TableValues => DependencyContainer.Current.GetInstance<ITableValues>();
        protected IRoomManager RoomManager => DependencyContainer.Current.GetInstance<IRoomManager>();
        protected IItemManager ItemManager => DependencyContainer.Current.GetInstance<IItemManager>();
        protected ICharacterManager CharacterManager => DependencyContainer.Current.GetInstance<ICharacterManager>();
        protected IAuraManager AuraManager => DependencyContainer.Current.GetInstance<IAuraManager>();
        protected IWeaponEffectManager WeaponEffectManager => DependencyContainer.Current.GetInstance<IWeaponEffectManager>();
        protected IWiznet Wiznet => DependencyContainer.Current.GetInstance<IWiznet>();

        protected CharacterBase(Guid guid, string name, string description)
            : base(guid, name, description)
        {
            _inventory = new List<IItem>();
            _equipments = new List<IEquippedItem>();
            _baseAttributes = new int[EnumHelpers.GetCount<CharacterAttributes>()];
            _currentAttributes = new int[EnumHelpers.GetCount<CharacterAttributes>()];
            _maxResources = new int[EnumHelpers.GetCount<ResourceKinds>()];
            _currentResources = new int[EnumHelpers.GetCount<ResourceKinds>()];
            _cooldownsPulseLeft = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            _learnedAbilities = new Dictionary<string, IAbilityLearned>(StringComparer.InvariantCultureIgnoreCase); // handled by RecomputeKnownAbilities

            Position = Positions.Standing;
            Form = Forms.Normal;

            CharacterFlags = new CharacterFlags();
            BodyParts = new BodyParts();
            BodyForms = new BodyForms();
            Immunities = new IRVFlags();
            Resistances = new IRVFlags();
            Vulnerabilities = new IRVFlags();
        }

        #region ICharacter

        #region IEntity

        // TODO: override RelativeDescription ?

        public override bool ChangeIncarnation(IAdmin admin)
        {
            bool result = base.ChangeIncarnation(admin);
            if (result)
            {
                RecomputeKnownAbilities();
                Recompute();
            }
            return result;
        }

        public override void OnRemoved()
        {
            base.OnRemoved();

            // Leave follower
            Leader?.RemoveFollower(this);

            // Release followers
            foreach (ICharacter follower in CharacterManager.Characters.Where(x => x.Leader == this))
                RemoveFollower(follower);
        }

        #endregion

        #region IContainer

        public IEnumerable<IItem> Content => _inventory.Where(x => x.IsValid);

        public bool PutInContainer(IItem obj)
        {
            //if (obj.ContainedInto != null)
            //{
            //    Log.Default.WriteLine(LogLevels.Error, "PutInContainer: {0} is already in container {1}.", obj.DebugName, obj.ContainedInto.DebugName);
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

        public ICharacter Fighting { get; protected set; }

        public IEnumerable<IEquippedItem> Equipments => _equipments;
        public IEnumerable<IItem> Inventory => Content;
        public virtual int MaxCarryWeight => TableValues.CarryBonus(this) * 10 + Level * 25;
        public virtual int MaxCarryNumber => Equipments.Count() + 2 * this[BasicAttributes.Dexterity] + Level;
        public int CarryWeight => Inventory.Sum(x => x.Weight) + Equipments.Where(x => x.Item != null).Sum(x => x.Item.Weight);
        public int CarryNumber => Inventory.Sum(x => x.CarryCount) + Equipments.Where(x => x.Item != null).Sum(x => x.Item.CarryCount);

        // Money
        public long SilverCoins { get; protected set; }
        public long GoldCoins { get; protected set; }

        public (long silver, long gold) DeductCost(long cost)
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
                Log.Default.WriteLine(LogLevels.Error, "DeductCost: gold {0} < 0", GoldCoins);
                GoldCoins = 0;
            }
            if (SilverCoins < 0)
            {
                Log.Default.WriteLine(LogLevels.Error, "DeductCost: silver {0} < 0", SilverCoins);
                SilverCoins = 0;
            }

            return (silver, gold);
        }


        // Furniture (sleep/sit/stand)
        public IItemFurniture Furniture { get; protected set; }

        // Position
        public Positions Position { get; protected set; }
        public int Stunned { get; protected set; }

        // Class/Race
        public IClass Class { get; protected set; }
        public IRace Race { get; protected set; }

        // Attributes
        public int Level { get; protected set; }
        public int HitPoints { get; protected set; }
        public int MaxHitPoints => _currentAttributes[(int)CharacterAttributes.MaxHitPoints];
        public int MovePoints { get; protected set; }
        public int MaxMovePoints => _currentAttributes[(int)CharacterAttributes.MaxMovePoints];

        public ICharacterFlags BaseCharacterFlags { get; protected set; }
        public ICharacterFlags CharacterFlags { get; protected set; }

        public IIRVFlags BaseImmunities { get; protected set; }
        public IIRVFlags Immunities { get; protected set; }
        public IIRVFlags BaseResistances { get; protected set; }
        public IIRVFlags Resistances { get; protected set; }
        public IIRVFlags BaseVulnerabilities { get; protected set; }
        public IIRVFlags Vulnerabilities { get; protected set; }

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
            get
            {
                int index = (int)attribute;
                if (index >= _currentAttributes.Length)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Trying to get current attribute for attribute {0} (index {1}) but current attribute length is smaller", attribute, index);
                    return 0;
                }
                return _currentAttributes[index];
            }
            protected set 
            {
                int index = (int)attribute;
                if (index >= _currentAttributes.Length)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Trying to set current attribute for attribute {0} (index {1}) but current attribute length is smaller", attribute, index);
                    return;
                }
                _currentAttributes[index] = value;
            }
        }

        public int this[BasicAttributes attribute] => this[(CharacterAttributes)attribute];
        public int this[Armors armor] => this[(CharacterAttributes)armor] + (Position > Positions.Sleeping ? TableValues.DefensiveBonus(this) : 0);
        public int HitRoll => this[CharacterAttributes.HitRoll] + TableValues.HitBonus(this);
        public int DamRoll => this[CharacterAttributes.DamRoll] + TableValues.DamBonus(this);

        public int this[ResourceKinds resource]
        {
            get 
            {
                int index = (int)resource;
                if (index >= _currentResources.Length)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Trying to get current resource for resource {0} (index {1}) but current resource length is smaller", resource, index);
                    return 0;
                }
                return _currentResources[index];
            }
            protected set
            {
                int index = (int)resource;
                if (index >= _currentResources.Length)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Trying to set current resource for resource {0} (index {1}) but current resource length is smaller", resource, index);
                    return;
                }
                _currentResources[index] = value;
            }
        }

        public IEnumerable<ResourceKinds> CurrentResourceKinds { get; private set; }

        public IBodyForms BaseBodyForms { get; protected set; }
        public IBodyForms BodyForms { get; protected set; }
        public IBodyParts BaseBodyParts { get; protected set; }
        public IBodyParts BodyParts { get; protected set; }

        // Abilities
        public IEnumerable<IAbilityLearned> LearnedAbilities => _learnedAbilities.Values;

        // Form
        public Forms Form { get; private set; }

        // Followers
        public ICharacter Leader { get; protected set; }

        public void AddFollower(ICharacter character)
        {
            if (character.Leader == this)
                return;
            // check if A->B->C->A
            ICharacter next = Leader;
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
            if (character is INonPlayableCharacter npcCharacter)
            {
                npcCharacter.RemoveBaseCharacterFlags(true, "Charm");
                npcCharacter.RemoveAuras(x => x.AbilityName == "Charm Person", true);
                npcCharacter.ChangeMaster(null);
            }
        }

        public void ChangeLeader(ICharacter character)
        {
            Leader = character;
        }

        // Group
        public virtual bool IsSameGroupOrPet(ICharacter character)
        {
            IPlayableCharacter pcCh1 = this as IPlayableCharacter;
            IPlayableCharacter pcCh2 = character as IPlayableCharacter;
            return (pcCh1 != null && pcCh1.IsSameGroupOrPet(character)) || (pcCh2 != null && pcCh2.IsSameGroupOrPet(this));
        }

        // Act
        // IFormattable cannot be used because formatting depends on who'll receive the message (CanSee check)
        public void Act(ActOptions option, string format, params object[] arguments)
        {
            //
            IEnumerable<ICharacter> targets = GetActTargets(option);
            //
            foreach (ICharacter target in targets)
            {
                string phrase = FormatActOneLine(target, format, arguments);
                target.Send(phrase);
            }
        }

        public void ActToNotVictim(ICharacter victim, string format, params object[] arguments) // to everyone except this and victim
        {
            foreach (ICharacter to in Room.People.Where(x => x != this && x != victim))
            {
                string phrase = FormatActOneLine(to, format, arguments);
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

        // Money
        public void UpdateMoney(long silverCoins, long goldCoins)
        {
            SilverCoins = Math.Max(0, SilverCoins + silverCoins);
            GoldCoins = Math.Max(0, GoldCoins + goldCoins);
        }

        // Furniture
        public bool ChangeFurniture(IItemFurniture furniture)
        {
            Furniture = furniture;
            return true;
        }

        // Position
        public bool ChangePosition(Positions position)
        {
            Position = position;
            return true;
        }

        public void ChangeStunned(int fightRound)
        {
            Stunned = fightRound;
            return;
        }

        // Visibility
        public bool CanSee(ICharacter victim)
        {
            if ((this as IPlayableCharacter)?.IsImmortal == true)
                return true;
            if (victim == this)
                return true;
            // blind
            if (CharacterFlags.IsSet("Blind"))
                return false;
            // infrared + dark
            if (!CharacterFlags.IsSet("Infrared") && Room.IsDark)
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
                var sneakInfo = victim.GetAbilityLearnedInfo("Sneak"); // this can be slow
                int chance = sneakInfo.percentage;
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

        public virtual bool CanSee(IItem item)
        {
            if ((this as IPlayableCharacter)?.IsImmortal == true)
                return true;

            // visible death
            if (item.ItemFlags.IsSet("VisibleDeath"))
                return false;

            // blind except if potion
            if (CharacterFlags.IsSet("Blind") && item is IItemPotion)
                return false;

            // Light
            if (item is IItemLight light && light.IsLighten)
                return true;

            // invis
            if (item.ItemFlags.IsSet("Invis")
                && !CharacterFlags.IsSet("DetectInvis"))
                return false;

            // quest item
            IPlayableCharacter pc = this as IPlayableCharacter;
            if (item is IItemQuest questItem && (pc == null || !questItem.IsQuestObjective(pc)))
                return false;

            // glow
            if (item.ItemFlags.IsSet("Glowing"))
                return true;

            // room dark
            if (Room.IsDark && !CharacterFlags.IsSet("Infrared"))
                return false;

            return true;
        }

        public bool CanSee(IExit exit)
        {
            if ((this as IPlayableCharacter)?.IsImmortal == true)
                return true;
            if (CharacterFlags.IsSet("DetectHidden"))
                return true;
            //if (exit.ExitFlags.HasFlag(ExitFlags.IsHidden))
            //    return false;
            return true; // TODO: Hidden
        }

        public bool CanSee(IRoom room)
        {
            // infrared + dark
            if (room.IsDark && !CharacterFlags.IsSet("Infrared"))
                return false;
            //        if (IS_SET(pRoomIndex->room_flags, ROOM_IMP_ONLY)
            //&& get_trust(ch) < MAX_LEVEL)
            //            return FALSE;

            if (room.RoomFlags.IsSet("GodsOnly")
                && (this as IPlayableCharacter)?.IsImmortal != true)
                return false;

            //        if (IS_SET(pRoomIndex->room_flags, ROOM_HEROES_ONLY)
            //        && !IS_IMMORTAL(ch))
            //            return FALSE;

            if (room.RoomFlags.IsSet("NewbiesOnly") && Level > 5 && (this as IPlayableCharacter)?.IsImmortal != true)
                return false;

            //        if (!IS_IMMORTAL(ch) && pRoomIndex->clan && ch->clan != pRoomIndex->clan)
            //            return FALSE;

            //        return TRUE;

            return true; // TODO
        }

        // Attributes
        public int BaseAttribute(CharacterAttributes attribute)
        {
            int index = (int)attribute;
            if (index >= _baseAttributes.Length)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to get base attribute for attribute {0} (index {1}) but base attribute length is smaller", attribute, index);
                return 0;
            }
            return _baseAttributes[index];
        }

        public void UpdateBaseAttribute(CharacterAttributes attribute, int amount)
        {
            int index = (int)attribute;
            if (index >= _baseAttributes.Length)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to set base attribute for attribute {0} (index {1}) but base attribute length is smaller", attribute, index);
                return;
            }
            _baseAttributes[index] += amount;
            if (index >= _currentAttributes.Length)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to set base current attribute for attribute {0} (index {1}) but current attribute length is smaller", attribute, index);
                return;
            }
            _currentAttributes[index] = Math.Min(_currentAttributes[index], _baseAttributes[index]);
        }

        public int MaxResource(ResourceKinds resourceKind)
        {
            int index = (int)resourceKind;
            if (index >= _maxResources.Length)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to get max resource for resource {0} (index {1}) but max resource length is smaller", resourceKind, index);
                return 0;
            }
            return _maxResources[index];
        }

        public void UpdateMaxResource(ResourceKinds resourceKind, int amount)
        {
            int index = (int)resourceKind;
            if (index >= _maxResources.Length)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to get max resource for resource {0} (index {1}) but max resource length is smaller", resourceKind, index);
                return;
            }
            _maxResources[index] += amount;
            if (index >= _currentResources.Length)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to set current resource for resource {0} (index {1}) but current resource length is smaller", resourceKind, index);
                return;
            }
            _currentResources[index] = Math.Min(_currentResources[index], _maxResources[index]);
        }

        public void UpdateResource(ResourceKinds resourceKind, int amount)
        {
            this[resourceKind] = (this[resourceKind] + amount).Range(0, _maxResources[(int)resourceKind]);
        }

        public void UpdateHitPoints(int amount)
        {
            HitPoints = (HitPoints + amount).Range(0, MaxHitPoints);
        }

        public void UpdateMovePoints(int amount)
        {
            MovePoints = (MovePoints + amount).Range(0, MaxMovePoints);
        }

        public void UpdateAlignment(int amount) 
        {
            Alignment = (Alignment + amount).Range(MinAlignment, MaxAlignment);
            // impact on equipment
            bool recompute = false;
            foreach (var item in Equipments.Where(x => x.Item != null).Select(x => x.Item))
            {
                if ((IsEvil && item.ItemFlags.IsSet("AntiEvil"))
                    || (IsGood && item.ItemFlags.IsSet("AntiGood"))
                    || (IsNeutral && item.ItemFlags.IsSet("AntiNeutral")))
                {
                    Act(ActOptions.ToAll, "{0:N} {0:b} zapped by {1}.", this, item);
                    item.ChangeEquippedBy(null, false);
                    item.ChangeContainer(Room);
                    recompute = true;
                }
            }

            if (recompute)
            {
                Recompute();
                Room.Recompute();
            }
        }

        public void Regen()
        {
            // Hp/Move
            (int hitGain, int moveGain, int manaGain, int psyGain) gains = RegenBaseValues();
            int hitGain = gains.hitGain;
            int moveGain = gains.moveGain;
            int manaGain = gains.manaGain;
            int psyGain = gains.psyGain;

            hitGain = hitGain * Room.HealRate / 100;
            manaGain = manaGain * Room.ResourceRate / 100;
            psyGain = psyGain * Room.ResourceRate / 100;

            if (Furniture != null && Furniture?.HealBonus != 0)
            {
                hitGain = (hitGain * Furniture.HealBonus) / 100;
                moveGain = (moveGain * Furniture.HealBonus) / 100;
            }

            if (Furniture != null && Furniture?.ResourceBonus != 0)
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
            HitPoints = Math.Min(HitPoints + hitGain, MaxHitPoints);
            MovePoints = Math.Min(MovePoints + moveGain, MaxMovePoints);
            UpdateResource(ResourceKinds.Mana, manaGain);
            UpdateResource(ResourceKinds.Psy, psyGain);

            // Other resources
        }

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

        // Form
        public bool ChangeForm(Forms form)
        {
            if (form == Form)
                return false;

            if (form == Forms.Normal)
                Send("You regain your normal form");

            Form = form;

            RecomputeKnownAbilities();
            Recompute();
            RecomputeCurrentResourceKinds();

            // Start values  TODO: depends on class ?
            this[ResourceKinds.Mana] = 100;
            this[ResourceKinds.Psy] = 0;

            return true;
        }

        // Recompute
        public override void ResetAttributes()
        {
            for (int i = 0; i < _baseAttributes.Length; i++)
                _currentAttributes[i] = _baseAttributes[i];
            Sex = BaseSex;
            Size = BaseSize;
            CharacterFlags.Copy(BaseCharacterFlags);
            Immunities.Copy(BaseImmunities);
            Resistances.Copy(BaseResistances);
            Vulnerabilities.Copy(BaseVulnerabilities);
            BodyForms.Copy(BaseBodyForms);
            BodyParts.Copy(BaseBodyParts);
        }

        public override void Recompute()
        {
            Log.Default.WriteLine(LogLevels.Debug, "CharacterBase.Recompute: {0}", DebugName);

            // Reset current attributes
            ResetAttributes();

            // 1) Apply room auras
            if (Room != null)
                ApplyAuras(Room);

            // 2) Apply equipment auras
            foreach (IEquippedItem equipment in Equipments.Where(x => x.Item != null))
                ApplyAuras(equipment.Item);

            // 3) Apply equipment armor
            foreach (IEquippedItem equippedItem in Equipments.Where(x => x.Item is IItemArmor || x.Item is IItemShield))
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

            // 5) Check if weapon can still be wielded
            if (this is IPlayableCharacter)
            {
                bool shouldRecompute = false;
                foreach (IEquippedItem equipedItem in Equipments.Where(x => x.Slot == EquipmentSlots.MainHand && x.Item is IItemWeapon))
                {
                    if (equipedItem.Item is IItemWeapon weapon) // always true
                    {
                        if (!weapon.CanWield(this))
                        {
                            Act(ActOptions.ToAll, "{0:N} can't use {1} anymore.", this, weapon);
                            weapon.ChangeEquippedBy(null, false);
                            weapon.ChangeContainer(this);
                            shouldRecompute = true;
                        }
                    }
                }
                if (shouldRecompute)
                    Recompute();
            }

            // Keep attributes in valid range
            HitPoints = Math.Min(HitPoints, MaxHitPoints);
            MovePoints = Math.Min(MovePoints, MaxMovePoints);
            for (int i = 0; i < _currentResources.Length; i++)
                _currentResources[i] = Math.Min(_currentResources[i], _maxResources[i]);
        }

        // Move
        public bool Move(ExitDirections direction, bool following, bool forceFollowers)
        {
            IRoom fromRoom = Room;

            //TODO exit flags such as climb, ...

            if (this is INonPlayableCharacter npc && npc.Master != null && npc.Master.Room == Room) // TODO: no more cast like this
            {
                // Slave cannot leave a room without Master
                Send("What? And leave your beloved master?");
                return false;
            }

            // Under certain circumstances, direction can be modified (Drunk anyone?)
            direction = ChangeDirectionBeforeMove(direction, fromRoom);

            // Get exit and destination room
            IExit exit = fromRoom[direction];
            IRoom toRoom = exit?.Destination;

            // Check if existing exit
            if (exit == null || toRoom == null)
            {
                Send("You almost goes {0}, but suddenly realize that there's no exit there.", direction);
                Act(ActOptions.ToRoom, "{0} looks like {0:e}'s about to go {1}, but suddenly stops short and looks confused.", this, direction);
                return false;
            }
            // Closed ?
            if (exit.IsClosed && (!CharacterFlags.IsSet("PassDoor") || exit.ExitFlags.HasFlag(ExitFlags.NoPass)))
            {
                Act(ActOptions.ToCharacter, "The {0} is closed.", exit);
                return false;
            }
            // Private ?
            if (toRoom.IsPrivate)
            {
                Send("That room is private right now.");
                return false;
            }
            // Size
            if (toRoom.MaxSize.HasValue && toRoom.MaxSize.Value < Size)
            {
                Send("You're too huge to go that direction.");
                return false;
            }
            // Flying
            if ((fromRoom.SectorType == SectorTypes.Air || toRoom.SectorType == SectorTypes.Air)
                && (!CharacterFlags.IsSet("Flying") && (this as IPlayableCharacter)?.IsImmortal != true))
            {
                Send("You can't fly.");
                return false;
            }
            // Water
            if ((fromRoom.SectorType == SectorTypes.WaterSwim || toRoom.SectorType == SectorTypes.WaterSwim)
                && (this as IPlayableCharacter)?.IsImmortal != true
                && !CharacterFlags.IsSet("Swim")
                && !CharacterFlags.IsSet("Flying")
                && !Inventory.OfType<IItemBoat>().Any()) // TODO: WalkOnWater
            {
                Send("You need a boat to go there, or be swimming, flying or walking on water.");
                return false;
            }
            // Water no swim or underwater
            if ((fromRoom.SectorType == SectorTypes.WaterNoSwim || toRoom.SectorType == SectorTypes.WaterNoSwim)
                && (this as IPlayableCharacter)?.IsImmortal != true
                && !CharacterFlags.IsSet("Flying")) // TODO: WalkOnWater
            {
                Send("You need to be flying or walking on water.");
                return false;
            }

            // Check move points left or drunk special phrase
            bool beforeMove = BeforeMove(direction, fromRoom, toRoom);
            if (!beforeMove)
                return false;

            //
            if (!CharacterFlags.IsSet("Sneak"))
                Act(ActOptions.ToRoom, "{0} leaves {1}.", this, direction);
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

            if (portal.PortalFlags.HasFlag(PortalFlags.Closed))
            {
                Send("You can't seem to find a way in.");
                return false;
            }

            if ((portal.PortalFlags.HasFlag(PortalFlags.NoCurse) && CharacterFlags.IsSet("Curse"))
                || Room.RoomFlags.IsSet("NoRecall"))
            {
                Send("Something prevents you from leaving...");
                return false;
            }

            // Default destination is portal stored destination
            IRoom destination = portal.Destination;
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
                || !CanSee(destination)
                || destination.IsPrivate
                || destination.RoomFlags.IsSet("Private"))
            {
                Act(ActOptions.ToCharacter, "{0:N} doesn't seem to go anywhere.", portal);
                return false;
            }

            if (this is INonPlayableCharacter npc && npc.ActFlags.IsSet("Aggressive") && destination.RoomFlags.IsSet("Law"))
            {
                Send("Something prevents you from leaving...");
                return false;
            }

            if (destination.MaxSize.HasValue && destination.MaxSize.Value < Size)
            {
                Send("You're too huge to enter that.");
                return false;
            }

            IRoom wasRoom = Room;

            Act(ActOptions.ToRoom, "{0:N} steps into {1}.", this, portal);
            Act(ActOptions.ToCharacter, "You walk through {0} and find yourself somewhere else...", portal);

            ChangeRoom(destination, false);

            // take portal along
            if (portal.PortalFlags.HasFlag(PortalFlags.GoWith) && portal.ContainedInto is IRoom)
            {
                portal.ChangeContainer(Room);
            }

            if (AutomaticallyDisplayRoom)
            {
                StringBuilder sb = new StringBuilder();
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
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.ChangeRoom: {0} is not valid anymore", DebugName);
                return;
            }

            Log.Default.WriteLine(LogLevels.Debug, "ICharacter.ChangeRoom: {0} from: {1} to {2}", DebugName, Room == null ? "<<no room>>" : Room.DebugName, destination == null ? "<<no room>>" : destination.DebugName);
            Room?.Leave(this);
            if (recompute)
                Room?.Recompute();
            Room = destination;
            destination?.Enter(this);
            if (recompute)
                destination?.Recompute();
        }

        // Combat
        public bool StartFighting(ICharacter victim) // equivalent to set_fighting in fight.C:3441
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "StartFighting: {0} is not valid anymore", DebugName);
                return false;
            }

            Log.Default.WriteLine(LogLevels.Debug, "{0} starts fighting {1}", DebugName, victim.DebugName);

            Fighting = victim;
            return true;
        }

        public bool StopFighting(bool both) // equivalent to stop_fighting in fight.C:3441
        {
            Log.Default.WriteLine(LogLevels.Debug, "{0} stops fighting {1}", Name, Fighting?.Name ?? "<<no victim>>");

            Fighting = null;
            Stunned = 0;
            ChangePosition(Positions.Standing);
            if (both)
            {
                foreach (ICharacter victim in CharacterManager.Characters.Where(x => x.Fighting == this))
                    victim.StopFighting(false);
            }
            return true;
        }

        public void MultiHit(ICharacter victim) // 'this' starts a combat with 'victim'
        {
            MultiHit(victim, null);
        }

        public abstract void MultiHit(ICharacter victim, IMultiHitModifier multiHitModifier); // 'this' starts a combat with 'victim' and has been initiated by an ability

        public DamageResults AbilityDamage(ICharacter source, int damage, SchoolTypes damageType, string damageNoun, bool display) // 'this' is dealt damage by 'source' using an ability
        {
            return Damage(source, damage, damageType, damageNoun, display);
        }

        public DamageResults HitDamage(ICharacter source, IItemWeapon wield, int damage, SchoolTypes damageType, string damageNoun, bool display) // 'this' is dealt damage by 'source' using a weapon
        {
            return Damage(source, damage, damageType, damageNoun, display);
        }

        public DamageResults Damage(ICharacter source, int damage, SchoolTypes damageType, string damageNoun, bool display) // 'this' is dealt damage by 'source'
        {
            if (HitPoints <= 0)
                return DamageResults.Dead;
            // damage reduction
            if (damage > 35)
                damage = (damage - 35) / 2 + 35;
            if (damage > 80)
                damage = (damage - 80) / 2 + 80;

            if (this != source)
            {
                // Certain attacks are forbidden.
                // Most other attacks are returned.
                string safeResult = IsSafe(source);
                if (safeResult != null)
                {
                    source.Send(safeResult);
                    return DamageResults.Safe;
                }
                // TODO: check_killer
                if (Fighting == null)
                    StartFighting(source);
                    // TODO: if victim.Timer <= 4 -> victim.Position = Positions.Fighting
                if (source.Fighting == null)
                    source.StartFighting(this);
                // more charm stuff
                if (this is INonPlayableCharacter npcVictim && npcVictim.Master == source) // TODO: no more cast like this
                    npcVictim.ChangeMaster(null);
            }
            // inviso attack
            if (CharacterFlags.IsSet("Invisible"))
            {
                RemoveBaseCharacterFlags(false, "Invisible");
                RemoveAuras(x => x.AbilityName == "Invisibility", false);
                Recompute(); // force a recompute to check if there is something special that gives invis
                // if not anymore invis
                if (!CharacterFlags.IsSet("Invisible"))
                    Act(ActOptions.ToRoom, "{0:N} fades into existence.", this);
            }
            // TODO: remove invis, mass invis, flags, ... + "$n fades into existence."
            // damage modifiers
            if (damage > 1 && this is IPlayableCharacter pcVictim && pcVictim[Conditions.Drunk] > 10)
                damage -= damage / 10;
            if (damage > 1 && CharacterFlags.IsSet("Sanctuary"))
                damage /= 2;
            if (damage > 1
                && ((CharacterFlags.IsSet("ProtectEvil") && source.IsEvil)
                    || (CharacterFlags.IsSet("ProtectGood") && source.IsGood)))
                damage -= damage / 4;
            ResistanceLevels resistanceLevel = CheckResistance(damageType);
            switch (resistanceLevel)
            {
                case ResistanceLevels.Immune:
                    damage = 0;
                    break;
                case ResistanceLevels.Resistant:
                    damage -= damage / 3;
                    break;
                case ResistanceLevels.Vulnerable:
                    damage += damage / 2;
                    break;
            }

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
                string damagePhraseSelf = StringHelpers.DamagePhraseSelf(damage);
                string damagePhraseOther = StringHelpers.DamagePhraseOther(damage);
                if (this == source)
                {
                    source.Act(ActOptions.ToRoom, phraseOther, source, this, damagePhraseOther, damageNoun, damage);
                    source.Act(ActOptions.ToCharacter, phraseSource, this, damagePhraseSelf, damageNoun, damage);
                }
                else
                {
                    source.ActToNotVictim(this, phraseOther, source, this, damagePhraseOther, damageNoun, damage);
                    source.Act(ActOptions.ToCharacter, phraseSource, this, damagePhraseSelf, damageNoun, damage);
                    Act(ActOptions.ToCharacter, phraseVictim, source, damagePhraseOther, damageNoun, damage);
                }
            }

            // no damage done, stops here
            if (damage <= 0)
                return DamageResults.NoDamage;

            // hurt the victim
            HitPoints -= damage; // don't use UpdateHitPoints because value will not be allowed to go below 0
            bool isDead = HitPoints < 0;
            // immortals don't really die
            if ((this as IPlayableCharacter)?.IsImmortal == true
                && HitPoints < 1)
                HitPoints = 1;
            if (isDead)
            {
                Send("You have been KILLED!!");
                Act(ActOptions.ToRoom, "{0:N} is dead.", this);
            }
            else
            { 
                if (damage > MaxHitPoints / 4)
                    Send("That really did HURT!");
                else if (HitPoints < MaxHitPoints / 4)
                    Send("You sure are BLEEDING!");
            }

            // dead or sleep spells
            if (isDead || Position == Positions.Sleeping)
                StopFighting(true); // StopFighting will set position to standing

            // handle dead people
            if (isDead)
            {
                RawKilled(source, true); // group group_gain + dying penalty + raw_kill
                return DamageResults.Killed;
            }

            if (this == source)
                return DamageResults.Done;

            // TODO: take care of link-dead people
            HandleWimpy(damage);

            return DamageResults.Done;
        }

        public ResistanceLevels CheckResistance(SchoolTypes damageType)
        {
            string irvFlags;
            // Generic resistance
            ResistanceLevels defaultResistance = ResistanceLevels.Normal;
            if (damageType <= SchoolTypes.Slash) // Physical
            {
                if (Immunities.IsSet("Weapon"))
                    defaultResistance = ResistanceLevels.Immune;
                else if (Resistances.IsSet("Weapon"))
                    defaultResistance = ResistanceLevels.Resistant;
                else if (Vulnerabilities.IsSet("Weapon"))
                    defaultResistance = ResistanceLevels.Normal;
            }
            else // Magic
            {
                if (Immunities.IsSet("Magic"))
                    defaultResistance = ResistanceLevels.Immune;
                else if (Resistances.IsSet("Magic"))
                    defaultResistance = ResistanceLevels.Resistant;
                else if (Vulnerabilities.IsSet("Magic"))
                    defaultResistance = ResistanceLevels.Normal;
            }
            switch (damageType)
            {
                case SchoolTypes.None:
                    return ResistanceLevels.None; // no Resistance
                case SchoolTypes.Bash:
                case SchoolTypes.Pierce:
                case SchoolTypes.Slash:
                    irvFlags = "Weapon";
                    break;
                case SchoolTypes.Fire:
                    irvFlags = "Fire";
                    break;
                case SchoolTypes.Cold:
                    irvFlags = "Cold";
                    break;
                case SchoolTypes.Lightning:
                    irvFlags = "Lightning";
                    break;
                case SchoolTypes.Acid:
                    irvFlags = "Acid";
                    break;
                case SchoolTypes.Poison:
                    irvFlags = "Poison";
                    break;
                case SchoolTypes.Negative:
                    irvFlags = "Negative";
                    break;
                case SchoolTypes.Holy:
                    irvFlags = "Holy";
                    break;
                case SchoolTypes.Energy:
                    irvFlags = "Energy";
                    break;
                case SchoolTypes.Mental:
                    irvFlags = "Mental";
                    break;
                case SchoolTypes.Disease:
                    irvFlags = "Disease";
                    break;
                case SchoolTypes.Drowning:
                    irvFlags = "Drowning";
                    break;
                case SchoolTypes.Light:
                    irvFlags = "Light";
                    break;
                case SchoolTypes.Other: // no specific IRV
                    return defaultResistance;
                case SchoolTypes.Harm: // no specific IRV
                    return defaultResistance;
                case SchoolTypes.Charm:
                    irvFlags = "Charm";
                    break;
                case SchoolTypes.Sound:
                    irvFlags = "Sound";
                    break;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "CharacterBase.CheckResistance: Unknown {0} {1}", nameof(SchoolTypes), damageType);
                    return defaultResistance;
            }
            // Following code has been reworked because Rom24 was testing on currently computed resistance (imm) instead of defaultResistance (def)
            ResistanceLevels resistance = ResistanceLevels.None;
            if (Immunities.IsSet(irvFlags))
                resistance = ResistanceLevels.Immune;
            else if (Resistances.IsSet(irvFlags) && defaultResistance != ResistanceLevels.Immune)
                resistance = ResistanceLevels.Resistant;
            else if (Vulnerabilities.IsSet(irvFlags))
            {
                if (defaultResistance == ResistanceLevels.Immune)
                    resistance = ResistanceLevels.Resistant;
                else if (defaultResistance == ResistanceLevels.Resistant)
                    resistance = ResistanceLevels.Normal;
                else
                    resistance = ResistanceLevels.Vulnerable;
            }
            // if no specific resistance found, return generic one
            if (resistance == ResistanceLevels.None)
                return defaultResistance;
            // else, return specific resistance
            return resistance;
        }

        public IItemCorpse RawKilled(ICharacter killer, bool payoff)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Warning, "RawKilled: {0} is not valid anymore", DebugName);
                return null;
            }

            if (this is INonPlayableCharacter)
                Wiznet.Wiznet($"{DebugName} got toasted by {killer?.DebugName ?? "???"} at {Room?.DebugName ?? "???"}", WiznetFlags.MobDeaths);
            else
                Wiznet.Wiznet($"{DebugName} got toasted by {killer?.DebugName ?? "???"} at {Room?.DebugName ?? "???"}", WiznetFlags.Deaths);

            StopFighting(true);
            // Remove auras
            RemoveAuras(_ => true, false);

            // Death cry
            ActToNotVictim(this, "You hear {0}'s death cry.", this); // TODO: custom death cry

            // Create corpse
            ItemCorpseBlueprint itemCorpseBlueprint = ItemManager.GetItemBlueprint<ItemCorpseBlueprint>(Settings.CorpseBlueprintId);
            IItemCorpse corpse = null;
            if (itemCorpseBlueprint != null)
            {
                if (killer != null)
                    corpse = ItemManager.AddItemCorpse(Guid.NewGuid(), Room, this, killer);
                else
                    corpse = ItemManager.AddItemCorpse(Guid.NewGuid(), Room, this);
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Error, "ItemCorpseBlueprint (id:{0}) doesn't exist !!!", Settings.CorpseBlueprintId);
            }

            // Gain/lose xp/reputation auto loot/gold/sac
            if (payoff)
            {
                killer?.KillingPayoff(this, corpse);
                DeathPayoff(killer);
            }

            //
            HandleDeath();

            return corpse;
        }

        public abstract void KillingPayoff(ICharacter victim, IItemCorpse corpse);

        public bool SavesSpell(int level, SchoolTypes damageType)
        {
            ICharacter victim = this;
            int save = 50 + (victim.Level - level) * 5 - victim[CharacterAttributes.SavingThrow] * 2;
            if (victim.CharacterFlags.IsSet("Berserk"))
                save += victim.Level / 2;
            ResistanceLevels resist = victim.CheckResistance(damageType);
            switch (resist)
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
            if (victim.Class?.CurrentResourceKinds(victim.Form).Contains(ResourceKinds.Mana) == true)
                save = (save * 9) / 10;
            save = save.Range(5, 95);
            return RandomManager.Chance(save);
        }

        public bool IsSafeSpell(ICharacter caster, bool area)
        {
            ICharacter victim = this;
            if (!victim.IsValid || victim.Room == null || !caster.IsValid || caster.Room == null)
                return true;
            if (area && caster == victim)
                return true;
            if (victim.Fighting == caster || victim == caster)
                return false;
            if (!area && (caster is IPlayableCharacter pcCaster && pcCaster.IsImmortal))
                return false;
            // Killing npc
            if (victim is INonPlayableCharacter npcVictim)
            {
                // safe room ?
                if (victim.Room.RoomFlags.IsSet("Safe"))
                    return true;

                if (npcVictim.Blueprint is CharacterShopBlueprint)
                {
                    caster.Send("The shopkeeper wouldn't like that.");
                    return true;
                }

                if (npcVictim.ActFlags.HasAny("Train", "Gain", "Practice", "IsHealer")
                    || npcVictim.Blueprint is CharacterQuestorBlueprint)
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
                    // TODO: legal kill? -- cannot hit mob fighting non-group member
                    //if (victim->fighting != NULL && !is_same_group(ch,victim->fighting)) -> true
                }
                // Player doing the killing
                else
                {
                    // TODO: area effect spells do not hit other mobs
                    //if (area && !is_same_group(victim,ch->fighting)) -> true
                }
            }
            // Killing players
            else
            {
                if (area && (victim is IPlayableCharacter pcVictim && pcVictim.IsImmortal))
                    return true;
                // Npc doing the killing
                if (caster is INonPlayableCharacter npcCaster)
                {
                    // charmed mobs and pets cannot attack players while owned
                    if (caster.CharacterFlags.IsSet("Charm") && npcCaster.Master!= null && npcCaster.Master.Fighting != victim)
                        return true;
                    // safe room
                    if (victim.Room.RoomFlags.IsSet("Safe"))
                        return true;
                    // TODO:  legal kill? -- mobs only hit players grouped with opponent
                    //if (ch->fighting != NULL && !is_same_group(ch->fighting, victim))
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

        public string IsSafe(ICharacter aggressor)
        {
            ICharacter victim = this;
            if (!victim.IsValid || victim.Room == null || !aggressor.IsValid || aggressor.Room == null)
                return "Invalid target!";
            if (victim.Fighting == aggressor || victim == aggressor)
                return null;
            if (aggressor is IPlayableCharacter pcCaster && pcCaster.IsImmortal)
                return null;
            // Killing npc
            if (victim is INonPlayableCharacter npcVictim)
            {
                if (victim.Room.RoomFlags.IsSet("Safe"))
                    return "Not in this room.";

                if (npcVictim.Blueprint is CharacterShopBlueprint)
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
                    // safe room
                    if (victim.Room.RoomFlags.IsSet("Safe"))
                        return "Not in this room.";
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
                return false;

            IRoom from = Room;

            // Try 6 times to find an exit
            for (int attempt = 0; attempt < 6; attempt++)
            {
                ExitDirections randomExit = RandomManager.Random<ExitDirections>();
                IExit exit = Room.Exits[(int) randomExit];
                IRoom destination = exit?.Destination;
                if (destination != null && !exit.IsClosed
                                        && !(this is INonPlayableCharacter && destination.RoomFlags.IsSet("NoMob")))
                {
                    // Try to move without checking if in combat or not
                    Move(randomExit, false, false); // followers will not follow
                    if (Room != from) // successful only if effectively moved away
                    {
                        //
                        StopFighting(true);
                        //
                        Send("You flee from combat!");
                        Act(from.People, "{0} has fled!", this);

                        if (this is IPlayableCharacter pc)
                        {
                            Send("You lost 10 exp.");
                            pc.GainExperience(-10);
                        }

                        return true;
                    }
                }
            }
            Send("PANIC! You couldn't escape!");
            return false;
        }

        // Abilities
        public abstract (int percentage, IAbilityLearned abilityLearned) GetWeaponLearnedInfo(IItemWeapon weapon);

        public abstract (int percentage, IAbilityLearned abilityLearned) GetAbilityLearnedInfo(string abilityName);

        public IAbilityLearned GetAbilityLearned(string abilityName)
        {
            if (!_learnedAbilities.TryGetValue(abilityName, out var abilityLearned))
                return null;
            return abilityLearned;
        }

        public IDictionary<string, int> AbilitiesInCooldown => _cooldownsPulseLeft;

        public bool HasAbilitiesInCooldown => _cooldownsPulseLeft.Any();

        public int CooldownPulseLeft(string abilityName)
        {
            int pulseLeft;
            if (_cooldownsPulseLeft.TryGetValue(abilityName, out pulseLeft))
                return pulseLeft;
            return int.MinValue;
        }

        public void SetCooldown(string abilityName, TimeSpan timeSpan)
        {
            _cooldownsPulseLeft[abilityName] = Pulse.FromTimeSpan(timeSpan);
        }

        public bool DecreaseCooldown(string abilityName, int pulseCount)
        {
            int pulseLeft;
            if (_cooldownsPulseLeft.TryGetValue(abilityName, out pulseLeft))
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
                Send("%c%{0} is available.%x%", abilityName);
        }

        // Equipment
        public IItem GetEquipment(EquipmentSlots slot) => Equipments.FirstOrDefault(x => x.Slot == slot && x.Item != null)?.Item;

        public T GetEquipment<T>(EquipmentSlots slot)
            where T : IItem => Equipments.Where(x => x.Slot == slot && x.Item is T).Select(x => x.Item).OfType<T>().FirstOrDefault();

        public IEquippedItem SearchEquipmentSlot(IItem item, bool replace)
        {
            switch (item.WearLocation)
            {
                case WearLocations.None:
                    return null;
                case WearLocations.Light:
                    return SearchEquipmentSlot(EquipmentSlots.Light, replace);
                case WearLocations.Head:
                    return SearchEquipmentSlot(EquipmentSlots.Head, replace);
                case WearLocations.Amulet:
                    return SearchEquipmentSlot(EquipmentSlots.Amulet, replace);
                case WearLocations.Chest:
                    return SearchEquipmentSlot(EquipmentSlots.Chest, replace);
                case WearLocations.Cloak:
                    return SearchEquipmentSlot(EquipmentSlots.Cloak, replace);
                case WearLocations.Waist:
                    return SearchEquipmentSlot(EquipmentSlots.Waist, replace);
                case WearLocations.Wrists:
                    return SearchEquipmentSlot(EquipmentSlots.Wrists, replace);
                case WearLocations.Arms:
                    return SearchEquipmentSlot(EquipmentSlots.Arms, replace);
                case WearLocations.Hands:
                    return SearchEquipmentSlot(EquipmentSlots.Hands, replace);
                case WearLocations.Ring:
                    return SearchEquipmentSlot(EquipmentSlots.Ring, replace);
                case WearLocations.Legs:
                    return SearchEquipmentSlot(EquipmentSlots.Legs, replace);
                case WearLocations.Feet:
                    return SearchEquipmentSlot(EquipmentSlots.Feet, replace);
                case WearLocations.Wield:
                    // Search empty mainhand, then empty offhand, TODO use offhand only if mainhand is not wielding a 2H
                    return SearchOneHandedWeaponEquipmentSlot(replace);
                case WearLocations.Hold:
                    // only if mainhand is not wielding a 2H
                    return SearchOffhandEquipmentSlot(replace);
                case WearLocations.Shield:
                    // only if mainhand is not wielding a 2H
                    return SearchOffhandEquipmentSlot(replace);
                case WearLocations.Wield2H:
                    // Search empty mainhand + empty offhand (no autoreplace) // TODO can wield 2H on one hand if giant or specific ability
                    return SearchTwoHandedWeaponEquipmentSlot(replace);
                case WearLocations.Float:
                    return SearchEquipmentSlot(EquipmentSlots.Float, replace);
            }
            return null;
        }

        // Misc
        public virtual bool GetItem(IItem item, IContainer container) // equivalent to get_obj in act_obj.C:211
        {
            //
            if (item.NoTake)
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
            //
            string condition = "is here.";
            int maxHitPoints = MaxHitPoints;
            if (maxHitPoints > 0)
            {
                int percent = (100 * HitPoints) / maxHitPoints;
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
            sb.AppendLine($"{RelativeDisplayName(viewer)} {condition}");

            //
            if (Equipments.Any(x => x.Item != null))
            {
                sb.AppendLine($"{RelativeDisplayName(viewer)} is using:");
                foreach (IEquippedItem equippedItem in Equipments.Where(x => x.Item != null))
                {
                    sb.Append(equippedItem.EquipmentSlotsToString());
                    equippedItem.Item.Append(sb, viewer, true);
                    sb.AppendLine();
                }
            }

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
            // display flags
            if (CharacterFlags.IsSet("Charm"))
                sb.Append("%C%(Charmed)%x%");
            if (CharacterFlags.IsSet("Flying"))
                sb.Append("%c%(Flying)%x%");
            if (CharacterFlags.IsSet("Invisible"))
                sb.Append("%y%(Invis)%x%");
            if (CharacterFlags.IsSet("Hide"))
                sb.Append("%b%(Hide)%x%");
            if (CharacterFlags.IsSet("Sneak"))
                sb.Append("%R%(Sneaking)%x%");
            if (CharacterFlags.IsSet("PassDoor"))
                sb.Append("%c%(Translucent)%x%");
            if (CharacterFlags.IsSet("FaerieFire"))
                sb.Append("%m%(Pink Aura)%x%");
            if (CharacterFlags.IsSet("DetectEvil"))
                sb.Append("%r%(Red Aura)%x%");
            if (CharacterFlags.IsSet("DetectGood"))
                sb.Append("%Y%(Golden Aura)%x%");
            if (CharacterFlags.IsSet("Sanctuary"))
                sb.Append("%W%(White Aura)%x%");
            // TODO: killer/thief
            // TODO: display long description and stop if position = start position for NPC

            // last case of POS_STANDING
            sb.Append(RelativeDisplayName(viewer));
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
                default:
                    if (Stunned > 0)
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
                            Log.Default.WriteLine(LogLevels.Warning, "{0} is fighting {1} in a different room.", DebugName, Fighting.DebugName);
                            sb.Append("someone who left??");
                        }
                    }
                    break;
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

        public void ApplyAffect(ICharacterAttributeAffect affect)
        {
            if (affect.Location == CharacterAttributeAffectLocations.None)
                return;
            if (affect.Location == CharacterAttributeAffectLocations.Characteristics)
            {
                switch (affect.Operator)
                {
                    case AffectOperators.Add:
                        _currentAttributes[(int)CharacterAttributes.Strength] += affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.Intelligence] += affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.Wisdom] += affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.Dexterity] += affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.Constitution] += affect.Modifier;
                        break;
                    case AffectOperators.Assign:
                        _currentAttributes[(int)CharacterAttributes.Strength] = affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.Intelligence] = affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.Wisdom] = affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.Dexterity] = affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.Constitution] = affect.Modifier;
                        break;
                    case AffectOperators.Or:
                    case AffectOperators.Nor:
                        Log.Default.WriteLine(LogLevels.Error, "Invalid AffectOperators {0} for CharacterAttributeAffect Characteristics", affect.Operator);
                        break;
                }
                return;
            }
            if (affect.Location == CharacterAttributeAffectLocations.AllArmor)
            {
                switch (affect.Operator)
                {
                    case AffectOperators.Add:
                        _currentAttributes[(int)CharacterAttributes.ArmorBash] += affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.ArmorPierce] += affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.ArmorSlash] += affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.ArmorExotic] += affect.Modifier;
                        break;
                    case AffectOperators.Assign:
                        _currentAttributes[(int)CharacterAttributes.ArmorBash] = affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.ArmorPierce] = affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.ArmorSlash] = affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.ArmorExotic] = affect.Modifier;
                        break;
                    case AffectOperators.Or:
                    case AffectOperators.Nor:
                        Log.Default.WriteLine(LogLevels.Error, "Invalid AffectOperators {0} for CharacterAttributeAffect AllArmor", affect.Operator);
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
                case CharacterAttributeAffectLocations.MaxHitPoints: attribute = CharacterAttributes.MaxHitPoints; break;
                case CharacterAttributeAffectLocations.SavingThrow: attribute = CharacterAttributes.SavingThrow; break;
                case CharacterAttributeAffectLocations.HitRoll: attribute = CharacterAttributes.HitRoll; break;
                case CharacterAttributeAffectLocations.DamRoll: attribute = CharacterAttributes.DamRoll; break;
                case CharacterAttributeAffectLocations.MaxMovePoints: attribute = CharacterAttributes.MaxMovePoints; break;
                case CharacterAttributeAffectLocations.ArmorBash: attribute = CharacterAttributes.ArmorBash; break;
                case CharacterAttributeAffectLocations.ArmorPierce: attribute = CharacterAttributes.ArmorPierce; break;
                case CharacterAttributeAffectLocations.ArmorSlash: attribute = CharacterAttributes.ArmorSlash; break;
                case CharacterAttributeAffectLocations.ArmorMagic: attribute = CharacterAttributes.ArmorExotic; break;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "CharacterBase.ApplyAffect: Unexpected CharacterAttributeAffectLocations {0}", affect.Location);
                    return;
            }
            switch (affect.Operator)
            {
                case AffectOperators.Add:
                    _currentAttributes[(int)attribute] += affect.Modifier;
                    break;
                case AffectOperators.Assign:
                    _currentAttributes[(int)attribute] = affect.Modifier;
                    break;
                case AffectOperators.Or:
                case AffectOperators.Nor:
                    Log.Default.WriteLine(LogLevels.Error, "Invalid AffectOperators {0} for CharacterAttributeAffect {1}", affect.Operator, affect.Location);
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

        #endregion

        protected abstract ExitDirections ChangeDirectionBeforeMove(ExitDirections direction, IRoom fromRoom);

        protected abstract bool BeforeMove(ExitDirections direction, IRoom fromRoom, IRoom toRoom);

        protected abstract void AfterMove(ExitDirections direction, IRoom fromRoom, IRoom toRoom);

        protected abstract (int hitGain, int moveGain, int manaGain, int psyGain) RegenBaseValues();

        protected virtual bool AutomaticallyDisplayRoom => IncarnatedBy != null;

        protected virtual void MoveFollow(IRoom fromRoom, IRoom toRoom, ExitDirections direction)
        {
            if (fromRoom != toRoom)
            {
                IReadOnlyCollection<ICharacter> followers = new ReadOnlyCollection<ICharacter>(fromRoom.People.Where(x => x.Leader == this && x.Position == Positions.Standing && x.CanSee(toRoom)).ToList()); // clone because Move will modify fromRoom.People
                foreach (ICharacter follower in followers)
                {
                    follower.Send("You follow {0}.", DebugName);
                    follower.Move(direction, true, true);
                }
            }
        }

        protected virtual void EnterFollow(IRoom wasRoom, IRoom destination, IItemPortal portal)
        {
            // Followers will not automatically enter portal
        }

        protected virtual IEnumerable<ICharacter> GetActTargets(ActOptions option)
        {
            switch (option)
            {
                case ActOptions.ToAll:
                    return Room.People;
                case ActOptions.ToRoom:
                    return Room.People.Where(x => x != this);
                case ActOptions.ToGroup:
                    Log.Default.WriteLine(LogLevels.Warning, "Act with option ToGroup used on generic CharacterBase");
                    return Enumerable.Empty<ICharacter>(); // defined only for PlayableCharacter
                case ActOptions.ToCharacter:
                    return this.Yield();
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Act with invalid option: {0}", option);
                    return Enumerable.Empty<ICharacter>();
            }
        }

        protected abstract void HandleDeath();

        protected abstract void HandleWimpy(int damage);

        protected abstract (int thac0_00, int thac0_32) GetThac0();

        protected abstract SchoolTypes NoWeaponDamageType { get; }

        protected abstract int NoWeaponBaseDamage { get; }

        protected abstract string NoWeaponDamageNoun { get; }

        protected int GetWeaponBaseDamage(IItemWeapon weapon, ICharacter victim, int weaponLearned)
        {
            int damage = RandomManager.Dice(weapon.DiceCount, weapon.DiceValue) * weaponLearned / 100;
            if (GetEquipment<IItemShield>(EquipmentSlots.OffHand) == null) // no shield -> more damage
                damage = 11 * damage / 10;
            foreach (string damageModifierWeaponEffect in WeaponEffectManager.WeaponEffectsByType<IDamageModifierWeaponEffect>(weapon))
            {
                IDamageModifierWeaponEffect effect = WeaponEffectManager.CreateInstance<IDamageModifierWeaponEffect>(damageModifierWeaponEffect);
                if (effect != null)
                    damage += effect.DamageModifier(this, victim, weapon, weaponLearned, damage);
            }

            return damage;
        }

        protected void OneHit(ICharacter victim, IItemWeapon wield, IHitModifier hitModifier) // 'this' hits 'victim' using hitModifier (optional, used only for backstab)
        {
            if (victim == this || victim == null)
                return;
            // can't beat a dead char!
            // guard against weird room-leavings.
            if (victim.Room != Room)
                return;
            SchoolTypes damageType = wield?.DamageType ?? NoWeaponDamageType;
            // get weapon skill
            var weaponLearnedInfo = GetWeaponLearnedInfo(wield);
            int learned = weaponLearnedInfo.percentage;
            // Calculate to-hit-armor-class-0 versus armor.
            (int thac0_00, int thac0_32) thac0Values = GetThac0();
            int thac0 = IntExtensions.Lerp(thac0Values.thac0_00, thac0Values.thac0_32, Level, 32);
            if (thac0 < 0)
                thac0 /= 2;
            if (thac0 < -5)
                thac0 = -5 + (thac0 + 5) / 2;
            thac0 -= HitRoll * learned / 100;
            thac0 += 5 * (100 - learned) / 100;
            if (hitModifier != null)
                thac0 = hitModifier.Thac0Modifier(thac0);
            int victimAc;
            switch (damageType)
            {
                case SchoolTypes.Bash:
                    victimAc = victim[Armors.Bash];
                    break;
                case SchoolTypes.Pierce:
                    victimAc = victim[Armors.Pierce];
                    break;
                case SchoolTypes.Slash:
                    victimAc = victim[Armors.Slash];
                    break;
                default:
                    victimAc = victim[Armors.Exotic];
                    break;
            }

            if (victimAc < -15)
                victimAc = -15 + (victimAc + 15) / 2;
            if (!CanSee(victim))
                victimAc -= 4;
            if (victim.Position < Positions.Standing)
                victimAc += 4;
            if (victim.Position < Positions.Resting)
                victimAc += 6;
            // miss ?  1d20 -> 1:miss  20: success
            int diceroll = RandomManager.Dice(1, 20);
            if (diceroll == 1
                || (diceroll != 20 && diceroll < thac0 - victimAc))
            {
                if (hitModifier?.AbilityName != null)
                    victim.AbilityDamage(this, 0, damageType, hitModifier.DamageNoun ?? "hit", true); // miss
                else
                {
                    string damageNoun = wield == null ? NoWeaponDamageNoun : wield.DamageNoun;
                    victim.HitDamage(this, wield, 0, damageType, damageNoun ?? "hit", true);
                }

                return;
            }

            // avoidance
            foreach (IAbilityInfo avoidanceAbility in AbilityManager.AbilitiesByExecutionType<IHitAvoidancePassive>())
            {
                IHitAvoidancePassive ability = AbilityManager.CreateInstance<IHitAvoidancePassive>(avoidanceAbility);
                if (ability != null)
                {
                    bool success = ability.Avoid(victim, this, damageType);
                    if (success)
                        return; // stops here
                }
            }

            // instant-death ?
            // must stop here because victim is dead, corpse has been created (an eventually looted/sacced)
            if (wield != null)
            {
                foreach (string instantDeathWeaponEffect in WeaponEffectManager.WeaponEffectsByType<IInstantDeathWeaponEffect>(wield))
                {
                    IInstantDeathWeaponEffect effect = WeaponEffectManager.CreateInstance<IInstantDeathWeaponEffect>(instantDeathWeaponEffect);
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
            if (weaponLearnedInfo.abilityLearned != null)
            {
                IPassive weaponAbility = AbilityManager.CreateInstance<IPassive>(weaponLearnedInfo.abilityLearned.Name);
                weaponAbility?.IsTriggered(this, victim, true, out _, out _); // TODO: maybe we should test return value (imagine a big bad boss which add CD to every skill)
            }

            // bonus
            foreach (IAbilityInfo enhancementAbility in AbilityManager.AbilitiesByExecutionType<IHitEnhancementPassive>())
            {
                IHitEnhancementPassive ability = AbilityManager.CreateInstance<IHitEnhancementPassive>(enhancementAbility);
                if (ability != null)
                    damage += ability.DamageModifier(this, victim, damageType, damage);
            }

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
                string damageNoun = wield == null ? NoWeaponDamageNoun : wield.DamageNoun;
                damageResult = victim.HitDamage(this, wield, damage, damageType, damageNoun ?? "hit", true);
            }

            if (Fighting != victim)
                return;

            // funky weapon ?
            if (damageResult == DamageResults.Done && wield != null)
            {
                foreach (string postHitDamageWeaponEffect in WeaponEffectManager.WeaponEffectsByType<IPostHitDamageWeaponEffect>(wield))
                {
                    IPostHitDamageWeaponEffect effect = WeaponEffectManager.CreateInstance<IPostHitDamageWeaponEffect>(postHitDamageWeaponEffect);
                    if (effect != null)
                        effect.Apply(this, victim, wield);

                    if (Fighting != victim) // stop if not anymore fighting
                        return;
                }
            }
        }

        protected abstract void DeathPayoff(ICharacter killer);

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
                    item.ChangeEquippedBy(null, false);
                    item.ChangeContainer(this);
                }

                _equipments.Clear();
                _equipments.AddRange(Race.EquipmentSlots.Select(x => new EquippedItem(x)));
                //Recompute();
            }
            else
            {
                _equipments.Add(new EquippedItem(EquipmentSlots.Light));
                _equipments.Add(new EquippedItem(EquipmentSlots.Head));
                _equipments.Add(new EquippedItem(EquipmentSlots.Amulet)); // 2 amulets
                _equipments.Add(new EquippedItem(EquipmentSlots.Amulet));
                _equipments.Add(new EquippedItem(EquipmentSlots.Chest));
                _equipments.Add(new EquippedItem(EquipmentSlots.Cloak));
                _equipments.Add(new EquippedItem(EquipmentSlots.Waist));
                _equipments.Add(new EquippedItem(EquipmentSlots.Wrists)); // 2 wrists
                _equipments.Add(new EquippedItem(EquipmentSlots.Wrists));
                _equipments.Add(new EquippedItem(EquipmentSlots.Arms));
                _equipments.Add(new EquippedItem(EquipmentSlots.Hands));
                _equipments.Add(new EquippedItem(EquipmentSlots.Ring)); // 2 rings
                _equipments.Add(new EquippedItem(EquipmentSlots.Ring));
                _equipments.Add(new EquippedItem(EquipmentSlots.Legs));
                _equipments.Add(new EquippedItem(EquipmentSlots.Feet));
                _equipments.Add(new EquippedItem(EquipmentSlots.MainHand)); // 2 hands
                _equipments.Add(new EquippedItem(EquipmentSlots.OffHand));
                _equipments.Add(new EquippedItem(EquipmentSlots.Float));
            }
        }

        protected IEquippedItem SearchEquipmentSlot(EquipmentSlots equipmentSlot, bool replace)
        {
            if (replace) // search empty slot, if not found, return first matching slot
                return Equipments.FirstOrDefault(x => x.Slot == equipmentSlot && x.Item == null) ?? Equipments.FirstOrDefault(x => x.Slot == equipmentSlot);
            return Equipments.FirstOrDefault(x => x.Slot == equipmentSlot && x.Item == null);
        }

        protected IEquippedItem SearchTwoHandedWeaponEquipmentSlot(bool replace)
        {
            // Search empty mainhand + empty offhand (no autoreplace) // TODO can wield 2H on one hand if size giant or specific ability
            var mainHand = Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.MainHand && x.Item == null);
            var offHand = Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.OffHand && x.Item == null);
            if (mainHand != null && offHand != null && mainHand.Item == null && offHand.Item == null)
                return mainHand;
            return null;
        }

        protected IEquippedItem SearchOneHandedWeaponEquipmentSlot(bool replace)
        {
            // Search empty mainhand, then empty offhand only if mainhand is not wielding a 2H
            if (replace)
            {
                // Search empty main hand
                var mainHand = Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.MainHand && x.Item == null);
                if (mainHand != null)
                    return mainHand;
                // Search empty off hand
                var offHand = SearchOffhandEquipmentSlot(false);
                if (offHand != null)
                    return offHand;
                // If not empty main/off hand, search an slot to replace
                return Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.MainHand) ?? SearchOffhandEquipmentSlot(true);
            }
            else
            {
                // Search empty main hand
                var mainHand = Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.MainHand && x.Item == null);
                if (mainHand != null)
                    return mainHand;
                // If not empty main hand found, search off hand
                return SearchOffhandEquipmentSlot(false);
            }
        }

        protected IEquippedItem SearchOffhandEquipmentSlot(bool replace)
        {
            // This can lead to strange looking equipments:
            // wield 1-H weapon -> first main hand
            // wield 2-H weapon -> second main hand
            // hold shield -> second off hand (should be first off hand)
            // Return offhand only if related mainhand is not wielding 2H
            int countMainhand2H = Equipments.Count(x => x.Slot == EquipmentSlots.MainHand && x.Item?.WearLocation == WearLocations.Wield2H);
            if (replace)
                return Equipments.Where(x => x.Slot == EquipmentSlots.OffHand && x.Item == null).ElementAtOrDefault(countMainhand2H) ?? Equipments.Where(x => x.Slot == EquipmentSlots.OffHand).ElementAtOrDefault(countMainhand2H);
            return Equipments.Where(x => x.Slot == EquipmentSlots.OffHand && x.Item == null).ElementAtOrDefault(countMainhand2H);
        }

        protected virtual void RecomputeKnownAbilities()
        {
            // Add abilities from Class/Race/...

            // Admins know every abilities
            //if (ImpersonatedBy is IAdmin)
            //    _knownAbilities.AddRange(AbilityManager.Abilities.Select(x => new AbilityAndLevel(1,x)));
            //else
            if (Class != null)
                MergeAbilities(Class.Abilities, false);
        }

        protected void RecomputeCurrentResourceKinds()
        {
            // Get current resource kind from class if any, every resource otherwise
            CurrentResourceKinds = (Class?.CurrentResourceKinds(Form) ?? EnumHelpers.GetValues<ResourceKinds>()).ToList();
        }

        protected void SetMaxResource(ResourceKinds resourceKind, int value, bool checkCurrent)
        {
            int index = (int)resourceKind;
            if (index >= _maxResources.Length)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to set max resource for resource {0} (index {1}) but max resource length is smaller", resourceKind, index);
                return;
            }
            _maxResources[index] = value;
            if (checkCurrent)
            {
                if (index >= _currentResources.Length)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Trying to set current resource for resource {0} (index {1}) but current resource length is smaller", resourceKind, index);
                    return;
                }
                _currentResources[index] = Math.Min(_currentResources[index], _maxResources[index]);
            }
        }

        protected void SetBaseAttributes(CharacterAttributes attribute, int value, bool checkCurrent)
        {
            int index = (int)attribute;
            if (index >= _baseAttributes.Length)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to set base attribute for attribute {0} (index {1}) but max attribute length is smaller", attribute, index);
                return;
            }
            _baseAttributes[index] = value;
            if (checkCurrent)
            {
                if (index >= _currentAttributes.Length)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Trying to set base current attribute for attribute {0} (index {1}) but current attribute length is smaller", attribute, index);
                    return;
                }
                _currentAttributes[index] = Math.Min(_currentAttributes[index], _baseAttributes[index]);
            }
        }

        protected void AddLearnedAbility(IAbilityUsage abilityUsage, bool naturalBorn)
        {
            if (!_learnedAbilities.ContainsKey(abilityUsage.Name))
            {
                var abilityLearned = new AbilityLearned(abilityUsage);
                _learnedAbilities.Add(abilityLearned.Name, abilityLearned);
                if (naturalBorn)
                {
                    abilityLearned.Update(1, 1);
                    abilityLearned.IncrementLearned(100);
                }
            }
        }

        protected void AddLearnedAbility(AbilityLearned abilityLearned)
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

        protected StringBuilder AppendPositionFurniture(StringBuilder sb, string verb, IItemFurniture furniture)
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
            foreach (IAbilityUsage abilityUsage in abilities)
            {
                var abilityLearnedInfo = GetAbilityLearnedInfo(abilityUsage.Name);
                if (abilityLearnedInfo.abilityLearned != null)
                {
                    //Log.Default.WriteLine(LogLevels.Debug, "Merging KnownAbility with AbilityUsage for {0} Ability {1}", DebugName, abilityUsage.Ability.Name);
                    abilityLearnedInfo.abilityLearned.Update(Math.Min(abilityUsage.Level, abilityUsage.Level), Math.Min(abilityUsage.Rating, abilityUsage.Rating), Math.Min(abilityUsage.CostAmount, abilityUsage.CostAmount));
                    // TODO: what should be we if multiple resource kind or operator ?
                }
                else
                {
                    Log.Default.WriteLine(LogLevels.Debug, "Adding AbilityLearned from AbilityUsage for {0} Ability {1}", DebugName, abilityUsage.Name);
                    AddLearnedAbility(abilityUsage, naturalBorn);
                }
            }
        }
    }
}
