// <copyright file="UIHelper.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor
{
    using System.Runtime.InteropServices;
    using BovineLabs.Anchor.Binding;
    using Unity.AppUI.MVVM;
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
            var view = this.GetView();

            view.ViewModel.Load();
            this.handle = GCHandle.Alloc(view.ViewModel, GCHandleType.Pinned);
            this.data = (TD*)UnsafeUtility.AddressOf(ref view.ViewModel.Value);
        }

        public void Unbind()
        {
            var view = this.GetView();

            view.ViewModel.Unload();
            this.handle.Free();
            this.handle = default;
            this.data = default;
        }

        /// <summary> Gets the view. </summary>
        /// <returns> The view that was loaded from this helper. </returns>
        /// <remarks> Not burst compatible. </remarks>
        public TV GetView()
        {
            // TODO cache
            return App.current.services.GetService<TV>();
        }
    }
}
#endif
