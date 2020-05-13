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
        public override void UpdatePosition()
        {
            if (HitPoints < 1)
            {
                Position = Positions.Dead;
                return;
            }
            base.UpdatePosition();
        }

        public override void MultiHit(ICharacter victim) // 'this' starts a combat with 'victim'
        {
            // no attacks for stunnies
            if (Position <= Positions.Stunned)
                return;

            IItemWeapon mainHand = GetEquipment<IItemWeapon>(EquipmentSlots.MainHand);
            // main attack
            OneHit(victim, mainHand);
            if (Fighting != victim)
                return;
            // area attack
            if (OffensiveFlags.HasFlag(OffensiveFlags.AreaAttack))
            {
                IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(Room.People.Where(x => x != this && x.Fighting == this).ToList());
                foreach (ICharacter character in clone)
                    OneHit(character, mainHand);
            }
            // main hand haste attack
            if ((CharacterFlags.HasFlag(CharacterFlags.Haste) || OffensiveFlags.HasFlag(OffensiveFlags.Fast))
                && !CharacterFlags.HasFlag(CharacterFlags.Slow))
                OneHit(victim, mainHand);
            if (Fighting != victim) // TODO: or stop here for backstab
                return;
            // main hand second attack
            var secondAttackLearnInfo = GetLearnInfo("Second attack");
            int secondAttackChance = secondAttackLearnInfo.learned / 2;
            if (CharacterFlags.HasFlag(CharacterFlags.Slow) && !OffensiveFlags.HasFlag(OffensiveFlags.Fast))
                secondAttackChance /= 2;
            if (RandomManager.Chance(secondAttackChance))
                OneHit(victim, mainHand);
            if (Fighting != victim)
                return;
            // main hand third attack
            var thirdAttackLearnInfo = GetLearnInfo("Third attack");
            int thirdAttackChance = thirdAttackLearnInfo.learned / 4;
            if (CharacterFlags.HasFlag(CharacterFlags.Slow) && !OffensiveFlags.HasFlag(OffensiveFlags.Fast))
                thirdAttackChance = 0;
            if (RandomManager.Chance(thirdAttackChance))
                OneHit(victim, mainHand);
            if (Fighting != victim)
                return;
            // fun stuff
            // TODO: if wait > 0 return
            int number = RandomManager.Range(0, 8);
            switch (number)
            {
                case 0: if (OffensiveFlags.HasFlag(OffensiveFlags.Bash))
                        DoBash(null, null);
                    break;
                case 1: if (OffensiveFlags.HasFlag(OffensiveFlags.Berserk) && !CharacterFlags.HasFlag(CharacterFlags.Berserk))
                        DoBerserk(null, null);
                    break;
                case 2: if (OffensiveFlags.HasFlag(OffensiveFlags.Disarm)) // TODO: also if wielding a weapon and ActFlags.Warrior or ActFlags.Thief
                        DoDisarm(null, null);
                    break;
                case 3: if (OffensiveFlags.HasFlag(OffensiveFlags.Kick))
                        DoKick(null, null);
                    break;
                case 4: if (OffensiveFlags.HasFlag(OffensiveFlags.DirtKick))
                        DoDirt(null, null);
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
                        DoBackstab(null, null); // TODO: this will never works because we cannot backstab while in combat
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
                    || (CharacterFlags.HasFlag(CharacterFlags.Charm) && ControlledBy != null && ControlledBy.Room != Room))
                    DoFlee(null, null);
            }
        }

        #endregion
    }
}
