namespace MusicLibrary.Enum
{

    public enum QueueUpdateReason
    {
        /// <summary>
        /// New q items added
        /// </summary>
        Added,
        /// <summary>
        /// Q items removed
        /// </summary>
        Removed,
        /// <summary>
        /// New queue is set.
        /// </summary>
        Set,

        /// <summary>
        /// Items moved
        /// </summary>
        Moved
    }
}