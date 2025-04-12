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
    using BovineLabs.Core.Extensions;
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

        public UIHelper(ref SystemState state, FixedString64Bytes name, ComponentType requiredComponent)
        {
            this = default;

            state.EntityManager.AddComponentData(state.SystemHandle, new UIStateInstance
            {
                Name = name,
                ComponentType = requiredComponent.TypeIndex,
            });

            var query = new EntityQueryBuilder(Allocator.Temp).WithAll(requiredComponent).Build(ref state);
            state.RequireForUpdate(query);
        }

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
