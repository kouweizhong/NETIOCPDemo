﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace AsyncSocketServer
{
    public class AsyncSocketUserToken
    {
        #region 属性
        public SocketAsyncEventArgs ReceiveEventArgs { get; set; }
        public SocketAsyncEventArgs SendEventArgs { get; set; }
        public DynamicBufferManager ReceiveBufferManager { get; set; }//用于多包融合的情况，比如采集器采集需要10包合到一起再进行处理
        public AsyncSendBufferManager SendBufferManager { get; set; }
        public DateTime ConnectDateTime { get; set; }
        public DateTime ActiveDateTime { get; set; }

        protected byte[] m_asyncReceiveBuffer;
        protected byte[] m_asyncSendBuffer;

        protected Socket m_connectSocket;
        public Socket ConnectSocket
        {
            get
            {
                return m_connectSocket;
            }
            set
            {
                m_connectSocket = value;
                if (m_connectSocket == null) //清理缓存
                {
                    ReceiveBufferManager.Clear(ReceiveBufferManager.DataCount);
                    SendBufferManager.ClearPacket();
                }
                ReceiveEventArgs.AcceptSocket = m_connectSocket;
                SendEventArgs.AcceptSocket = m_connectSocket;
            }
        }
        #endregion 属性

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="asyncReceiveBufferSize"></param>
        public AsyncSocketUserToken(int asyncReceiveBufferSize)
        {
            m_connectSocket = null;
            
            ReceiveEventArgs = new SocketAsyncEventArgs();
            ReceiveEventArgs.UserToken = this;
            m_asyncReceiveBuffer = new byte[asyncReceiveBufferSize];
            ReceiveEventArgs.SetBuffer(m_asyncReceiveBuffer, 0, m_asyncReceiveBuffer.Length);//设置要用于异步套接字方法的数据缓冲区。
            
            SendEventArgs = new SocketAsyncEventArgs();
            SendEventArgs.UserToken = this;
            //SendEventArgs.SetBuffer(m_asyncSendBuffer, 0, m_asyncReceiveBuffer.Length);

            ReceiveBufferManager = new DynamicBufferManager(ProtocolConst.InitBufferSize);
            SendBufferManager = new AsyncSendBufferManager(ProtocolConst.InitBufferSize); ;
        }
    }
}
