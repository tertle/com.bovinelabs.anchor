// <copyright file="NavigationStateSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// Synchronizes DOTS component states with the currently active Anchor navigation destination.
    /// </summary>
    [UpdateInGroup(typeof(UISystemGroup))]
    public partial struct NavigationStateSystem : ISystem
    {
        private FixedString32Bytes previous;

        private NativeHashMap<FixedString32Bytes, ComponentType> statesMap;

        /// <inheritdoc/>
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.statesMap = new NativeHashMap<FixedString32Bytes, ComponentType>(16, Allocator.Persistent);

            var comps = new NativeHashSet<ComponentType>(0, Allocator.Temp);

            var e = UISystemTypes.Enumerator();
            while (e.MoveNext())
            {
                var component = ComponentType.FromTypeIndex(TypeManager.GetTypeIndexFromStableTypeHash(e.Current.Value));

                if (!comps.Add(component))
                {
                    continue;
                }

                if (!this.statesMap.TryAdd(e.Current.Name, component))
                {
                    Debug.LogError($"Key {e.Current.Name} has already been registered");
                }
            }
        }

        /// <inheritdoc/>
        public void OnDestroy(ref SystemState state)
        {
            this.statesMap.Dispose();
        }

        /// <inheritdoc/>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var current = AnchorApp.CurrentDestination();

            if (this.previous == current)
            {
                return;
            }

            if (this.statesMap.TryGetValue(this.previous, out var previousComponent))
            {
                state.EntityManager.RemoveComponent(state.SystemHandle, previousComponent);
            }

            if (this.statesMap.TryGetValue(current, out var currentComponent))
            {
                state.EntityManager.AddComponent(state.SystemHandle, currentComponent);
            }

            this.previous = current;
        }
    }
}
