using System;
using System.Collections.Generic;
using NetworkTables.Exceptions;

namespace NetworkTables.Tables
{
    /// <summary>
    /// A table whose values can be read from and written to.
    /// </summary>
    public interface ITable
    {
        /// <summary>
        /// Determines whether the given key is in this table.
        /// </summary>
        /// <param name="key">The key to search for</param>
        /// <returns>If the table has a value assignend to the given key</returns>
        bool ContainsKey(string key);

        /// <summary>
        /// Determines whether there exists a non-empty subtable for this key in this table.
        /// </summary>
        /// <param name="key">The key to search for</param>
        /// <returns>If there is a subtable with the key which contains at least one key/subtable of its own</returns>
        bool ContainsSubTable(string key);

        /// <summary>
        /// Gets the subtable in this table for the given name.
        /// </summary>
        /// <param name="key">The name of the table relative to this one.</param>
        /// <returns>A sub table relative to this one</returns>
        ITable GetSubTable(string key);

        /// <summary>
        /// Gets the keys currently in the table with the specified flags
        /// </summary>
        /// <param name="types">The bitmask of flags to check for.</param>
        /// <returns>A set of the keys currently in the table with the specified flags.</returns>
        HashSet<string> GetKeys(NtType types);

        /// <summary>
        /// Gets all keys currently in the table.
        /// </summary>
        /// <returns>A set of all keys in the table.</returns>
        HashSet<string> GetKeys();

        /// <summary>
        /// Gets all sub-tables currently in the table.
        /// </summary>
        /// <returns>A set of all sub-tables in the table.</returns>
        HashSet<string> GetSubTables();

        /// <summary>
        /// Makes a key's value persistent through program restarts.
        /// </summary>
        /// <param name="key">The key name (cannot be null).</param>
        void SetPersistent(string key);

        /// <summary>
        /// Stop making a key's value persistent through program restarts.
        /// </summary>
        /// <param name="key">The key name (cannot be null).</param>
        void ClearPersistent(string key);

        /// <summary>
        /// Returns whether a value is persistent through program restarts.
        /// </summary>
        /// <param name="key">The key name (cannot be null).</param>
        /// <returns>True if the value is persistent.</returns>
        bool IsPersistent(string key);

        /// <summary>
        /// Sets flags on the specified key in this table.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <param name="flags">The flags to set. (Bitmask)</param>
        void SetFlags(string key, EntryFlags flags);

        /// <summary>
        /// Clears flags on the specified key in this table.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <param name="flags">The flags to clear. (Bitmask)</param>
        void ClearFlags(string key, EntryFlags flags);

        /// <summary>
        /// Returns the flags for the specified key.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <returns>The flags attached to the key.</returns>
        EntryFlags GetFlags(string key);

        /// <summary>
        /// Deletes the specifed key in this table.
        /// </summary>
        /// <param name="key">The key name.</param>
        void Delete(string key);

        /// <summary>
        /// Returns the value that the key maps to.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <param name="defaultValue">The default value if the key does not exist or is null</param>
        /// <returns>The value requested, or defaultValue if the key does not exist or is null.</returns>
        Value GetValue(string key, Value defaultValue);

        /// <summary>
        /// Returns the value that the key maps to.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <returns>The requested value.</returns>
        /// <exception cref="TableKeyNotDefinedException">Thrown if the key does not 
        /// exist in the table, or if the key is null.</exception>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if the key exists
        /// as a different type in the table.</exception>
        Value GetValue(string key);

        /// <summary>
        /// Maps the specified key to the specified value in the table. 
        /// </summary>
        /// <remarks>
        /// The key cannot be null. The value can be retreived by calling the Get method with
        /// the key used to Put the number in.
        /// </remarks>
        /// <param name="key">The key to map the value to.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>True if the key was set properly, otherwise false</returns>
        /// <exception cref="ArgumentException">Thrown if the value is not a type supported
        /// by the table.</exception>
        bool PutValue(string key, Value value);

