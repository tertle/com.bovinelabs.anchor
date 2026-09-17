namespace BovineLabs.Anchor.MVVM
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using UnityEngine.UIElements;

    [Serializable]
    public abstract class ObservableObject : INotifyPropertyChanged, INotifyPropertyChanging, INotifyBindablePropertyChanged
    {
        public event PropertyChangingEventHandler PropertyChanging;

        public event PropertyChangedEventHandler PropertyChanged;

        private event EventHandler<BindablePropertyChangedEventArgs> BindablePropertyChanged;

        event EventHandler<BindablePropertyChangedEventArgs> INotifyBindablePropertyChanged.propertyChanged
        {
            add => this.BindablePropertyChanged += value;
            remove => this.BindablePropertyChanged -= value;
        }

        protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            this.PropertyChanging?.Invoke(this, e);
        }

        protected void OnPropertyChanging([CallerMemberName] string propertyName = null)
        {
            this.OnPropertyChanging(new PropertyChangingEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            this.PropertyChanged?.Invoke(this, e);
            this.BindablePropertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(e.PropertyName));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            this.OnPropertyChanging(propertyName);
            field = newValue;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetProperty<T>(ref T field, T newValue, IEqualityComparer<T> comparer, [CallerMemberName] string propertyName = null)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            if (comparer.Equals(field, newValue))
            {
                return false;
            }

            this.OnPropertyChanging(propertyName);
            field = newValue;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetProperty<T>(T oldValue, T newValue, Action<T> callback, [CallerMemberName] string propertyName = null)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                return false;
            }

            this.OnPropertyChanging(propertyName);
            callback(newValue);
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetProperty<T>(T oldValue, T newValue, IEqualityComparer<T> comparer, Action<T> callback, [CallerMemberName] string propertyName = null)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (comparer.Equals(oldValue, newValue))
            {
                return false;
            }

            this.OnPropertyChanging(propertyName);
            callback(newValue);
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetProperty<T, TModel>(T oldValue, T newValue, TModel model, Action<TModel, T> callback, [CallerMemberName] string propertyName = null)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                return false;
            }

            this.OnPropertyChanging(propertyName);
            callback(model, newValue);
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}
