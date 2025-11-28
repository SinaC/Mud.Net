using Microsoft.Extensions.Options;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Options;
using System.Text;

namespace Mud.Server.Commands.Character.PlayableCharacter.Group;

[PlayableCharacterCommand("group", "Group", "Information")]
[Syntax(
       "[cmd]",
       "[cmd] <character>")]
[Help(
@"[cmd] <character> makes someone who is following you a member of your group.
Group members share experience points from kills and may use the GTELL and
SPLIT commands.  If anyone in your group is attacked, you will automatically
join the fight.")]
public class Group : PlayableCharacterGameAction
{
    private int MaxLevel { get; }

    public Group(IOptions<WorldOptions> worldOptions)
    {
        MaxLevel = worldOptions.Value.MaxLevel;
    }

    protected enum Actions
    {
        DisplayPets,
        DisplayGroup,
        Create,
        Add,
        Remove
    }

    protected Actions Action { get; set; }
    protected IPlayableCharacter Whom { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        // no parameter: display group/pets info
        if (actionInput.Parameters.Length == 0)
        {
            if (Actor.Group == null)
            {
                // not in a group and no pet
                if (!Actor.Pets.Any())
                    return "You aren't in a group.";
                // not in a group but pets
                Action = Actions.DisplayPets;
                return null;
            }
            Action = Actions.DisplayGroup;
            return null;
        }

        // add/remove member to group
        Whom = FindHelpers.FindByName(Actor.Room.PlayableCharacters, actionInput.Parameters[0])!;
        if (Whom == null)
            return "They aren't here.";

        // can't group ourself
        if (Whom == Actor)
            return "You can't group yourself.";

        // we are not in a group -> add
        if (Actor.Group == null)
        {
            if (Whom.Group != null)
                return Actor.ActPhrase("{0:N} is already in a group.", Whom);
            Action = Actions.Create;
            return null;
        }
        // we are in a group -> add or remove
        // only the leader can add or remove
        if (Actor.Group.Leader != Actor)
            return "You are not the group leader.";
        // if target is already in a group -> remove or nop
        if (Whom.Group != null)
        {
            // not in the same group
            if (Whom.Group != Actor.Group)
                return "{0:N} is already in a group.";
            Action = Actions.Remove;
            return null;
        }

        // add target in the group
        Action = Actions.Add;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        switch (Action)
        {
            case Actions.DisplayPets:
                {
                    StringBuilder sb = new();
                    foreach (var pet in Actor.Pets)
                        AppendPetGroupMemberInfo(sb, pet);
                    Actor.Send(sb);
                    return;
                }
            case Actions.DisplayGroup:
                {
                    StringBuilder sb = new();
                    sb.AppendFormatLine("{0}'s group:", Actor.Group!.Leader.DisplayName);
                    foreach (var member in Actor.Group.Members)
                    {
                        // display member info
                        AppendPlayerGroupMemberInfo(sb, member);
                        if (member.Pets.Any())
                        {
                            // display member's pet info
                            foreach (INonPlayableCharacter pet in member.Pets)
                                AppendPetGroupMemberInfo(sb, pet);
                        }
                    }
                    Actor.Send(sb);
                    return;
                }
            case Actions.Create:
                {
                    // create a new group with Actor as Leader
                    var group = new Server.Group.Group(Actor);
                    // add target in the group
                    group.AddMember(Whom);
                    // assign group to actor
                    Actor.ChangeGroup(group);
                    return;
                }
            case Actions.Remove:
                // remove target from the group or disband
                if (Actor.Group!.Members.Count() <= 2) // group will contain only one member, disband
                    Actor.Group.Disband();
                else
                    Actor.Group.RemoveMember(Whom); // simple remove
                return;
            case Actions.Add:
                Actor.Group!.AddMember(Whom);
                return;
        }
    }

    private void AppendPlayerGroupMemberInfo(StringBuilder sb, IPlayableCharacter member)
    {
        sb.AppendFormat("[{0,3} {1,3}] {2,20} {3,5}/{4,5} hp {5} {6,5}/{7,5} mv", member.Level, member.Class.ShortName, member.DisplayName.MaxLength(20), member.HitPoints, member.MaxHitPoints, BuildResources(member), member.MovePoints, member.MaxMovePoints);
        if (member.Level >= MaxLevel)
            sb.AppendFormat(" {0} nxt", member.ExperienceToLevel);
        sb.AppendLine();
    }

    private void AppendPetGroupMemberInfo(StringBuilder sb, INonPlayableCharacter member)
    {
        sb.AppendFormatLine("[{0,3} Pet] {1,20} {2,5}/{3,5} hp {4} {5,5}/{6,5} mv", member.Level, member.DisplayName.MaxLength(20), member.HitPoints, member.MaxHitPoints, BuildResources(member), member.MovePoints, member.MaxMovePoints);
    }

    private static StringBuilder BuildResources(ICharacter character)
    {
        StringBuilder sb = new ();
        foreach (ResourceKinds resource in character.CurrentResourceKinds)
            sb.AppendFormat("{0,5}/{1,5} {2}", character[resource], character.MaxResource(resource), resource.ToString().ToLowerInvariant());
        return sb;
    }
}
