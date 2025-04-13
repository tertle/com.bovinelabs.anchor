// <copyright file="UIHelper.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor
{
    using System;
    using System.Runtime.InteropServices;
    using BovineLabs.Anchor.Binding;
    using BovineLabs.Anchor.Contracts;
    using BovineLabs.Anchor.Services;
    using Unity.AppUI.MVVM;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;

    public unsafe struct UIHelper<TM, TD>
        where TM : class, IBindingObject<TD>
        where TD : unmanaged
    {
        private GCHandle handle;
        private TD* data;

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

#if BL_CORE
        public UIHelper(ref SystemState state, FixedString32Bytes name)
            : this(ref state, ComponentType.FromTypeIndex(TypeManager.GetTypeIndexFromStableTypeHash(UISystemTypes.NameToKey(name))))
        {
        }
#endif

        public ref TD Binding => ref UnsafeUtility.AsRef<TD>(this.data);

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

        public void Unbind()
        {
            var viewModel = App.current.services.GetRequiredService<IViewModelService>().Get<TM>();

            if (viewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }

            viewModel.Unload();
            this.handle.Free();
            this.handle = default;
            this.data = null;

            App.current.services.GetRequiredService<IViewModelService>().Unload<TM>();
        }
    }
}
#endif
