using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server.Character
{
    public struct Immunities
    {
        public long Summon { get; set; }
        public long Charm { get; set; }
        public long Magic { get; set; }
        public long Weapon { get; set; }
        public long Bash { get; set; }
        public long Pierce { get; set; }
        public long Slash { get; set; }
        public long Fire { get; set; }
        public long Cold { get; set; }
        public long Ligthning { get; set; }
        public long Acid { get; set; }
        public long Poison { get; set; }
        public long Negative { get; set; }
        public long Holy { get; set; }
        public long Energy { get; set; }
        /* from OLC_VALUE.H
         #define IRV_SUMMON              (A)
#define IRV_CHARM               (B)
#define IRV_MAGIC               (C)
#define IRV_WEAPON              (D)
#define IRV_BASH                (E)
#define IRV_PIERCE              (F)
#define IRV_SLASH               (G)
#define IRV_FIRE                (H)
#define IRV_COLD                (I)
#define IRV_LIGHTNING           (J)
#define IRV_ACID                (K)
#define IRV_POISON              (L)
#define IRV_NEGATIVE            (M)
#define IRV_HOLY                (N)
#define IRV_ENERGY              (O)
#define IRV_MENTAL              (P)
#define IRV_DISEASE             (Q)
#define IRV_DROWNING            (R)
#define IRV_LIGHT		(S)
#define IRV_SOUND		(T)
#define IRV_PARALYSIS           (V)
#define IRV_WOOD                (X)
#define IRV_SILVER              (Y)
#define IRV_IRON                (Z)
// Added by SinaC 2001
#define IRV_DAYLIGHT            (aa)
// Added by SinaC 2003
#define IRV_EARTH               (bb)
// SinaC 2003
#define IRV_WEAKEN              (cc)
         */
    }
}
