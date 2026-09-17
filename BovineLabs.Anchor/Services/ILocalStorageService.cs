namespace BovineLabs.Anchor.Services
{
    public interface ILocalStorageService
    {
        bool HasKey(string key);

        void DeleteKey(string key);

        string GetValue(string key, string defaultValue = null);

        void SetValue(string key, string value);

        int GetValue(string key, int defaultValue);

        void SetValue(string key, int value);

        bool GetValue(string key, bool defaultValue);

        void SetValue(string key, bool value);
    }
}
