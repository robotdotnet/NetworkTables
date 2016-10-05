using System;
using System.Collections.Generic;
using NetworkTables.Exceptions;
using System.Threading.Tasks;
using System.Linq;
#if !CORE
using NetworkTables.Logging;
#endif
#if CORE
using NetworkTables.Core.Native;
#endif

namespace NetworkTables
{
    /// <summary>
    /// This class contains all NtCore methods exposed by the underlying library.
    /// </summary>
    /// <remarks>All keys in this method start from the root of the table</remarks>
    /// <seealso cref="NetworkTable"/>
    public static class NtCore
    {
        /// <summary>
        /// Sets an entry value in the table if it does not exist.
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <param name="value">The value to set</param>
        /// <returns>False if the type does not match existing value</returns>
        public static bool SetDefaultEntryBoolean(string name, bool value)
        {
#if CORE
            return CoreMethods.SetDefaultEntryBoolean(name, value);
#else
            return Storage.Instance.SetDefaultEntryValue(name, Value.MakeBoolean(value));
#endif
        }

        /// <summary>
        /// Sets an entry value in the table if it does not exist.
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <param name="value">The value to set</param>
        /// <returns>False if the type does not match existing value</returns>
        public static bool SetDefaultEntryDouble(string name, double value)
        {
#if CORE
            return CoreMethods.SetDefaultEntryDouble(name, value);
#else
            return Storage.Instance.SetDefaultEntryValue(name, Value.MakeDouble(value));
#endif
        }

        /// <summary>
        /// Sets an entry value in the table if it does not exist.
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <param name="value">The value to set</param>
        /// <returns>False if the type does not match existing value</returns>
        public static bool SetDefaultEntryString(string name, string value)
        {
#if CORE
            return CoreMethods.SetDefaultEntryString(name, value);
#else
            return Storage.Instance.SetDefaultEntryValue(name, Value.MakeString(value));
#endif
        }

        /// <summary>
        /// Sets an entry value in the table if it does not exist.
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <param name="value">The value to set</param>
        /// <returns>False if the type does not match existing value</returns>
        public static bool SetDefaultEntryBooleanArray(string name, IList<bool> value)
        {
#if CORE
            return CoreMethods.SetDefaultEntryBooleanArray(name, value);
#else
            return Storage.Instance.SetDefaultEntryValue(name, Value.MakeBooleanArray(value));
#endif
        }

        /// <summary>
        /// Sets an entry value in the table if it does not exist.
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <param name="value">The value to set</param>
        /// <returns>False if the type does not match existing value</returns>
        public static bool SetDefaultEntryDoubleArray(string name, IList<double> value)
        {
#if CORE
            return CoreMethods.SetDefaultEntryDoubleArray(name, value);
#else
            return Storage.Instance.SetDefaultEntryValue(name, Value.MakeDoubleArray(value));
#endif
        }

        /// <summary>
        /// Sets an entry value in the table if it does not exist.
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <param name="value">The value to set</param>
        /// <returns>False if the type does not match existing value</returns>
        public static bool SetDefaultEntryStringArray(string name, IList<string> value)
        {
#if CORE
            return CoreMethods.SetDefaultEntryStringArray(name, value);
#else
            return Storage.Instance.SetDefaultEntryValue(name, Value.MakeStringArray(value));
#endif
        }

        /// <summary>
        /// Sets an entry value in the table if it does not exist.
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <param name="value">The value to set</param>
        /// <returns>False if the type does not match existing value</returns>
        public static bool SetDefaultEntryRaw(string name, IList<byte> value)
        {
#if CORE
            return CoreMethods.SetDefaultEntryRaw(name, value);
#else
            return Storage.Instance.SetDefaultEntryValue(name, Value.MakeRaw(value));
#endif
        }

