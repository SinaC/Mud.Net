using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.Domain.SerializationData.Avatar;
using Mud.Random;
using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Options;
using Mud.Server.Quest.Objectives;

namespace Mud.Server.Item;

[Item(typeof(ItemQuestBlueprint), typeof(ItemData))]
public class ItemQuest : ItemBase, IItemQuest
{
    public ItemQuest(ILogger<ItemQuest> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, gameActionManager, commandParser, messageForwardOptions, worldOptions, randomManager, auraManager)
    {
    }

    public void Initialize(Guid guid, ItemQuestBlueprint blueprint, IContainer containedInto) 
    {
        base.Initialize(guid, blueprint, $"Quest[{blueprint.Id}]", containedInto);

        UpdateQuestObjective(containedInto, false);
    }

    public void Initialize(Guid guid, ItemQuestBlueprint blueprint, ItemData itemData, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, itemData, containedInto);

        // don't call UpdateQuestObjective because it will increase objective item count each time the player reconnect
        // could maybe call it with force = true
    }

    #region ItemBase

    public override bool IsQuestObjective(IPlayableCharacter questingCharacter, bool checkCompleted)
    {
        return questingCharacter.ActiveQuests
            .Where(q => !checkCompleted || (checkCompleted && !q.AreObjectivesFulfilled))
            .SelectMany(q => q.Objectives.Where(x => !checkCompleted || (checkCompleted && !x.IsCompleted)))
            .OfType<ItemQuestObjectiveBase>()
            .Any(o => o.ItemBlueprint.Id == Blueprint.Id);
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
            foreach (IQuest quest in character.ActiveQuests)
                quest.Update(this, force);
        }
    }

}
