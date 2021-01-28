using SpotifyLibV2.Helpers;

namespace SpotifyUwpV2.Helpers
{
    /// <summary>
    ///     A <c>struct</c> that automatically resolves a service via <see cref="ServiceLocator" />, without assigning a value.
    /// </summary>
    /// <typeparam name="T">Service to resolve</typeparam>
    public struct LazyService<T>
    {
        /// <summary>
        ///     Returns the service if is initialized, null otherwise
        /// </summary>
        public T Value => ServiceLocator.Default.GetInstance<T>();
    }
}
