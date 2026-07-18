using System;

namespace PipelinedCommands
{
    [Flags]
    public enum OnceBehavior
    {
        None = 0,

        UnbindOnComplete = 1 << 0,
        UnbindOnBreak = 1 << 1,
        UnbindOnFail = 1 << 2,
        UnbindOnTriggerFail = 1 << 3,

        Default = UnbindOnComplete | UnbindOnBreak | UnbindOnFail | UnbindOnTriggerFail,
        Anyway = Default
    }
}
