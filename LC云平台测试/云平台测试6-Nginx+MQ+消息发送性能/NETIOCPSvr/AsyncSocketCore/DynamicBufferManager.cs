﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncSocketServer
{
    /// <summary>
    /// ====用于把命令和数据同时写入到缓存中，调用一次发送或者接收，可以提高性能====
    /// 动态缓存是随着数据量大小动态增长，申请的内存在运行过程中重复利用，不释放.
    /// 这样对内存只进行读写，不进行申请和释放，整体性能较高，因为内存申请释放比读写的效率低很多.
    /// 因为申请释放内存需要进行加锁，进行系统内核和用户切换，因此使用动态缓存可以降低内核和用户态切换，提高性能。
    /// </summary>
    public class DynamicBufferManager
    {
        public byte[] Buffer { get; set; } //存放内存的数组
        public int DataCount { get; set; } //写入数据大小

        //构造函数
        public DynamicBufferManager(int bufferSize)
        {
            DataCount = 0;
            Buffer = new byte[bufferSize];
        }

        /// <summary>
        /// 获得当前写入的字节数
        /// </summary>
        /// <returns></returns>
        public int GetDataCount() 
        {
            return DataCount;
        }

        /// <summary>
        /// 获得剩余的字节数
        /// </summary>
        /// <returns></returns>
        public int GetReserveCount() 
        {
            return Buffer.Length - DataCount;
        }

        public void Clear()
        {
            DataCount = 0;
        }

        /// <summary>
        /// 清理指定大小的数据
        /// </summary>
        /// <param name="count"></param>
        public void Clear(int count) 
        {
            if (count >= DataCount) //如果需要清理的数据大于现有数据大小，则全部清理
            {
                DataCount = 0;
            }
            else
            {
                for (int i = 0; i < DataCount - count; i++) //否则后面的数据往前移
                {
                    Buffer[i] = Buffer[count + i];
                }
                DataCount = DataCount - count;
            }
        }

        /// <summary>
        /// 设置缓存大小
        /// </summary>
        /// <param name="size"></param>
        public void SetBufferSize(int size) 
        {
            if (Buffer.Length < size)
            {
                byte[] tmpBuffer = new byte[size];
                Array.Copy(Buffer, 0, tmpBuffer, 0, DataCount); //复制以前的数据
                Buffer = tmpBuffer; //替换
            }
        }

        /// <summary>
        /// 写入Buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void WriteBuffer(byte[] buffer, int offset, int count)
        {
            if (GetReserveCount() >= count) //缓冲区空间够，不需要申请
            {
                Array.Copy(buffer, offset, Buffer, DataCount, count);
                DataCount = DataCount + count;
            }
            else //缓冲区空间不够，需要申请更大的内存，并进行移位
            {
                int totalSize = Buffer.Length + count - GetReserveCount(); //总大小-空余大小
                byte[] tmpBuffer = new byte[totalSize];
                Array.Copy(Buffer, 0, tmpBuffer, 0, DataCount); //复制以前的数据
                Array.Copy(buffer, offset, tmpBuffer, DataCount, count); //复制新写入的数据
                DataCount = DataCount + count;
                Buffer = tmpBuffer; //替换
            }
        }

        /// <summary>
        /// 从0位置写入Buffer
        /// </summary>
        /// <param name="buffer"></param>
        public void WriteBuffer(byte[] buffer)
        {
            WriteBuffer(buffer, 0, buffer.Length);
        }

        public void WriteShort(short value, bool convert)
        {
            if (convert)
            {
                //NET是小头结构，网络字节是大头结构，需要客户端和服务器约定好
                value = System.Net.IPAddress.HostToNetworkOrder(value);
            }
            byte[] tmpBuffer = BitConverter.GetBytes(value);
            WriteBuffer(tmpBuffer);
        }

        public void WriteInt(int value, bool convert)
        {
            if (convert)
            {
                //NET是小头结构，网络字节是大头结构，需要客户端和服务器约定好
                value = System.Net.IPAddress.HostToNetworkOrder(value); 
            }            
            byte[] tmpBuffer = BitConverter.GetBytes(value);
            WriteBuffer(tmpBuffer);
        }

        public void WriteLong(long value, bool convert)
        {
            if (convert)
            {
                //NET是小头结构，网络字节是大头结构，需要客户端和服务器约定好
                value = System.Net.IPAddress.HostToNetworkOrder(value); 
            }
            byte[] tmpBuffer = BitConverter.GetBytes(value);
            WriteBuffer(tmpBuffer);
        }

        //文本全部转成UTF8，UTF8兼容性好
        public void WriteString(string value) 
        {
            byte[] tmpBuffer = Encoding.UTF8.GetBytes(value);
            WriteBuffer(tmpBuffer);
        }
    }
}
