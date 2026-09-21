namespace BovineLabs.Anchor.Nav
{
    using System;
    using UnityEngine.Scripting;

    /// <summary>
    /// Automatic registration requires a static, parameterless method returning a navigation action.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AnchorNavActionAttribute : PreserveAttribute
    {
        public AnchorNavActionAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Action name cannot be null or whitespace.", nameof(name));
            }

            Name = name;
        }

        public string Name { get; }
    }
}
