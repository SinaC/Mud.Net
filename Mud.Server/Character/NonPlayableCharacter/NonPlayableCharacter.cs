using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Server.Abilities;
using Mud.Server.Blueprints.Character;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;
using Mud.Server.Quest;

namespace Mud.Server.Character.NonPlayableCharacter
{
    public class NonPlayableCharacter : CharacterBase, INonPlayableCharacter
    {
        private static readonly Lazy<IReadOnlyTrie<CommandMethodInfo>> NonPlayableCharacterCommands = new Lazy<IReadOnlyTrie<CommandMethodInfo>>(GetCommands<NonPlayableCharacter>);

        public NonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, IRoom room) // NPC
            : base(guid, blueprint.Name, blueprint.Description)
        {
            Blueprint = blueprint;

            Position = Positions.Standing;
            // TODO: race, class, ...
            Level = blueprint.Level;
            DamageNoun = blueprint.DamageNoun;
            DamageType = blueprint.DamageType;
            DamageDiceCount = blueprint.DamageDiceCount;
            DamageDiceValue = blueprint.DamageDiceValue;
            DamageDiceBonus = blueprint.DamageDiceBonus;
            ActFlags = blueprint.ActFlags;
            OffensiveFlags = blueprint.OffensiveFlags;
            BaseCharacterFlags = blueprint.CharacterFlags;
            BaseImmunities = blueprint.Immunities;
            BaseResistances = blueprint.Resistances;
            BaseVulnerabilities = blueprint.Vulnerabilities;
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
            SetBaseAttributes(CharacterAttributes.Strength, baseValue, false);
            SetBaseAttributes(CharacterAttributes.Intelligence, baseValue, false);
            SetBaseAttributes(CharacterAttributes.Wisdom, baseValue, false);
            SetBaseAttributes(CharacterAttributes.Dexterity, baseValue, false);
            SetBaseAttributes(CharacterAttributes.Constitution, baseValue, false);
            // TODO: use Act/Off/size to change values
            int maxHitPoints = RandomManager.Dice(blueprint.HitPointDiceCount, blueprint.HitPointDiceValue) + blueprint.HitPointDiceBonus;
            SetBaseAttributes(CharacterAttributes.MaxHitPoints, maxHitPoints, false); // OK
            SetBaseAttributes(CharacterAttributes.SavingThrow, 0, false);
            SetBaseAttributes(CharacterAttributes.HitRoll, blueprint.HitRollBonus, false); // OK
            SetBaseAttributes(CharacterAttributes.DamRoll, Level, false);
            SetBaseAttributes(CharacterAttributes.MaxMovePoints, 1000, false);
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

            Room = room;
            room.Enter(this);

            RecomputeKnownAbilities();
            ResetCurrentAttributes();
            RecomputeCurrentResourceKinds();
            BuildEquipmentSlots();
        }

