using System;

namespace PipelinedCommands
{
    /// <summary>
    /// Applies the same command chain / options to multiple signal bindings.
    /// </summary>
    public sealed class CommandBindingComposite
    {
        private readonly CommandBindingBase[] _bindings;

        internal CommandBindingComposite(params CommandBindingBase[] bindings)
        {
            _bindings = bindings;
        }

        public CommandBindingComposite And<TSignal>()
        {
            var source = _bindings[0];
            var next = source.Map.BindInternal<TSignal>();
            next.CopyStepsFrom(source);
            next.CopyConfigFrom(source);

            var list = new CommandBindingBase[_bindings.Length + 1];
            Array.Copy(_bindings, list, _bindings.Length);
            list[^1] = next;
            return new CommandBindingComposite(list);
        }

        public CommandBindingComposite To0<TCommand>() where TCommand : Command
        {
            foreach (var b in _bindings)
                b.AddStepInternal(typeof(TCommand), EmptyStepParams.Instance);
            return this;
        }

        public CommandBindingComposite To<TCommand>() where TCommand : Command => To0<TCommand>();

        public CommandBindingComposite To1<TCommand, T1>(T1 param01) where TCommand : Command<T1>
        {
            foreach (var b in _bindings)
                b.AddStepInternal(typeof(TCommand), new Fixed1StepParams(param01));
            return this;
        }

        public CommandBindingComposite To1<TCommand, T1>(Func<T1> param01) where TCommand : Command<T1>
        {
            foreach (var b in _bindings)
                b.AddStepInternal(typeof(TCommand), new Fixed1StepParams(() => param01()));
            return this;
        }

        public CommandBindingComposite To2<TCommand, T1, T2>(T1 param01, T2 param02) where TCommand : Command<T1, T2>
        {
            foreach (var b in _bindings)
                b.AddStepInternal(typeof(TCommand), new Fixed2StepParams(param01, param02));
            return this;
        }

        public CommandBindingComposite To3<TCommand, T1, T2, T3>(T1 p1, T2 p2, T3 p3)
            where TCommand : Command<T1, T2, T3>
        {
            foreach (var b in _bindings)
                b.AddStepInternal(typeof(TCommand), new Fixed3StepParams(p1, p2, p3));
            return this;
        }

        public CommandBindingComposite InSequence() =>
            ForEach(b => b.ApplyMode(CommandExecutionMode.Sequence));

        public CommandBindingComposite InParallel() =>
            ForEach(b => b.ApplyMode(CommandExecutionMode.Parallel));

        public CommandBindingComposite Once() => Once(OnceBehavior.Default);

        public CommandBindingComposite Once(OnceBehavior behavior) =>
            ForEach(b => b.ApplyOnce(behavior));

        public CommandBindingComposite OnComplete(Action action) =>
            ForEach(b => b.ApplyOnComplete(action));

        public CommandBindingComposite OnBreak(Action action) =>
            ForEach(b => b.ApplyOnBreak(action));

        public CommandBindingComposite OnFail(Action<Exception> action) =>
            ForEach(b => b.ApplyOnFail(action));

        public CommandBindingComposite OnCompleteFire<TSignal>() where TSignal : new() =>
            ForEach(b => b.ApplyCompleteSignal(() => new TSignal()));

        public CommandBindingComposite OnBreakFire<TSignal>() where TSignal : new() =>
            ForEach(b => b.ApplyBreakSignal(() => new TSignal()));

        public CommandBindingComposite OnFailFire<TFailSignal>() where TFailSignal : new() =>
            ForEach(b => b.ApplyFailSignal(_ => new TFailSignal()));

        private CommandBindingComposite ForEach(Action<CommandBindingBase> apply)
        {
            foreach (var b in _bindings)
                apply(b);
            return this;
        }
    }
}
