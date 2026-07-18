using System;

namespace PipelinedCommands
{
    /// <summary>Fluent binding: signal <typeparamref name="TSignal"/> → command chain.</summary>
    public sealed class CommandBinding<TSignal> : CommandBindingBase
    {
        private Func<TSignal, bool> _condition;
        private TSignal _triggerValue;
        private bool _hasTriggerValue;

        internal CommandBinding(CommandMap map) : base(map, typeof(TSignal))
        {
        }

        // ---- command steps ----

        /// <summary>Parameterless command.</summary>
        public CommandBinding<TSignal> To0<TCommand>() where TCommand : Command
        {
            AddStep(typeof(TCommand), EmptyStepParams.Instance);
            return this;
        }

        /// <summary>Alias of <see cref="To0{TCommand}"/>.</summary>
        public CommandBinding<TSignal> To<TCommand>() where TCommand : Command
        {
            return To0<TCommand>();
        }

        /// <summary>Command&lt;TSignal&gt; receives the fired signal.</summary>
        public CommandBinding<TSignal> To1<TCommand>() where TCommand : Command<TSignal>
        {
            AddStep(typeof(TCommand), TriggerPayloadStepParams.Instance);
            return this;
        }

        /// <summary>Command&lt;T1&gt; with fixed / lazy arg (ignores signal as param).</summary>
        public CommandBinding<TSignal> To1<TCommand, T1>(T1 param01) where TCommand : Command<T1>
        {
            AddStep(typeof(TCommand), new Fixed1StepParams(param01));
            return this;
        }

        public CommandBinding<TSignal> To1<TCommand, T1>(Func<T1> param01) where TCommand : Command<T1>
        {
            AddStep(typeof(TCommand), new Fixed1StepParams(() => param01()));
            return this;
        }

        /// <summary>Command&lt;TSignal, T2&gt; — signal + fixed second arg.</summary>
        public CommandBinding<TSignal> To2<TCommand, T2>(T2 param02) where TCommand : Command<TSignal, T2>
        {
            AddStep(typeof(TCommand), new Fixed2StepParams(null, param02, p1FromTrigger: true));
            return this;
        }

        /// <summary>Command&lt;T1, T2&gt; with two fixed args.</summary>
        public CommandBinding<TSignal> To2<TCommand, T1, T2>(T1 param01, T2 param02) where TCommand : Command<T1, T2>
        {
            AddStep(typeof(TCommand), new Fixed2StepParams(param01, param02));
            return this;
        }

        /// <summary>Command&lt;TSignal, T2, T3&gt;.</summary>
        public CommandBinding<TSignal> To3<TCommand, T2, T3>(T2 param02, T3 param03)
            where TCommand : Command<TSignal, T2, T3>
        {
            AddStep(typeof(TCommand), new Fixed3StepParams(null, param02, param03, p1FromTrigger: true));
            return this;
        }

        public CommandBinding<TSignal> To3<TCommand, T1, T2, T3>(T1 param01, T2 param02, T3 param03)
            where TCommand : Command<T1, T2, T3>
        {
            AddStep(typeof(TCommand), new Fixed3StepParams(param01, param02, param03));
            return this;
        }

        // ---- composite ----

        /// <summary>Also apply the same chain (steps already added) to another signal type.</summary>
        public CommandBindingComposite And<TOtherSignal>()
        {
            var other = Map.BindInternal<TOtherSignal>();
            other.CopyStepsFrom(this);
            other.CopyConfigFrom(this);
            return new CommandBindingComposite(this, other);
        }

        // ---- mode / once / conditions ----

        public CommandBinding<TSignal> InSequence()
        {
            SetMode(CommandExecutionMode.Sequence);
            return this;
        }

        public CommandBinding<TSignal> InParallel()
        {
            SetMode(CommandExecutionMode.Parallel);
            return this;
        }

        public CommandBinding<TSignal> Once() => Once(OnceBehavior.Default);

        public CommandBinding<TSignal> Once(OnceBehavior behavior)
        {
            SetOnce(behavior);
            return this;
        }

        public CommandBinding<TSignal> When(Func<TSignal, bool> condition)
        {
            _condition = condition;
            return this;
        }

        public CommandBinding<TSignal> When(TSignal exactValue)
        {
            _triggerValue = exactValue;
            _hasTriggerValue = true;
            return this;
        }

        public CommandBinding<TSignal> TriggerCondition(Func<TSignal, bool> condition) => When(condition);

        public CommandBinding<TSignal> TriggerCondition(TSignal exactValue) => When(exactValue);

        // ---- complete / break / fail ----

        public CommandBinding<TSignal> OnComplete(Action action)
        {
            SetOnComplete(action);
            return this;
        }

        public CommandBinding<TSignal> OnCompleteFire<TCompleteSignal>(Func<TCompleteSignal> factory = null)
            where TCompleteSignal : new()
        {
            SetCompleteSignalFactory(() => factory != null ? factory() : new TCompleteSignal());
            return this;
        }

        public CommandBinding<TSignal> OnBreak(Action action)
        {
            SetOnBreak(action);
            return this;
        }

        public CommandBinding<TSignal> OnBreakFire<TBreakSignal>(Func<TBreakSignal> factory = null)
            where TBreakSignal : new()
        {
            SetBreakSignalFactory(() => factory != null ? factory() : new TBreakSignal());
            return this;
        }

        public CommandBinding<TSignal> OnFail(Action<Exception> action)
        {
            SetOnFail(action);
            return this;
        }

        public CommandBinding<TSignal> OnFailFire<TFailSignal>(Func<Exception, TFailSignal> factory)
        {
            SetFailSignalFactory(ex => factory(ex));
            return this;
        }

        public CommandBinding<TSignal> OnFailFire<TFailSignal>() where TFailSignal : new()
        {
            SetFailSignalFactory(_ => new TFailSignal());
            return this;
        }

        public CommandBinding<TSignal> Out(out CommandBindingBase binding)
        {
            binding = this;
            return this;
        }

        internal bool CheckCondition(TSignal signal)
        {
            if (_hasTriggerValue && !Equals(_triggerValue, signal))
                return false;

            return _condition == null || _condition(signal);
        }
    }
}