        /// <summary>
        /// Sets an entry value
        /// </summary>
        /// <remarks>If the type of the new value differs from the type currently
        /// stored and the force parameter is false (default), returns error and
        /// does not update value. If force is true, the value type in the
        /// table is changed</remarks>
        /// <param name="name">The entry name</param>
        /// <param name="value">The value to set</param>
        /// <param name="force">True to force an update even if types are different</param>
        /// <returns>True on success, otherwise false</returns>
        public static bool SetEntryBoolean(string name, bool value, bool force = false)
        {
#if CORE
            return CoreMethods.SetEntryBoolean(name, value, force);
#else
            if (force)
            {
                Storage.Instance.SetEntryTypeValue(name, Value.MakeBoolean(value));
                return true;
            }
            return Storage.Instance.SetEntryValue(name, Value.MakeBoolean(value));
#endif
        }

        /// <summary>
        /// Sets an entry value
        /// </summary>
        /// <remarks>If the type of the new value differs from the type currently
        /// stored and the force parameter is false (default), returns error and
        /// does not update value. If force is true, the value type in the
        /// table is changed</remarks>
        /// <param name="name">The entry name</param>
        /// <param name="value">The value to set</param>
        /// <param name="force">True to force an update even if types are different</param>
        /// <returns>True on success, otherwise false</returns>
        public static bool SetEntryDouble(string name, double value, bool force = false)
        {
#if CORE
            return CoreMethods.SetEntryDouble(name, value, force);
#else
            if (force)
            {
                Storage.Instance.SetEntryTypeValue(name, Value.MakeDouble(value));
                return true;
            }
            return Storage.Instance.SetEntryValue(name, Value.MakeDouble(value));
#endif
        }

        /// <summary>
        /// Sets an entry value
        /// </summary>
        /// <remarks>If the type of the new value differs from the type currently
        /// stored and the force parameter is false (default), returns error and
        /// does not update value. If force is true, the value type in the
        /// table is changed</remarks>
        /// <param name="name">The entry name</param>
        /// <param name="value">The value to set</param>
        /// <param name="force">True to force an update even if types are different</param>
        /// <returns>True on success, otherwise false</returns>
        public static bool SetEntryString(string name, string value, bool force = false)
        {
#if CORE
            return CoreMethods.SetEntryString(name, value, force);
#else
            if (force)
            {
                Storage.Instance.SetEntryTypeValue(name, Value.MakeString(value));
                return true;
            }
            return Storage.Instance.SetEntryValue(name, Value.MakeString(value));
#endif
        }

        /// <summary>
        /// Sets an entry value
        /// </summary>
        /// <remarks>If the type of the new value differs from the type currently
        /// stored and the force parameter is false (default), returns error and
        /// does not update value. If force is true, the value type in the
        /// table is changed</remarks>
        /// <param name="name">The entry name</param>
        /// <param name="value">The value to set</param>
        /// <param name="force">True to force an update even if types are different</param>
        /// <returns>True on success, otherwise false</returns>
        public static bool SetEntryBooleanArray(string name, IList<bool> value, bool force = false)
        {
#if CORE
            return CoreMethods.SetEntryBooleanArray(name, value, force);
#else
            if (force)
            {
                Storage.Instance.SetEntryTypeValue(name, Value.MakeBooleanArray(value));
                return true;
            }
            return Storage.Instance.SetEntryValue(name, Value.MakeBooleanArray(value));
#endif
        }

        /// <summary>
        /// Sets an entry value
        /// </summary>
        /// <remarks>If the type of the new value differs from the type currently
        /// stored and the force parameter is false (default), returns error and
        /// does not update value. If force is true, the value type in the
        /// table is changed</remarks>
        /// <param name="name">The entry name</param>
        /// <param name="value">The value to set</param>
        /// <param name="force">True to force an update even if types are different</param>
        /// <returns>True on success, otherwise false</returns>
        public static bool SetEntryDoubleArray(string name, IList<double> value, bool force = false)
        {
#if CORE
            return CoreMethods.SetEntryDoubleArray(name, value, force);
#else
            if (force)
            {
                Storage.Instance.SetEntryTypeValue(name, Value.MakeDoubleArray(value));
                return true;
            }
            return Storage.Instance.SetEntryValue(name, Value.MakeDoubleArray(value));
#endif
        }

