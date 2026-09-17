namespace BovineLabs.Anchor.Binding
{
    using Unity.Collections;
    using UnityEngine.UIElements;

    public interface IBindingObjectNotify : INotifyBindablePropertyChanged
    {
        void OnPropertyChanging(in FixedString64Bytes property);

        void OnPropertyChanged(in FixedString64Bytes property);
    }
}
