// <copyright file="NavigationStateSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;

    [UpdateInGroup(typeof(UISystemGroup))]
    public partial struct NavigationStateSystem : ISystem, ISystemStartStop
    {
        private FixedString64Bytes previous;

        private NativeHashMap<FixedString64Bytes, ComponentType> statesMap;

        /// <inheritdoc/>
        public void OnCreate(ref SystemState state)
        {
            this.statesMap = new NativeHashMap<FixedString64Bytes, ComponentType>(16, Allocator.Persistent);
        }

        /// <inheritdoc/>
        public void OnStartRunning(ref SystemState state)
        {
            foreach (var instance in SystemAPI.Query<UIStateInstance>().WithOptions(EntityQueryOptions.IncludeSystems))
            {
                var stateInstanceComponent = ComponentType.FromTypeIndex(instance.ComponentType);

                if (this.statesMap.TryAdd(instance.Name, stateInstanceComponent))
                {
                    continue;
                }

                var existing = this.statesMap[instance.Name];

                if (existing != stateInstanceComponent)
                {
                    Debug.LogError(
                        $"Key {instance.Name} has already been registered with a different instance component {existing} vs {stateInstanceComponent}");
                }
            }
        }

        /// <inheritdoc/>
        public void OnStopRunning(ref SystemState state)
        {
            this.statesMap.Clear();
        }

        /// <inheritdoc/>
        public void OnDestroy(ref SystemState state)
        {
            this.statesMap.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var current = AnchorApp.CurrentDestination();

            if (this.previous == current)
            {
                return;
            }

            Debug.Log($"Changed from {this.previous} to {current}");

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
#endif
