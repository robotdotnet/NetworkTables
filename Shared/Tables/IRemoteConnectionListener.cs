// ReSharper disable UnusedParameter.Global
namespace NetworkTables.Tables
{
    /// <summary>
    /// A listener that listens for connection changes in an <see cref="IRemote"/> object.
    /// </summary>
    public interface IRemoteConnectionListener
    {
        /// <summary>
        /// Called when an <see cref="IRemote"/> is connected
        /// </summary>
        /// <param name="remote">The object that connected.</param>
        /// <param name="info">An object containing information about the 
        /// connected remote.</param>
        void Connected(IRemote remote, ConnectionInfo info);

        /// <summary>
        /// Called when an <see cref="IRemote"/> is disconnected.
        /// </summary>
        /// <param name="remote">The object that disconnected.</param>
        /// <param name="info">An object containing information about the
        /// disconnected remote.</param>
        void Disconnected(IRemote remote, ConnectionInfo info);
    }
}