        /// <summary>
        /// Sets an entry value
        /// </summary>
        /// <remarks>If the type of the new value differs from the type currently
        /// stored and the force parameter is false (default), returns error and
        /// does not update value. If force is true, the value type in the
        /// table is changed</remarks>
        /// <param name="name">The entry name</param>
        /// <param name="value">The value to set</param>
        /// <param name="force">True to force an update even if types are different</param>
        /// <returns>True on success, otherwise false</returns>
        public static bool SetEntryStringArray(string name, IList<string> value, bool force = false)
        {
#if CORE
            return CoreMethods.SetEntryStringArray(name, value, force);
#else
            if (force)
            {
                Storage.Instance.SetEntryTypeValue(name, Value.MakeStringArray(value));
                return true;
            }
            return Storage.Instance.SetEntryValue(name, Value.MakeStringArray(value));
#endif
        }

        /// <summary>
        /// Sets an entry value
        /// </summary>
        /// <remarks>If the type of the new value differs from the type currently
        /// stored and the force parameter is false (default), returns error and
        /// does not update value. If force is true, the value type in the
        /// table is changed</remarks>
        /// <param name="name">The entry name</param>
        /// <param name="value">The value to set</param>
        /// <param name="force">True to force an update even if types are different</param>
        /// <returns>True on success, otherwise false</returns>
        public static bool SetEntryRaw(string name, IList<byte> value, bool force = false)
        {
#if CORE
            return CoreMethods.SetEntryRaw(name, value, force);
#else
            if (force)
            {
                Storage.Instance.SetEntryTypeValue(name, Value.MakeRaw(value));
                return true;
            }
            return Storage.Instance.SetEntryValue(name, Value.MakeRaw(value));
#endif
        }



        #region ThrowingGetters

#if !CORE
        internal static Exception GetValueException(string name, Value v, NtType requestedType)
        {
            if (v == null || v.Type == NtType.Unassigned)
            {
                return new TableKeyNotDefinedException(name);
            }
            else
            {
                return new TableKeyDifferentTypeException(name, requestedType, v.Type);
            }
        }
#endif

        /// <summary>
        /// Gets an entry value
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <returns>The value in the table</returns>
        /// <exception cref="TableKeyNotDefinedException">Thrown if the key does
        /// not exist in the table</exception>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if the type
        /// requested does not match the type in the table</exception>
        public static bool GetEntryBoolean(string name)
        {
#if CORE
            return CoreMethods.GetEntryBoolean(name);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsBoolean()) throw GetValueException(name, v, NtType.Boolean);
            return v.GetBoolean();
#endif
        }

        /// <summary>
        /// Gets an entry value
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <returns>The value in the table</returns>
        /// <exception cref="TableKeyNotDefinedException">Thrown if the key does
        /// not exist in the table</exception>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if the type
        /// requested does not match the type in the table</exception>
        public static double GetEntryDouble(string name)
        {
#if CORE
            return CoreMethods.GetEntryDouble(name);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsDouble()) throw GetValueException(name, v, NtType.Double);
            return v.GetDouble();
#endif
        }

        /// <summary>
        /// Gets an entry value
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <returns>The value in the table</returns>
        /// <exception cref="TableKeyNotDefinedException">Thrown if the key does
        /// not exist in the table</exception>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if the type
        /// requested does not match the type in the table</exception>
        public static string GetEntryString(string name)
        {
#if CORE
            return CoreMethods.GetEntryString(name);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsString()) throw GetValueException(name, v, NtType.String);
            return v.GetString();
#endif
        }

        /// <summary>
        /// Gets an entry value
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <returns>The value in the table</returns>
        /// <exception cref="TableKeyNotDefinedException">Thrown if the key does
        /// not exist in the table</exception>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if the type
        /// requested does not match the type in the table</exception>
        public static bool[] GetEntryBooleanArray(string name)
        {
#if CORE
            return CoreMethods.GetEntryBooleanArray(name);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsBooleanArray()) throw GetValueException(name, v, NtType.BooleanArray);
            return v.GetBooleanArray();
