┌─────────────────────────────┐
│         Player/Mob           │
│  - CurrentRoom               │
│  - StatusEffects (List)      │
│  - Skills                    │
│  - Modifiers (computed)      │
└─────────────┬───────────────┘
              │
              │ Applies / Adds
              ▼
┌─────────────────────────────┐
│       StatusEffect           │  <-- Instance
│  - Type (Sneak, Poison...)   │
│  - Duration / RemainingTicks │
│  - Level                      │
│  - Modifiers (hit/dam/AC...) │
│  - Rule (IStatusEffectRule)  │
└─────────────┬───────────────┘
              │ Executes
              ▼
┌─────────────────────────────┐
│    IStatusEffectRule         │  <-- Shared logic class
│  - OnApply(Mob, World)       │
│  - OnTick(Mob, World)        │
│  - OnRemove(Mob, World)      │
│  - ModifyCombat(CombatCtx)   │
│  - ModifyDetection(DetectCtx)│
└─────────────┬───────────────┘
              │ Called by
              ▼
┌─────────────────────────────┐
│       Combat Engine          │
│  - MultiHitAction            │
│  - SkillAction               │
│  - DamageAction              │
│  - Backstab, Bash, Trip      │
│  - Sanctuary, Poison effects │
└─────────────┬───────────────┘
              │ Uses
              ▼
┌─────────────────────────────┐
│     Detection Engine         │
│  - Sneak / Hide rules        │
│  - DetectHidden, DetectInvis │
│  - AutoAssist targeting      │
└─────────────┬───────────────┘
              │ Uses
              ▼
┌─────────────────────────────┐
│       SkillAction            │
│  - References StatusEffect   │
│  - Executes IStatusEffectRule│
│  - Handles cooldown & chance │
└─────────────────────────────┘