namespace BovineLabs.Anchor
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    /// Marks a service as transient so the Anchor dependency container creates a new instance for each resolution.
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class TransientAttribute : Attribute
    {
    }
}
