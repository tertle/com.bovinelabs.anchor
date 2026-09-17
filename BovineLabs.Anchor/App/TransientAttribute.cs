namespace BovineLabs.Anchor
{
    using System;
    using JetBrains.Annotations;

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class TransientAttribute : Attribute
    {
    }
}
