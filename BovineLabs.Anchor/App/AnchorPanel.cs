namespace BovineLabs.Anchor
{
    using Unity.AppUI.UI;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;
#if UNITY_LOCALIZATION
    using System.Threading.Tasks;
    using Unity.AppUI.Core;
    using Unity.Localization;
#endif

    [Preserve]
    public class AnchorPanel : Panel, IAnchorPanel
    {
#if UNITY_LOCALIZATION
        public AnchorPanel()
        {
            this.RegisterCallback<AttachToPanelEvent>(this.OnLocalizationAttached);
            this.RegisterCallback<DetachFromPanelEvent>(this.OnLocalizationDetached);
        }

        private void OnLocalizationAttached(AttachToPanelEvent evt)
        {
            LocalizationSettings.InitializationCompleted += this.OnLocalizationInitialized;
            LocalizationSettings.SelectedLocaleChanged += this.OnLocaleChanged;
            this.OnLocalizationInitialized();
        }

        private void OnLocalizationDetached(DetachFromPanelEvent evt)
        {
            LocalizationSettings.InitializationCompleted -= this.OnLocalizationInitialized;
            LocalizationSettings.SelectedLocaleChanged -= this.OnLocaleChanged;
        }

        private void OnLocalizationInitialized()
        {
            this.OnLocaleChanged(LocalizationSettings.SelectedLocale);
        }

        private void OnLocaleChanged(Locale locale)
        {
            this.ProvideContext(new LangContext(locale?.Identifier.Code ?? this.lang)
            {
                GetLocalizedStringAsyncFunc = GetLocalizedStringAsync,
            });
        }

        private static async Task<string> GetLocalizedStringAsync(string reference, string language, params object[] arguments)
        {
            if (!LocalizationUtils.TryGetTableAndEntry(reference, out var table, out var entry))
            {
                return reference;
            }

            await LocalizationSettings.InitializeAsync();
            var locale = LocalizationSettings.Instance.GetLocale(language);
            var database = LocalizationSettings.ResourceDatabase;
            var resource = await database.GetEntryAsync(table, entry, locale);
            if (resource is not IStringEntry || (resource.SharedEntry.IsSmart && (arguments == null || arguments.Length == 0)))
            {
                return reference;
            }

            return await database.GetLocalizedStringAsync(table, entry, locale, arguments);
        }
#endif

        public VisualElement RootVisualElement => this;

        public string Theme
        {
            get => this.theme;
            set => this.theme = value;
        }

        public string Scale
        {
            get => this.scale;
            set => this.scale = value;
        }
    }
}
