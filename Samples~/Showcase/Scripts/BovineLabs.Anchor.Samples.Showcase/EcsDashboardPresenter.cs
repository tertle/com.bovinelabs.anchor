namespace BovineLabs.Anchor.Samples.Showcase
{
    using System;
    using System.Collections.Generic;
    using Unity.Entities;
    using Unity.Scenes;
    using UnityEngine;
    using Hash128 = Unity.Entities.Hash128;

    public sealed class EcsDashboardPresenter : IDisposable
    {
        private readonly World _world;
        private readonly Entity _scene;
        private bool _loaded;

        public EcsDashboardPresenter(Hash128 scene)
        {
            _world = new World("Anchor Burst dashboard", WorldFlags.Game);
            var systems = new List<Type>();
            foreach (var system in DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default))
            {
                var assembly = system.Assembly.GetName().Name;
                if (assembly is "Unity.Entities" or "Unity.Scenes")
                {
                    systems.Add(system);
                }
            }

            systems.Add(typeof(UpdateWorldTimeSystem));
            systems.Add(typeof(EcsDashboardSystem));
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(_world, systems);
            _world.GetExistingSystemManaged<SimulationSystemGroup>().Enabled = false;
            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(_world);
            _scene = SceneSystem.LoadSceneAsync(_world.Unmanaged, scene);
        }

        public void Update()
        {
            if (!_loaded && SceneSystem.IsSceneLoaded(_world.Unmanaged, _scene))
            {
                _world.GetExistingSystemManaged<SimulationSystemGroup>().Enabled = true;
                _loaded = true;
            }
        }

        public void Dispose()
        {
            ScriptBehaviourUpdateOrder.RemoveWorldFromCurrentPlayerLoop(_world);
            _world.Dispose();
        }
    }
}
