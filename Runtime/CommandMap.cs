using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace PipelinedCommands
{
    /// <summary>
    /// PostMVC-style command map over a host Zenject-compatible <see cref="SignalBus"/>.
    /// Bind a signal type to a chain of commands (sequence/parallel, Retain/Release, pooling).
    /// Requires assembly name <c>Zenject</c> (stock Zenject, Extenject, or equivalent fork).
    /// </summary>
    public sealed class CommandMap : ICommandMap, ICommandRunner, IDisposable
    {
        private readonly SignalBus _signals;
        private readonly CommandPool _pool;

        private readonly Dictionary<Type, List<CommandBindingBase>> _bindingsBySignal = new();
        private readonly Dictionary<Type, Action> _unsubscribeBySignal = new();
        private readonly List<CommandBindingBase> _pendingUnbind = new();
        private int _executionDepth;

        public CommandMap(
            SignalBus signals,
            DiContainer container)
        {
            _signals = signals;
            _pool = new CommandPool(container);
        }

        public CommandBinding<TSignal> Bind<TSignal>() => BindInternal<TSignal>();

        internal CommandBinding<TSignal> BindInternal<TSignal>()
        {
            EnsureSubscribed<TSignal>();

            var binding = new CommandBinding<TSignal>(this);
            var key = typeof(TSignal);

            if (!_bindingsBySignal.TryGetValue(key, out var list))
            {
                list = new List<CommandBindingBase>(2);
                _bindingsBySignal[key] = list;
            }

            list.Add(binding);
            return binding;
        }

        public FlowBinding Flow() => new FlowBinding(this);
        public FlowBinding<T1> Flow<T1>() => new FlowBinding<T1>(this);
        public FlowBinding<T1, T2> Flow<T1, T2>() => new FlowBinding<T1, T2>(this);
        public FlowBinding<T1, T2, T3> Flow<T1, T2, T3>() => new FlowBinding<T1, T2, T3>(this);

        public void Unbind(CommandBindingBase binding)
        {
            if (binding == null)
                return;

            if (_executionDepth > 0)
            {
                if (!_pendingUnbind.Contains(binding))
                    _pendingUnbind.Add(binding);
                return;
            }

            UnbindImmediate(binding);
        }

        public void UnbindAll<TSignal>()
        {
            var key = typeof(TSignal);
            if (!_bindingsBySignal.TryGetValue(key, out var list))
                return;

            var copy = list.ToArray();
            foreach (var b in copy)
                Unbind(b);
        }

        public void UnbindAll()
        {
            var all = new List<CommandBindingBase>();
            foreach (var list in _bindingsBySignal.Values)
                all.AddRange(list);

            foreach (var b in all)
                Unbind(b);
        }

        public void Break(CommandBindingBase binding)
        {
            if (binding is { IsExecuting: true })
                binding.RegisterBreak();
        }

        public void BreakAll<TSignal>()
        {
            if (!_bindingsBySignal.TryGetValue(typeof(TSignal), out var list))
                return;

            foreach (var b in list)
            {
                if (b.IsExecuting)
                    b.RegisterBreak();
            }
        }

        public IReadOnlyList<CommandBindingBase> GetBindings<TSignal>()
        {
            if (_bindingsBySignal.TryGetValue(typeof(TSignal), out var list))
                return list;

            return Array.Empty<CommandBindingBase>();
        }

        public void ForEachBinding(Action<CommandBindingBase> handler)
        {
            foreach (var list in _bindingsBySignal.Values)
            foreach (var b in list)
                handler(b);
        }

        public void Dispose()
        {
            foreach (var unsub in _unsubscribeBySignal.Values)
                unsub?.Invoke();

            _unsubscribeBySignal.Clear();
            _bindingsBySignal.Clear();
            _pendingUnbind.Clear();
        }

        internal void ExecuteFlow(CommandBindingBase binding, object payload)
        {
            if (binding.IsExecuting)
                throw new CommandException("Flow is already executing.");

            if (binding.CommandCount == 0)
                return;

            binding.BeginExecution(payload);
            ProcessBindingCommand(binding, 0);
        }

        private void EnsureSubscribed<TSignal>()
        {
            var key = typeof(TSignal);
            if (_unsubscribeBySignal.ContainsKey(key))
                return;

            Action<TSignal> handler = OnSignal;
            _signals.Subscribe(handler);
            _unsubscribeBySignal[key] = () => _signals.Unsubscribe(handler);
        }

        private void OnSignal<TSignal>(TSignal signal)
        {
            if (!_bindingsBySignal.TryGetValue(typeof(TSignal), out var bindings) || bindings.Count == 0)
                return;

            _executionDepth++;
            var snapshot = bindings.ToArray();

            foreach (var raw in snapshot)
            {
                if (raw is not CommandBinding<TSignal> binding)
                    continue;

                if (!binding.CheckCondition(signal))
                {
                    if ((binding.OnceBehavior & OnceBehavior.UnbindOnTriggerFail) != 0)
                        Unbind(binding);
                    continue;
                }

                if (binding.IsExecuting)
                {
                    Debug.LogError(
                        $"[CommandMap] Binding for {typeof(TSignal).Name} already executing; skipped re-entry.");
                    continue;
                }

                if (binding.CommandCount == 0)
                    continue;

                binding.BeginExecution(signal);
                ProcessBindingCommand(binding, 0);
            }

            _executionDepth--;
            FlushPendingUnbind();
        }

        private void ProcessBindingCommand(CommandBindingBase binding, int index)
        {
            if (binding.IsBreak)
            {
                FinishBinding(binding);
                return;
            }

            if (binding.HasFails && binding.Mode == CommandExecutionMode.Sequence)
            {
                FinishBinding(binding);
                return;
            }

            if (index >= binding.CommandCount)
            {
                if (binding.CheckAllReleasedSimple())
                    FinishBinding(binding);
                return;
            }

            if (binding.CheckAllReleasedSimple() && binding.AllExecuted())
            {
                FinishBinding(binding);
                return;
            }

            CommandBase command;
            try
            {
                command = CreateCommand(binding, index);
            }
            catch (Exception ex)
            {
                binding.RegisterFail(ex);
                FinishBinding(binding);
                return;
            }

            try
            {
                command.RunExecute();
            }
            catch (Exception ex)
            {
                if (!command.IsResolved)
                    command.FailFromRunner(ex);
            }

            command.MarkExecuted();
            binding.RegisterExecute();

            if (!command.IsRetained)
            {
                if (command.IsFailed)
                    OnCommandFailed(command, command.Exception ?? new CommandException(command.GetType().Name));
                else
                    OnCommandFinished(command);
                return;
            }

            if (binding.Mode == CommandExecutionMode.Parallel)
                ProcessBindingCommand(binding, index + 1);
        }

        private CommandBase CreateCommand(CommandBindingBase binding, int index)
        {
            var type = binding.CommandTypes[index]; // IReadOnlyList access
            var command = _pool.Get(type, out _);
            command.Attach(this, binding, index);
            binding.GetStepParams(index).ApplyTo(command, binding.RuntimePayload);
            return command;
        }

        public void OnCommandFinished(CommandBase command)
        {
            if (!command.IsExecuted)
                return;

            var binding = command.Binding;
            var index = command.Index;

            binding.RegisterRelease();
            if (command.IsBreak)
                binding.RegisterBreak();

            _pool.Return(command);

            if (binding.Mode == CommandExecutionMode.Sequence)
            {
                if (binding.IsBreak)
                    FinishBinding(binding);
                else
                    ProcessBindingCommand(binding, index + 1);
                return;
            }

            // Parallel
            if (binding.CheckAllReleasedSimple())
            {
                FinishBinding(binding);
                return;
            }

            if (!binding.AllExecuted())
            {
                if (binding.IsBreak)
                    FinishBinding(binding);
                else
                    ProcessBindingCommand(binding, index + 1);
            }
        }

        public void OnCommandFailed(CommandBase command, Exception exception)
        {
            if (!command.IsExecuted)
                return;

            var binding = command.Binding;
            var index = command.Index;

            binding.RegisterFail(exception);
            binding.RegisterRelease();
            _pool.Return(command);

            if (binding.Mode == CommandExecutionMode.Sequence)
            {
                FinishBinding(binding);
                return;
            }

            if (binding.CheckAllReleasedSimple())
            {
                FinishBinding(binding);
                return;
            }

            if (!binding.AllExecuted())
                ProcessBindingCommand(binding, index + 1);
        }

        private void FinishBinding(CommandBindingBase binding)
        {
            var isBreak = binding.IsBreak;
            var hasFails = binding.HasFails;
            var firstFail = hasFails ? binding.FailuresSafe() : null;
            var once = binding.OnceBehavior;

            var onComplete = binding.OnCompleteAction;
            var onBreak = binding.OnBreakAction;
            var onFail = binding.OnFailAction;
            var completeSignal = binding.CompleteSignalFactory;
            var breakSignal = binding.BreakSignalFactory;
            var failSignal = binding.FailSignalFactory;

            binding.EndExecution();

            if (isBreak)
            {
                if ((once & OnceBehavior.UnbindOnBreak) != 0)
                    Unbind(binding);

                onBreak?.Invoke();
                FireSignal(breakSignal);
                return;
            }

            if (hasFails)
            {
                if ((once & OnceBehavior.UnbindOnFail) != 0)
                    Unbind(binding);

                onFail?.Invoke(firstFail);

                if (failSignal != null)
                {
                    var signal = failSignal(firstFail);
                    if (signal != null)
                        _signals.Fire(signal);
                }

                if (onFail == null && failSignal == null && firstFail != null)
                    throw firstFail;

                return;
            }

            if ((once & OnceBehavior.UnbindOnComplete) != 0)
                Unbind(binding);

            onComplete?.Invoke();
            FireSignal(completeSignal);
        }

        private void FireSignal(Func<object> factory)
        {
            if (factory == null)
                return;

            var signal = factory();
            if (signal != null)
                _signals.Fire(signal);
        }

        private void UnbindImmediate(CommandBindingBase binding)
        {
            if (binding.SignalType == null)
                return;

            if (!_bindingsBySignal.TryGetValue(binding.SignalType, out var list))
                return;

            if (!list.Remove(binding))
                return;

            if (list.Count == 0)
            {
                _bindingsBySignal.Remove(binding.SignalType);
                if (_unsubscribeBySignal.TryGetValue(binding.SignalType, out var unsub))
                {
                    unsub();
                    _unsubscribeBySignal.Remove(binding.SignalType);
                }
            }
        }

        private void FlushPendingUnbind()
        {
            if (_pendingUnbind.Count == 0 || _executionDepth > 0)
                return;

            for (var i = 0; i < _pendingUnbind.Count; i++)
                UnbindImmediate(_pendingUnbind[i]);

            _pendingUnbind.Clear();
        }
    }

    internal static class CommandBindingBaseExtensions
    {
        public static Exception FailuresSafe(this CommandBindingBase binding)
        {
            return binding.CommandsFailed != null && binding.CommandsFailed.Count > 0
                ? binding.CommandsFailed[0]
                : new CommandException("Command binding failed.");
        }
    }
}

