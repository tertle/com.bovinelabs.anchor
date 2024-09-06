// <copyright file="MemoryToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if !BL_DISABLE_TOOLBAR
namespace BovineLabs.Anchor.Debug.ToolbarTabs.Views
{
    using BovineLabs.Anchor.Debug.ToolbarTabs.ViewModels;
    using BovineLabs.Anchor.Toolbar;
    using Unity.Properties;
    using UnityEngine.UIElements;
    using KeyValueElement = BovineLabs.Anchor.Toolbar.KeyValueElement;

    [AutoToolbar("Memory")]
    public class MemoryToolbarView : VisualElement, IView
    {
        private readonly MemoryToolbarViewModel viewModel = new();

        public MemoryToolbarView()
        {
            TypeConverter<int, string> typeConverter = (ref int value) => $"{value} MB";

            this.Add(KeyValueElement.Create(this.viewModel, typeConverter, "Allocated", nameof(MemoryToolbarViewModel.TotalAllocatedMemoryMB)));
            this.Add(KeyValueElement.Create(this.viewModel, typeConverter, "Reserved", nameof(MemoryToolbarViewModel.TotalReservedMemoryMB)));
            this.Add(KeyValueElement.Create(this.viewModel, typeConverter, "Mono", nameof(MemoryToolbarViewModel.MonoUsedSizeMB)));
            this.Add(KeyValueElement.Create(this.viewModel, typeConverter, "Graphics", nameof(MemoryToolbarViewModel.AllocatedMemoryForGraphicsMB)));

            this.schedule.Execute(this.viewModel.Update).Every(1);
        }

        /// <inheritdoc/>
        object IView.ViewModel => this.viewModel;
    }
}
#endif
