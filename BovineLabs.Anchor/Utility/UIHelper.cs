// <copyright file="UIHelper.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Runtime.InteropServices;
    using BovineLabs.Anchor.Binding;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Services;
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
        private TD* data;
        private IntPtr bindingHandle;

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
            if (this.bindingHandle != IntPtr.Zero)
            {
                throw new InvalidOperationException($"{nameof(UIHelper<TM, TD>)} is already bound.");
            }

            var viewModelService = AnchorApp.Current.Services.GetRequiredService<IViewModelService>();
            var viewModel = viewModelService.Load<TM>();
            var pinned = false;
            var loaded = false;

            try
            {
                viewModel.PinObject();
                pinned = true;

                if (viewModel is ILoadable loadable)
                {
                    loadable.Load();
                    loaded = true;
                }

                this.data = (TD*)UnsafeUtility.AddressOf(ref viewModel.Value);
                var handle = GCHandle.Alloc(new BindingContext(viewModelService, viewModel));
                this.bindingHandle = GCHandle.ToIntPtr(handle);
            }
            catch
            {
                if (loaded && viewModel is ILoadable loadable)
                {
                    loadable.Unload();
                }

                if (pinned)
                {
                    viewModel.UnpinObject();
                }

                viewModelService.Unload<TM>();
                this.data = null;
                throw;
            }
        }

        /// <summary>Unpins and unloads the view model that was previously bound.</summary>
        public void Unbind()
        {
            if (this.bindingHandle == IntPtr.Zero)
            {
                this.data = null;
                return;
            }

            var handle = GCHandle.FromIntPtr(this.bindingHandle);
            var context = (BindingContext)handle.Target;

            try
            {
                if (context != null)
                {
                    try
                    {
                        if (context.ViewModel is ILoadable loadable)
                        {
                            loadable.Unload();
                        }
                    }
                    finally
                    {
                        try
                        {
                            context.ViewModel.UnpinObject();
                        }
                        finally
                        {
                            context.ViewModelService.Unload<TM>();
                        }
                    }
                }
            }
            finally
            {
                handle.Free();
                this.bindingHandle = IntPtr.Zero;
                this.data = null;
            }
        }

        private sealed class BindingContext
        {
            public BindingContext(IViewModelService viewModelService, TM viewModel)
            {
                this.ViewModelService = viewModelService;
                this.ViewModel = viewModel;
            }

            public IViewModelService ViewModelService { get; }

            public TM ViewModel { get; }
        }
    }
}
