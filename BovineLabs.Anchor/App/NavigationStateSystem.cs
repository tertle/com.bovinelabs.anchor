namespace BovineLabs.Anchor
{
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Core;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [WorldSystemFilter(WorldSystemFilterFlags.Presentation | Worlds.Menu)]
    [UpdateInGroup(typeof(UISystemGroup))]
    public partial struct NavigationStateSystem : ISystem
    {
        private FixedString32Bytes _previous;

        private NativeHashMap<FixedString32Bytes, ComponentType> _statesMap;

        public void OnCreate(ref SystemState state)
        {
            _statesMap = new NativeHashMap<FixedString32Bytes, ComponentType>(16, Allocator.Persistent);

            var e = UISystemTypes.Enumerator();
            while (e.MoveNext())
            {
                var component = ComponentType.FromTypeIndex(TypeManager.GetTypeIndexFromStableTypeHash(e.Current.Value));

                if (!_statesMap.TryAdd(e.Current.Name, component))
                {
                    BLGlobalLogger.LogErrorString($"Navigation state '{e.Current.Name}' has already been registered.");
                }
            }
        }

        public void OnDestroy(ref SystemState state)
        {
            _statesMap.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var current = AnchorNavHost.Burst.CurrentDestination();

            if (_previous == current)
            {
                return;
            }

            var hasPrevious = _statesMap.TryGetValue(_previous, out var previousComponent);
            var hasCurrent = _statesMap.TryGetValue(current, out var currentComponent);

            if (hasPrevious && hasCurrent && previousComponent == currentComponent)
            {
                _previous = current;
                return;
            }

            if (hasPrevious)
            {
                state.EntityManager.RemoveComponent(state.SystemHandle, previousComponent);
            }

            if (hasCurrent)
            {
                state.EntityManager.AddComponent(state.SystemHandle, currentComponent);
            }

            _previous = current;
        }
    }
}
