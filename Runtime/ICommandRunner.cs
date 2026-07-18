using System;

namespace PipelinedCommands
{
    internal interface ICommandRunner
    {
        void OnCommandFinished(CommandBase command);
        void OnCommandFailed(CommandBase command, Exception exception);
    }
}
