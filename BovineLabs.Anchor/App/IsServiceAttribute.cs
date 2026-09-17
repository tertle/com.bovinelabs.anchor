namespace BovineLabs.Anchor
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    /// Automatically registers non-visual services; VisualElement implementations are rejected.
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class IsServiceAttribute : Attribute
    {
    }
}