        public NonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, PetData petData, IRoom room) // NPC
            : base(guid, petData.Name, blueprint.Description)
        {
            Blueprint = blueprint;

            Position = Positions.Standing;
            // TODO: race, class, ...
            Level = petData.Level;
            HitPoints = petData.HitPoints;
            MovePoints = petData.MovePoints;
            DamageNoun = blueprint.DamageNoun;
            DamageType = blueprint.DamageType;
            DamageDiceCount = blueprint.DamageDiceCount;
            DamageDiceValue = blueprint.DamageDiceValue;
            DamageDiceBonus = blueprint.DamageDiceBonus;
            ActFlags = blueprint.ActFlags;
            OffensiveFlags = blueprint.OffensiveFlags;
            BaseCharacterFlags = petData.CharacterFlags;
            BaseImmunities = petData.Immunities;
            BaseResistances = petData.Resistances;
            BaseVulnerabilities = petData.Vulnerabilities;
            BaseSex = petData.Sex;
            BaseSize = petData.Size;
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

            // Must be built before equiping
            BuildEquipmentSlots();

            // Equipped items
            if (petData.Equipments != null)
            {
                // Create item in inventory and try to equip it
                foreach (EquippedItemData equippedItemData in petData.Equipments)
                {
                    // Create in inventory
                    var item = World.AddItem(Guid.NewGuid(), equippedItemData.Item, this);

                    // Try to equip it
                    EquippedItem equippedItem = SearchEquipmentSlot(equippedItemData.Slot, false);
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
                    World.AddItem(Guid.NewGuid(), itemData, this);
            }
            // Auras
            if (petData.Auras != null)
            {
                foreach (AuraData auraData in petData.Auras)
                    AddAura(new Aura.Aura(auraData), false); // TODO: !!! auras is not added thru World.AddAura
            }

            Room = room;
            room.Enter(this);

            RecomputeKnownAbilities();
            ResetCurrentAttributes();
            RecomputeCurrentResourceKinds();
            BuildEquipmentSlots();
        }

        #region INonPlayableCharacter

        #region ICharacter

        #region IEntity

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands => NonPlayableCharacterCommands.Value;

        public override void Send(string message, bool addTrailingNewLine)
        {
            // TODO: use Act formatter ?
            base.Send(message, addTrailingNewLine);
            // TODO: do we really need to receive message sent to slave ?
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
            if (playableBeholder?.ImpersonatedBy is IAdmin)
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
            Room = World.NullRoom; // this will avoid a lot of problem, will be set to null in Cleanup phase
        }

        public override void OnCleaned() // called when removing definitively an entity from the game
        {
            Blueprint = null;
            Room = null;
        }

        #endregion

        public override int MaxCarryWeight => ActFlags.HasFlag(ActFlags.Pet)
            ? 0
            : base.MaxCarryWeight;

        public override int MaxCarryNumber => ActFlags.HasFlag(ActFlags.Pet)
            ? 0
            : base.MaxCarryNumber;

        // Combat
        public override void UpdatePosition()
        {
            if (HitPoints < 1)
            {
                Position = Positions.Dead;
                return;
            }
            base.UpdatePosition();
        }

        public override void MultiHit(ICharacter victim, IMultiHitModifier multiHitModifier) // 'this' starts a combat with 'victim'
        {
            // no attacks for stunnies
            if (Position <= Positions.Stunned)
                return;

            IItemWeapon mainHand = GetEquipment<IItemWeapon>(EquipmentSlots.MainHand);
            // main attack
            OneHit(victim, mainHand, multiHitModifier);
            if (Fighting != victim)
                return;
            if (multiHitModifier?.MaxAttackCount <= 1)
                return;
            // area attack
            if (OffensiveFlags.HasFlag(OffensiveFlags.AreaAttack))
            {
                IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(Room.People.Where(x => x != this && x.Fighting == this).ToList());
                foreach (ICharacter character in clone)
                    OneHit(character, mainHand, multiHitModifier);
            }
            // main hand haste attack
            if ((CharacterFlags.HasFlag(CharacterFlags.Haste) || OffensiveFlags.HasFlag(OffensiveFlags.Fast))
                && !CharacterFlags.HasFlag(CharacterFlags.Slow))
                OneHit(victim, mainHand, multiHitModifier);
            if (Fighting != victim)
                return;
            if (multiHitModifier?.MaxAttackCount <= 2)
                return;
            // main hand second attack
            var secondAttackLearnInfo = GetLearnInfo("Second attack");
            int secondAttackChance = secondAttackLearnInfo.learned / 2;
            if (CharacterFlags.HasFlag(CharacterFlags.Slow) && !OffensiveFlags.HasFlag(OffensiveFlags.Fast))
                secondAttackChance /= 2;
            if (RandomManager.Chance(secondAttackChance))
                OneHit(victim, mainHand, multiHitModifier);
            if (Fighting != victim)
                return;
            if (multiHitModifier?.MaxAttackCount <= 3)
                return;
            // main hand third attack
            var thirdAttackLearnInfo = GetLearnInfo("Third attack");
            int thirdAttackChance = thirdAttackLearnInfo.learned / 4;
            if (CharacterFlags.HasFlag(CharacterFlags.Slow) && !OffensiveFlags.HasFlag(OffensiveFlags.Fast))
                thirdAttackChance = 0;
            if (RandomManager.Chance(thirdAttackChance))
                OneHit(victim, mainHand, multiHitModifier);
            if (Fighting != victim)
                return;
            if (multiHitModifier?.MaxAttackCount <= 4)
                return;
            // fun stuff
            // TODO: if wait > 0 return
            int number = 0;//int number = RandomManager.Range(0, 8);
            switch (number)
            {
                case 0: if (OffensiveFlags.HasFlag(OffensiveFlags.Bash))
                        DoBash(string.Empty, Enumerable.Empty<CommandParameter>().ToArray());
                    break;
                case 1: if (OffensiveFlags.HasFlag(OffensiveFlags.Berserk) && !CharacterFlags.HasFlag(CharacterFlags.Berserk))
                        DoBerserk(string.Empty, null);
                    break;
                case 2: if (OffensiveFlags.HasFlag(OffensiveFlags.Disarm)
                        || ActFlags.HasFlag(ActFlags.Warrior) // TODO: check if weapon skill is not hand to hand
                        || ActFlags.HasFlag(ActFlags.Thief))
                        DoDisarm(string.Empty, null);
                    break;
                case 3: if (OffensiveFlags.HasFlag(OffensiveFlags.Kick))
                        DoKick(string.Empty, null);
                    break;
                case 4: if (OffensiveFlags.HasFlag(OffensiveFlags.DirtKick))
                        DoDirt(string.Empty, null);
                    break;
                case 5: if (OffensiveFlags.HasFlag(OffensiveFlags.Tail))
                        ; // TODO: see raceabilities.C:639
                    break;
                case 6: if (OffensiveFlags.HasFlag(OffensiveFlags.Trip))
                        DoTrip(null, null);
                    break;
                case 7: if (OffensiveFlags.HasFlag(OffensiveFlags.Crush))
                        ; // TODO: see raceabilities.C:525
                    break;
                case 8:
                    if (OffensiveFlags.HasFlag(OffensiveFlags.Backstab))
                        DoBackstab(string.Empty, null); // TODO: this will never works because we cannot backstab while in combat
                    break;
            }
        }

        public override void KillingPayoff(ICharacter victim)
        {
            // NOP
        }

        public override void DeathPayoff(ICharacter killer)
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

        public ActFlags ActFlags { get; protected set; }

        public OffensiveFlags OffensiveFlags { get; protected set; }

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

        public bool Order(string rawParameters, params CommandParameter[] parameters)
        {
            if (Master == null)
                return false;
            Act(ActOptions.ToCharacter, "{0:N} orders you to '{1}'.", Master, rawParameters);
            CommandHelpers.ExtractCommandAndParameters(CommandHelpers.JoinParameters(parameters), out string command, out rawParameters, out parameters);
            bool executed = ExecuteCommand(command, rawParameters, parameters);
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
        public override (int learned, KnownAbility knownAbility) GetWeaponLearnInfo(IItemWeapon weapon)
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

        public override (int learned, KnownAbility knownAbility) GetLearnInfo(IAbility ability) // TODO: replace with npc class
        {
            KnownAbility knownAbility = this[ability];
            //int learned = 0;
            //if (knownAbility != null && knownAbility.Level <= Level)
            //    learned = knownAbility.Learned;

            // TODO: spells
            int learned = 0;
            switch (ability.Name)
            {
                case "Sneak":
                case "Hide":
                    learned = 20 + 2 * Level;
                    break;
                case "Dodge":
                    if (OffensiveFlags.HasFlag(OffensiveFlags.Dodge))
                        learned = 2 * Level;
                    break;
                case "Parry":
                    if (OffensiveFlags.HasFlag(OffensiveFlags.Parry))
                        learned = 2 * Level;
                    break;
                case "Shield block":
                    learned = 10 + 2 * Level;
                    break;
                case "Second attack":
                    if (ActFlags.HasFlag(ActFlags.Warrior)
                        || ActFlags.HasFlag(ActFlags.Thief))
                        learned = 10 + 3 * Level;
                    break;
                case "Third attack":
                    if (ActFlags.HasFlag(ActFlags.Warrior))
                        learned = 4 * Level - 40;
                    break;
                case "Hand to hand":
                    learned = 40 + 2 * Level;
                    break;
                case "Trip":
                    if (OffensiveFlags.HasFlag(OffensiveFlags.Trip))
                        learned = 10 + 3 * Level;
                    break;
                case "Bash":
                    if (OffensiveFlags.HasFlag(OffensiveFlags.Bash))
                        learned = 10 + 3 * Level;
                    break;
                case "Disarm":
                    if (OffensiveFlags.HasFlag(OffensiveFlags.Disarm)
                        || ActFlags.HasFlag(ActFlags.Warrior)
                        || ActFlags.HasFlag(ActFlags.Thief))
                        learned = 20 + 3 * Level;
                    break;
                case "Berserk":
                    if (OffensiveFlags.HasFlag(OffensiveFlags.Berserk))
                        learned = 3 * Level;
                    break;
                case "Kick":
                    if (OffensiveFlags.HasFlag(OffensiveFlags.Kick))
                        learned = 10 + 3 * Level;
                    break;
                case "Backstab":
                    if (ActFlags.HasFlag(ActFlags.Thief))
                        learned = 20 + 2 * Level;
                    break;
                case "Rescue":
                    learned = 40 + Level;
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
            return (learned, knownAbility);
        }


        protected override (int hitGain, int moveGain, int manaGain) RegenBaseValues()
        {
            int hitGain = 5 + Level;
            int moveGain = Level;
            int manaGain = 5 + Level;
            if (CharacterFlags.HasFlag(CharacterFlags.Regeneration))
                hitGain *= 2;
            switch (Position)
            {
                case Positions.Sleeping:
                    hitGain = (3 * hitGain) / 2;
                    manaGain = (3 * manaGain) / 2;
                    break;
                case Positions.Resting:
                    // nop
                    break;
                case Positions.Fighting:
                    hitGain /= 3;
                    manaGain /= 3;
                    break;
                default:
                    hitGain /= 2;
                    manaGain /= 2;
                    break;
            }
            return (hitGain, moveGain, manaGain);
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
                AutoLook();
            }
        }

        protected override void HandleDeath()
        {
            World.RemoveCharacter(this);
        }

        protected override void HandleWimpy(int damage)
        {
            if (damage > 0) // TODO add test on wait < PULSE_VIOLENCE / 2
            {
                if ((ActFlags.HasFlag(ActFlags.Wimpy) && HitPoints < MaxHitPoints / 5 && RandomManager.Chance(25))
                    || (CharacterFlags.HasFlag(CharacterFlags.Charm) && Master != null && Master.Room != Room))
                    DoFlee(null, null);
            }
        }

        protected override (int thac0_00, int thac0_32) GetThac0()
        {
            if (Class != null)
                return Class.Thac0;

            int thac0_00 = 20;
            int thac0_32 = -4; // as good as thief

            if (ActFlags.HasFlag(ActFlags.Warrior))
                thac0_32 = -10;
            else if (ActFlags.HasFlag(ActFlags.Thief))
                thac0_32 = -4;
            else if (ActFlags.HasFlag(ActFlags.Cleric))
                thac0_32 = 2;
            else if (ActFlags.HasFlag(ActFlags.Mage))
                thac0_32 = 6;

            return (thac0_00, thac0_32);
        }

        protected override SchoolTypes NoWeaponDamageType => DamageType;

        protected override int NoWeaponBaseDamage => RandomManager.Dice(DamageDiceCount, DamageDiceValue) + DamageDiceBonus;

        protected override string NoWeaponDamageNoun => DamageNoun;

        #endregion
    }
}
