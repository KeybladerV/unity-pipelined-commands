using System;

namespace PipelinedCommands.Utility
{
    [Poolable]
    public class InvokeActionCommand : Command<Action>
    {
        public override void Execute(Action action)
        {
            action?.Invoke();
        }
    }
}