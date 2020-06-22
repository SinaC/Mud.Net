namespace Mud.POC.GroupsPetsFollowers
{

    public enum CommandExecutionResults
    {
        Ok,
        SyntaxError, // will display command syntax
        SyntaxErrorNoDisplay, // will NOT display command syntax
        TargetNotFound, // item/character/room/... has not been found
        InvalidParameter, // parameter invalid such as negative number
        InvalidTarget, // target cannot be used for this command
        NoExecution, //
        Error // will display an error in log and AfterCommand will not be executed
    }
}
