// <copyright file="SubSceneToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE_EXTENSIONS && !BL_DISABLE_SUBSCENE
namespace BovineLabs.Anchor.Debug.ToolbarTabs.ViewModels
{
    using System.Collections.Generic;
    using BovineLabs.Core.Extensions;
    using BovineLabs.Core.UI;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Properties;

    public class SubSceneToolbarViewModel : BLObservableObject<SubSceneToolbarViewModel.Data>
    {
        private readonly List<string> subScenes = new();
        private readonly List<int> valuesCache = new();

        private static readonly SequenceComparer<int> SelectedComparer = new();

        private IEnumerable<int> values = new List<int>();
        private Data data;

        public override ref Data Value => ref this.data;

        [CreateProperty]
        public List<string> SubScenes
        {
            get
            {
                if (this.data.SubScenes.IsCreated)
                {
                    this.subScenes.Clear();
                    foreach (var n in this.data.SubScenes.AsArray())
                    {
                        this.subScenes.Add(n.ToString());
                    }
                }

                return this.subScenes;
            }
        }

        [CreateProperty]
        public IEnumerable<int> SubSceneSelected
        {
            get
            {
                if (this.data.SubScenes.IsCreated)
                {
                    this.valuesCache.Clear();
                    this.valuesCache.AddRangeNative(this.data.Values.AsArray());
                    this.values = this.valuesCache;
                }

                return this.values;
            }

            set
            {
                if (this.SetProperty(ref this.values, value, SelectedComparer))
                {
                    this.data.Values.Clear();
                    foreach (var v in this.values)
                    {
                        this.data.Values.Add(v);
                        this.data.SubSceneSelectedChanged = true;
                    }
                }
            }
        }

        public bool SubSceneSelectedChanged { get; set; }

        public struct Data : IBindingObjectNotifyData
        {
            private NativeList<int> values;
            private NativeList<FixedString64Bytes> subScenes;

            public NativeList<int> Values
            {
                readonly get => this.values;
                set
                {
                    this.values = value;
                    this.Notify();
                }
            }

            public NativeList<FixedString64Bytes> SubScenes
            {
                readonly get => this.subScenes;
                set
                {
                    this.subScenes = value;
                    this.Notify();
                }
            }

            public bool SubSceneSelectedChanged { get; set; }

            public FunctionPointer<OnPropertyChangedDelegate> Notify { get; set; }
        }
    }
}
#endif
