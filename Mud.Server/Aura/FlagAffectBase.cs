﻿using System;
using System.Text;
using Mud.Domain;
using Mud.Server.Helpers;

namespace Mud.Server.Aura
{
    public abstract class FlagAffectBase<T> : IAffect
        where T:Enum
    {
        public AffectOperators Operator { get; set; } // Add and Or are identical
        public T Modifier { get; set; }

        protected abstract string Target { get; }

        public void Append(StringBuilder sb)
        {
            sb.AppendFormat("%c%modifies %y%{0} %c%{1} %y%{2}%x%", Target, Operator.PrettyPrint(), Modifier);
        }

        public abstract AffectDataBase MapAffectData();
    }
}