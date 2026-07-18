using System;
using System.Collections.Generic;

namespace PipelinedCommands
{
    public interface ICommandMap
    {
        CommandBinding<TSignal> Bind<TSignal>();

        FlowBinding Flow();
        FlowBinding<T1> Flow<T1>();
        FlowBinding<T1, T2> Flow<T1, T2>();
        FlowBinding<T1, T2, T3> Flow<T1, T2, T3>();

        void Unbind(CommandBindingBase binding);
        void UnbindAll<TSignal>();
        void UnbindAll();

        void Break(CommandBindingBase binding);
        void BreakAll<TSignal>();

        IReadOnlyList<CommandBindingBase> GetBindings<TSignal>();
        void ForEachBinding(Action<CommandBindingBase> handler);
    }
}
