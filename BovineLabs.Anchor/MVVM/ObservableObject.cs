// <copyright file="ObservableObject.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.MVVM
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using UnityEngine.UIElements;

    /// <summary>
    /// Base class for observable models used by Anchor view models.
    /// </summary>
    [Serializable]
    public abstract class ObservableObject : INotifyPropertyChanged, INotifyPropertyChanging, INotifyBindablePropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangingEventHandler PropertyChanging;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        private event EventHandler<BindablePropertyChangedEventArgs> BindablePropertyChanged;

        /// <inheritdoc/>
        event EventHandler<BindablePropertyChangedEventArgs> INotifyBindablePropertyChanged.propertyChanged
        {
            add => this.BindablePropertyChanged += value;
            remove => this.BindablePropertyChanged -= value;
        }

        /// <summary>
        /// Raises <see cref="PropertyChanging"/>.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            this.PropertyChanging?.Invoke(this, e);
        }

        /// <summary>
        /// Raises <see cref="PropertyChanging"/>.
        /// </summary>
        /// <param name="propertyName">The changing property name.</param>
        protected void OnPropertyChanging([CallerMemberName] string propertyName = null)
        {
            this.OnPropertyChanging(new PropertyChangingEventArgs(propertyName));
        }

        /// <summary>
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            this.PropertyChanged?.Invoke(this, e);
            this.BindablePropertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(e.PropertyName));
        }

        /// <summary>
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <param name="propertyName">The changed property name.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets a backing field and raises changing/changed notifications when required.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="field">The backing field.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>True when the value changed.</returns>
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

        /// <summary>
        /// Sets a backing field and raises changing/changed notifications when required.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="field">The backing field.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="comparer">The comparer used to determine equality.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>True when the value changed.</returns>
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

        /// <summary>
        /// Sets a value through callback and raises changing/changed notifications when required.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="oldValue">The current value.</param>
        /// <param name="newValue">The next value.</param>
        /// <param name="callback">The assignment callback.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>True when the value changed.</returns>
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

        /// <summary>
        /// Sets a value through callback and raises changing/changed notifications when required.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="oldValue">The current value.</param>
        /// <param name="newValue">The next value.</param>
        /// <param name="comparer">The comparer used to determine equality.</param>
        /// <param name="callback">The assignment callback.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>True when the value changed.</returns>
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

        /// <summary>
        /// Sets a model value through callback and raises changing/changed notifications when required.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <param name="oldValue">The current value.</param>
        /// <param name="newValue">The next value.</param>
        /// <param name="model">The model instance.</param>
        /// <param name="callback">The assignment callback.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>True when the value changed.</returns>
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
