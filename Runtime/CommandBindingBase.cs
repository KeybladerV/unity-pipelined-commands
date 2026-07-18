using System;
using System.Collections.Generic;

namespace PipelinedCommands
{
    public abstract class CommandBindingBase
    {
        private readonly List<Type> _commandTypes = new();
        private readonly List<CommandStepParams> _stepParams = new();

        internal IReadOnlyList<Type> CommandTypes => _commandTypes;

        internal CommandMap Map { get; }
        internal Type SignalType { get; }

        internal CommandExecutionMode Mode { get; private set; } = CommandExecutionMode.Sequence;
        internal OnceBehavior OnceBehavior { get; private set; } = OnceBehavior.None;

        internal bool IsExecuting { get; private set; }
        internal bool IsBreak { get; private set; }

        internal int CommandsExecuted { get; private set; }
        internal int CommandsReleased { get; private set; }
        internal List<Exception> CommandsFailed { get; private set; }

        internal object RuntimePayload { get; private set; }

        internal Action OnCompleteAction { get; private set; }
        internal Action OnBreakAction { get; private set; }
        internal Action<Exception> OnFailAction { get; private set; }

        internal Func<object> CompleteSignalFactory { get; private set; }
        internal Func<object> BreakSignalFactory { get; private set; }
        internal Func<Exception, object> FailSignalFactory { get; private set; }

        internal bool HasFails => CommandsFailed != null && CommandsFailed.Count > 0;

        protected CommandBindingBase(CommandMap map, Type signalType)
        {
            Map = map;
            SignalType = signalType;
        }

        internal void AddStep(Type commandType, CommandStepParams stepParams)
        {
            _commandTypes.Add(commandType);
            _stepParams.Add(stepParams ?? EmptyStepParams.Instance);
        }

        internal void AddStepInternal(Type commandType, CommandStepParams stepParams) =>
            AddStep(commandType, stepParams);

        internal void CopyConfigFrom(CommandBindingBase source)
        {
            SetMode(source.Mode);
            SetOnce(source.OnceBehavior);
            if (source.OnCompleteAction != null)
                SetOnComplete(source.OnCompleteAction);
            if (source.OnBreakAction != null)
                SetOnBreak(source.OnBreakAction);
            if (source.OnFailAction != null)
                SetOnFail(source.OnFailAction);
            if (source.CompleteSignalFactory != null)
                SetCompleteSignalFactory(source.CompleteSignalFactory);
            if (source.BreakSignalFactory != null)
                SetBreakSignalFactory(source.BreakSignalFactory);
            if (source.FailSignalFactory != null)
                SetFailSignalFactory(source.FailSignalFactory);
        }

        internal void CopyStepsFrom(CommandBindingBase source)
        {
            for (var i = 0; i < source._commandTypes.Count; i++)
                AddStep(source._commandTypes[i], source._stepParams[i]);
        }

        internal CommandStepParams GetStepParams(int index) => _stepParams[index];

        protected void SetMode(CommandExecutionMode mode) => Mode = mode;

        protected void SetOnce(OnceBehavior behavior) => OnceBehavior = behavior;

        protected void SetOnComplete(Action action) => OnCompleteAction = action;

        protected void SetOnBreak(Action action) => OnBreakAction = action;

        protected void SetOnFail(Action<Exception> action) => OnFailAction = action;

        protected void SetCompleteSignalFactory(Func<object> factory) => CompleteSignalFactory = factory;

        protected void SetBreakSignalFactory(Func<object> factory) => BreakSignalFactory = factory;

        protected void SetFailSignalFactory(Func<Exception, object> factory) => FailSignalFactory = factory;

        // Package-access for composite / map helpers
        internal void ApplyMode(CommandExecutionMode mode) => SetMode(mode);
        internal void ApplyOnce(OnceBehavior behavior) => SetOnce(behavior);
        internal void ApplyOnComplete(Action action) => SetOnComplete(action);
        internal void ApplyOnBreak(Action action) => SetOnBreak(action);
        internal void ApplyOnFail(Action<Exception> action) => SetOnFail(action);
        internal void ApplyCompleteSignal(Func<object> factory) => SetCompleteSignalFactory(factory);
        internal void ApplyBreakSignal(Func<object> factory) => SetBreakSignalFactory(factory);
        internal void ApplyFailSignal(Func<Exception, object> factory) => SetFailSignalFactory(factory);

        internal void BeginExecution(object payload)
        {
            IsExecuting = true;
            IsBreak = false;
            RuntimePayload = payload;
            CommandsExecuted = 0;
            CommandsReleased = 0;
            CommandsFailed = null;
        }

        internal void EndExecution()
        {
            IsExecuting = false;
            IsBreak = false;
            RuntimePayload = null;
            CommandsExecuted = 0;
            CommandsReleased = 0;
            CommandsFailed = null;
        }

        internal void RegisterExecute() => CommandsExecuted++;

        internal void RegisterRelease() => CommandsReleased++;

        internal void RegisterBreak() => IsBreak = true;

        internal void RegisterFail(Exception exception)
        {
            CommandsFailed ??= new List<Exception>(1);
            CommandsFailed.Add(exception);
        }

        internal bool AllExecuted() => CommandsExecuted >= _commandTypes.Count;

        internal bool AllReleased()
        {
            if (Mode == CommandExecutionMode.Sequence)
                return CommandsReleased >= _commandTypes.Count || HasFails;

            return CommandsReleased + (CommandsFailed?.Count ?? 0) >= _commandTypes.Count
                   || CommandsReleased >= _commandTypes.Count;
        }

        internal bool CheckAllReleasedSimple() => CommandsReleased >= _commandTypes.Count;

        internal int CommandCount => _commandTypes.Count;
    }
}
