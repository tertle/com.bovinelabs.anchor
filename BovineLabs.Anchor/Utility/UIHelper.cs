// <copyright file="UIHelper.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Runtime.InteropServices;
    using BovineLabs.Anchor.Binding;
    using BovineLabs.Anchor.Services;
    using Unity.AppUI.MVVM;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;

    /// <summary>
    /// Helper that pins a view model's unmanaged data so burst systems can mutate UI state directly.
    /// </summary>
    public unsafe struct UIHelper<TM, TD>
        where TM : class, IBindingObjectNotify<TD>
        where TD : unmanaged
    {
        private GCHandle handle;
        private TD* data;

        /// <summary>
        /// Initializes a new instance of the <see cref="UIHelper{TM, TD}"/> struct.
        /// </summary>
        /// <param name="state">System state used to satisfy component requirements.</param>
        /// <param name="requiredComponent">Component type that must exist before the UI helper binds.</param>
        public UIHelper(ref SystemState state, ComponentType requiredComponent)
        {
            this = default;

            var list = new FixedList32Bytes<ComponentType>
            {
                new()
                {
                    TypeIndex = requiredComponent.TypeIndex,
                    AccessModeType = ComponentType.AccessMode.ReadOnly,
                },
            };

            var query = new EntityQueryBuilder(Allocator.Temp).WithAll(ref list).WithOptions(EntityQueryOptions.IncludeSystems).Build(ref state);
            state.RequireForUpdate(query);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UIHelper{TM, TD}"/> struct.
        /// </summary>
        /// <param name="state">System state used to satisfy component requirements.</param>
        /// <param name="name">Navigation state that should be resolved to a component requirement.</param>
        public UIHelper(ref SystemState state, FixedString32Bytes name)
            : this(ref state, ComponentType.FromTypeIndex(TypeManager.GetTypeIndexFromStableTypeHash(UISystemTypes.NameToKey(name))))
        {
        }

        /// <summary>Gets direct access to the pinned view model data.</summary>
        public ref TD Binding => ref UnsafeUtility.AsRef<TD>(this.data);

        /// <summary>Loads and pins the view model so the helper can forward changes.</summary>
        public void Bind()
        {
            var viewModel = App.current.services.GetRequiredService<IViewModelService>().Load<TM>();
            viewModel.Load();

            if (viewModel is IInitializable initializable)
            {
                initializable.Initialize();
            }

            this.handle = GCHandle.Alloc(viewModel.Value, GCHandleType.Pinned);
            this.data = (TD*)UnsafeUtility.AddressOf(ref viewModel.Value);
        }

        /// <summary>Unpins and unloads the view model that was previously bound.</summary>
        public void Unbind()
        {
            var viewModel = App.current.services.GetRequiredService<IViewModelService>().Get<TM>();

            if (viewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }

            viewModel?.Unload();
            this.handle.Free();
            this.handle = default;
            this.data = null;

            App.current.services.GetRequiredService<IViewModelService>().Unload<TM>();
        }
    }
}
