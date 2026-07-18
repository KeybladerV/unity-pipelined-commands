using System;

namespace PipelinedCommands
{
    public abstract class Command : CommandBase
    {
        public abstract void Execute();

        internal override void RunExecute() => Execute();

        internal override void ApplyParams(object p1, object p2, object p3)
        {
        }
    }

    public abstract class Command<T1> : CommandBase
    {
        public T1 Param01 { get; private set; }

        public abstract void Execute(T1 param01);

        internal override void RunExecute() => Execute(Param01);

        internal override void ApplyParams(object p1, object p2, object p3)
        {
            Param01 = CommandParamUtil.Cast<T1>(p1, nameof(Param01));
        }

        internal override void ResetState()
        {
            Param01 = default;
            base.ResetState();
        }
    }

    public abstract class Command<T1, T2> : CommandBase
    {
        public T1 Param01 { get; private set; }
        public T2 Param02 { get; private set; }

        public abstract void Execute(T1 param01, T2 param02);

        internal override void RunExecute() => Execute(Param01, Param02);

        internal override void ApplyParams(object p1, object p2, object p3)
        {
            Param01 = CommandParamUtil.Cast<T1>(p1, nameof(Param01));
            Param02 = CommandParamUtil.Cast<T2>(p2, nameof(Param02));
        }

        internal override void ResetState()
        {
            Param01 = default;
            Param02 = default;
            base.ResetState();
        }
    }

    public abstract class Command<T1, T2, T3> : CommandBase
    {
        public T1 Param01 { get; private set; }
        public T2 Param02 { get; private set; }
        public T3 Param03 { get; private set; }

        public abstract void Execute(T1 param01, T2 param02, T3 param03);

        internal override void RunExecute() => Execute(Param01, Param02, Param03);

        internal override void ApplyParams(object p1, object p2, object p3)
        {
            Param01 = CommandParamUtil.Cast<T1>(p1, nameof(Param01));
            Param02 = CommandParamUtil.Cast<T2>(p2, nameof(Param02));
            Param03 = CommandParamUtil.Cast<T3>(p3, nameof(Param03));
        }

        internal override void ResetState()
        {
            Param01 = default;
            Param02 = default;
            Param03 = default;
            base.ResetState();
        }
    }

    internal static class CommandParamUtil
    {
        public static T Cast<T>(object value, string name)
        {
            if (value == null)
            {
                if (default(T) == null)
                    return default;

                throw new CommandException($"Command param {name} ({typeof(T).Name}) is null.");
            }

            if (value is T typed)
                return typed;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                throw new CommandException(
                    $"Cannot cast command param {name} from {value.GetType().Name} to {typeof(T).Name}.", ex);
            }
        }
    }
}
