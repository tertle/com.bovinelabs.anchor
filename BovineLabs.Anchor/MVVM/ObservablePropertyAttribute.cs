namespace BovineLabs.Anchor.MVVM
{
    using System;

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class ObservablePropertyAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class AlsoNotifyChangeForAttribute : Attribute
    {
        public AlsoNotifyChangeForAttribute(string propertyName)
        {
        }

        public AlsoNotifyChangeForAttribute(string propertyName, params string[] propertyNames)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class AlsoExecuteAttribute : Attribute
    {
        public AlsoExecuteAttribute(string methodName)
        {
        }

        public AlsoExecuteAttribute(string methodName, params string[] methodNames)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class DependsOnAttribute : Attribute
    {
        public DependsOnAttribute(string propertyName)
        {
        }

        public DependsOnAttribute(string propertyName, params string[] propertyNames)
        {
        }
    }
}
