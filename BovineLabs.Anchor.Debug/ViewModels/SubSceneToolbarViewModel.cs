// <copyright file="SubSceneToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE_EXTENSIONS && !BL_DISABLE_SUBSCENE
namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Contracts;
    using Unity.AppUI.UI;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Properties;

    public partial class SubSceneToolbarViewModel : SystemObservableObject<SubSceneToolbarViewModel.Data>, IInitializable, IDisposable
    {
        [CreateProperty]
        public UIArray<Data.SubSceneName> SubScenes => this.Value.SubScenes;

        [CreateProperty]
        public IEnumerable<int> SubSceneValues
        {
            get => this.Value.SubSceneValues.Value.AsArray();
            set => this.SetProperty(this.Value.SubSceneValues, value);
        }

        public void Initialize()
        {
            this.Value.Initialize();
        }

        public void Dispose()
        {
            this.Value.Dispose();
        }

        public void BindItem(DropdownItem item, int index)
        {
            const string scenePrefix = "Scene: ";
            const string sceneSectionPrefix = "SceneSection: ";

            var name = this.Value.SubScenes[index].Name.ToString();

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

            internal void Initialize()
            {
                this.subSceneValues = new NativeList<int>(Allocator.Persistent);
                this.subScenes = new NativeList<SubSceneName>(Allocator.Persistent);
            }

            internal void Dispose()
            {
                this.subSceneValues.Value.Dispose();
                this.subScenes.Dispose();
            }

            public struct SubSceneName : IComparable<SubSceneName>, IEquatable<SubSceneName>
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
            }
        }
    }
}
#endif
