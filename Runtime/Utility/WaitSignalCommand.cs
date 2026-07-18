using System;
using Zenject;

namespace PipelinedCommands.Utility
{
    /// <summary>Retains until <typeparamref name="TSignal"/> is fired once, then Releases.</summary>
    public sealed class WaitSignalCommand<TSignal> : Command, IDisposable
    {
        private readonly SignalBus _signals;
        private Action<TSignal> _handler;

        public WaitSignalCommand(SignalBus signals)
        {
            _signals = signals;
        }

        public override void Execute()
        {
            Retain();
            _handler = OnSignal;
            _signals.Subscribe(_handler);
        }

        private void OnSignal(TSignal _)
        {
            Unsubscribe();
            if (!IsResolved)
                Release();
        }

        public void Dispose() => Unsubscribe();

        private void Unsubscribe()
        {
            if (_handler == null)
                return;

            _signals.Unsubscribe(_handler);
            _handler = null;
        }
    }
}
