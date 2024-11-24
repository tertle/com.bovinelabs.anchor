// <copyright file="MemoryToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Views
{
    using System;
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.Toolbar;
    using Unity.Properties;
    using UnityEngine.UIElements;

    [AutoToolbar("Memory")]
    public class MemoryToolbarView : VisualElement, IView<MemoryToolbarViewModel>
    {
        public const string UssClassName = "bl-memory-tab";

        public MemoryToolbarView()
        {
            this.AddToClassList(UssClassName);

            TypeConverter<int, string> typeConverter = (ref int value) => $"{value} MB";

            this.Add(KeyValueGroup.Create(this.ViewModel,
                new (string, string, Action<DataBinding>)[]
                {
                    ("Allocated", nameof(MemoryToolbarViewModel.TotalAllocatedMemoryMB), db => db.sourceToUiConverters.AddConverter(typeConverter)),
                    ("Reserved", nameof(MemoryToolbarViewModel.TotalReservedMemoryMB), db => db.sourceToUiConverters.AddConverter(typeConverter)),
                    ("Mono", nameof(MemoryToolbarViewModel.MonoUsedSizeMB), db => db.sourceToUiConverters.AddConverter(typeConverter)),
                    ("Graphics", nameof(MemoryToolbarViewModel.AllocatedMemoryForGraphicsMB), db => db.sourceToUiConverters.AddConverter(typeConverter)),
                }));

            this.schedule.Execute(this.ViewModel.Update).Every(1);
        }

        /// <inheritdoc />
        public MemoryToolbarViewModel ViewModel { get; } = new();
    }
}
