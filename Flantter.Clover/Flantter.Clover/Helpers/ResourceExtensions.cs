using Windows.ApplicationModel.Resources;

namespace Flantter.Clover.Helpers
{
    public static class ResourceExtensions
    {
        private static ResourceLoader _resourceLoader = new ResourceLoader();

        public static string GetLocalized(this string resourceKey)
        {
            return _resourceLoader.GetString(resourceKey);
        }
    }
}