#endif
        }

        /// <summary>
        /// Gets an entry value
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <returns>The value in the table</returns>
        /// <exception cref="TableKeyNotDefinedException">Thrown if the key does
        /// not exist in the table</exception>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if the type
        /// requested does not match the type in the table</exception>
        public static double[] GetEntryDoubleArray(string name)
        {
#if CORE
            return CoreMethods.GetEntryDoubleArray(name);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsDoubleArray()) throw GetValueException(name, v, NtType.DoubleArray);
            return v.GetDoubleArray();
#endif
        }

        /// <summary>
        /// Gets an entry value
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <returns>The value in the table</returns>
        /// <exception cref="TableKeyNotDefinedException">Thrown if the key does
        /// not exist in the table</exception>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if the type
        /// requested does not match the type in the table</exception>
        public static string[] GetEntryStringArray(string name)
        {
#if CORE
            return CoreMethods.GetEntryStringArray(name);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsStringArray()) throw GetValueException(name, v, NtType.StringArray);
            return v.GetStringArray();
#endif
        }

        /// <summary>
        /// Gets an entry value
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <returns>The value in the table</returns>
        /// <exception cref="TableKeyNotDefinedException">Thrown if the key does
        /// not exist in the table</exception>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if the type
        /// requested does not match the type in the table</exception>
        public static byte[] GetEntryRaw(string name)
        {
#if CORE
            return CoreMethods.GetEntryRaw(name);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsRaw()) throw GetValueException(name, v, NtType.Raw);
            return v.GetRaw();
#endif
        }

        #endregion

        #region DefaultGetters

        /// <summary>
        /// Gets an entry value
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <param name="defaultValue">The default value if the key does not exist or is the wrong typel</param>
        /// <returns>The value requested, or defaultValue if the key does not exist or is the wrong type</returns>
        public static bool GetEntryBoolean(string name, bool defaultValue)
        {
#if CORE
            return CoreMethods.GetEntryBoolean(name, defaultValue);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsBoolean()) return defaultValue;
            return v.GetBoolean();
#endif
        }

        /// <summary>
        /// Gets an entry value
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <param name="defaultValue">The default value if the key does not exist or is the wrong typel</param>
        /// <returns>The value requested, or defaultValue if the key does not exist or is the wrong type</returns>
        public static double GetEntryDouble(string name, double defaultValue)
        {
#if CORE
            return CoreMethods.GetEntryDouble(name, defaultValue);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsDouble()) return defaultValue;
            return v.GetDouble();
#endif
        }

        /// <summary>
        /// Gets an entry value
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <param name="defaultValue">The default value if the key does not exist or is the wrong typel</param>
        /// <returns>The value requested, or defaultValue if the key does not exist or is the wrong type</returns>
        public static string GetEntryString(string name, string defaultValue)
        {
#if CORE
            return CoreMethods.GetEntryString(name, defaultValue);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsString()) return defaultValue;
            return v.GetString();
#endif
        }

        /// <summary>
        /// Gets an entry value
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <param name="defaultValue">The default value if the key does not exist or is the wrong typel</param>
        /// <returns>The value requested, or defaultValue if the key does not exist or is the wrong type</returns>
        public static bool[] GetEntryBooleanArray(string name, bool[] defaultValue)
        {
#if CORE
            return CoreMethods.GetEntryBooleanArray(name, defaultValue);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsBooleanArray())
            {
                return defaultValue;
            }
            return v.GetBooleanArray();
#endif
        }

        /// <summary>
        /// Gets an entry value
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <param name="defaultValue">The default value if the key does not exist or is the wrong typel</param>
        /// <returns>The value requested, or defaultValue if the key does not exist or is the wrong type</returns>
        public static double[] GetEntryDoubleArray(string name, double[] defaultValue)
        {
#if CORE
            return CoreMethods.GetEntryDoubleArray(name, defaultValue);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsDoubleArray())
            {
                return defaultValue;
            }
            return v.GetDoubleArray();
