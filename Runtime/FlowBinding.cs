using System;

namespace PipelinedCommands
{
    public sealed class FlowBinding : CommandBindingBase
    {
        internal FlowBinding(CommandMap map) : base(map, signalType: null)
        {
        }

        public FlowBinding To0<TCommand>() where TCommand : Command
        {
            AddStep(typeof(TCommand), EmptyStepParams.Instance);
            return this;
        }

        public FlowBinding To<TCommand>() where TCommand : Command => To0<TCommand>();

        public FlowBinding To1<TCommand, T1>(T1 param01) where TCommand : Command<T1>
        {
            AddStep(typeof(TCommand), new Fixed1StepParams(param01));
            return this;
        }

        public FlowBinding To1<TCommand, T1>(Func<T1> param01) where TCommand : Command<T1>
        {
            AddStep(typeof(TCommand), new Fixed1StepParams(() => param01()));
            return this;
        }

        public FlowBinding To2<TCommand, T1, T2>(T1 p1, T2 p2) where TCommand : Command<T1, T2>
        {
            AddStep(typeof(TCommand), new Fixed2StepParams(p1, p2));
            return this;
        }

        public FlowBinding To3<TCommand, T1, T2, T3>(T1 p1, T2 p2, T3 p3) where TCommand : Command<T1, T2, T3>
        {
            AddStep(typeof(TCommand), new Fixed3StepParams(p1, p2, p3));
            return this;
        }

        public FlowBinding InSequence()
        {
            SetMode(CommandExecutionMode.Sequence);
            return this;
        }

        public FlowBinding InParallel()
        {
            SetMode(CommandExecutionMode.Parallel);
            return this;
        }

        public FlowBinding OnComplete(Action action)
        {
            SetOnComplete(action);
            return this;
        }

        public FlowBinding OnBreak(Action action)
        {
            SetOnBreak(action);
            return this;
        }

        public FlowBinding OnFail(Action<Exception> action)
        {
            SetOnFail(action);
            return this;
        }

        public FlowBinding OnCompleteFire<TSignal>() where TSignal : new()
        {
            SetCompleteSignalFactory(() => new TSignal());
            return this;
        }

        public FlowBinding OnBreakFire<TSignal>() where TSignal : new()
        {
            SetBreakSignalFactory(() => new TSignal());
            return this;
        }

        public FlowBinding OnFailFire<TSignal>() where TSignal : new()
        {
            SetFailSignalFactory(_ => new TSignal());
            return this;
        }

        public FlowBinding Out(out CommandBindingBase binding)
        {
            binding = this;
            return this;
        }

        public void Execute() => Map.ExecuteFlow(this, null);
    }

    public sealed class FlowBinding<T1> : CommandBindingBase
    {
        internal FlowBinding(CommandMap map) : base(map, signalType: null)
        {
        }

        public FlowBinding<T1> To0<TCommand>() where TCommand : Command
        {
            AddStep(typeof(TCommand), EmptyStepParams.Instance);
            return this;
        }

        public FlowBinding<T1> To1<TCommand>() where TCommand : Command<T1>
        {
            AddStep(typeof(TCommand), TriggerPayloadStepParams.Instance);
            return this;
        }

        public FlowBinding<T1> To1<TCommand, TP1>(TP1 param01) where TCommand : Command<TP1>
        {
            AddStep(typeof(TCommand), new Fixed1StepParams(param01));
            return this;
        }

        public FlowBinding<T1> To2<TCommand, T2>(T2 param02) where TCommand : Command<T1, T2>
        {
            AddStep(typeof(TCommand), new Fixed2StepParams(null, param02, p1FromTrigger: true));
            return this;
        }

        public FlowBinding<T1> To2<TCommand, TP1, T2>(TP1 p1, T2 p2) where TCommand : Command<TP1, T2>
        {
            AddStep(typeof(TCommand), new Fixed2StepParams(p1, p2));
            return this;
        }

        public FlowBinding<T1> To3<TCommand, T2, T3>(T2 p2, T3 p3) where TCommand : Command<T1, T2, T3>
        {
            AddStep(typeof(TCommand), new Fixed3StepParams(null, p2, p3, p1FromTrigger: true));
            return this;
        }

        public FlowBinding<T1> InSequence()
        {
            SetMode(CommandExecutionMode.Sequence);
            return this;
        }

        public FlowBinding<T1> InParallel()
        {
            SetMode(CommandExecutionMode.Parallel);
            return this;
        }

        public FlowBinding<T1> OnComplete(Action action)
        {
            SetOnComplete(action);
            return this;
        }

        public FlowBinding<T1> OnBreak(Action action)
        {
            SetOnBreak(action);
            return this;
        }

        public FlowBinding<T1> OnFail(Action<Exception> action)
        {
            SetOnFail(action);
            return this;
        }

        public FlowBinding<T1> OnCompleteFire<TSignal>() where TSignal : new()
        {
            SetCompleteSignalFactory(() => new TSignal());
            return this;
        }

        public FlowBinding<T1> Out(out CommandBindingBase binding)
        {
            binding = this;
            return this;
        }

