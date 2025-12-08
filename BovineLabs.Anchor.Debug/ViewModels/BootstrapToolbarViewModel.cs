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

#if !UNITY_SERVER
        private bool local;
        private bool localEnabled;
#if UNITY_NETCODE
        private bool client;
        private bool clientEnabled;
#endif
#endif

#if UNITY_NETCODE && !UNITY_CLIENT
        private bool server;
        private bool serverEnabled;
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
            get => this.local;
            set
            {
                if (this.SetProperty(ref this.local, value))
                {
                    if (value)
                    {
                        BovineLabsBootstrap.Instance.CreateGameWorld();
                    }
                    else
                    {
                        BovineLabsBootstrap.Instance.DestroyGameWorld();
                    }
                }
            }
        }

        [CreateProperty(ReadOnly = true)]
        public bool LocalEnabled
        {
            get => this.localEnabled;
            set => this.SetProperty(ref this.localEnabled, value);
        }

#if UNITY_NETCODE
        [CreateProperty]
        public bool Client
        {
            get => this.client;
            set
            {
                if (this.SetProperty(ref this.client, value))
                {
                    if (value)
                    {
                        BovineLabsBootstrap.Instance.CreateClientWorld();
                    }
                    else
                    {
                        BovineLabsBootstrap.Instance.DestroyClientWorld();
                    }
                }
            }
        }

        [CreateProperty(ReadOnly = true)]
        public bool ClientEnabled
        {
            get => this.clientEnabled;
            set => this.SetProperty(ref this.clientEnabled, value);
        }

#endif // UNITY_NETCODE
#endif // !UNITY_SERVER

#if !UNITY_CLIENT
#if UNITY_NETCODE
        [CreateProperty]
        public bool Server
        {
            get => this.server;
            set
            {
                if (this.SetProperty(ref this.server, value))
                {
                    if (value)
                    {
                        BovineLabsBootstrap.Instance.CreateServerWorld();
                    }
                    else
                    {
                        BovineLabsBootstrap.Instance.DestroyServerWorld();
                    }
                }
            }
        }

        [CreateProperty(ReadOnly = true)]
        public bool ServerEnabled
        {
            get => this.serverEnabled;
            set => this.SetProperty(ref this.serverEnabled, value);
        }
#endif
#endif

        public void Update()
        {
#if !UNITY_SERVER
            this.Local = BovineLabsBootstrap.GameWorld != null;
#endif

#if UNITY_NETCODE
            var clientWorldExists = ClientServerBootstrap.ClientWorld != null;
            var serverWorldExists = ClientServerBootstrap.ServerWorld != null;
#endif

#if UNITY_NETCODE && !UNITY_CLIENT && !UNITY_SERVER
            this.Host = clientWorldExists && serverWorldExists;
            this.HostEnabled = !this.Local && (clientWorldExists == serverWorldExists);
#endif

#if UNITY_NETCODE && !UNITY_SERVER
            this.Client = clientWorldExists;
            this.ClientEnabled = !this.Local;
#endif

#if UNITY_NETCODE && !UNITY_CLIENT
            this.Server = serverWorldExists;
#if !UNITY_SERVER
            this.ServerEnabled = !this.Local;
#else
            this.ServerEnabled = true;
#endif
#endif

#if !UNITY_SERVER
#if UNITY_NETCODE
#if UNITY_CLIENT || UNITY_SERVER
            this.LocalEnabled = false;
#else
            this.LocalEnabled = !this.Client && !this.Server;
#endif
#else
            this.LocalEnabled = true;
#endif
#endif
        }
    }
}
#endif
