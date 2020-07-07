using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mud.Common;
using Mud.Container;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Ability.Skill;
using Mud.Server.Blueprints.Character;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Quest;

namespace Mud.Server.Character.NonPlayableCharacter
{
    public class NonPlayableCharacter : CharacterBase, INonPlayableCharacter
    {
        private IRaceManager RaceManager => DependencyContainer.Current.GetInstance<IRaceManager>();
        private IClassManager ClassManager => DependencyContainer.Current.GetInstance<IClassManager>();

        protected NonPlayableCharacter(Guid guid, string name, string description, CharacterBlueprintBase blueprint, IRoom room)
            : base(guid, name, description)
        {
            Blueprint = blueprint;

            Level = blueprint.Level;
            Position = Positions.Standing;
            Race = RaceManager[blueprint.Race];
            if (Race == null && !string.IsNullOrWhiteSpace(blueprint.Race))
                Log.Default.WriteLine(LogLevels.Warning, "Unknown race '{0}' for npc {1}", blueprint.Race, blueprint.Id);
            Class = ClassManager[blueprint.Class];
            DamageNoun = blueprint.DamageNoun;
            DamageType = blueprint.DamageType;
            DamageDiceCount = blueprint.DamageDiceCount;
            DamageDiceValue = blueprint.DamageDiceValue;
            DamageDiceBonus = blueprint.DamageDiceBonus;
            ActFlags = NewAndCopyAndSet<IActFlags, IActFlagValues>(() => new ActFlags(), blueprint.ActFlags, Race?.ActFlags);
            OffensiveFlags = NewAndCopyAndSet<IOffensiveFlags, IOffensiveFlagValues>(() => new OffensiveFlags(), blueprint.OffensiveFlags, Race?.OffensiveFlags);
            AssistFlags = NewAndCopyAndSet<IAssistFlags, IAssistFlagValues>(() => new AssistFlags(), blueprint.AssistFlags, Race?.AssistFlags);
            BaseBodyForms = NewAndCopyAndSet<IBodyForms, IBodyFormValues>(() => new BodyForms(), Blueprint.BodyForms, Race?.BodyForms);
            BaseBodyParts = NewAndCopyAndSet<IBodyParts, IBodyPartValues>(() => new BodyParts(), Blueprint.BodyParts, Race?.BodyParts);
            BaseCharacterFlags = NewAndCopyAndSet<ICharacterFlags, ICharacterFlagValues>(() => new CharacterFlags(), blueprint.CharacterFlags, Race?.CharacterFlags);
            BaseImmunities = NewAndCopyAndSet<IIRVFlags, IIRVFlagValues>(() => new IRVFlags(), blueprint.Immunities, Race?.Immunities);
            BaseResistances = NewAndCopyAndSet<IIRVFlags, IIRVFlagValues>(() => new IRVFlags(), blueprint.Resistances, Race?.Resistances);
            BaseVulnerabilities = NewAndCopyAndSet<IIRVFlags, IIRVFlagValues>(() => new IRVFlags(), blueprint.Vulnerabilities, Race?.Vulnerabilities);
            BaseSex = blueprint.Sex;
            BaseSize = blueprint.Size;
            Alignment = blueprint.Alignment.Range(-1000, 1000);
            if (blueprint.Wealth == 0)
            {
                SilverCoins = 0;
                GoldCoins = 0;
            }
            else
            {
                long wealth = RandomManager.Range(blueprint.Wealth / 2, 3 * blueprint.Wealth / 2);
                GoldCoins = RandomManager.Range(wealth / 200, wealth / 100);
                SilverCoins = wealth - (GoldCoins * 100);
            }
            // TODO: see db.C:Create_Mobile
            // TODO: following values must be extracted from blueprint
            int baseValue = Math.Min(25, 11 + Level / 4);
            SetBaseAttributes(CharacterAttributes.Strength, baseValue, false); // TODO
            SetBaseAttributes(CharacterAttributes.Intelligence, baseValue, false); // TODO
            SetBaseAttributes(CharacterAttributes.Wisdom, baseValue, false); // TODO
            SetBaseAttributes(CharacterAttributes.Dexterity, baseValue, false); // TODO
            SetBaseAttributes(CharacterAttributes.Constitution, baseValue, false); // TODO
            // TODO: use Act/Off/size to change values
            int maxHitPoints = RandomManager.Dice(blueprint.HitPointDiceCount, blueprint.HitPointDiceValue) + blueprint.HitPointDiceBonus;
            SetBaseAttributes(CharacterAttributes.MaxHitPoints, maxHitPoints, false); // OK
            SetBaseAttributes(CharacterAttributes.SavingThrow, 0, false); // TODO
            SetBaseAttributes(CharacterAttributes.HitRoll, blueprint.HitRollBonus, false); // OK
            SetBaseAttributes(CharacterAttributes.DamRoll, Level, false); // TODO
            SetBaseAttributes(CharacterAttributes.MaxMovePoints, 1000, false); // TODO
            SetBaseAttributes(CharacterAttributes.ArmorBash, blueprint.ArmorBash, false); // OK
            SetBaseAttributes(CharacterAttributes.ArmorPierce, blueprint.ArmorPierce, false); // OK
            SetBaseAttributes(CharacterAttributes.ArmorSlash, blueprint.ArmorSlash, false); // OK
            SetBaseAttributes(CharacterAttributes.ArmorExotic, blueprint.ArmorExotic, false); // OK

            // resources (should be extracted from blueprint)
            int maxMana = RandomManager.Dice(blueprint.ManaDiceCount, blueprint.ManaDiceValue) + blueprint.ManaDiceBonus;
            foreach (var resource in EnumHelpers.GetValues<ResourceKinds>())
            {
                SetMaxResource(resource, maxMana, false);
                this[resource] = maxMana;
            }
            HitPoints = BaseAttribute(CharacterAttributes.MaxHitPoints); // can't use this[MaxHitPoints] because current has been been computed, it will be computed in ResetCurrentAttributes
            MovePoints = BaseAttribute(CharacterAttributes.MaxMovePoints);

            BuildEquipmentSlots();

            Room = room;
            room.Enter(this);
        }