        /// <summary>
        /// Maps the specified key to the specified value in the table. 
        /// </summary>
        /// <remarks>
        /// The key cannot be null. The value can be retreived by calling the Get method with
        /// the key used to Put the number in.
        /// </remarks>
        /// <param name="key">The key to map the value to.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>True if the key was set properly, otherwise false</returns>
        bool PutNumber(string key, double value);

        /// <summary>
        /// Returns the value that the key maps to.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <param name="defaultValue">The default value if the key does not exist or is null</param>
        /// <returns>The value requested, or defaultValue if the key does not exist or is null.</returns>
        double GetNumber(string key, double defaultValue);

        /// <summary>
        /// Returns the value that the key maps to.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <returns>The requested value.</returns>
        /// <exception cref="TableKeyNotDefinedException">Thrown if the key does not 
        /// exist in the table, or if the key is null.</exception>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if the key exists
        /// as a different type in the table.</exception>
        double GetNumber(string key);

        /// <summary>
        /// Maps the specified key to the specified value in the table. 
        /// </summary>
        /// <remarks>
        /// The key cannot be null. The value can be retreived by calling the Get method with
        /// the key used to Put the number in.
        /// </remarks>
        /// <param name="key">The key to map the value to.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>True if the key was set properly, otherwise false</returns>
        bool PutString(string key, string value);

        /// <summary>
        /// Returns the value that the key maps to.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <param name="defaultValue">The default value if the key does not exist or is null</param>
        /// <returns>The value requested, or defaultValue if the key does not exist or is null.</returns>
        string GetString(string key, string defaultValue);

        /// <summary>
        /// Returns the value that the key maps to.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <returns>The requested value.</returns>
        /// <exception cref="TableKeyNotDefinedException">Thrown if the key does not 
        /// exist in the table, or if the key is null.</exception>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if the key exists
        /// as a different type in the table.</exception>
        string GetString(string key);

        /// <summary>
        /// Maps the specified key to the specified value in the table. 
        /// </summary>
        /// <remarks>
        /// The key cannot be null. The value can be retreived by calling the Get method with
        /// the key used to Put the number in.
        /// </remarks>
        /// <param name="key">The key to map the value to.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>True if the key was set properly, otherwise false</returns>
        bool PutBoolean(string key, bool value);

        /// <summary>
        /// Returns the value that the key maps to.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <param name="defaultValue">The default value if the key does not exist or is null</param>
        /// <returns>The value requested, or defaultValue if the key does not exist or is null.</returns>
        bool GetBoolean(string key, bool defaultValue);

        /// <summary>
        /// Returns the value that the key maps to.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <returns>The requested value.</returns>
        /// <exception cref="TableKeyNotDefinedException">Thrown if the key does not 
        /// exist in the table, or if the key is null.</exception>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if the key exists
        /// as a different type in the table.</exception>
        bool GetBoolean(string key);

        /// <summary>
        /// Maps the specified key to the specified array of values in the table. 
        /// </summary>
        /// <remarks>
        /// The key cannot be null. The value can be retreived by calling the Get method with
        /// the key used to Put the number in.
        /// </remarks>
        /// <param name="key">The key to map the value to.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>True if the key was set properly, otherwise false</returns>
        bool PutBooleanArray(string key, bool[] value);

        /// <summary>
        /// Returns the value array that the key maps to.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <returns>The requested value array.</returns>
        /// <exception cref="TableKeyNotDefinedException">Thrown if the key does not 
        /// exist in the table, or if the key is null.</exception>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if the key exists
        /// as a different type in the table.</exception>
        bool[] GetBooleanArray(string key);

        /// <summary>
        /// Returns the value array that the key maps to.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <param name="defaultValue">The default value if the key does not exist or is null</param>
        /// <returns>The value array requested, or defaultValue if the key does not exist or is null.</returns>
        bool[] GetBooleanArray(string key, bool[] defaultValue);

