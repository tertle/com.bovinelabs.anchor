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
            RegisterCallback<AttachToPanelEvent>(OnLocalizationAttached);
            RegisterCallback<DetachFromPanelEvent>(OnLocalizationDetached);
        }

        private void OnLocalizationAttached(AttachToPanelEvent evt)
        {
            LocalizationSettings.InitializationCompleted += OnLocalizationInitialized;
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            OnLocalizationInitialized();
        }

        private void OnLocalizationDetached(DetachFromPanelEvent evt)
        {
            LocalizationSettings.InitializationCompleted -= OnLocalizationInitialized;
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }

        private void OnLocalizationInitialized()
        {
            OnLocaleChanged(LocalizationSettings.SelectedLocale);
        }

        private void OnLocaleChanged(Locale locale)
        {
            this.ProvideContext(new LangContext(locale?.Identifier.Code ?? lang)
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
            get => theme;
            set => theme = value;
        }

        public string Scale
        {
            get => scale;
            set => scale = value;
        }
    }
}
