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

    public unsafe struct UIHelper<TM, TD>
        where TM : class, IBindingObjectNotify<TD>
        where TD : unmanaged
    {
        private TD* _data;
        private IntPtr _bindingHandle;

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

        public UIHelper(ref SystemState state, FixedString32Bytes name)
            : this(ref state, ComponentType.FromTypeIndex(TypeManager.GetTypeIndexFromStableTypeHash(UISystemTypes.NameToKey(name))))
        {
        }

        public ref TD Binding => ref UnsafeUtility.AsRef<TD>(_data);

        public void Bind()
        {
            if (_bindingHandle != IntPtr.Zero)
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

                _data = (TD*)UnsafeUtility.AddressOf(ref viewModel.Value);
                var handle = GCHandle.Alloc(new BindingContext(viewModelService, viewModel));
                _bindingHandle = GCHandle.ToIntPtr(handle);
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
                _data = null;
                throw;
            }
        }

        public void Unbind()
        {
            if (_bindingHandle == IntPtr.Zero)
            {
                _data = null;
                return;
            }

            var handle = GCHandle.FromIntPtr(_bindingHandle);
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
                _bindingHandle = IntPtr.Zero;
                _data = null;
            }
        }

        private sealed class BindingContext
        {
            public BindingContext(IViewModelService viewModelService, TM viewModel)
            {
                ViewModelService = viewModelService;
                ViewModel = viewModel;
            }

            public IViewModelService ViewModelService { get; }

            public TM ViewModel { get; }
        }
    }
}
