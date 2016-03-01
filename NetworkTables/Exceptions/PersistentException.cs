using System.IO;

namespace NetworkTables.Exceptions
{
    /// <summary>
    /// An exception thrown when the persistent load/save fails in a <see cref="NetworkTable"/>.
    /// </summary>
    public class PersistentException : IOException
    {
        /// <summary>
        /// Creates a new <see cref="PersistentException"/>.
        /// </summary>
        /// <param name="message">The error message.</param>
        public PersistentException(string message) : base(message)
        {

        }
    }
}