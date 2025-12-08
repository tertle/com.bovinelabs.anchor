// <copyright file="BootstrapToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE_1_3_3_OR_NEWER && BL_CORE_EXTENSIONS
namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Core;
    using Unity.AppUI.MVVM;
#if UNITY_NETCODE
    using Unity.NetCode;
#endif
    using Unity.Properties;

    public class BootstrapToolbarViewModel : ObservableObject
    {
#if UNITY_NETCODE && !UNITY_CLIENT && !UNITY_SERVER
        private bool host;
        private bool hostEnabled;
#endif

#if UNITY_NETCODE && !UNITY_CLIENT && !UNITY_SERVER
        [CreateProperty]
        public bool Host
        {
            get => this.host;
            set
            {
                if (this.SetProperty(ref this.host, value))
                {
                    if (value)
                    {
                        BovineLabsBootstrap.Instance.CreateClientServerWorlds();
                    }
                    else
                    {
                        BovineLabsBootstrap.Instance.DestroyClientServerWorlds();
                    }
                }
            }
        }

        [CreateProperty(ReadOnly = true)]
        public bool HostEnabled
        {
            get => this.hostEnabled;
            set => this.SetProperty(ref this.hostEnabled, value);
        }
#endif

#if !UNITY_SERVER
        [CreateProperty]
        public bool Local
        {
            get => BovineLabsBootstrap.GameWorld != null;
            set => this.SetProperty(this.Local, value, value =>
            {
                if (value)
                {
                    BovineLabsBootstrap.Instance.CreateGameWorld();
                }
                else
                {
                    BovineLabsBootstrap.Instance.DestroyGameWorld();
                }
            });
        }

        [CreateProperty]
        public bool LocalEnabled
        {
            get
            {
#if UNITY_NETCODE
#if UNITY_CLIENT || UNITY_SERVER
                return false;
#else
                return !this.Client && !this.Server;
#endif
#else
                return true;
#endif
            }
        }

#if UNITY_NETCODE
        [CreateProperty]
        public bool Client
        {
            get => ClientServerBootstrap.ClientWorld != null;
            set
            {
                this.SetProperty(this.Client, value, value =>
                {
                    if (value)
                    {
                        BovineLabsBootstrap.Instance.CreateClientWorld();
                    }
                    else
                    {
                        BovineLabsBootstrap.Instance.DestroyClientWorld();
                    }
                });
            }
        }

        [CreateProperty]
        public bool ClientEnabled => !this.Local;

#endif // UNITY_NETCODE
#endif // !UNITY_SERVER

#if !UNITY_CLIENT
#if UNITY_NETCODE
        [CreateProperty]
        public bool Server
        {
            get => ClientServerBootstrap.ServerWorld != null;
            set => this.SetProperty(this.Server, value, value =>
            {
                if (value)
                {
                    BovineLabsBootstrap.Instance.CreateServerWorld();
                }
                else
                {
                    BovineLabsBootstrap.Instance.DestroyServerWorld();
                }
            });
        }

        [CreateProperty]
        public bool ServerEnabled
        {
            get
            {
#if !UNITY_SERVER
                return !this.Local;
#else
                return true;
#endif
            }
        }
#endif
#endif

        public void Update()
        {
#if UNITY_NETCODE && !UNITY_CLIENT && !UNITY_SERVER
            this.Host = ClientServerBootstrap.ClientWorld != null && ClientServerBootstrap.ServerWorld != null;
            this.HostEnabled = !this.Local && (ClientServerBootstrap.ClientWorld != null) == (ClientServerBootstrap.ServerWorld != null);
#endif
        }

    }
}
#endif
