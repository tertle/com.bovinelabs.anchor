// <copyright file="UIHelper.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System.Runtime.InteropServices;
    using BovineLabs.Anchor.Binding;
    using BovineLabs.Anchor.Services;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine.UIElements;

    public unsafe struct UIHelper<TV, TM, TD>
        where TV : VisualElement, IView<TM>
        where TM : class, IBindingObject<TD>
        where TD : unmanaged
    {
        private GCHandle handle;
        private TD* data;

        public ref TD Binding => ref UnsafeUtility.AsRef<TD>(this.data);

        public void Bind()
        {
            var view = ViewProvider.Instance.GetView<TV>();

            var binding = view.ViewModel;
            this.handle = GCHandle.Alloc(binding, GCHandleType.Pinned);
            this.data = (TD*)UnsafeUtility.AddressOf(ref binding.Value);

            binding.Load();
        }

        public void Unbind()
        {
            this.handle.Free();
            this.handle = default;
            this.data = default;
        }

        // Not burst compatible
        public TV GetView()
        {
            return ViewProvider.Instance.GetView<TV>();
        }
    }
}