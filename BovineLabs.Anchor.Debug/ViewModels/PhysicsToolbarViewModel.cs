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
    using Unity.Burst;
    using Unity.Properties;

    [Transient]
    [UsedImplicitly]
    public class PhysicsToolbarViewModel : SystemObservableObject<PhysicsToolbarViewModel.Data>
    {
        private const string BaseKey = "bl.ui.physics.";

        private readonly ILocalStorageService storageService;

        private int world;

        private Data data;

        public PhysicsToolbarViewModel(ILocalStorageService storageService)
        {
            this.storageService = storageService;
            this.PropertyChanged += this.OnPropertyChanged;
        }

        public override ref Data Value => ref this.data;

        [CreateProperty]
        public bool DrawColliderEdges
        {
            get => this.GetValue(nameof(this.DrawColliderEdges));
            set => this.SetProperty(this.DrawColliderEdges, value, this, (m, v) =>
            {
                m.SetValue(v);
                this.data.DrawColliderEdges = value;
            });
        }

        [CreateProperty]
        public bool DrawColliderAabbs
        {
            get => this.data.DrawColliderAabbs;
            set => this.SetProperty(this.data.DrawColliderAabbs, value, this, (m, v) =>
            {
                m.SetValue(v);
                this.data.DrawColliderAabbs = value;
            });
        }

        [CreateProperty]
        public bool DrawCollisionEvents
        {
            get => this.data.DrawCollisionEvents;
            set => this.SetProperty(this.data.DrawCollisionEvents, value, this, (m, v) =>
            {
                m.SetValue(v);
                this.data.DrawCollisionEvents = value;
            });
        }

        [CreateProperty]
        public bool DrawTriggerEvents
        {
            get => this.data.DrawTriggerEvents;
            set => this.SetProperty(this.data.DrawTriggerEvents, value, this, (m, v) =>
            {
                m.SetValue(v);
                this.data.DrawTriggerEvents = value;
            });
        }

#if BL_QUILL
        [CreateProperty]
        public bool DrawMeshColliderEdges
        {
            get => this.data.DrawMeshColliderEdges;
            set => this.SetProperty(this.data.DrawMeshColliderEdges, value, this, (m, v) =>
            {
                m.SetValue(v);
                this.data.DrawMeshColliderEdges = value;
            });
        }

        [CreateProperty]
        public bool DrawTerrainColliderEdges
        {
            get => this.data.DrawTerrainColliderEdges;
            set => this.SetProperty(this.data.DrawTerrainColliderEdges, value, this, (m, v) =>
            {
                m.SetValue(v);
                this.data.DrawTerrainColliderEdges = value;
            });
        }
#endif

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "World")
            {
                this.world = this.data.World;

                // Load our saved value
                this.data.DrawColliderEdges = this.GetValue(nameof(this.DrawColliderEdges));
                this.data.DrawColliderAabbs = this.GetValue(nameof(this.DrawColliderAabbs));
                this.data.DrawCollisionEvents = this.GetValue(nameof(this.DrawCollisionEvents));
                this.data.DrawTriggerEvents = this.GetValue(nameof(this.DrawTriggerEvents));
#if BL_QUILL
                this.data.DrawMeshColliderEdges = this.GetValue(nameof(this.DrawMeshColliderEdges));
                this.data.DrawTerrainColliderEdges = this.GetValue(nameof(this.DrawTerrainColliderEdges));
#endif
            }
        }

        private bool GetValue(string key)
        {
            return this.storageService.GetValue($"{BaseKey}.{this.world}.{key}", false);
        }

        private void SetValue(bool value, [CallerMemberName] string propertyName = null)
        {
            this.storageService.SetValue($"{BaseKey}.{this.world}.{propertyName}", value);
        }

        public struct Data : IBindingObjectNotifyData
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

            public FunctionPointer<OnPropertyChangedDelegate> Notify { get; set; }
        }
    }
}
#endif