        public void Execute(T1 param01) => Map.ExecuteFlow(this, param01);
    }

    public sealed class FlowBinding<T1, T2> : CommandBindingBase
    {
        internal FlowBinding(CommandMap map) : base(map, signalType: null)
        {
        }

        public FlowBinding<T1, T2> To0<TCommand>() where TCommand : Command
        {
            AddStep(typeof(TCommand), EmptyStepParams.Instance);
            return this;
        }

        public FlowBinding<T1, T2> To2<TCommand>() where TCommand : Command<T1, T2>
        {
            AddStep(typeof(TCommand), new FlowPayload2StepParams());
            return this;
        }

        public FlowBinding<T1, T2> To1<TCommand>() where TCommand : Command<T1>
        {
            AddStep(typeof(TCommand), new FlowPayloadFirstOnlyStepParams());
            return this;
        }

        public FlowBinding<T1, T2> To3<TCommand, T3>(T3 p3) where TCommand : Command<T1, T2, T3>
        {
            AddStep(typeof(TCommand), new FlowPayload2PlusFixed3StepParams(p3));
            return this;
        }

        public FlowBinding<T1, T2> InSequence()
        {
            SetMode(CommandExecutionMode.Sequence);
            return this;
        }

        public FlowBinding<T1, T2> InParallel()
        {
            SetMode(CommandExecutionMode.Parallel);
            return this;
        }

        public FlowBinding<T1, T2> OnComplete(Action action)
        {
            SetOnComplete(action);
            return this;
        }

        public FlowBinding<T1, T2> OnFail(Action<Exception> action)
        {
            SetOnFail(action);
            return this;
        }

        public FlowBinding<T1, T2> Out(out CommandBindingBase binding)
        {
            binding = this;
            return this;
        }

        public void Execute(T1 param01, T2 param02) =>
            Map.ExecuteFlow(this, new FlowPayload2(param01, param02));
    }

    public sealed class FlowBinding<T1, T2, T3> : CommandBindingBase
    {
        internal FlowBinding(CommandMap map) : base(map, signalType: null)
        {
        }

        public FlowBinding<T1, T2, T3> To0<TCommand>() where TCommand : Command
        {
            AddStep(typeof(TCommand), EmptyStepParams.Instance);
            return this;
        }

        public FlowBinding<T1, T2, T3> To3<TCommand>() where TCommand : Command<T1, T2, T3>
        {
            AddStep(typeof(TCommand), new FlowPayload3StepParams());
            return this;
        }

        public FlowBinding<T1, T2, T3> InSequence()
        {
            SetMode(CommandExecutionMode.Sequence);
            return this;
        }

        public FlowBinding<T1, T2, T3> InParallel()
        {
            SetMode(CommandExecutionMode.Parallel);
            return this;
        }

        public FlowBinding<T1, T2, T3> OnComplete(Action action)
        {
            SetOnComplete(action);
            return this;
        }

        public FlowBinding<T1, T2, T3> OnFail(Action<Exception> action)
        {
            SetOnFail(action);
            return this;
        }

        public FlowBinding<T1, T2, T3> Out(out CommandBindingBase binding)
        {
            binding = this;
            return this;
        }

        public void Execute(T1 p1, T2 p2, T3 p3) =>
            Map.ExecuteFlow(this, new FlowPayload3(p1, p2, p3));
    }

    internal readonly struct FlowPayload2
    {
        public readonly object P1;
        public readonly object P2;

        public FlowPayload2(object p1, object p2)
        {
            P1 = p1;
            P2 = p2;
        }
    }

    internal readonly struct FlowPayload3
    {
        public readonly object P1;
        public readonly object P2;
        public readonly object P3;

        public FlowPayload3(object p1, object p2, object p3)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
        }
    }

    internal sealed class FlowPayload2StepParams : CommandStepParams
    {
        public override void ApplyTo(CommandBase command, object triggerPayload)
        {
            var p = (FlowPayload2)triggerPayload;
            command.ApplyParams(p.P1, p.P2, null);
        }
    }

    internal sealed class FlowPayloadFirstOnlyStepParams : CommandStepParams
    {
        public override void ApplyTo(CommandBase command, object triggerPayload)
        {
            if (triggerPayload is FlowPayload2 p2)
                command.ApplyParams(p2.P1, null, null);
            else if (triggerPayload is FlowPayload3 p3)
                command.ApplyParams(p3.P1, null, null);
            else
                command.ApplyParams(triggerPayload, null, null);
        }
    }

    internal sealed class FlowPayload2PlusFixed3StepParams : CommandStepParams
    {
        private readonly object _p3;

        public FlowPayload2PlusFixed3StepParams(object p3) => _p3 = p3;

        public override void ApplyTo(CommandBase command, object triggerPayload)
        {
            var p = (FlowPayload2)triggerPayload;
            command.ApplyParams(p.P1, p.P2, _p3);
        }
    }

    internal sealed class FlowPayload3StepParams : CommandStepParams
    {
        public override void ApplyTo(CommandBase command, object triggerPayload)
        {
            var p = (FlowPayload3)triggerPayload;
            command.ApplyParams(p.P1, p.P2, p.P3);
        }
    }
}
