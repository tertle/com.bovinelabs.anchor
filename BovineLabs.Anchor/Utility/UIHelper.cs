// <copyright file="UIHelper.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor
{
    using System.Runtime.InteropServices;
    using BovineLabs.Anchor.Binding;
    using BovineLabs.Anchor.Services;
    using Unity.AppUI.MVVM;
    using Unity.Collections.LowLevel.Unsafe;

    public unsafe struct UIHelper<TM, TD>
        where TM : class, IBindingObject<TD>
        where TD : unmanaged
    {
        private GCHandle handle;
        private TD* data;

        public ref TD Binding => ref UnsafeUtility.AsRef<TD>(this.data);

        public void Bind()
        {
            var viewModel = App.current.services.GetService<IViewModelService>().Load<TM>();

            viewModel.Load();
            this.handle = GCHandle.Alloc(viewModel, GCHandleType.Pinned);
            this.data = (TD*)UnsafeUtility.AddressOf(ref viewModel.Value);
        }

        public void Unbind()
        {
            var viewModel = App.current.services.GetService<IViewModelService>().Get<TM>();

            viewModel.Unload();
            this.handle.Free();
            this.handle = default;
            this.data = default;

            App.current.services.GetService<IViewModelService>().Unload<TM>();
        }
    }
}
#endif
