using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;
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

            // TODO: race, class, flags, armor, damage, ...
            Level = blueprint.Level;
            ActFlags = blueprint.ActFlags;
            OffensiveFlags = blueprint.OffensiveFlags;
            BaseCharacterFlags = blueprint.CharacterFlags;
            BaseImmunities = blueprint.Immunities;
            BaseResistances = blueprint.Resistances;
            BaseVulnerabilities = blueprint.Vulnerabilities;
            BaseSex = blueprint.Sex;
            Alignment = blueprint.Alignment.Range(-1000, 1000);
            RecomputeBaseAttributes(null);

            Room = room;
            room.Enter(this);

            RecomputeKnownAbilities();
            ResetAttributes();
            RecomputeCurrentResourceKinds();
            BuildEquipmentSlots();
        }

        #region INonPlayableCharacter

        #region ICharacter

        #region IEntity

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
                displayName.Append($" [{Blueprint?.Id.ToString() ?? " ??? "}]");
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

        #endregion

        #region CharacterBase

        // http://wow.gamepedia.com/Attack_power (Mob attack power)
        protected override int NoWeaponDamage => (Level * 50) / 14; // TODO: simulate weapon dps using level

        protected override int HitPointMinValue => 0;

        protected override IReadOnlyTrie<CommandMethodInfo> StaticCommands => NonPlayableCharacterCommands.Value;

        protected override bool BeforeMove(ExitDirections direction, IRoom fromRoom, IRoom toRoom)
        {
            return true;
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
