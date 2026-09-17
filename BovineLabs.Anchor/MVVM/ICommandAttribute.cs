namespace BovineLabs.Anchor.MVVM
{
    using System;

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class ICommandAttribute : Attribute
    {
        public string CanExecuteMethod { get; set; }

        public string CanExecuteProperty { get; set; }
    }
}
