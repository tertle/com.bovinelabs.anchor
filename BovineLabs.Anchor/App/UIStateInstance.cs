// <copyright file="UIStateInstance.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using Unity.Collections;
    using Unity.Entities;

    public struct UIStateInstance : IComponentData
    {
        public FixedString64Bytes Name;
        public TypeIndex ComponentType;
    }
}
