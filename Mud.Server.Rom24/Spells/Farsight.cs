using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;
using System.Text;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.None, PulseWaitTime = 36)]
[Syntax(
    "cast [spell]",
    "cast [spell] <direction>")]
[Help(
@"The farsight spell expands the caster's consciousness, allowing him or her
to see far away beings like they were in the same room.  It takes intense
concentration, often leaving the caster helpless for several minutes.
The spell may be used for a general scan that reaches a short distance in
all directions, or with a directional component to see creatures much
farther away.")]
public class Farsight : NoTargetSpellBase
{
    private const string SpellName = "Farsight";

    // {0} is the victim
    // {1} is the direction
    private static readonly string[] DistanceFormat = ["{0} right here.", "{0} nearby to the {1}.", "{0} not far {1}.", "{0} off in the distance {1}."];

    public Farsight(ILogger<Farsight> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected ExitDirections? Direction { get; set; } = default!;

    public override string? Setup(ISpellActionInput spellActionInput)
    {
        var baseSetup = base.Setup(spellActionInput);
        if (baseSetup != null)
            return baseSetup;

        if (Caster.Room == null)
            return "You are nowhere.";
        if (Caster.CharacterFlags.IsSet("Blind"))
            return "Maybe it would help if you could see?";
        if (Caster.Room.RoomFlags.IsSet("NoScan"))
            return "Your vision is clouded by a mysterious force.";
        if (spellActionInput.Parameters.Length > 0)
        {
            if (ExitDirectionsExtensions.TryFindDirection(spellActionInput.Parameters[0].Value, out ExitDirections direction))
            {
                Direction = direction;
                return null;
            }
            else
                return "Which way do you want to scan?";
        }
        return null;
    }

    protected override void Invoke()
    {
        // identical to Scan command
        StringBuilder sb = new(1024);
        if (Direction == null)
        {
            Caster.Act(ActOptions.ToRoom, "{0:N} looks all around.", Caster);
            // Current room
            foreach (ICharacter victim in Caster.Room.People.Where(x => Caster.CanSee(x)))
                sb.AppendFormatLine(DistanceFormat[0], victim.RelativeDisplayName(Caster));
            // Scan in each direction with a max distance of 1
            foreach (var direction in Enum.GetValues<ExitDirections>())
                ScanOneDirection(sb, direction, 1);
        }
        else
        {
            Caster.Act(ActOptions.ToAll, "{0:N} peer{0:v} intently {1}.", Caster, Direction.Value.DisplayName());
            sb.AppendFormatLine("Looking {0} you see:", Direction.Value.DisplayNameLowerCase());
            // Scan in specify direction with a max distance of 3
            ScanOneDirection(sb, Direction.Value, 3);
        }
        Caster.Send(sb);
    }

    private void ScanOneDirection(StringBuilder sb, ExitDirections direction, int maxDistance)
    {
        var currentRoom = Caster.Room; // starting point
        for (int distance = 1; distance < maxDistance+1; distance++)
        {
            var exit = currentRoom[direction];
            var destination = exit?.Destination;
            if (destination == null)
                break; // stop in that direction if no exit found or no linked room found
            if (destination.RoomFlags.IsSet("NoScan"))
                break; // no need to scan further
            if (exit?.IsClosed == true)
                break; // can't see thru closed door
            ScanRoom(sb, destination, direction, distance);
            currentRoom = destination;
        }
    }

    private void ScanRoom(StringBuilder sb, IRoom room, ExitDirections direction, int distance)
    {
        foreach (ICharacter victim in room.People.Where(x => Caster.CanSee(x)))
            sb.AppendFormatLine(DistanceFormat[distance], victim.RelativeDisplayName(Caster), direction);
    }
}
