using System;

namespace PipelinedCommands
{
    /// <summary>Per-step fixed / lazy args for a command in a binding.</summary>
    internal abstract class CommandStepParams
    {
        public abstract void ApplyTo(CommandBase command, object triggerPayload);
    }

    internal sealed class EmptyStepParams : CommandStepParams
    {
        public static readonly EmptyStepParams Instance = new();

        public override void ApplyTo(CommandBase command, object triggerPayload)
        {
            // Parameterless Command — ignore payload.
        }
    }

    /// <summary>Command&lt;T&gt; gets trigger payload (signal / flow arg).</summary>
    internal sealed class TriggerPayloadStepParams : CommandStepParams
    {
        public static readonly TriggerPayloadStepParams Instance = new();

        public override void ApplyTo(CommandBase command, object triggerPayload)
        {
            command.ApplyParams(triggerPayload, null, null);
        }
    }

    internal sealed class Fixed1StepParams : CommandStepParams
    {
        private readonly object _p1;
        private readonly Func<object> _g1;

        public Fixed1StepParams(object p1)
        {
            _p1 = p1;
        }

        public Fixed1StepParams(Func<object> g1)
        {
            _g1 = g1;
        }

        public override void ApplyTo(CommandBase command, object triggerPayload)
        {
            var v1 = _g1 != null ? _g1() : _p1;
            command.ApplyParams(v1, null, null);
        }
    }

    internal sealed class Fixed2StepParams : CommandStepParams
    {
        private readonly object _p1;
        private readonly object _p2;
        private readonly Func<object> _g1;
        private readonly Func<object> _g2;
        private readonly bool _p1FromTrigger;

        public Fixed2StepParams(object p1, object p2, bool p1FromTrigger = false)
        {
            _p1 = p1;
            _p2 = p2;
            _p1FromTrigger = p1FromTrigger;
        }

        public Fixed2StepParams(Func<object> g1, Func<object> g2)
        {
            _g1 = g1;
            _g2 = g2;
        }

        public override void ApplyTo(CommandBase command, object triggerPayload)
        {
            var v1 = _p1FromTrigger ? triggerPayload : (_g1 != null ? _g1() : _p1);
            var v2 = _g2 != null ? _g2() : _p2;
            command.ApplyParams(v1, v2, null);
        }
    }

    internal sealed class Fixed3StepParams : CommandStepParams
    {
        private readonly object _p1;
        private readonly object _p2;
        private readonly object _p3;
        private readonly bool _p1FromTrigger;

        public Fixed3StepParams(object p1, object p2, object p3, bool p1FromTrigger = false)
        {
            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _p1FromTrigger = p1FromTrigger;
        }

        public override void ApplyTo(CommandBase command, object triggerPayload)
        {
            var v1 = _p1FromTrigger ? triggerPayload : _p1;
            command.ApplyParams(v1, _p2, _p3);
        }
    }
}
