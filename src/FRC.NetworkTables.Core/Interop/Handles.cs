using System;
using System.Collections.Generic;
using System.Text;

namespace FRC.NetworkTables.Interop
{
    public readonly struct NT_Handle
    {
        private readonly int m_value;

        public NT_Handle(int value)
        {
            m_value = value;
        }

        public int Get()
        {
            return m_value;
        }
    }

    public readonly struct NT_Inst
    {
        private readonly NT_Handle m_value;

        public NT_Inst(int value)
        {
            m_value = new NT_Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }

        public static implicit operator NT_Handle(NT_Inst value)
        {
            return value.m_value;
        }
    }

    public readonly struct NT_Entry
    {
        private readonly NT_Handle m_value;

        public NT_Entry(int value)
        {
            m_value = new NT_Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }

        public static implicit operator NT_Handle(NT_Entry value)
        {
            return value.m_value;
        }
    }

    public readonly struct NT_EntryListener
    {
        private readonly NT_Handle m_value;

        public NT_EntryListener(int value)
        {
            m_value = new NT_Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }

        public static implicit operator NT_Handle(NT_EntryListener value)
        {
            return value.m_value;
        }
    }

    public readonly struct NT_EntryListenerPoller
    {
        private readonly NT_Handle m_value;

        public NT_EntryListenerPoller(int value)
        {
            m_value = new NT_Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator NT_Handle(NT_EntryListenerPoller value)
        {
            return value.m_value;
        }
    }

    public readonly struct NT_ConnectionListener
    {
        private readonly NT_Handle m_value;

        public NT_ConnectionListener(int value)
        {
            m_value = new NT_Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator NT_Handle(NT_ConnectionListener value)
        {
            return value.m_value;
        }
    }

    public readonly struct NT_ConnectionListenerPoller
    {
        private readonly NT_Handle m_value;

        public NT_ConnectionListenerPoller(int value)
        {
            m_value = new NT_Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator NT_Handle(NT_ConnectionListenerPoller value)
        {
            return value.m_value;
        }
    }

    public readonly struct NT_Logger
    {
        private readonly NT_Handle m_value;

        public NT_Logger(int value)
        {
            m_value = new NT_Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator NT_Handle(NT_Logger value)
        {
            return value.m_value;
        }
    }

    public readonly struct NT_LoggerPoller
    {
        private readonly NT_Handle m_value;

        public NT_LoggerPoller(int value)
        {
            m_value = new NT_Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator NT_Handle(NT_LoggerPoller value)
        {
            return value.m_value;
        }
    }

    public readonly struct NT_RpcCall
    {
        private readonly NT_Handle m_value;

        public NT_RpcCall(int value)
        {
            m_value = new NT_Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator NT_Handle(NT_RpcCall value)
        {
            return value.m_value;
        }
    }

    public readonly struct NT_RpcCallPoller
    {
        private readonly NT_Handle m_value;

        public NT_RpcCallPoller(int value)
        {
            m_value = new NT_Handle(value);
        }

        public int Get()
        {
            return m_value.Get();
        }


        public static implicit operator NT_Handle(NT_RpcCallPoller value)
        {
            return value.m_value;
        }
    }
}
