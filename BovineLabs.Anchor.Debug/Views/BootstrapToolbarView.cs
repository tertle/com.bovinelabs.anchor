// <copyright file="BootstrapToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE_1_3_3_OR_NEWER && BL_CORE_EXTENSIONS
namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Toolbar;
    using BovineLabs.Core;
    using Toggle = Unity.AppUI.UI.Toggle;

    [AutoToolbar("Bootstrap")]
    public class BootstrapToolbarView : View<BootstrapToolbarViewModel>
    {
        public const string UssClassName = "bl-net-bootstrap-tab";

        public BootstrapToolbarView()
            : base(new BootstrapToolbarViewModel())
        {
            if (BovineLabsBootstrap.Instance == null)
            {
                this.schedule.Execute(() => this.parent?.RemoveFromHierarchy());
                return;
            }

            this.AddToClassList(UssClassName);

#if UNITY_NETCODE && !UNITY_CLIENT && !UNITY_SERVER
            var host = new Toggle { label = "Host", dataSource = this.ViewModel };
            host.SetBindingTwoWay(nameof(Toggle.value), nameof(BootstrapToolbarViewModel.Host));
            host.SetBindingToUI(nameof(this.enabledSelf), nameof(BootstrapToolbarViewModel.HostEnabled));
            this.Add(host);
#endif

#if !UNITY_CLIENT
#if UNITY_NETCODE
            var server = new Toggle
            {
                label = "Server",
                dataSource = this.ViewModel,
            };

            server.SetBindingTwoWay(nameof(Toggle.value), nameof(BootstrapToolbarViewModel.Server));
            server.SetBindingToUI(nameof(this.enabledSelf), nameof(BootstrapToolbarViewModel.ServerEnabled));
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

            client.SetBindingTwoWay(nameof(Toggle.value), nameof(BootstrapToolbarViewModel.Client));
            client.SetBindingToUI(nameof(this.enabledSelf), nameof(BootstrapToolbarViewModel.ClientEnabled));
            this.Add(client);
#endif

            var game = new Toggle
            {
                label = "Local",
                dataSource = this.ViewModel,
            };

            game.SetBindingTwoWay(nameof(Toggle.value), nameof(BootstrapToolbarViewModel.Local));
            game.SetBindingToUI(nameof(this.enabledSelf), nameof(BootstrapToolbarViewModel.LocalEnabled));
            this.Add(game);
#endif

            this.schedule.Execute(this.ViewModel.Update).Every(1);
        }
    }
}
#endif
