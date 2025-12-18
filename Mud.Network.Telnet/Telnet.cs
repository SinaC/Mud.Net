namespace Mud.Network.Telnet;

// http://mud-dev.wikidot.com/telnet:negotiation
internal static class Telnet
{
    // protocol
    public const byte IAC                   = 0xFF; // Marks the start of a negotiation process
    public const byte DONT                  = 0xFE; // Indicate unwillingness to negotiate
    public const byte DO                    = 0xFD; // Indicate willingness to negotiate
    public const byte WONT                  = 0xFC; // Confirm unwillingness to negotiate
    public const byte WILL                  = 0xFB; // Confirm willingness to negotiate
    public const byte SB                    = 0xFA; // The start of sub-negotiation options
    public const byte GA                    = 0xF9; // Usef for prompt marking
    public const byte EL                    = 0xF8;
    public const byte EC                    = 0xF7;
    public const byte AYT                   = 0xF6;
    public const byte AO                    = 0xF5;
    public const byte IP                    = 0xF4;
    public const byte BREAK                 = 0xF3;
    public const byte DM                    = 0xF2;
    public const byte NOP                   = 0xF1; // No operation (used for keep alive messages)
    public const byte SE                    = 0xF0; // the end of sub-negotiation options
    public const byte EOR                   = 0xEF;
    public const byte ABORT                 = 0xEE;
    public const byte SUSP                  = 0xED;
    public const byte xEOF                  = 0xEC;

    // options
    public const byte TELOPT_ECHO           = 0x01; // used to toggle local echo
    public const byte TELOPT_SGA            = 0x03; // used to toggle character mode
    public const byte TELOPT_TTYPE          = 0x18; // used to send client terminal type
    public const byte TELOPT_EOR            = 0x19; // used to toggle eor mode
    public const byte TELOPT_NAWS           = 0x1F; // Windows size option
    public const byte TELOPT_NEW_ENVIRON    = 0x27; // Environment variables
    public const byte TELOPT_CHARSET        = 0x2A; // used to detect UTF-8 support
    public const byte TELOPT_MSDP           = 0x45; // used to send mud server data
    public const byte TELOPT_MSSP           = 0x46; // used to send mud server information
    public const byte TELOPT_MCCP2          = 0x56; // used to toggle Mud Client Compression Protocol v2
    public const byte TELOPT_MCCP3          = 0x57; // used to toggle Mud Client Compression Protocol v3
    public const byte TELOPT_MSP            = 0x5A; // used to toggle Mud Sound Protocol
    public const byte TELOPT_MXP            = 0x5B; // used to toggle Mud Extention Protocol
    public const byte TELOPT_GMCP           = 0xC9;

    public const byte ENV_IS                = 0x00; // Sub-negotiation IS command
    public const byte ENV_SEND              = 0x01; // Sub-negotiation SEND command
    public const byte ENV_INFO              = 0x02; // Sub-negotiation INFO command

    public const byte ENV_VAR               = 0x00; // NEW-ENVIRON command
    public const byte ENV_VALUE             = 0x01; // NEW-ENVIRON command
    public const byte ENV_ESC               = 0x02; // NEW-ENVIRON command
    public const byte ENV_USERVAR           = 0x03; // NEW-ENVIRON command

    public const byte CHARSET_REQUEST       = 0x01;
    public const byte CHARSET_ACCEPTED      = 0x02;
    public const byte CHARSET_REJECTED      = 0x03;

    public const byte MSSP_VAR              = 0x01;
    public const byte MSSP_VAL              = 0x02;

    public const byte MSDP_VAR              = 0x01;
    public const byte MSDP_VAL             = 0x02;
    public const byte MSDP_TABLE_OPEN      = 0x03;
    public const byte MSDP_TABLE_CLOSE     = 0x04;
    public const byte MSDP_ARRAY_OPEN      = 0x05;
    public const byte MSDP_ARRAY_CLOSE     = 0x06;
}
