namespace GMaster.Core.Tools
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public class RunnableMethodAttribute : Attribute
    {
        public MethodGroup Group { get; }

        public RunnableMethodAttribute(MethodGroup group)
        {
            Group = group;
        }
    }
}