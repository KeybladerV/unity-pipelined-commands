using System;

namespace PipelinedCommands
{
    /// <summary>Command instances are reused via <see cref="CommandPool"/> instead of destroyed each run.</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PoolableAttribute : Attribute
    {
    }
}
