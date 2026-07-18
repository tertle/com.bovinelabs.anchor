// <copyright file="MemoryToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Views
{
    using System;
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Elements;
    using Unity.Properties;
    using UnityEngine.UIElements;
    using UnityEngine.Scripting;

    [Preserve]
    public class MemoryToolbarView : VisualElement
    {
        public const string UssClassName = "bl-memory-tab";

        [Preserve]
        public MemoryToolbarView(MemoryToolbarViewModel viewModel)
        {
            this.dataSource = viewModel;
            this.AddToClassList(UssClassName);

            TypeConverter<int, string> typeConverter = (ref int value) => $"{value} MB";

            this.Add(KeyValueGroup.Create(viewModel,
                new (string, string, Action<DataBinding>)[]
                {
                    ("Allocated", nameof(MemoryToolbarViewModel.TotalAllocatedMemoryMB), db => db.sourceToUiConverters.AddConverter(typeConverter)),
                    ("Reserved", nameof(MemoryToolbarViewModel.TotalReservedMemoryMB), db => db.sourceToUiConverters.AddConverter(typeConverter)),
                    ("Mono", nameof(MemoryToolbarViewModel.MonoUsedSizeMB), db => db.sourceToUiConverters.AddConverter(typeConverter)),
                    ("Graphics", nameof(MemoryToolbarViewModel.AllocatedMemoryForGraphicsMB), db => db.sourceToUiConverters.AddConverter(typeConverter)),
                }));

            this.schedule.Execute(this.UpdateModel).Every(1);
        }

        private MemoryToolbarViewModel Model => (MemoryToolbarViewModel)this.dataSource;

        private void UpdateModel()
        {
            this.Model.Update();
        }
    }
}
