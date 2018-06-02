using System;
using System.Collections.Generic;
using System.Text;

namespace FRC.NetworkTables.Interop
{
    public readonly struct NtHandle
    {
        private readonly int m_value;

        public NtHandle(int value)
        {
            m_value = value;
        }

        public int Get()
        {
            return m_value;
        }
    }

    public readonly struct NtInst
    {
        private readonly NtHandle m_value;

        public NtInst(int value)
        {
            m_value = new NtHandle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }

        public static implicit operator NtHandle(NtInst value)
        {
            return value.m_value;
        }
    }

    public readonly struct NtEntry
    {
        private readonly NtHandle m_value;

        public NtEntry(int value)
        {
            m_value = new NtHandle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }

        public static implicit operator NtHandle(NtEntry value)
        {
            return value.m_value;
        }
    }

    public readonly struct NtEntryListener
    {
        private readonly NtHandle m_value;

        public NtEntryListener(int value)
        {
            m_value = new NtHandle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }

        public static implicit operator NtHandle(NtEntryListener value)
        {
            return value.m_value;
        }
    }

    public readonly struct NtEntryListenerPoller
    {
        private readonly NtHandle m_value;

        public NtEntryListenerPoller(int value)
        {
            m_value = new NtHandle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator NtHandle(NtEntryListenerPoller value)
        {
            return value.m_value;
        }
    }

    public readonly struct NtConnectionListener
    {
        private readonly NtHandle m_value;

        public NtConnectionListener(int value)
        {
            m_value = new NtHandle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator NtHandle(NtConnectionListener value)
        {
            return value.m_value;
        }
    }

    public readonly struct NtConnectionListenerPoller
    {
        private readonly NtHandle m_value;

        public NtConnectionListenerPoller(int value)
        {
            m_value = new NtHandle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator NtHandle(NtConnectionListenerPoller value)
        {
            return value.m_value;
        }
    }

    public readonly struct NtLogger
    {
        private readonly NtHandle m_value;

        public NtLogger(int value)
        {
            m_value = new NtHandle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator NtHandle(NtLogger value)
        {
            return value.m_value;
        }
    }

    public readonly struct NtLoggerPoller
    {
        private readonly NtHandle m_value;

        public NtLoggerPoller(int value)
        {
            m_value = new NtHandle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator NtHandle(NtLoggerPoller value)
        {
            return value.m_value;
        }
    }

    public readonly struct NtRpcCall
    {
        private readonly NtHandle m_value;

        public NtRpcCall(int value)
        {
            m_value = new NtHandle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator NtHandle(NtRpcCall value)
        {
            return value.m_value;
        }
    }

    public readonly struct NtRpcCallPoller
    {
        private readonly NtHandle m_value;

        public NtRpcCallPoller(int value)
        {
            m_value = new NtHandle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator NtHandle(NtRpcCallPoller value)
        {
            return value.m_value;
        }
    }
}