        public NonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, IRoom room) // NPC
            : this(guid, blueprint.Name, blueprint.Description, blueprint, room)
        {
            RecomputeKnownAbilities();
            ResetAttributes();
            RecomputeCurrentResourceKinds();
        }

        public NonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, PetData petData, IRoom room) // Pet
            : this(guid, petData.Name, blueprint.Description, blueprint, room)
        {
            BaseSex = petData.Sex;
            BaseSize = petData.Size;
            // attributes
            if (petData.Attributes != null)
            {
                foreach (var attributeData in petData.Attributes)
                    SetBaseAttributes(attributeData.Key, attributeData.Value, false);
            }
            else
            {
                Wiznet.Wiznet($"NonPlayableCharacter.ctor: attributes not found in pfile for {petData.Name}", WiznetFlags.Bugs, AdminLevels.Implementor);
                // set to 1 if not found
                foreach (CharacterAttributes attribute in EnumHelpers.GetValues<CharacterAttributes>())
                    this[attribute] = 1;
            }
            // resources
            if (petData.CurrentResources != null)
            {
                foreach (var currentResourceData in petData.CurrentResources)
                    this[currentResourceData.Key] = currentResourceData.Value;
            }
            else
            {
                Wiznet.Wiznet($"NonPlayableCharacter.ctor: currentResources not found in pfile for {petData.Name}", WiznetFlags.Bugs, AdminLevels.Implementor);
                // set to 1 if not found
                foreach (ResourceKinds resource in EnumHelpers.GetValues<ResourceKinds>())
                    this[resource] = 1;
            }
            if (petData.MaxResources != null)
            {
                foreach (var maxResourceData in petData.MaxResources)
                    SetMaxResource(maxResourceData.Key, maxResourceData.Value, false);
            }

            // Equipped items
            if (petData.Equipments != null)
            {
                // Create item in inventory and try to equip it
                foreach (EquippedItemData equippedItemData in petData.Equipments)
                {
                    // Create in inventory
                    var item = ItemManager.AddItem(Guid.NewGuid(), equippedItemData.Item, this);

                    // Try to equip it
                    IEquippedItem equippedItem = SearchEquipmentSlot(equippedItemData.Slot, false);
                    if (equippedItem != null)
                    {
                        if (item.WearLocation != WearLocations.None)
                        {
                            equippedItem.Item = item;
                            item.ChangeContainer(null); // remove from inventory
                            item.ChangeEquippedBy(this, false); // set as equipped by this
                        }
                        else
                        {
                            Wiznet.Wiznet($"Item blueprint Id {equippedItemData.Item.ItemId} cannot be equipped anymore in slot {equippedItemData.Slot} for character {petData.Name}.", WiznetFlags.Bugs, AdminLevels.Implementor);
                        }
                    }
                    else
                    {
                        Wiznet.Wiznet($"Item blueprint Id {equippedItemData.Item.ItemId} was supposed to be equipped in first empty slot {equippedItemData.Slot} for character {petData.Name} but this slot doesn't exist anymore.", WiznetFlags.Bugs, AdminLevels.Implementor);
                    }
                }
            }
            // Inventory
            if (petData.Inventory != null)
            {
                foreach (ItemData itemData in petData.Inventory)
                    ItemManager.AddItem(Guid.NewGuid(), itemData, this);
            }
            // Auras
            if (petData.Auras != null)
            {
                foreach (AuraData auraData in petData.Auras)
                    AuraManager.AddAura(this, auraData, false);
            }

            RecomputeKnownAbilities();
            ResetAttributes();
            RecomputeCurrentResourceKinds();
        }

        #region INonPlayableCharacter

        #region ICharacter

        #region IEntity

        #region IActor

        public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<NonPlayableCharacter>();

        public override void Send(string message, bool addTrailingNewLine)
        {
            base.Send(message, addTrailingNewLine);
            if (Settings.ForwardSlaveMessages && Master != null)
            {
                if (Settings.PrefixForwardedMessages)
                    message = "<CTRL|" + DisplayName + ">" + message;
                Master.Send(message, addTrailingNewLine);
            }
        }

        public override void Page(StringBuilder text)
        {
            base.Page(text);
            if (Settings.ForwardSlaveMessages)
                Master?.Page(text);
        }

        #endregion

        public override string DisplayName => Blueprint.ShortDescription;

        public override string DebugName => $"{DisplayName}[{Blueprint.Id}]";

        public override string RelativeDisplayName(ICharacter beholder, bool capitalizeFirstLetter = false)
        {
            StringBuilder displayName = new StringBuilder();
            IPlayableCharacter playableBeholder = beholder as IPlayableCharacter;
            if (playableBeholder != null && IsQuestObjective(playableBeholder))
                displayName.Append(StringHelpers.QuestPrefix);
            if (beholder.CanSee(this))
                displayName.Append(DisplayName);
            else if (capitalizeFirstLetter)
                displayName.Append("Someone");
            else
                displayName.Append("someone");
            if (playableBeholder?.IsImmortal == true)
                displayName.Append($" [id: {Blueprint?.Id.ToString() ?? " ??? "}]");
            return displayName.ToString();
        }

        public override void OnRemoved() // called before removing a character from the game
        {
            // Free from slavery, must be done before base.OnRemoved because CharacterBase.OnRemoved will call Leader.RemoveFollower which will reset Master
            Master?.RemovePet(this);

            base.OnRemoved();

            StopFighting(true);
            // TODO: what if character is incarnated
            ResetCooldowns();
            DeleteInventory();
            DeleteEquipments();
            Room = RoomManager.NullRoom; // this will avoid a lot of problem, will be set to null in Cleanup phase
        }

        public override void OnCleaned() // called when removing definitively an entity from the game
        {
            Blueprint = null;
            Room = null;
        }

        #endregion

        public override int MaxCarryWeight => ActFlags.IsSet("Pet")
            ? 0
            : base.MaxCarryWeight;

        public override int MaxCarryNumber => ActFlags.IsSet("Pet")
            ? 0
            : base.MaxCarryNumber;

        // Combat
        public override void MultiHit(ICharacter victim, IMultiHitModifier multiHitModifier) // 'this' starts a combat with 'victim'
        {
            // no attacks for stunnies
            if (Stunned > 0)
            {
                Stunned--;
                if (Stunned == 0)
                    Act(ActOptions.ToAll, "%W%{0:N} regain{0:v} {0:s} equilibrium.%x%", this);
                return;
            }

            IItemWeapon mainHand = GetEquipment<IItemWeapon>(EquipmentSlots.MainHand);
            IItemWeapon offHand = GetEquipment<IItemWeapon>(EquipmentSlots.OffHand);
            // main hand attack
            OneHit(victim, mainHand, multiHitModifier);
            if (Fighting != victim)
                return;
            if (multiHitModifier?.MaxAttackCount <= 1)
                return;
            // area attack
            if (OffensiveFlags.IsSet("AreaAttack"))
            {
                IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(Room.People.Where(x => x != this && x.Fighting == this).ToList());
                foreach (ICharacter character in clone)
                    OneHit(character, mainHand, multiHitModifier);
            }
            // off hand attack
            var dualWield = AbilityManager.CreateInstance<IPassive>("Dual Wield");
            if (offHand != null && dualWield != null && dualWield.IsTriggered(this, victim, false, out _, out _))
                OneHit(victim, offHand, multiHitModifier);
            if (Fighting != victim)
                return;
            if (multiHitModifier?.MaxAttackCount <= 2)
                return;
            // main hand haste attack
            if ((CharacterFlags.IsSet("Haste") || OffensiveFlags.IsSet("Fast"))
                && !CharacterFlags.IsSet("Slow"))
                OneHit(victim, mainHand, multiHitModifier);
            if (Fighting != victim)
                return;
            if (multiHitModifier?.MaxAttackCount <= 3)
                return;
            // main hand second attack
            var secondAttack = AbilityManager.CreateInstance<IPassive>("Second Attack");
            if (secondAttack != null && secondAttack.IsTriggered(this, victim, false, out _, out _))
                OneHit(victim, mainHand, multiHitModifier);
            if (Fighting != victim)
                return;
            if (multiHitModifier?.MaxAttackCount <= 4)
                return;
            // main hand third attack
            var thirdAttack = AbilityManager.CreateInstance<IPassive>("Third Attack");
            if (thirdAttack != null && thirdAttack.IsTriggered(this, victim, false, out _, out _))
                OneHit(victim, mainHand, multiHitModifier);
            if (Fighting != victim)
                return;
            if (multiHitModifier?.MaxAttackCount <= 5)
                return;
            // TODO: only if wielding 3 or 4 weapons
            //// 3rd hand
            //var thirdWieldLearnInfo = GetLearnInfo("Third wield");
            //var thirdWieldChance = thirdWieldLearnInfo.learned / 6;
            //if (CharacterFlags.HasFlag(CharacterFlags.Slow))
            //    thirdWieldChance = 0;
            //if (RandomManager.Chance(thirdWieldChance))
            //    OneHit(victim, mainHand, multiHitModifier);
            //if (Fighting != victim)
            //    return;
            //if (multiHitModifier?.MaxAttackCount <= 5)
            //    return;
            //// 4th hand
            //var FourthWieldLearnInfo = GetLearnInfo("Fourth wield");
            //var FourthWieldChance = FourthWieldLearnInfo.learned / 8;
            //if (CharacterFlags.HasFlag(CharacterFlags.Slow))
            //    FourthWieldChance = 0;
            //if (RandomManager.Chance(FourthWieldChance))
            //    OneHit(victim, mainHand, multiHitModifier);
            //if (Fighting != victim)
            //    return;
            //if (multiHitModifier?.MaxAttackCount <= 6)
            //    return;
            // fun stuff
            // TODO: if wait > 0 return
            int number = RandomManager.Range(0, 9);
            switch (number)
            {
                case 0: if (OffensiveFlags.IsSet("Bash"))
                        UseSkill("Bash", CommandHelpers.NoParameters);
                    break;
                case 1: if (OffensiveFlags.IsSet("Berserk") && !CharacterFlags.IsSet("Berserk"))
                        UseSkill("Berserk", CommandHelpers.NoParameters);
                    break;
                case 2: if (OffensiveFlags.IsSet("Disarm")
                        || ActFlags.HasAny("Warrior", "Thief")) // TODO: check if weapon skill is not hand to hand
                        UseSkill("Disarm", CommandHelpers.NoParameters);
                    break;
                case 3: if (OffensiveFlags.IsSet("Kick"))
                        UseSkill("Kick", CommandHelpers.NoParameters);
                    break;
                case 4: if (OffensiveFlags.IsSet("DirtKick"))
                        UseSkill("Dirt Kicking", CommandHelpers.NoParameters);
                    break;
                case 5: if (OffensiveFlags.IsSet("Tail"))
                        UseSkill("Tail", CommandHelpers.NoParameters);
                    break;
                case 6: if (OffensiveFlags.IsSet("Trip"))
                        UseSkill("Trip", CommandHelpers.NoParameters);
                    break;
                case 7: if (OffensiveFlags.IsSet("Crush"))
                        UseSkill("Crush", CommandHelpers.NoParameters);
                    break;
                case 8: if (OffensiveFlags.IsSet("Backstab"))
                        UseSkill("Backstab", CommandHelpers.NoParameters); // TODO: this will never works because we cannot backstab while in combat -> replace with circle
                    break;
                case 9: if (OffensiveFlags.IsSet("Bite"))
                        UseSkill("Bite", CommandHelpers.NoParameters);
                    break;
            }
        }

        public override void KillingPayoff(ICharacter victim, IItemCorpse corpse)
        {
            // NOP
        }

        #endregion

        public CharacterBlueprintBase Blueprint { get; private set; }

        public string DamageNoun { get; protected set; }
        public SchoolTypes DamageType { get; protected set; }
        public int DamageDiceCount { get; protected set; }
        public int DamageDiceValue { get; protected set; }
        public int DamageDiceBonus { get; protected set; }

        public IActFlags ActFlags { get; protected set; }

        public IOffensiveFlags OffensiveFlags { get; protected set; }

        public IAssistFlags AssistFlags { get; protected set; }

        public bool IsQuestObjective(IPlayableCharacter questingCharacter)
        {
            // If 'this' is NPC and in object list or in kill loot table
            return questingCharacter.Quests.Where(q => !q.IsCompleted).SelectMany(q => q.Objectives).OfType<KillQuestObjective>().Any(o => o.Blueprint.Id == Blueprint.Id)
                                     || questingCharacter.Quests.Where(q => !q.IsCompleted).Any(q => q.Blueprint.KillLootTable.ContainsKey(Blueprint.Id));
        }


        public IPlayableCharacter Master { get; protected set; }

        public void ChangeMaster(IPlayableCharacter master)
        {
            if (Master != null && master != null)
                return; // cannot change from one master to another
            Master = master;
        }

        public bool Order(string commandLine)
        {
            if (Master == null)
                return false;
            Act(ActOptions.ToCharacter, "{0:N} orders you to '{1}'.", Master, commandLine);
            CommandHelpers.ExtractCommandAndParameters(commandLine, out string command, out ICommandParameter[] parameters);
            bool executed = ExecuteCommand(commandLine, command, parameters);
            return executed;
        }

        // Mapping
        public PetData MapPetData()
        {
            PetData data = new PetData
            {
                BlueprintId = Blueprint.Id,
                Name = Name,
                //RoomId = Room?.Blueprint?.Id ?? 0,
                Race = Race?.Name ?? string.Empty,
                Class = Class?.Name ?? string.Empty,
                Level = Level,
                Sex = BaseSex,
                Size = BaseSize,
                //SilverCoins = SilverCoins,
                //GoldCoins = GoldCoins,
                HitPoints = HitPoints,
                MovePoints = MovePoints,
                CurrentResources = EnumHelpers.GetValues<ResourceKinds>().ToDictionary(x => x, x => this[x]),
                MaxResources = EnumHelpers.GetValues<ResourceKinds>().ToDictionary(x => x, MaxResource),
                Equipments = Equipments.Where(x => x.Item != null).Select(x => x.MapEquippedData()).ToArray(),
                Inventory = Inventory.Select(x => x.MapItemData()).ToArray(),
                Auras = MapAuraData(),
                CharacterFlags = BaseCharacterFlags,
                Immunities = BaseImmunities,
                Resistances = BaseResistances,
                Vulnerabilities = BaseVulnerabilities,
                Attributes = EnumHelpers.GetValues<CharacterAttributes>().ToDictionary(x => x, BaseAttribute),
                //KnownAbilities = KnownAbilities.Select(x => x.MapKnownAbilityData()).ToArray(),
                //Cooldowns = AbilitiesInCooldown.ToDictionary(x => x.Key.Id, x => x.Value),
            };
            return data;
        }

        #endregion

        #region CharacterBase

        // Abilities
        public override (int percentage, IAbilityLearned abilityLearned) GetWeaponLearnedInfo(IItemWeapon weapon)
        {
            int learned;
            if (weapon == null)
                learned = 40 + 2 * Level;
            else
            {
                switch (weapon.Type)
                {
                    case WeaponTypes.Exotic:
                        learned = 3 * Level;
                        break;
                    default:
                        learned = 40 + (5 * Level) / 2;
                        break;
                }
            }

            learned = learned.Range(0, 100);

            return (learned, null);
        }

        public override (int percentage, IAbilityLearned abilityLearned) GetAbilityLearnedInfo(string abilityName) // TODO: replace with npc class
        {
            IAbilityLearned abilityLearned = GetAbilityLearned(abilityName);
            //int learned = 0;
            //if (knownAbility != null && knownAbility.Level <= Level)
            //    learned = knownAbility.Learned;

            // TODO: spells
            int learned = 0;
            switch (abilityName)
            {
                case "Sneak":
                case "Hide":
                    learned = 20 + 2 * Level;
                    break;
                case "Dodge":
                    if (OffensiveFlags.IsSet("Dodge"))
                        learned = 2 * Level;
                    break;
                case "Parry":
                    if (OffensiveFlags.IsSet("Parry"))
                        learned = 2 * Level;
                    break;
                case "Shield block":
                    learned = 10 + 2 * Level;
                    break;
                case "Second attack":
                    if (ActFlags.HasAny("Warrior", "Thief"))
                        learned = 10 + 3 * Level;
                    break;
                case "Third attack":
                    if (ActFlags.IsSet("Warrior"))
                        learned = 4 * Level - 40;
                    break;
                case "Hand to hand":
                    learned = 40 + 2 * Level;
                    break;
                case "Trip":
                    if (OffensiveFlags.IsSet("Trip"))
                        learned = 10 + 3 * Level;
                    break;
                case "Bash":
                    if (OffensiveFlags.IsSet("Bash"))
                        learned = 10 + 3 * Level;
                    break;
                case "Disarm":
                    if (OffensiveFlags.IsSet("Disarm")
                        || ActFlags.HasAny("Warrior", "Thief"))
                        learned = 20 + 3 * Level;
                    break;
                case "Berserk":
                    if (OffensiveFlags.IsSet("Berserk"))
                        learned = 3 * Level;
                    break;
                case "Kick":
                    if (OffensiveFlags.IsSet("Kick"))
                        learned = 10 + 3 * Level;
                    break;
                case "Backstab":
                    if (ActFlags.IsSet("Thief"))
                        learned = 20 + 2 * Level;
                    break;
                case "Rescue":
                    learned = 40 + Level;
                    break;
                case "Tail":
                    if (OffensiveFlags.IsSet("Tail"))
                        learned = 10 + 3 * Level;
                    break;
                case "Bite":
                    if (OffensiveFlags.IsSet("Bite"))
                        learned = 10 + 3 * Level;
                    break;
                case "Crush":
                    if (OffensiveFlags.IsSet("Crush"))
                        learned = 10 + 3 * Level;
                    break;
                case "Recall":
                    learned = 40 + Level;
                    break;
                case "Axe":
                case "Dagger":
                case "Flail":
                case "Mace":
                case "Poleam":
                case "Spear":
                case "Staves":
                case "Sword":
                case "Whip":
                    learned = 40 + 5 * Level / 2;
                    break;
                default:
                    learned = 0;
                    break;
            }

            // TODO: if daze /=2 for spell and *2/3 if otherwise

            learned = learned.Range(0, 100);
            return (learned, abilityLearned);
        }


        protected override (int hitGain, int moveGain, int manaGain, int psyGain) RegenBaseValues()
        {
            int hitGain = 5 + Level;
            int moveGain = Level;
            int manaGain = 5 + Level;
            int psyGain = 5 + Level;
            if (CharacterFlags.IsSet("Regeneration"))
                hitGain *= 2;
            switch (Position)
            {
                case Positions.Sleeping:
                    hitGain = (3 * hitGain) / 2;
                    manaGain = (3 * manaGain) / 2;
                    psyGain = (3 * psyGain) / 2;
                    break;
                case Positions.Resting:
                    // nop
                    break;
                default:
                    if (Fighting != null)
                    {
                        hitGain /= 3;
                        manaGain /= 3;
                        psyGain /= 3;
                    }
                    else
                    {
                        hitGain /= 2;
                        manaGain /= 2;
                        psyGain /= 2;
                    }

                    break;
            }
            return (hitGain, moveGain, manaGain, psyGain);
        }

        protected override ExitDirections ChangeDirectionBeforeMove(ExitDirections direction, IRoom fromRoom)
        {
            return direction; // no direction change
        }

        protected override bool BeforeMove(ExitDirections direction, IRoom fromRoom, IRoom toRoom)
        {
            return true; // nop
        }

        protected override void AfterMove(ExitDirections direction, IRoom fromRoom, IRoom toRoom)
        {
            if (IncarnatedBy != null)
            {
                StringBuilder sb = new StringBuilder();
                Room.Append(sb, this);
                Send(sb);
            }
        }

        protected override void HandleDeath()
        {
            CharacterManager.RemoveCharacter(this);
        }

        protected override void HandleWimpy(int damage)
        {
            if (damage > 0) // TODO add test on wait < PULSE_VIOLENCE / 2
            {
                if ((ActFlags.IsSet("Wimpy") && HitPoints < MaxHitPoints / 5 && RandomManager.Chance(25))
                    || (CharacterFlags.IsSet("Charm") && Master != null && Master.Room != Room))
                    Flee();
            }
        }

        protected override (int thac0_00, int thac0_32) GetThac0()
        {
            if (Class != null)
                return Class.Thac0;

            int thac0_00 = 20;
            int thac0_32 = -4; // as good as thief

            if (ActFlags.IsSet("Warrior"))
                thac0_32 = -10;
            else if (ActFlags.IsSet("Thief"))
                thac0_32 = -4;
            else if (ActFlags.IsSet("Cleric"))
                thac0_32 = 2;
            else if (ActFlags.IsSet("Mage"))
                thac0_32 = 6;

            return (thac0_00, thac0_32);
        }

        protected override SchoolTypes NoWeaponDamageType => DamageType;

        protected override int NoWeaponBaseDamage => RandomManager.Dice(DamageDiceCount, DamageDiceValue) + DamageDiceBonus;

        protected override string NoWeaponDamageNoun => DamageNoun;

        protected override void DeathPayoff(ICharacter killer)
        {
            // NOP
        }

        #endregion

        protected bool UseSkill(string skillName, params ICommandParameter[] parameters)
        {
            Log.Default.WriteLine(LogLevels.Info, "{0} tries to use {1} on {2}.", DebugName, skillName, Fighting?.DebugName ?? "???");
            var abilityInfo = AbilityManager.Search(skillName, AbilityTypes.Skill);
            if (abilityInfo == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Unknown skill {0}.", skillName);
                Send("This skill doesn't exist.");
                return false;
            }
            ISkill skillInstance = AbilityManager.CreateInstance<ISkill>(abilityInfo.Name);
            var skillActionInput = new SkillActionInput(abilityInfo, this, parameters);
            string setupResult = skillInstance.Setup(skillActionInput);
            if (setupResult != null)
            {
                Send(setupResult);
                return false;
            }
            skillInstance.Execute();
            return true;
        }
    }
}
