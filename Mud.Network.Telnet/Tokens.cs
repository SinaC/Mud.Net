namespace Mud.Network.Telnet
{
    // http://mud-dev.wikidot.com/telnet:negotiation
    internal class Tokens
    {
        public const byte Iac = 0xFF; // Marks the start of a negotiation process
        public const byte Will = 0xFB; // Confirm willingness to negotiate
        public const byte Wont = 0xFC; // Confirm unwillingness to negotiate
        public const byte Do = 0xFD; // Indicate willingness to negotiate
        public const byte Dont = 0xFE; // Indicate unwillingness to negotiate
        public const byte Nop = 0xF1; // No operation
        public const byte Sb = 0xFA; // The start of sub-negotiation options
        public const byte Se = 0xF0; // the end of sub-negotiation options
        public const byte Is = 0x00; // Sub-negotiation IS command
        public const byte Send = 0x01; // Sub-negotiation SEND command

        public const byte Naws = 0x1F; // Windows size option

        public const byte NewEnviron = 0x27; // Environment variables
        public const byte Info = 0x02; // Sub-negotiation INFO command
        public const byte Var = 0x00; // NEW-ENVIRON command
        public const byte Value = 0x01; // NEW-ENVIRON command
        public const byte Esc = 0x02; // NEW-ENVIRON command
        public const byte Uservar = 0x03; // NEW-ENVIRON command


    }
}
