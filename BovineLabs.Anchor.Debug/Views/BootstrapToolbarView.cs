// <copyright file="BootstrapToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE_1_3_3_OR_NEWER && BL_CORE_EXTENSIONS
namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Toolbar;
    using BovineLabs.Core;
    using Unity.Properties;
    using UnityEngine.UIElements;
    using Toggle = Unity.AppUI.UI.Toggle;

    [AutoToolbar("Bootstrap")]
    public class BootstrapToolbarView : VisualElement, IView<BootstrapToolbarViewModel>
    {
        public const string UssClassName = "bl-net-bootstrap-tab";

        public BootstrapToolbarView()
        {
            if (BovineLabsBootstrap.Instance == null)
            {
                this.schedule.Execute(() => this.parent?.RemoveFromHierarchy());
                return;
            }

            this.AddToClassList(UssClassName);

#if UNITY_NETCODE && !UNITY_CLIENT && !UNITY_SERVER
            var host = new Toggle
            {
                label = "Host",
                dataSource = this.ViewModel,
            };

            host.SetBinding(nameof(Toggle.value), new DataBinding
            {
                updateTrigger = BindingUpdateTrigger.EveryUpdate,
                dataSourcePath = new PropertyPath(nameof(BootstrapToolbarViewModel.Host)),
            });

            host.SetBinding(nameof(this.enabledSelf), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                updateTrigger = BindingUpdateTrigger.EveryUpdate,
                dataSourcePath = new PropertyPath(nameof(BootstrapToolbarViewModel.HostEnabled)),
            });

            this.Add(host);
#endif

#if !UNITY_CLIENT
#if UNITY_NETCODE
            var server = new Toggle
            {
                label = "Server",
                dataSource = this.ViewModel,
            };

            server.SetBinding(nameof(Toggle.value), new DataBinding
            {
                updateTrigger = BindingUpdateTrigger.EveryUpdate,
                dataSourcePath = new PropertyPath(nameof(BootstrapToolbarViewModel.Server)),
            });

            server.SetBinding(nameof(this.enabledSelf), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                updateTrigger = BindingUpdateTrigger.EveryUpdate,
                dataSourcePath = new PropertyPath(nameof(BootstrapToolbarViewModel.ServerEnabled)),
            });

            this.Add(server);
#endif
#endif

#if !UNITY_SERVER
#if UNITY_NETCODE
            var client = new Toggle
            {
                label = "Client",
                dataSource = this.ViewModel,
            };

            client.SetBinding(nameof(Toggle.value), new DataBinding
            {
                updateTrigger = BindingUpdateTrigger.EveryUpdate,
                dataSourcePath = new PropertyPath(nameof(BootstrapToolbarViewModel.Client)),
            });

            client.SetBinding(nameof(this.enabledSelf), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                updateTrigger = BindingUpdateTrigger.EveryUpdate,
                dataSourcePath = new PropertyPath(nameof(BootstrapToolbarViewModel.ClientEnabled)),
            });

            this.Add(client);
#endif

            var game = new Toggle
            {
                label = "Local",
                dataSource = this.ViewModel,
            };

            game.SetBinding(nameof(Toggle.value), new DataBinding
            {
                updateTrigger = BindingUpdateTrigger.EveryUpdate,
                dataSourcePath = new PropertyPath(nameof(BootstrapToolbarViewModel.Local)),
            });

            game.SetBinding(nameof(this.enabledSelf), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                updateTrigger = BindingUpdateTrigger.EveryUpdate,
                dataSourcePath = new PropertyPath(nameof(BootstrapToolbarViewModel.LocalEnabled)),
            });

            this.Add(game);
#endif
        }

        public BootstrapToolbarViewModel ViewModel { get; } = new();
    }
}
#endif
