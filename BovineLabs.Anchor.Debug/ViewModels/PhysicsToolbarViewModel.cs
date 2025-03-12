// <copyright file="PhysicsToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_QUILL || UNITY_PHYSICS
namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using BovineLabs.Anchor.Binding;
    using BovineLabs.Anchor.Services;
    using JetBrains.Annotations;
    using Unity.Properties;

    [Transient]
    [UsedImplicitly]
    public class PhysicsToolbarViewModel : SystemObservableObject<PhysicsToolbarViewModel.Data>
    {
        private const string BaseKey = "bl.ui.physics.";

        private readonly ILocalStorageService storageService;

        public PhysicsToolbarViewModel(ILocalStorageService storageService)
        {
            this.storageService = storageService;
            this.PropertyChanged += this.OnPropertyChanged;
        }

        [CreateProperty]
        public bool DrawColliderEdges
        {
            get => this.GetValue(nameof(this.DrawColliderEdges));
            set => this.SetProperty(this.DrawColliderEdges, value, this, (m, v) =>
            {
                m.SetValue(v);
                this.Value.DrawColliderEdges = value;
            });
        }

        [CreateProperty]
        public bool DrawColliderAabbs
        {
            get => this.Value.DrawColliderAabbs;
            set => this.SetProperty(this.Value.DrawColliderAabbs, value, this, (m, v) =>
            {
                m.SetValue(v);
                this.Value.DrawColliderAabbs = value;
            });
        }

        [CreateProperty]
        public bool DrawCollisionEvents
        {
            get => this.Value.DrawCollisionEvents;
            set => this.SetProperty(this.Value.DrawCollisionEvents, value, this, (m, v) =>
            {
                m.SetValue(v);
                this.Value.DrawCollisionEvents = value;
            });
        }

        [CreateProperty]
        public bool DrawTriggerEvents
        {
            get => this.Value.DrawTriggerEvents;
            set => this.SetProperty(this.Value.DrawTriggerEvents, value, this, (m, v) =>
            {
                m.SetValue(v);
                this.Value.DrawTriggerEvents = value;
            });
        }

#if BL_QUILL
        [CreateProperty]
        public bool DrawMeshColliderEdges
        {
            get => this.Value.DrawMeshColliderEdges;
            set => this.SetProperty(this.Value.DrawMeshColliderEdges, value, this, (m, v) =>
            {
                m.SetValue(v);
                this.Value.DrawMeshColliderEdges = value;
            });
        }

        [CreateProperty]
        public bool DrawTerrainColliderEdges
        {
            get => this.Value.DrawTerrainColliderEdges;
            set => this.SetProperty(this.Value.DrawTerrainColliderEdges, value, this, (m, v) =>
            {
                m.SetValue(v);
                this.Value.DrawTerrainColliderEdges = value;
            });
        }
#endif

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "World")
            {
                // Load our saved value
                this.Value.DrawColliderEdges = this.GetValue(nameof(this.DrawColliderEdges));
                this.Value.DrawColliderAabbs = this.GetValue(nameof(this.DrawColliderAabbs));
                this.Value.DrawCollisionEvents = this.GetValue(nameof(this.DrawCollisionEvents));
                this.Value.DrawTriggerEvents = this.GetValue(nameof(this.DrawTriggerEvents));
#if BL_QUILL
                this.Value.DrawMeshColliderEdges = this.GetValue(nameof(this.DrawMeshColliderEdges));
                this.Value.DrawTerrainColliderEdges = this.GetValue(nameof(this.DrawTerrainColliderEdges));
#endif
            }
        }

        private bool GetValue(string key)
        {
            return this.storageService.GetValue($"{BaseKey}.{this.Value.World}.{key}", false);
        }

        private void SetValue(bool value, [CallerMemberName] string propertyName = null)
        {
            this.storageService.SetValue($"{BaseKey}.{this.Value.World}.{propertyName}", value);
        }

        public struct Data
        {
            private int world;
            private bool drawColliderEdges;
            private bool drawColliderAabbs;
            private bool drawCollisionEvents;
            private bool drawTriggerEvents;
#if BL_QUILL
            private bool drawMeshColliderEdges;
            private bool drawTerrainColliderEdges;
#endif

            public int World
            {
                readonly get => this.world;
                set => this.SetProperty(ref this.world, value);
            }

            public bool DrawColliderEdges
            {
                readonly get => this.drawColliderEdges;
                set => this.SetProperty(ref this.drawColliderEdges, value);
            }

            public bool DrawColliderAabbs
            {
                readonly get => this.drawColliderAabbs;
                set => this.SetProperty(ref this.drawColliderAabbs, value);
            }

            public bool DrawCollisionEvents
            {
                readonly get => this.drawCollisionEvents;
                set => this.SetProperty(ref this.drawCollisionEvents, value);
            }

            public bool DrawTriggerEvents
            {
                readonly get => this.drawTriggerEvents;
                set => this.SetProperty(ref this.drawTriggerEvents, value);
            }

            public bool DrawMeshColliderEdges
            {
                readonly get => this.drawMeshColliderEdges;
                set => this.SetProperty(ref this.drawMeshColliderEdges, value);
            }

            public bool DrawTerrainColliderEdges
            {
                readonly get => this.drawTerrainColliderEdges;
                set => this.SetProperty(ref this.drawTerrainColliderEdges, value);
            }
        }
    }
}
#endif
