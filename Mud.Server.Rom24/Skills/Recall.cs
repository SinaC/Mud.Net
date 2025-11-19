using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;
using System.Text;

namespace Mud.Server.Rom24.Skills;

[CharacterCommand("recall", "Ability", "Skill", "Transportation")]
[Skill(SkillName, AbilityEffects.Transportation, LearnDifficultyMultiplier = 6)]
public class Recall : NoTargetSkillBase
{
    private const string SkillName = "Recall";

    public Recall(ILogger<Recall> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected IRoom RecallRoom { get; set; } = default!;

    public override string? Setup(ISkillActionInput skillActionInput)
    {
        var baseSetupResult = base.Setup(skillActionInput);
        if (baseSetupResult != null)
            return baseSetupResult;

        if (User is not IPlayableCharacter pcUser)
            return "Only players can recall.";

        pcUser.Act(ActOptions.ToRoom, "{0} prays for transportation!", User);

        RecallRoom = pcUser.RecallRoom;
        if (RecallRoom == null)
        {
            Logger.LogError("No recall room found for {0}", pcUser.DebugName);
            return "You are completely lost.";
        }

        if (pcUser.CharacterFlags.IsSet("Curse")
            || pcUser.Room.RoomFlags.IsSet("NoRecall"))
            return "Mota has forsaken you."; // TODO: message related to deity

        //if (recallRoom == pcUser.Room)
        //    return UseResults.InvalidTarget;

        return null;
    }

    protected override bool Invoke()
    {
        if (User is not IPlayableCharacter pcUser)
            return false;

        var originalRoom = pcUser.Room;

        if (pcUser.Fighting != null)
        {
            int chance = (80 * Learned) / 100;
            if (!RandomManager.Chance(chance))
            {
                pcUser.Send("You failed.");
                return false;
            }

            int lose = 50;
            pcUser.GainExperience(-lose);
            pcUser.Send("You recall from combat! You lose {0} exps.", lose);
            pcUser.StopFighting(true);
        }

        pcUser.UpdateMovePoints(-pcUser.MovePoints / 2); // half move
        pcUser.Act(ActOptions.ToRoom, "{0:N} disappears.", pcUser);
        pcUser.ChangeRoom(RecallRoom, false);
        pcUser.Act(ActOptions.ToRoom, "{0:N} appears in the room.", pcUser);

        StringBuilder sb = new ();
        pcUser.Room.Append(sb, pcUser);
        pcUser.Send(sb);

        // Pets follows
        foreach (var pet in pcUser.Pets)
        {
            // no recursive call because DoRecall has been coded for IPlayableCharacter
            if (pet.CharacterFlags.IsSet("Curse"))
                continue; // pet failing doesn't impact return value
            if (pet.Fighting != null)
            {
                if (!RandomManager.Chance(80))
                    continue;// pet failing doesn't impact return value
                pet.StopFighting(true);
            }

            pet.Act(ActOptions.ToRoom, "{0:N} disappears.", pet);
            pet.ChangeRoom(RecallRoom, false);
            pet.Act(ActOptions.ToRoom, "{0:N} appears in the room.", pet);
            // Needed ?
            //StringBuilder sbPet = new StringBuilder();
            //pet.Room.Append(sbPet, pet);
            //pet.Send(sbPet);
        }

        originalRoom?.Recompute();
        if (pcUser.Room != originalRoom)
            pcUser.Room.Recompute();

        return true;
    }
}
