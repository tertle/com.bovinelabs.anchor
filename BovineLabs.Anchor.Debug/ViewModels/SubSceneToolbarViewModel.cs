#if BL_NERVE
namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.Views;
    using Unity.AppUI.UI;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Properties;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public partial class SubSceneToolbarViewModel : SystemObservableObject<SubSceneToolbarViewModel.Data>, ILoadable, IToolbarElement
    {
        [CreateProperty]
        public UIArray<Data.SubSceneName> SubScenes => Value.SubScenes;

        [CreateProperty]
        public IEnumerable<int> SubSceneValues
        {
            get => Value.SubSceneValues.Value.AsArray();
            set => SetProperty(Value.SubSceneValues, value);
        }

        public void Load()
        {
            Value.Initialize();
        }

        public void Unload()
        {
            Value.Dispose();
        }

        public VisualElement CreateElement()
        {
            return new SubSceneToolbarView(this);
        }

        public void BindItem(DropdownItem item, int index)
        {
            const string scenePrefix = "Scene: ";
            const string sceneSectionPrefix = "SceneSection: ";

            var name = Value.SubScenes[index].Name.ToString();

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
            private ChangedList<int> _subSceneValues;

            [SystemProperty]
            private NativeList<SubSceneName> _subScenes;

            internal void Initialize()
            {
                _subSceneValues = new NativeList<int>(Allocator.Persistent);
                _subScenes = new NativeList<SubSceneName>(Allocator.Persistent);
            }

            internal void Dispose()
            {
                _subSceneValues.Value.Dispose();
                _subScenes.Dispose();
            }

            public struct SubSceneName : IComparable<SubSceneName>, IEquatable<SubSceneName>
            {
                public Entity Entity;
                public FixedString128Bytes Name;

                public int CompareTo(SubSceneName other)
                {
                    return Name.CompareTo(other.Name);
                }

                public bool Equals(SubSceneName other)
                {
                    return Entity.Equals(other.Entity) && Name.Equals(other.Name);
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        return (Entity.GetHashCode() * 397) ^ Name.GetHashCode();
                    }
                }
            }
        }
    }
}
#endif
