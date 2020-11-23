using System;

namespace Interface
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DispatcherAttribute : Attribute
    {
        public DispatcherAttribute(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
