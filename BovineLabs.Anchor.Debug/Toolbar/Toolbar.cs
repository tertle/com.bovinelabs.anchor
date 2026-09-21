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
    using Unity.Collections;
    using Unity.Scripting.LifecycleManagement;
    using UnityEngine;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    [Configurable]
    [IsService]
    public sealed unsafe class Toolbar : IAnchorToolbarHost, IDisposable
    {
        public const float UpdateRateSeconds = 1 / 4f;

        private const string ActiveTabKey = "bl.active-tab";
        private const string ShowRibbonKey = "bl.show-ribbon";

        [ConfigVar("anchor.toolbar", true, "Should the toolbar be shown", true)]
        private static readonly SharedStatic<bool> Show = SharedStatic<bool>.GetOrCreate<Toolbar, EnabledVar>();

        [NoAutoStaticsCleanup]
        private static long nextOwnerId;

        private readonly SortedDictionary<int, Registration> _registrations = new();
        private readonly IServiceProvider _serviceProvider;
        private readonly ToolbarViewModel _viewModel;
        private readonly ILocalStorageService _storageService;
        private readonly long _ownerId;

        private ToolbarView _currentView;
        private string _activeTabName;
        private int _nextRegistrationId;
        private bool _isRibbonVisible;
        private bool _isToolbarHidden;
        private bool _disposed;

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

            _ownerId = ++nextOwnerId;
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _activeTabName = storageService.GetValue(ActiveTabKey, string.Empty);
            _isRibbonVisible = storageService.GetValue(ShowRibbonKey, false);
            _isToolbarHidden = !Show.Data;

            SetBurstActiveTab(_activeTabName);

            try
            {
                RegisterAutoToolbars(autoToolbarTypes ?? throw new ArgumentNullException(nameof(autoToolbarTypes)));
                Current = this;
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        [NoAutoStaticsCleanup]
        internal static Toolbar Current { get; private set; }

        internal static bool IsAvailable => Current != null;

        internal string ActiveTabName => _activeTabName;

        internal bool IsRibbonVisible => _isRibbonVisible;

        internal bool IsToolbarHidden => _isToolbarHidden;

        public VisualElement CreateRootVisualElement()
        {
            ThrowIfDisposed();
            ReleaseRootVisualElement();

            var view = new ToolbarView(this, _viewModel);

            try
            {
                foreach (var registration in _registrations.Values)
                {
                    Materialize(view, registration);
                }

                view.CompleteComposition();
                _currentView = view;
                return view;
            }
            catch
            {
                view.Dispose();
                throw;
            }
        }

        public void ReleaseRootVisualElement()
        {
            var view = _currentView;
            _currentView = null;
            view?.Dispose();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Current = null;
            ToolbarViewData.ActiveTab.Data = default;

            var regArray = _registrations.Values.ToArray();
            _registrations.Clear();

            ReleaseRootVisualElement();

            foreach (var registration in regArray)
            {
                _viewModel.RemoveSelection(registration.ElementName);
                registration.Release();
            }
        }

        public ToolbarRegistrationHandle Register<TModel, TData>(string tabName, string elementName, out TData* data)
            where TModel : class, IToolbarElement, IBindingObjectNotify<TData>, new()
            where TData : unmanaged
        {
            ThrowIfDisposed();
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

                return AddRegistration(tabName, elementName, model, () => ReleaseDynamicModel<TModel, TData>(model, isSerializable, saveKey));
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

        internal bool Remove(ToolbarRegistrationHandle handle)
        {
            if (handle.OwnerId != _ownerId || !_registrations.Remove(handle.RegistrationId, out var registration))
            {
                return false;
            }

            _currentView?.RemoveRegistration(handle.RegistrationId);
            _viewModel.RemoveSelection(registration.ElementName);
            registration.Release();
            return true;
        }

        internal static Toolbar GetRequired()
        {
            return Current ?? throw new InvalidOperationException("The Anchor toolbar service is not available.");
        }

        internal void SetActiveTab(string tabName)
        {
            _activeTabName = tabName ?? string.Empty;
            SetBurstActiveTab(_activeTabName);
            _storageService.SetValue(ActiveTabKey, _activeTabName);
        }

        internal void SetRibbonVisible(bool visible)
        {
            _isRibbonVisible = visible;
            _storageService.SetValue(ShowRibbonKey, visible);
        }

        internal void SetToolbarHidden(bool hidden)
        {
            _isToolbarHidden = hidden;
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

        private static void SetBurstActiveTab(string tabName)
        {
            var activeTab = default(FixedString32Bytes);

            if (activeTab.CopyFromTruncated(tabName) != CopyError.None)
            {
                activeTab = default;
            }

            ToolbarViewData.ActiveTab.Data = activeTab;
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

                if (_serviceProvider.GetService(entry.Type) is not IToolbarElement model)
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

                    AddRegistration(tabName, entry.Attribute.ElementName, model, () =>
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
            var registrationId = ++_nextRegistrationId;
            var handle = new ToolbarRegistrationHandle(_ownerId, registrationId);
            var registration = new Registration(handle, tabName, elementName, model, release);

            _registrations.Add(registrationId, registration);

            try
            {
                _viewModel.AddSelection(elementName);

                if (_currentView != null)
                {
                    Materialize(_currentView, registration);
                }

                return handle;
            }
            catch
            {
                _currentView?.RemoveRegistration(registrationId);
                _viewModel.RemoveSelection(elementName);
                _registrations.Remove(registrationId);
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
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Toolbar));
            }
        }

        private sealed class Registration
        {
            public Registration(ToolbarRegistrationHandle handle, string tabName, string elementName, IToolbarElement model, Action release)
            {
                Handle = handle;
                TabName = tabName;
                ElementName = elementName;
                Model = model;
                Release = release;
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
