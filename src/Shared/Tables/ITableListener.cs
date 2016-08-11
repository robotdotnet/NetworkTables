namespace NetworkTables.Tables
{
    /// <summary>
    /// A listener that listens to changes in values in an <see cref="ITable"/>
    /// </summary>
    public interface ITableListener
    {
        /// <summary>
        /// Called when a key-value pair is changed in an <see cref="ITable"/>.
        /// </summary>
        /// <param name="source">The table the key-value pair exist in.</param>
        /// <param name="key">The key associated with the value that changed.</param>
        /// <param name="value">The new value.</param>
        /// <param name="flags">The update flags.</param>
        void ValueChanged(ITable source, string key, Value value, NotifyFlags flags);
    }
}