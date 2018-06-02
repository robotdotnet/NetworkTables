using System;
using System.Collections.Generic;
using System.Text;

namespace FRC.NetworkTables.Interop
{
    public readonly struct Handle
    {
        private readonly int m_value;

        public Handle(int value)
        {
            m_value = value;
        }

        public int Get()
        {
            return m_value;
        }
    }

    public readonly struct Instance
    {
        private readonly Handle m_value;

        public Instance(int value)
        {
            m_value = new Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }

        public static implicit operator Handle(Instance value)
        {
            return value.m_value;
        }
    }

    public readonly struct Entry
    {
        private readonly Handle m_value;

        public Entry(int value)
        {
            m_value = new Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }

        public static implicit operator Handle(Entry value)
        {
            return value.m_value;
        }
    }

    public readonly struct EntryListener
    {
        private readonly Handle m_value;

        public EntryListener(int value)
        {
            m_value = new Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }

        public static implicit operator Handle(EntryListener value)
        {
            return value.m_value;
        }
    }

    public readonly struct EntryListenerPoller
    {
        private readonly Handle m_value;

        public EntryListenerPoller(int value)
        {
            m_value = new Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator Handle(EntryListenerPoller value)
        {
            return value.m_value;
        }
    }

    public readonly struct ConnectionListener
    {
        private readonly Handle m_value;

        public ConnectionListener(int value)
        {
            m_value = new Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator Handle(ConnectionListener value)
        {
            return value.m_value;
        }
    }

    public readonly struct ConnectionListenerPoller
    {
        private readonly Handle m_value;

        public ConnectionListenerPoller(int value)
        {
            m_value = new Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator Handle(ConnectionListenerPoller value)
        {
            return value.m_value;
        }
    }

    public readonly struct Logger
    {
        private readonly Handle m_value;

        public Logger(int value)
        {
            m_value = new Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator Handle(Logger value)
        {
            return value.m_value;
        }
    }

    public readonly struct LoggerPoller
    {
        private readonly Handle m_value;

        public LoggerPoller(int value)
        {
            m_value = new Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator Handle(LoggerPoller value)
        {
            return value.m_value;
        }
    }

    public readonly struct RpcCall
    {
        private readonly Handle m_value;

        public RpcCall(int value)
        {
            m_value = new Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator Handle(RpcCall value)
        {
            return value.m_value;
        }
    }

    public readonly struct RpcCallPoller
    {
        private readonly Handle m_value;

        public RpcCallPoller(int value)
        {
            m_value = new Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator Handle(RpcCallPoller value)
        {
            return value.m_value;
        }
    }
}