#endif
        }

        /// <summary>
        /// Gets an entry value
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <param name="defaultValue">The default value if the key does not exist or is the wrong typel</param>
        /// <returns>The value requested, or defaultValue if the key does not exist or is the wrong type</returns>
        public static string[] GetEntryStringArray(string name, string[] defaultValue)
        {
#if CORE
            return CoreMethods.GetEntryStringArray(name, defaultValue);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsStringArray())
            {
                return defaultValue;
            }
            return v.GetStringArray();
#endif
        }

        /// <summary>
        /// Gets an entry value
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <param name="defaultValue">The default value if the key does not exist or is the wrong typel</param>
        /// <returns>The value requested, or defaultValue if the key does not exist or is the wrong type</returns>
        public static byte[] GetEntryRaw(string name, byte[] defaultValue)
        {
#if CORE
            return CoreMethods.GetEntryRaw(name, defaultValue);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsRaw())
            {
                return defaultValue;
            }
            return v.GetRaw();
#endif
        }
#endregion

        /// <summary>
        /// Returns a copy of the current entry value
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <returns>The value in the table</returns>
        /// <remarks>Note if the value does not exist in the table, returns null.
        /// In addition, <see cref="NtType.Unassigned"/> is a possible type</remarks>
        public static Value GetEntryValue(string name)
        {
#if CORE
            return CoreMethods.GetEntryValue(name);
#else
            return Storage.Instance.GetEntryValue(name);
#endif
        }

        /// <summary>
        /// Sets a new entry value
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <param name="value">The value to set</param>
        /// <returns>True on successful set, false on error</returns>
        /// <remarks>If type of new value differs from the type of the
        /// currently stored entry, returns false and does not update value</remarks>
        public static bool SetEntryValue(string name, Value value)
        {
#if CORE
            return CoreMethods.SetEntryValue(name, value);
#else
            return Storage.Instance.SetEntryValue(name, value);
#endif
        }

        /// <summary>
        /// Sets an entry value in the table if it does not exist.
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <param name="value">The value to set</param>
        /// <returns>False if the type does not match existing value</returns>
        public static bool SetDefaultEntryValue(string name, Value value)
        {
#if CORE
            return CoreMethods.SetDefaultEntryValue(name, value);
#else
            return Storage.Instance.SetDefaultEntryValue(name, value);
#endif
        }

        /// <summary>
        /// Sets a new entry value, forcing an update on a type mismatch
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <param name="value">The value to set</param>
        /// <remarks>If type of new value differs from the type of the
        /// currently stored entry, the value will be force updated</remarks>
        public static void SetEntryTypeValue(string name, Value value)
        {
#if CORE
            CoreMethods.SetEntryValue(name, value, true);
#else
            Storage.Instance.SetEntryTypeValue(name, value);
#endif
        }

        /// <summary>
        /// Sets flags associated with an entry
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <param name="flags">The bitmask of the flags to set</param>
        public static void SetEntryFlags(string name, EntryFlags flags)
        {
#if CORE
            CoreMethods.SetEntryFlags(name, flags);
#else
            Storage.Instance.SetEntryFlags(name, flags);
#endif
        }

        /// <summary>
        /// Gets flags associated with an entry
        /// </summary>
        /// <param name="name">The entry names</param>
        /// <returns>Bitmask of the flags contained in the entry</returns>
        public static EntryFlags GetEntryFlags(string name)
        {
#if CORE
            return CoreMethods.GetEntryFlags(name);
#else
            return Storage.Instance.GetEntryFlags(name);
#endif
        }

        /// <summary>
        /// Deletes an entry from the table
        /// </summary>
        /// <param name="name">The entry name</param>
        public static void DeleteEntry(string name)
        {
#if CORE
            CoreMethods.DeleteEntry(name);
#else
            Storage.Instance.DeleteEntry(name);
#endif
        }

        /// <summary>
        ///Deletes all non-persistent entries from the table
        /// </summary>
        public static void DeleteAllEntries()
        {
#if CORE
            CoreMethods.DeleteAllEntries();
#else
            Storage.Instance.DeleteAllEntries();
#endif
        }

        /// <summary>
        /// Gets an array of entry information
        /// </summary>
        /// <remarks>
        /// The results are optionally filtered by a string prefix and entry type to only
        /// return a subset of all entries
        /// </remarks>
        /// <param name="prefix">A required entry prefix. Only entries with this prefix will be returned</param>
        /// <param name="types">Bitmask of <see cref="NtType"/> values, 0 is "don't care"</param>
        /// <returns>Array of entry information</returns>
        public static List<EntryInfo> GetEntryInfo(string prefix, NtType types)
        {
#if CORE
            return CoreMethods.GetEntryInfo(prefix, types);
#else
            return Storage.Instance.GetEntryInfo(prefix, types);
#endif
        }

        /// <summary>
        /// Gets the type of a specified entry
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <returns>The type of the entry. <see cref="NtType.Unassigned"/> if the entry does not exist</returns>
        public static NtType GetType(string name)
        {
#if CORE
            return CoreMethods.GetType(name);
#else
            var v = GetEntryValue(name);
            if (v == null) return NtType.Unassigned;
            return v.Type;
#endif
        }

        /// <summary>
        /// Gets if the table contains a specific entry
        /// </summary>
        /// <param name="name">The entry name</param>
        /// <returns>True if the entry exists, otherwise false</returns>
        public static bool ContainsEntry(string name)
        {
#if CORE
            return CoreMethods.ContainsEntry(name);
#else
            return GetType(name) != NtType.Unassigned;
#endif
        }

        /// <summary>
        /// Forces an immediate update of all entry changes to the network
        /// </summary>
        /// <remarks>This is done on a regularly scheduled interval set by
        /// <see cref="SetUpdateRate"/>. Fluses are rate limited to avoid
        /// excessive network trafic</remarks>
        public static void Flush()
        {
#if CORE
            CoreMethods.Flush();
#else
            Dispatcher.Instance.Flush();
#endif
        }

        /// <summary>
        /// Adds a listener for a specified prefix in the table
        /// </summary>
        /// <param name="prefix">The prefix to listen for in the table</param>
        /// <param name="callback">The callback to call when any entry with the specified prefix is updated</param>
        /// <param name="flags">The flags to use for notifying</param>
        /// <returns>The id of the entry listener</returns>
        public static int AddEntryListener(string prefix, EntryListenerCallback callback, NotifyFlags flags)
        {
#if CORE
            return CoreMethods.AddEntryListener(prefix, callback, flags);
#else
            Notifier notifier = Notifier.Instance;
            int uid = notifier.AddEntryListener(prefix, callback, flags);
            notifier.Start();
            if ((flags & NotifyFlags.NotifyImmediate) != 0)
                Storage.Instance.NotifyEntries(prefix, callback);
            return uid;
#endif
        }

        /// <summary>
        /// Removed an entry listener from the table
        /// </summary>
        /// <param name="uid">The entry listener id</param>
        public static void RemoveEntryListener(int uid)
        {
#if CORE
            CoreMethods.RemoveEntryListener(uid);
#else
            Notifier.Instance.RemoveEntryListener(uid);
#endif
        }

        /// <summary>
        /// Adds a connection listener to the table
        /// </summary>
        /// <param name="callback">The callback to call when a new remote connects or disconnects</param>
        /// <param name="immediateNotify">True to notify immediately with all connected remotes</param>
        /// <returns>The id of the connection listener</returns>
        public static int AddConnectionListener(ConnectionListenerCallback callback, bool immediateNotify)
        {
#if CORE
            return CoreMethods.AddConnectionListener(callback, immediateNotify);
#else
            Notifier notifier = Notifier.Instance;
            int uid = notifier.AddConnectionListener(callback);
            notifier.Start();
            if (immediateNotify) Dispatcher.Instance.NotifyConnections(callback);
            return uid;
#endif
        }

        /// <summary>
        /// Removes a connection listener from the table
        /// </summary>
        /// <param name="uid">The connection listener id</param>
        public static void RemoveConnectionListener(int uid)
        {
#if CORE
            CoreMethods.RemoveConnectionListener(uid);
#else
            Notifier.Instance.RemoveConnectionListener(uid);
#endif
        }

        /// <summary>
        /// Gets if the notifier thread for callbacks has been destroyed
        /// </summary>
        /// <returns>True if the notifier has been destroyed</returns>
        public static bool NotifierDestroyed()
        {
#if CORE
            return CoreMethods.NotifierDestroyed();
#else
            return Notifier.Instance.Destroyed();
#endif
        }

        /// <summary>
        /// Sets the remote identity for this table
        /// </summary>
        /// <remarks>This is the id that will be seen by
        /// the remotes</remarks>
        /// <param name="name">The id</param>
        public static void SetNetworkIdentity(string name)
        {
#if CORE
            CoreMethods.SetNetworkIdentity(name);
#else
            Dispatcher.Instance.Identity = name;
#endif
        }

        /// <summary>
        /// Starts this table in server mode
        /// </summary>
        /// <param name="persistFilename">The filename to use for the persistent file</param>
        /// <param name="listenAddress">The specific ip address to listen on</param>
        /// <param name="port">The port to listen on</param>
        public static void StartServer(string persistFilename, string listenAddress, int port)
        {
#if CORE
            CoreMethods.StartServer(persistFilename, listenAddress, port);
#else
            Dispatcher.Instance.StartServer(persistFilename, listenAddress, port);
#endif
        }

        /// <summary>
        /// Stops the server if it is running
        /// </summary>
        public static void StopServer()
        {
#if CORE
            CoreMethods.StopServer();
#else
            Dispatcher.Instance.Stop();
#endif
        }

        /// <summary>
        /// Starts this table in client mode
        /// </summary>
        /// <param name="serverName">The server name</param>
        /// <param name="port">The server port</param>
        public static void StartClient(string serverName, int port)
        {
#if CORE
            CoreMethods.StartClient(serverName, (uint)port);
#else
            Dispatcher.Instance.StartClient(serverName, port);
#endif
        }

        /// <summary>
        /// Starts this table in client mode attempting to connect to multiple servers
        /// in round robin fashion
        /// </summary>
        /// <param name="servers">The servers to try and connect to</param>
        public static void StartClient(IList<NtIPAddress> servers)
        {
#if CORE
            CoreMethods.StartClient(servers);
#else
            Dispatcher.Instance.StartClient(servers);
#endif
        }

        /// <summary>
        /// Stops the client if it is running
        /// </summary>
        public static void StopClient()
        {
#if CORE
            CoreMethods.StopClient();
#else
            Dispatcher.Instance.Stop();
#endif
        }

        /// <summary>
        /// Stops the Rpc server if it is running
        /// </summary>
        public static void StopRpcServer()
        {
#if CORE
            CoreMethods.StopRpcServer();
#else
            RpcServer.Instance.Stop();
#endif
        }

        /// <summary>
        /// Stops the notifier listener thread if it is running
        /// </summary>
        public static void StopNotifier()
        {
#if CORE
            CoreMethods.StopNotifier();
#else
            Notifier.Instance.Stop();
#endif
        }

        /// <summary>
        /// Sets the update rate for the table (seconds)
        /// </summary>
        /// <param name="interval">The interval to update the table in (seconds)</param>
        public static void SetUpdateRate(double interval)
        {
#if CORE
            CoreMethods.SetUpdateRate(interval);
#else
            Dispatcher.Instance.UpdateRate = interval;
#endif
        }

        /// <summary>
        /// Gets an array of all the connections in the table.
        /// </summary>
        /// <returns>The table's remote connections</returns>
        public static List<ConnectionInfo> GetConnections()
        {
#if CORE
            return CoreMethods.GetConnections();
#else
            return Dispatcher.Instance.GetConnections();
#endif
        }

        /// <summary>
        /// Saves all persistent variables to the files specified
        /// </summary>
        /// <param name="filename">The file to save to</param>
        /// <returns>Error string, or null on success</returns>
        public static string SavePersistent(string filename)
        {
#if CORE
            return CoreMethods.SavePersistent(filename);
#else
            return Storage.Instance.SavePersistent(filename, false);
#endif
        }

        /// <summary>
        /// Saves all persistent variables to the files specified asynchronously
        /// </summary>
        /// <param name="filename">The file to save to</param>
        /// <returns>Error string, or null on success</returns>
        public static async Task<string> SavePersistentAsync(string filename)
        {
#if CORE
            return await Task.Run(() => CoreMethods.SavePersistent(filename)).ConfigureAwait(false);
#else
            return await Storage.Instance.SavePersistentAsync(filename, false).ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// Loads persistent variables from a specified file
        /// </summary>
        /// <param name="filename">The file to load from</param>
        /// <param name="warn">Function called whenever an error is seen during loading. Int is line number, string is message.</param>
        /// <returns>Error string, or null on success</returns>
        public static string LoadPersistent(string filename, Action<int, string> warn)
        {
#if CORE
            return CoreMethods.LoadPersistent(filename, warn);
#else
            return Storage.Instance.LoadPersistent(filename, warn);
#endif
        }

        /// <summary>
        /// Loads persistent variables from a specified file asynchronously
        /// </summary>
        /// <param name="filename">The file to load from</param>
        /// <param name="warn">Function called whenever an error is seen during loading. Int is line number, string is message.</param>
        /// <returns>Error string, or null on success</returns>
        public static async Task<string> LoadPersistentAsync(string filename, Action<int, string> warn)
        {
#if CORE
            return await Task.Run(() => CoreMethods.LoadPersistent(filename, warn)).ConfigureAwait(false);
#else
            return await Storage.Instance.LoadPersistentAsync(filename, warn).ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// Gets the current timestamp of the library to be used for value time comparisons 
        /// </summary>
        /// <returns>The current time in milliseconds</returns>
        public static long Now()
        {
            return Support.Timestamp.Now();
        }

        /// <summary>
        /// Sets the logger to be used when an error is encountered by NetworkTables
        /// </summary>
        /// <param name="func">The function to use to log</param>
        /// <param name="minLevel">The minimum level required to log</param>
        public static void SetLogger(LogFunc func, LogLevel minLevel)
        {
#if CORE
            CoreMethods.SetLogger(func, minLevel);
#else
            Logger logger = Logger.Instance;
            logger.SetLogger(func);
            logger.MinLevel = minLevel;
#endif
        }

        /// <summary>
        /// Loads persistent variables from a specified file
        /// </summary>
        /// <param name="filename">The file to load from</param>
        /// <returns>An array of all errors reported during loading</returns>
        public static IList<string> LoadPersistent(string filename)
        {
            List<string> warns = new List<string>();
            var err = LoadPersistent(filename, (i, s) =>
            {
                warns.Add($"{i}: {s}");
            });
            if (err != null) throw new PersistentException($"Load Persistent Failed: {err}");
            return warns.ToArray();
        }

        /// <summary>
        /// Loads persistent variables from a specified file asynchronously
        /// </summary>
        /// <param name="filename">The file to load from</param>
        /// <returns>An array of all errors reported during loading</returns>
        public static async Task<IList<string>> LoadPersistentAsync(string filename)
        {
            List<string> warns = new List<string>();
            var err = await LoadPersistentAsync(filename, (i, s) =>
            {
                warns.Add($"{i}: {s}");
            }).ConfigureAwait(false);
            if (err != null) throw new PersistentException($"Load Persistent Failed: {err}");
            return warns.ToArray();
        }
    }
}
