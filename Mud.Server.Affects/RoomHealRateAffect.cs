﻿using System.Text;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Affects
{
    public class RoomHealRateAffect : IRoomHealRateAffect
    {
        public int Modifier { get; set; }

        public AffectOperators Operator { get; set; }

        public RoomHealRateAffect()
        {
        }

        public RoomHealRateAffect(RoomHealRateAffectData data)
        {
            Modifier = data.Modifier;
            Operator = data.Operator;
        }

        public void Append(StringBuilder sb)
        {
            sb.AppendFormat("%c%modifies %y%heal rate %c%{0} %y%{1}%%x%", Operator.PrettyPrint(), Modifier.ToString());
        }

        public void Apply(IRoom room)
        {
            room.ApplyAffect(this);
        }

        public AffectDataBase MapAffectData()
        {
            return new RoomHealRateAffectData
            {
                Operator = Operator,
                Modifier = Modifier
            };
        }
    }
}