        /// <summary>
        /// Maps the specified key to the specified array of values in the table. 
        /// </summary>
        /// <remarks>
        /// The key cannot be null. The value can be retreived by calling the Get method with
        /// the key used to Put the number in.
        /// </remarks>
        /// <param name="key">The key to map the value to.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>True if the key was set properly, otherwise false</returns>
        bool PutNumberArray(string key, double[] value);
        /// <summary>
        /// Returns the value array that the key maps to.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <returns>The requested value array.</returns>
        /// <exception cref="TableKeyNotDefinedException">Thrown if the key does not 
        /// exist in the table, or if the key is null.</exception>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if the key exists
        /// as a different type in the table.</exception>
        double[] GetNumberArray(string key);

        /// <summary>
        /// Returns the value array that the key maps to.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <param name="defaultValue">The default value if the key does not exist or is null</param>
        /// <returns>The value array requested, or defaultValue if the key does not exist or is null.</returns>
        double[] GetNumberArray(string key, double[] defaultValue);

        /// <summary>
        /// Maps the specified key to the specified array of values in the table. 
        /// </summary>
        /// <remarks>
        /// The key cannot be null. The value can be retreived by calling the Get method with
        /// the key used to Put the number in.
        /// </remarks>
        /// <param name="key">The key to map the value to.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>True if the key was set properly, otherwise false</returns>
        bool PutStringArray(string key, string[] value);

        /// <summary>
        /// Returns the value array that the key maps to.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <returns>The requested value array.</returns>
        /// <exception cref="TableKeyNotDefinedException">Thrown if the key does not 
        /// exist in the table, or if the key is null.</exception>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if the key exists
        /// as a different type in the table.</exception>
        string[] GetStringArray(string key);

        /// <summary>
        /// Returns the value array that the key maps to.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <param name="defaultValue">The default value if the key does not exist or is null</param>
        /// <returns>The value array requested, or defaultValue if the key does not exist or is null.</returns>
        string[] GetStringArray(string key, string[] defaultValue);

        /// <summary>
        /// Put a raw value (byte array) in the table.
        /// </summary>
        /// <param name="key">The key to be assigned to.</param>
        /// <param name="value">The value that will be assigned.</param>
        /// <returns>False if the table key already exists with a different type.</returns>
        bool PutRaw(string key, byte[] value);

        /// <summary>
        /// Returns the value array that the key maps to.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <returns>The requested value array.</returns>
        /// <exception cref="TableKeyNotDefinedException">Thrown if the key does not 
        /// exist in the table, or if the key is null.</exception>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if the key exists
        /// as a different type in the table.</exception>
        byte[] GetRaw(string key);

        /// <summary>
        /// Returns the value array that the key maps to.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <param name="defaultValue">The default value if the key does not exist or is null</param>
        /// <returns>The value array requested, or defaultValue if the key does not exist or is null.</returns>
        byte[] GetRaw(string key, byte[] defaultValue);


        /// <summary>
        /// Add a listener to changes to the table.
        /// </summary>
        /// <param name="listener">The listener to add</param>
        /// <param name="flags">The <see cref="EntryFlags"/> flags to use for the listener</param>
        void AddTableListenerEx(ITableListener listener, NotifyFlags flags);

        /// <summary>
        /// Add a listener for changes to a specific key in the table.
        /// </summary>
        /// <param name="key">The key to listen for</param>
        /// <param name="listener">The listener to add</param>
        /// <param name="flags">The <see cref="EntryFlags"/> flags to use for the listener</param>
        void AddTableListenerEx(string key, ITableListener listener, NotifyFlags flags);

        /// <summary>
        /// Adds a SubTable Listener.
        /// </summary>
        /// <param name="listener">The <see cref="ITableListener"/> to add.</param>
        /// <param name="localNotify">True if we want to notify local and remote listeners,
        /// otherwise just notify remote listeners.</param>
        void AddSubTableListener(ITableListener listener, bool localNotify);

