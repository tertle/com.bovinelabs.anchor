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
        public List<string> SubScenes => DropDownHelper.GetItems(this.subScenes, this.data.SubScenes.AsArray(), Formatter);

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

        [CreateProperty]
        public Action<DropdownItem, int> BindItem => DropDownHelper.BindItem(this.subScenes);

        private static string Formatter(string name)
        {
            const string scenePrefix = "Scene: ";

            var index = name.IndexOf(scenePrefix, StringComparison.Ordinal);
            if (index != -1)
            {
                name = name.Remove(index, scenePrefix.Length);
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

                public string GetString()
                {
                    return this.Name.ToString();
                }
            }
        }
    }
}
#endif
