namespace GMaster.Core.Tools
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public class RunnableActionAttribute : Attribute
    {
        public RunnableActionAttribute(MethodGroup group)
        {
            Group = group;
        }

        public MethodGroup Group { get; }
    }
}