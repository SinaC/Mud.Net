using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.DataStructures.Trie;
using Mud.Domain.SerializationData;
using Mud.Blueprints.Item;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using Mud.Server.Quest;

namespace Mud.Server.Item;

[Item(typeof(ItemQuestBlueprint), typeof(ItemData))]
public class ItemQuest : ItemBase, IItemQuest
{
    public ItemQuest(ILogger<ItemQuest> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager)
        : base(logger, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager)
    {
    }

    public void Initialize(Guid guid, ItemQuestBlueprint blueprint, IContainer containedInto) 
    {
        base.Initialize(guid, blueprint, containedInto);

        UpdateQuestObjective(containedInto, false);
    }

    public void Initialize(Guid guid, ItemQuestBlueprint blueprint, ItemData itemData, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, itemData, containedInto);

        // don't call UpdateQuestObjective because it will increase objective item count each time the player reconnect
        // could maybe call it with force = true
    }

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<ItemQuest>();

    #endregion

    #region ItemBase

    public override bool IsQuestObjective(IPlayableCharacter questingCharacter)
    {
        return questingCharacter.Quests.Where(q => !q.IsCompleted).SelectMany(q => q.Objectives).OfType<ItemQuestObjective>().Any(o => o.Blueprint.Id == Blueprint.Id);
    }

    public override bool ChangeContainer(IContainer? container)
    {
        // TODO: cannot be get if not on that quest
        bool baseResult = base.ChangeContainer(container);
        if (baseResult)
            UpdateQuestObjective(container, false);
        return baseResult;
    }

    #endregion

    private void UpdateQuestObjective(IContainer? container, bool force)
    {
        if (container is IPlayableCharacter character)
        {
            foreach (IQuest quest in character.Quests)
                quest.Update(this, force);
        }
    }

}
