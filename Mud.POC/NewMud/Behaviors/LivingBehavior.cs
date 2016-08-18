namespace Mud.POC.NewMud.Behaviors
{
    // A behavior defining traits specific to living beings (IE players and mobiles).
    public class LivingBehavior : Behavior
    {
        public Consciousness Consciousness { get; set; }

        public LivingBehavior()
        {
            Consciousness = Consciousness.Awake;
        }
    }
}
