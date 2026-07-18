using System;

namespace PipelinedCommands
{
    /// <summary>
    /// PostMVC-style command: sync by default; <see cref="Retain"/> holds until
    /// <see cref="Release"/> / <see cref="Fail"/> / <see cref="Break"/>.
    /// </summary>
    public abstract class CommandBase
    {
        private ICommandRunner _runner;
        private CommandBindingBase _binding;
        private int _index = -1;

        internal int Index => _index;
        internal CommandBindingBase Binding => _binding;
        /// <summary>For async command code (e.g. after await).</summary>
        protected bool IsResolved { get; private set; }

        internal bool IsRetained { get; private set; }
        internal bool IsFailed { get; private set; }
        internal bool IsBreak { get; private set; }
        internal bool IsExecuted { get; private set; }
        internal Exception Exception { get; private set; }

        internal void Attach(ICommandRunner runner, CommandBindingBase binding, int index)
        {
            _runner = runner;
            _binding = binding;
            _index = index;
            IsResolved = false;
            IsRetained = false;
            IsFailed = false;
            IsBreak = false;
            IsExecuted = false;
            Exception = null;
        }

        internal void MarkExecuted()
        {
            IsExecuted = true;
            if (!IsRetained)
                IsResolved = true;
        }

        internal abstract void RunExecute();

        /// <summary>Inject step parameters (0–3). Implemented by typed Command variants.</summary>
        internal abstract void ApplyParams(object p1, object p2, object p3);

        internal virtual void ResetState()
        {
            _runner = null;
            _binding = null;
            _index = -1;
            IsResolved = false;
            IsRetained = false;
            IsFailed = false;
            IsBreak = false;
            IsExecuted = false;
            Exception = null;
        }

        protected void Retain()
        {
            if (IsResolved)
                throw new CommandException($"Command {GetType().Name} is already resolved; cannot Retain.");

            IsRetained = true;
        }

        protected void Release()
        {
            if (IsResolved)
                throw new CommandException($"Command {GetType().Name} is already resolved; cannot Release.");

            IsResolved = true;
            IsRetained = false;

            if (IsExecuted)
                _runner?.OnCommandFinished(this);
        }

        protected void Break()
        {
            if (IsResolved)
                throw new CommandException($"Command {GetType().Name} is already resolved; cannot Break.");

            IsResolved = true;
            IsRetained = false;
            IsBreak = true;

            if (IsExecuted)
                _runner?.OnCommandFinished(this);
        }

        protected void Fail(Exception exception = null)
        {
            if (IsResolved)
                throw new CommandException($"Command {GetType().Name} is already resolved; cannot Fail.");

            IsResolved = true;
            IsRetained = false;
            IsFailed = true;
            Exception = exception ?? new CommandException($"Command {GetType().Name} failed.");

            if (IsExecuted)
                _runner?.OnCommandFailed(this, Exception);
        }

        internal void FailFromRunner(Exception exception)
        {
            if (IsResolved)
                return;

            IsResolved = true;
            IsRetained = false;
            IsFailed = true;
            Exception = exception ?? new CommandException($"Command {GetType().Name} failed.");
        }

        /// <summary>Retain with multi-handle (Release N times or Break/Fail once).</summary>
        protected CommandRetainHandle ToHandle() => new CommandRetainHandle(this);

        internal void RetainInternal() => Retain();
        internal void ReleaseInternal() => Release();
        internal void BreakInternal() => Break();
        internal void FailInternal(Exception exception) => Fail(exception);
    }

    /// <summary>Multi-retain handle: each Retain increments; Release decrements; zero → command Release.</summary>
    public sealed class CommandRetainHandle
    {
        private readonly CommandBase _command;
        private int _count;

        internal CommandRetainHandle(CommandBase command)
        {
            _command = command;
        }

        public bool IsRetained => _command.IsRetained;

        public void Retain()
        {
            if (!_command.IsRetained)
                _command.RetainInternal();
            _count++;
        }

        public void Release()
        {
            _count--;
            if (_count <= 0 && !_command.IsBreak && !_command.IsFailed && _command.IsRetained)
                _command.ReleaseInternal();
        }

        public void Break() => _command.BreakInternal();

        public void Fail(Exception exception = null) => _command.FailInternal(exception);
    }
}

