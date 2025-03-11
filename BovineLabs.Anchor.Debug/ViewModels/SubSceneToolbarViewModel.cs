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
        private readonly List<string> subScenes = new();
        private readonly List<int> values = new();

        private Data data;

        public override ref Data Value => ref this.data;

        [CreateProperty]
        public List<string> SubScenes => this.data.SubScenes.IsCreated
            ? DropDownHelper.GetItems(this.subScenes, this.data.SubScenes.AsArray(), Formatter)
            : this.subScenes;

        [CreateProperty]
        public IEnumerable<int> SubSceneValues
        {
            get => DropDownHelper.GetMultiValues(this.values, this.data.SubSceneValues);

            set
            {
                if (!this.data.SubSceneValues.IsCreated)
                {
                    return;
                }

                this.SetProperty(this.data.SubSceneValues.AsArray(), value, SequenceComparer.Int, value =>
                {
                    this.data.SubSceneSelectedChanged = true;
                    DropDownHelper.SetMultiValues(value, this.data.SubSceneValues);
                });
            }
        }

        public void BindItem(DropdownItem item, int index)
        {
            DropDownHelper.BindItem(this.subScenes, item, index);
        }

        private static string Formatter(string name)
        {
            const string scenePrefix = "Scene: ";
            const string sceneSectionPrefix = "SceneSection: ";

            var sceneIndex = name.IndexOf(scenePrefix, StringComparison.Ordinal);
            if (sceneIndex != -1)
            {
                name = name.Remove(sceneIndex, scenePrefix.Length);
            }

            var indexSection = name.IndexOf(sceneSectionPrefix, StringComparison.Ordinal);
            if (indexSection != -1)
            {
                name = name.Remove(indexSection, sceneSectionPrefix.Length);
            }

            return name;
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

            public bool SubSceneSelectedChanged { get; set; }

            public FunctionPointer<OnPropertyChangedDelegate> Notify { get; set; }

            public struct SubSceneName : IComparable<SubSceneName>, IEquatable<SubSceneName>, IDropDownItem
            {
                public Entity Entity;
                public FixedString128Bytes Name;

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

                public string GetString()
                {
                    return this.Name.ToString();
                }
            }
        }
    }
}
#endif
