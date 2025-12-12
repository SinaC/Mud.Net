using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Domain.SerializationData;
using Mud.Server.Interfaces.Affect.Room;
using Mud.Server.Interfaces.Room;
using System.Text;

namespace Mud.Server.Affects.Room;

[Affect("RoomResourceRateAffect", typeof(RoomResourceRateAffectData))]
public class RoomResourceRateAffect : IRoomResourceRateAffect
{
    public int Modifier { get; set; }

    public AffectOperators Operator { get; set; }

    public RoomResourceRateAffect()
    {
    }

    public void Initialize(RoomResourceRateAffectData data)
    {
        Modifier = data.Modifier;
        Operator = data.Operator;
    }

    public void Append(StringBuilder sb)
    {
        sb.AppendFormat("%c%modifies %y%resource rate %c%{0} %y%{1}%%x%", Operator.PrettyPrint(), Modifier.ToString());
    }

    public void Apply(IRoom room)
    {
        room.ApplyAffect(this);
    }

    public AffectDataBase MapAffectData()
    {
        return new RoomResourceRateAffectData
        {
            Operator = Operator,
            Modifier = Modifier
        };
    }
}
