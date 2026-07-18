using System;
using System.Collections.Generic;
using Zenject;

namespace PipelinedCommands
{
    internal sealed class CommandPool
    {
        private readonly DiContainer _container;
        private readonly Dictionary<Type, Stack<CommandBase>> _available = new();
        private readonly Dictionary<Type, bool> _poolableCache = new();

        public CommandPool(DiContainer container)
        {
            _container = container;
        }

        public CommandBase Get(Type commandType, out bool isNew)
        {
            if (IsPoolable(commandType)
                && _available.TryGetValue(commandType, out var stack)
                && stack.Count > 0)
            {
                isNew = false;
                return stack.Pop();
            }

            isNew = true;
            return (CommandBase)_container.Instantiate(commandType);
        }

        public void Return(CommandBase command)
        {
            if (command == null)
                return;

            var type = command.GetType();
            if (!IsPoolable(type))
            {
                if (command is IDisposable disposable)
                    disposable.Dispose();
                return;
            }

            command.ResetState();

            if (!_available.TryGetValue(type, out var stack))
            {
                stack = new Stack<CommandBase>();
                _available[type] = stack;
            }

            stack.Push(command);
        }

        private bool IsPoolable(Type type)
        {
            if (!_poolableCache.TryGetValue(type, out var poolable))
            {
                poolable = Attribute.IsDefined(type, typeof(PoolableAttribute));
                _poolableCache[type] = poolable;
            }

            return poolable;
        }
    }
}
