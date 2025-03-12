// <copyright file="SubSceneToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE_EXTENSIONS && !BL_DISABLE_SUBSCENE
namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Contracts;
    using BovineLabs.Core.Extensions;
    using Unity.AppUI.UI;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Properties;

    public partial class SubSceneToolbarViewModel : SystemObservableObject<SubSceneToolbarViewModel.Data>, IInitializable, IDisposable
    {
        [CreateProperty]
        public UIList<Data.SubSceneName> SubScenes => this.Value.SubScenes;

        [CreateProperty]
        public IEnumerable<int> SubSceneValues
        {
            get => this.Value.SubSceneValues.Value.AsArray();
            set => this.SetProperty(this.Value.SubSceneValues, value);
        }

        public void Initialize()
        {
            this.Value.SubSceneValues = new NativeList<int>(Allocator.Persistent);
            this.Value.SubScenes = new NativeList<Data.SubSceneName>(Allocator.Persistent);
        }

        public void Dispose()
        {
            this.Value.SubScenes.Dispose();
            this.Value.SubSceneValues.Value.Dispose();
        }

        public void BindItem(DropdownItem item, int index)
        {
            const string scenePrefix = "Scene: ";
            const string sceneSectionPrefix = "SceneSection: ";

            var name = this.Value.SubScenes[index].GetString();

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

            item.label = name;
        }

        public partial struct Data
        {
            [SystemProperty]
            private ChangedList<int> subSceneValues;

            [SystemProperty]
            private NativeList<SubSceneName> subScenes;

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
