using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Quest;

namespace Mud.Server.Character.NonPlayableCharacter
{
    public class NonPlayableCharacter : CharacterBase, INonPlayableCharacter
    {
        private static readonly Lazy<IReadOnlyTrie<CommandMethodInfo>> NonPlayableCharacterCommands = new Lazy<IReadOnlyTrie<CommandMethodInfo>>(() => CommandHelpers.GetCommands(typeof(NonPlayableCharacter)));

        public NonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, IRoom room) // NPC
            : base(guid, blueprint.Name, blueprint.Description)
        {
            Blueprint = blueprint;

            // TODO: race, class, flags, armor, damage, ...
            Sex = blueprint.Sex;
            Level = blueprint.Level;

            Room = room;
            room.Enter(this);

            RecomputeBaseAttributes();
            RecomputeKnownAbilities();
            ResetAttributes(true);
            RecomputeCommands();
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

        public bool IsQuestObjective(IPlayableCharacter questingCharacter)
        {
            // If 'this' is NPC and in object list or in kill loot table
            return questingCharacter.Quests.Where(q => !q.IsCompleted).SelectMany(q => q.Objectives).OfType<KillQuestObjective>().Any(o => o.Blueprint.Id == Blueprint.Id)
                                     || questingCharacter.Quests.Where(q => !q.IsCompleted).Any(q => q.Blueprint.KillLootTable.ContainsKey(Blueprint.Id));
        }

        #endregion

        #region CharacterBase

        // http://wow.gamepedia.com/Attack_power (Mob attack power)
        protected override int NoWeaponDamage => (Level * this[SecondaryAttributeTypes.AttackPower]) / 14; // TODO: simulate weapon dps using level

        protected override int HitPointMinValue => 0;

        protected override IReadOnlyTrie<CommandMethodInfo> StaticCommands => NonPlayableCharacterCommands.Value;

        protected override int ModifyCriticalDamage(int damage) => damage * 2; // TODO http://wow.gamepedia.com/Critical_strike

        protected override bool RawKilled(ICharacter killer, bool killingPayoff)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "RawKilled: {0} is not valid anymore", DebugName);
                return false;
            }

            string wiznetMsg;
            if (killer != null)
                wiznetMsg = $"{DebugName} got toasted by {killer.DebugName ?? "???"} at {Room?.DebugName ?? "???"}";
            else
                wiznetMsg = $"{DebugName} got toasted by an unknown source at {Room?.DebugName ?? "???"}";
            Wiznet.Wiznet(wiznetMsg, WiznetFlags.Deaths);

            StopFighting(true);
            // Remove periodic auras
            List<IPeriodicAura> periodicAuras = new List<IPeriodicAura>(PeriodicAuras); // clone
            foreach (IPeriodicAura pa in periodicAuras)
                RemovePeriodicAura(pa);
            // Remove auras
            List<IAura> auras = new List<IAura>(Auras); // clone
            foreach (IAura aura in auras)
                RemoveAura(aura, false);
            // no need to recompute

            // Death cry
            ActToNotVictim(this, "You hear {0}'s death cry.", this);

            // Gain/lose xp/reputation   damage.C:32
            if (killingPayoff)
                killer?.KillingPayoff(this);

            // Create corpse
            ItemCorpseBlueprint itemCorpseBlueprint = World.GetItemBlueprint<ItemCorpseBlueprint>(Settings.CorpseBlueprintId);
            if (itemCorpseBlueprint != null)
            {
                if (killer != null)
                    World.AddItemCorpse(Guid.NewGuid(), itemCorpseBlueprint, Room, this, killer);
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