        /// <summary>
        /// Add a listener to changes to the table.
        /// </summary>
        /// <param name="listener">The listener to add</param>
        /// <param name="immediateNotify">If true then this listener will be notified of all current entries (marked as new)</param>
        void AddTableListener(ITableListener listener, bool immediateNotify = false);

        /// <summary>
        /// Add a listener for changes to a specific key in the table.
        /// </summary>
        /// <param name="key">The key to listen for</param>
        /// <param name="listener">The listener to add</param>
        /// <param name="immediateNotify">If true then this listener will be notified of all current entries (marked as new)</param>
        void AddTableListener(string key, ITableListener listener, bool immediateNotify);

        /// <summary>
        /// Adds a SubTable Listener with the default flags, and without local notify.
        /// </summary>
        /// <param name="listener">The <see cref="ITableListener"/> to add.</param>
        void AddSubTableListener(ITableListener listener);

        /// <summary>
        /// Remove a listener from receiving table events.
        /// </summary>
        /// <param name="listener">The listener to be removed.</param>
        void RemoveTableListener(ITableListener listener);

        /// <summary>
        /// Add a listener to changes to the table.
        /// </summary>
        /// <param name="listenerDelegate">The Table Listener Delegate to add.</param>
        /// <param name="flags">The <see cref="EntryFlags"/> flags to use for the listener</param>
        void AddTableListenerEx(Action<ITable, string, Value, NotifyFlags> listenerDelegate, NotifyFlags flags);

        /// <summary>
        /// Add a listener for changes to a specific key in the table.
        /// </summary>
        /// <param name="key">The key to listen for</param>
        /// <param name="listenerDelegate">The Table Listener Delegate to add.</param>
        /// <param name="flags">The <see cref="EntryFlags"/> flags to use for the listener</param>
        void AddTableListenerEx(string key, Action<ITable, string, Value, NotifyFlags> listenerDelegate, NotifyFlags flags);

        /// <summary>
        /// Adds a SubTable Listener.
        /// </summary>
        /// <param name="listenerDelegate">The Table Listener Delegate to add.</param>
        /// <param name="localNotify">True if we want to notify local and remote listeners,
        /// otherwise just notify remote listeners.</param>
        void AddSubTableListener(Action<ITable, string, Value, NotifyFlags> listenerDelegate, bool localNotify);

        /// <summary>
        /// Add a listener to changes to the table.
        /// </summary>
        /// <param name="listenerDelegate">The Table Listener Delegate to add.</param>
        /// <param name="immediateNotify">If true then this listener will be notified of all current entries (marked as new)</param>
        void AddTableListener(Action<ITable, string, Value, NotifyFlags> listenerDelegate, bool immediateNotify = false);

        /// <summary>
        /// Add a listener for changes to a specific key in the table.
        /// </summary>
        /// <param name="key">The key to listen for</param>
        /// <param name="listenerDelegate">The Table Listener Delegate to add.</param>
        /// <param name="immediateNotify">If true then this listener will be notified of all current entries (marked as new)</param>
        void AddTableListener(string key, Action<ITable, string, Value, NotifyFlags> listenerDelegate, bool immediateNotify);

        /// <summary>
        /// Adds a SubTable Listener with the default flags, and without local notify.
        /// </summary>
        /// <param name="listenerDelegate">The Table Listener Delegate to add.</param>
        void AddSubTableListener(Action<ITable, string, Value, NotifyFlags> listenerDelegate);

        /// <summary>
        /// Remove a listener from receiving table events.
        /// </summary>
        /// <param name="listenerDelegate">The Table Listener Delegate to remove.</param>
        void RemoveTableListener(Action<ITable, string, Value, NotifyFlags> listenerDelegate);
    }
}