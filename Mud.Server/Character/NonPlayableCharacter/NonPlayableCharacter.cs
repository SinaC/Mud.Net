using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Abilities;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
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
            // TODO: race, class, flags, armor, damage, IRV ...
            Level = blueprint.Level;
            ActFlags = blueprint.ActFlags;
            OffensiveFlags = blueprint.OffensiveFlags;
            BaseCharacterFlags = blueprint.CharacterFlags;
            BaseImmunities = blueprint.Immunities;
            BaseResistances = blueprint.Resistances;
            BaseVulnerabilities = blueprint.Vulnerabilities;
            BaseSex = blueprint.Sex;
            Alignment = blueprint.Alignment.Range(-1000, 1000);

            // TODO: see db.C:Create_Mobile
            int baseValue = Math.Min(25, 11 + Level / 4);
            SetBaseAttributes(CharacterAttributes.Strength, baseValue, false);
            SetBaseAttributes(CharacterAttributes.Intelligence, baseValue, false);
            SetBaseAttributes(CharacterAttributes.Wisdom, baseValue, false);
            SetBaseAttributes(CharacterAttributes.Dexterity, baseValue, false);
            SetBaseAttributes(CharacterAttributes.Constitution, baseValue, false);
            // TODO: use Act/Off/size to change values
            // TODO: following values must be extracted from in blueprint
            SetBaseAttributes(CharacterAttributes.MaxHitPoints, 1000, false);
            SetBaseAttributes(CharacterAttributes.SavingThrow, 0, false);
            SetBaseAttributes(CharacterAttributes.DamRoll, Level, false);
            SetBaseAttributes(CharacterAttributes.HitRoll, Level, false);
            SetBaseAttributes(CharacterAttributes.MaxMovePoints, 1000, false);
            SetBaseAttributes(CharacterAttributes.ArmorBash, -Level, false);
            SetBaseAttributes(CharacterAttributes.ArmorPierce, -Level, false);
            SetBaseAttributes(CharacterAttributes.ArmorSlash, -Level, false);
            SetBaseAttributes(CharacterAttributes.ArmorExotic, -Level, false);
            // resources (should be extracted from blueprint)
            foreach (var resource in EnumHelpers.GetValues<ResourceKinds>())
            {
                SetMaxResource(resource, 1000, false);
                this[resource] = 1000;
            }
            HitPoints = 1000; // can't use this[MaxHitPoints] because current has been been computed, it will be computed in ResetCurrentAttributes
            MovePoints = 1000;

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
            base.OnRemoved();

            StopFighting(true);
            Slave?.ChangeController(null);
            // TODO: what if character is incarnated
            ResetCooldowns();
            DeleteInventory();
            DeleteEquipments();
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
        public override void KillingPayoff(ICharacter victim)
        {
            // Nop
        }

        #endregion

        public CharacterBlueprintBase Blueprint { get; }

        public ActFlags ActFlags { get; protected set; }

        public OffensiveFlags OffensiveFlags { get; protected set; }

        public bool IsQuestObjective(IPlayableCharacter questingCharacter)
        {
            // If 'this' is NPC and in object list or in kill loot table
            return questingCharacter.Quests.Where(q => !q.IsCompleted).SelectMany(q => q.Objectives).OfType<KillQuestObjective>().Any(o => o.Blueprint.Id == Blueprint.Id)
                                     || questingCharacter.Quests.Where(q => !q.IsCompleted).Any(q => q.Blueprint.KillLootTable.ContainsKey(Blueprint.Id));
        }

        // Abilities
        public override int GetWeaponLearned(IItemWeapon weapon)
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

            return learned.Range(0, 100);
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
                    learned = 10 + 3 * Level; // TODO: if warrior
                    break;
                case "Third attack":
                    learned = 4 * Level - 40; // TODO: if warrior
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
                    if (OffensiveFlags.HasFlag(OffensiveFlags.Disarm)) // TODO: or warrior or thief
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
                case "Backstab": // TODO: if thief
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

        #endregion

        #region CharacterBase

        // http://wow.gamepedia.com/Attack_power (Mob attack power)
        protected override int NoWeaponDamage => (Level * 50) / 14; // TODO: simulate weapon dps using level

        protected override int HitPointMinValue => 0;

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

        protected override int ModifyCriticalDamage(int damage) => damage * 2; // TODO http://wow.gamepedia.com/Critical_strike

        protected override bool RawKilled(IEntity killer, bool killingPayoff) // TODO: refactor, same code in PlayableCharacter
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "RawKilled: {0} is not valid anymore", DebugName);
                return false;
            }

            ICharacter characterKiller = killer as ICharacter;

            Wiznet.Wiznet($"{DebugName} got toasted by {killer?.DebugName ?? "???"} at {Room?.DebugName ?? "???"}", WiznetFlags.MobDeaths);

            StopFighting(true);
            // Remove periodic auras
            IReadOnlyCollection<IPeriodicAura> periodicAuras = new ReadOnlyCollection<IPeriodicAura>(PeriodicAuras.ToList()); // clone
            foreach (IPeriodicAura pa in periodicAuras)
                RemovePeriodicAura(pa);
            // Remove auras
            IReadOnlyCollection<IAura> auras = new ReadOnlyCollection<IAura>(Auras.ToList()); // clone
            foreach (IAura aura in auras)
                RemoveAura(aura, false);
            // no need to recompute

            // Death cry
            ActToNotVictim(this, "You hear {0}'s death cry.", this);

            // Gain/lose xp/reputation   damage.C:32
            if (killingPayoff)
                characterKiller?.KillingPayoff(this);

            // Create corpse
            ItemCorpseBlueprint itemCorpseBlueprint = World.GetItemBlueprint<ItemCorpseBlueprint>(Settings.CorpseBlueprintId);
            if (itemCorpseBlueprint != null)
            {
                if (characterKiller != null)
                    World.AddItemCorpse(Guid.NewGuid(), itemCorpseBlueprint, Room, this, characterKiller);
                else
                    World.AddItemCorpse(Guid.NewGuid(), itemCorpseBlueprint, Room, this);
            }
            else
            {
                string msg = $"ItemCorpseBlueprint (id:{Settings.CorpseBlueprintId}) doesn't exist !!!";
                Log.Default.WriteLine(LogLevels.Error, msg);
                Wiznet.Wiznet(msg, WiznetFlags.Bugs);
            }

            //
            World.RemoveCharacter(this);
            return true;
        }

        #endregion
    }
}
