// <copyright file="Toolbar.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Toolbar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using BovineLabs.Anchor.Binding;
    using BovineLabs.Anchor.Services;
    using BovineLabs.Anchor.Toolbar;
    using BovineLabs.Core.ConfigVars;
    using BovineLabs.Core.Utility;
    using Unity.Burst;
    using UnityEngine;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    /// <summary>
    /// Durable owner of toolbar registrations, models, persistence, and visual generations.
    /// </summary>
    [Preserve]
    [Configurable]
    [IsService]
    public sealed unsafe class Toolbar : IAnchorToolbarHost, IDisposable
    {
        /// <summary>Default polling interval in seconds for managed toolbar updates.</summary>
        public const float UpdateRateSeconds = 1 / 4f;

        private const string ActiveTabKey = "bl.active-tab";
        private const string ShowRibbonKey = "bl.show-ribbon";

        [ConfigVar("anchor.toolbar", true, "Should the toolbar be shown", true)]
        private static readonly SharedStatic<bool> Show = SharedStatic<bool>.GetOrCreate<Toolbar, EnabledVar>();

        private static long nextOwnerId;

        private readonly SortedDictionary<int, Registration> registrations = new();
        private readonly IServiceProvider serviceProvider;
        private readonly ToolbarViewModel viewModel;
        private readonly ILocalStorageService storageService;
        private readonly long ownerId;

        private ToolbarView currentView;
        private string activeTabName;
        private int nextRegistrationId;
        private bool isRibbonVisible;
        private bool isToolbarHidden;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Toolbar"/> class.
        /// </summary>
        /// <param name="serviceProvider">Anchor service provider used to resolve managed auto-toolbar models.</param>
        /// <param name="viewModel">Durable toolbar filter model.</param>
        /// <param name="storageService">Persistence layer for toolbar chrome state.</param>
        [Preserve]
        public Toolbar(IServiceProvider serviceProvider, ToolbarViewModel viewModel, ILocalStorageService storageService)
            : this(serviceProvider, viewModel, storageService, ReflectionUtility.GetAllWithAttribute<AutoToolbarAttribute>())
        {
        }

        internal Toolbar(IServiceProvider serviceProvider, ToolbarViewModel viewModel, ILocalStorageService storageService, IEnumerable<Type> autoToolbarTypes)
        {
            if (Current != null)
            {
                throw new InvalidOperationException("Only one Anchor toolbar service can be active.");
            }

            this.ownerId = ++nextOwnerId;
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            this.storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            this.activeTabName = storageService.GetValue(ActiveTabKey, string.Empty);
            this.isRibbonVisible = storageService.GetValue(ShowRibbonKey, false);
            this.isToolbarHidden = !Show.Data;

            ToolbarViewData.ActiveTab.Data = this.activeTabName;

            try
            {
                this.RegisterAutoToolbars(autoToolbarTypes ?? throw new ArgumentNullException(nameof(autoToolbarTypes)));
                Current = this;
            }
            catch
            {
                this.Dispose();
                throw;
            }
        }

        internal static Toolbar Current { get; private set; }

        internal static bool IsAvailable => Current != null;

        internal string ActiveTabName => this.activeTabName;

        internal bool IsRibbonVisible => this.isRibbonVisible;

        internal bool IsToolbarHidden => this.isToolbarHidden;

        /// <inheritdoc />
        public VisualElement CreateRootVisualElement()
        {
            this.ThrowIfDisposed();
            this.ReleaseRootVisualElement();

            var view = new ToolbarView(this, this.viewModel);

            try
            {
                foreach (var registration in this.registrations.Values)
                {
                    this.Materialize(view, registration);
                }

                view.CompleteComposition();
                this.currentView = view;
                return view;
            }
            catch
            {
                view.Dispose();
                throw;
            }
        }

        /// <inheritdoc />
        public void ReleaseRootVisualElement()
        {
            var view = this.currentView;
            this.currentView = null;
            view?.Dispose();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            Current = null;
            ToolbarViewData.ActiveTab.Data = default;

            var regArray = this.registrations.Values.ToArray();
            this.registrations.Clear();

            this.ReleaseRootVisualElement();

            foreach (var registration in regArray)
            {
                this.viewModel.RemoveSelection(registration.ElementName);
                registration.Release();
            }
        }

        /// <summary>
        /// Registers a new ECS-backed toolbar model and returns its pinned data pointer.
        /// </summary>
        /// <typeparam name="TModel">Toolbar model type.</typeparam>
        /// <typeparam name="TData">Unmanaged binding data type.</typeparam>
        /// <param name="tabName">Toolbar tab name.</param>
        /// <param name="elementName">Toolbar group/filter name.</param>
        /// <param name="data">Pinned data pointer used by the registering ECS system.</param>
        /// <returns>An opaque handle used to remove the registration.</returns>
        public ToolbarRegistrationHandle Register<TModel, TData>(string tabName, string elementName, out TData* data)
            where TModel : class, IToolbarElement, IBindingObjectNotify<TData>, new()
            where TData : unmanaged
        {
            this.ThrowIfDisposed();
            ValidatePresentationMetadata(tabName, elementName);

            data = null;

            var model = new TModel();
            var isSerializable = typeof(TModel).IsDefined(typeof(SerializableAttribute), false);
            var saveKey = GetSaveKey(tabName, elementName);
            var pinned = false;
            var loaded = false;

            try
            {
                if (isSerializable)
                {
                    var json = PlayerPrefs.GetString(saveKey, string.Empty);
                    if (!string.IsNullOrEmpty(json))
                    {
                        JsonUtility.FromJsonOverwrite(json, model);
                    }
                }

                data = model.PinObject();
                pinned = true;

                if (model is ILoadable loadable)
                {
                    loadable.Load();
                    loaded = true;
                }

                return this.AddRegistration(tabName, elementName, model, () => ReleaseDynamicModel<TModel, TData>(model, isSerializable, saveKey));
            }
            catch
            {
                if (loaded && model is ILoadable loadable)
                {
                    loadable.Unload();
                }

                if (pinned)
                {
                    model.UnpinObject();
                }

                data = null;
                throw;
            }
        }

        /// <summary>Removes a durable toolbar registration.</summary>
        /// <param name="handle">Handle returned when the registration was created.</param>
        /// <returns><c>true</c> when a live registration was removed; otherwise <c>false</c>.</returns>
        internal bool Remove(ToolbarRegistrationHandle handle)
        {
            if (handle.OwnerId != this.ownerId || !this.registrations.Remove(handle.RegistrationId, out var registration))
            {
                return false;
            }

            this.currentView?.RemoveRegistration(handle.RegistrationId);
            this.viewModel.RemoveSelection(registration.ElementName);
            registration.Release();
            return true;
        }

        internal static Toolbar GetRequired()
        {
            return Current ?? throw new InvalidOperationException("The Anchor toolbar service is not available.");
        }

        internal void SetActiveTab(string tabName)
        {
            this.activeTabName = tabName ?? string.Empty;
            ToolbarViewData.ActiveTab.Data = this.activeTabName;
            this.storageService.SetValue(ActiveTabKey, this.activeTabName);
        }

        internal void SetRibbonVisible(bool visible)
        {
            this.isRibbonVisible = visible;
            this.storageService.SetValue(ShowRibbonKey, visible);
        }

        internal void SetToolbarHidden(bool hidden)
        {
            this.isToolbarHidden = hidden;
        }

        private static void ValidatePresentationMetadata(string tabName, string elementName)
        {
            if (string.IsNullOrWhiteSpace(tabName))
            {
                throw new ArgumentException("Tab name cannot be null or whitespace.", nameof(tabName));
            }

            if (string.IsNullOrWhiteSpace(elementName))
            {
                throw new ArgumentException("Element name cannot be null or whitespace.", nameof(elementName));
            }
        }

        private static string GetSaveKey(string tabName, string elementName)
        {
            return $"bl.toolbar.{tabName}.{elementName}";
        }

        private static void ReleaseDynamicModel<TModel, TData>(TModel model, bool isSerializable, string saveKey)
            where TModel : class, IBindingObjectNotify<TData>
            where TData : unmanaged
        {
            if (isSerializable)
            {
                var saveData = JsonUtility.ToJson(model);
                PlayerPrefs.SetString(saveKey, saveData);
            }

            if (model is ILoadable loadable)
            {
                loadable.Unload();
            }

            model.UnpinObject();
        }

        private void RegisterAutoToolbars(IEnumerable<Type> autoToolbarTypes)
        {
            var serviceTabName = AnchorApp.Current?.ServiceTabName;
            if (string.IsNullOrWhiteSpace(serviceTabName))
            {
                serviceTabName = AnchorApp.DefaultServiceTabName;
            }

            var types = autoToolbarTypes
                .Distinct()
                .Select(type => (Type: type, Attribute: type.GetCustomAttribute<AutoToolbarAttribute>()))
                .Where(entry => entry.Attribute != null)
                .OrderBy(entry => entry.Attribute.TabName ?? serviceTabName, StringComparer.Ordinal)
                .ThenBy(entry => entry.Attribute.ElementName, StringComparer.Ordinal)
                .ThenBy(entry => entry.Type.FullName, StringComparer.Ordinal);

            foreach (var entry in types)
            {
                if (!typeof(IToolbarElement).IsAssignableFrom(entry.Type))
                {
                    throw new InvalidOperationException($"{entry.Type} does not implement {nameof(IToolbarElement)}.");
                }

                if (!entry.Type.IsDefined(typeof(IsServiceAttribute), true))
                {
                    throw new InvalidOperationException($"{entry.Type} is not defined as an Anchor service.");
                }

                if (this.serviceProvider.GetService(entry.Type) is not IToolbarElement model)
                {
                    throw new InvalidOperationException($"Unable to resolve auto-toolbar model '{entry.Type.FullName}'.");
                }

                var tabName = entry.Attribute.TabName ?? serviceTabName;
                var loaded = false;

                try
                {
                    if (model is ILoadable loadable)
                    {
                        loadable.Load();
                        loaded = true;
                    }

                    this.AddRegistration(tabName, entry.Attribute.ElementName, model, () =>
                    {
                        if (model is ILoadable registeredLoadable)
                        {
                            registeredLoadable.Unload();
                        }
                    });
                }
                catch
                {
                    if (loaded && model is ILoadable loadable)
                    {
                        loadable.Unload();
                    }

                    throw;
                }
            }
        }

        private ToolbarRegistrationHandle AddRegistration(string tabName, string elementName, IToolbarElement model, Action release)
        {
            var registrationId = ++this.nextRegistrationId;
            var handle = new ToolbarRegistrationHandle(this.ownerId, registrationId);
            var registration = new Registration(handle, tabName, elementName, model, release);

            this.registrations.Add(registrationId, registration);

            try
            {
                this.viewModel.AddSelection(elementName);

                if (this.currentView != null)
                {
                    this.Materialize(this.currentView, registration);
                }

                return handle;
            }
            catch
            {
                this.currentView?.RemoveRegistration(registrationId);
                this.viewModel.RemoveSelection(elementName);
                this.registrations.Remove(registrationId);
                throw;
            }
        }

        private void Materialize(ToolbarView view, Registration registration)
        {
            var element = registration.Model.CreateElement() ??
                throw new InvalidOperationException($"{registration.Model.GetType()} returned a null toolbar element.");

            element.dataSource = registration.Model;
            view.AddRegistration(registration.Handle.RegistrationId, registration.TabName, registration.ElementName, element);
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(Toolbar));
            }
        }

        private sealed class Registration
        {
            public Registration(ToolbarRegistrationHandle handle, string tabName, string elementName, IToolbarElement model, Action release)
            {
                this.Handle = handle;
                this.TabName = tabName;
                this.ElementName = elementName;
                this.Model = model;
                this.Release = release;
            }

            public ToolbarRegistrationHandle Handle { get; }

            public string TabName { get; }

            public string ElementName { get; }

            public IToolbarElement Model { get; }

            public Action Release { get; }
        }

        private struct EnabledVar
        {
        }
    }
}
