// <copyright file="SubSceneToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE_EXTENSIONS && !BL_DISABLE_SUBSCENE
namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Binding;
    using Unity.AppUI.UI;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Properties;

    public class SubSceneToolbarViewModel : SystemObservableObject<SubSceneToolbarViewModel.Data>
    {
        private List<string> subScenes = new();

        // private IEnumerable<int> values = new List<int>();
        private Data data;

        public override ref Data Value => ref this.data;

        [CreateProperty]
        public List<string> SubScenes
        {
            get
            {
                if (this.data.SubScenes.IsCreated)
                {
                    if (this.data.SubScenes.Length != this.subScenes.Count)
                    {
                        RebuildList();
                    }
                    else
                    {
                        for (var i = 0; i < this.data.SubScenes.Length; i++)
                        {
                            if (this.data.SubScenes[i].Name == this.subScenes[i])
                            {
                                continue;
                            }

                            RebuildList();
                            break;
                        }
                    }
                }

                return this.subScenes;

                void RebuildList()
                {
                    this.subScenes = new List<string>();
                    foreach (var n in this.data.SubScenes.AsArray())
                    {
                        this.subScenes.Add(n.Name.ToString());
                    }
                }
            }
        }

        [CreateProperty]
        public IEnumerable<int> SubSceneValues
        {
            get => this.data.SubSceneValues.IsCreated ? this.data.SubSceneValues.AsArray().ToArray() : Array.Empty<int>();

            set
            {
                if (!this.data.SubSceneValues.IsCreated)
                {
                    return;
                }

                if (this.data.IgnoreValueUpdate)
                {
                    this.data.IgnoreValueUpdate = false;
                    return;
                }

                this.SetProperty(this.data.SubSceneValues.AsArray(), value, SequenceComparer.Int, value =>
                {
                    this.data.SubSceneSelectedChanged = true;

                    this.data.SubSceneValues.Clear();
                    foreach (var v in value)
                    {
                        this.data.SubSceneValues.Add(v);
                    }
                });
            }
        }

        [CreateProperty]
        public Action<DropdownItem, int> BindItem => this.BindItemMethod;

        private void BindItemMethod(DropdownItem item, int i)
        {
            item.label = this.subScenes[i];
        }

        public struct Data : IBindingObjectNotifyData
        {
            private NativeList<int> subSceneValues;
            private NativeList<SubSceneName> subScenes;

            public NativeList<int> SubSceneValues
            {
                readonly get => this.subSceneValues;
                set
                {
                    this.subSceneValues = value;
                    this.Notify();
                }
            }

            public NativeList<SubSceneName> SubScenes
            {
                readonly get => this.subScenes;
                set
                {
                    this.subScenes = value;
                    this.Notify();
                }
            }

            public bool IgnoreValueUpdate { get; set; }

            public bool SubSceneSelectedChanged { get; set; }

            public FunctionPointer<OnPropertyChangedDelegate> Notify { get; set; }

            public struct SubSceneName : IComparable<SubSceneName>, IEquatable<SubSceneName>
            {
                public Entity Entity;
                public FixedString64Bytes Name;

                public int CompareTo(SubSceneName other)
                {
                    return this.Name.CompareTo(other.Name);
                }

                public bool Equals(SubSceneName other)
                {
                    return this.Entity.Equals(other.Entity) && this.Name.Equals(other.Name);
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        return (this.Entity.GetHashCode() * 397) ^ this.Name.GetHashCode();
                    }
                }
            }
        }
    }
}
#endif
