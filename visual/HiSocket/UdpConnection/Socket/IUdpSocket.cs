﻿/***************************************************************
 * Description: 
 *
 * Documents: https://github.com/hiramtan/HiSocket_unity
 * Author: hiramtan@live.com
***************************************************************/

using System;
using System.Net;
using System.Net.Sockets;

namespace HiSocket
{
    /// <summary>
    /// socket api
    /// </summary>
    public interface IUdpSocket
    {
        /// <summary>
        /// Get socket and modify it(for example: set timeout)
        /// </summary>
        Socket Socket { get; }

        /// <summary>
        /// trigger when get message
        /// </summary>
        event Action<byte[]> OnSocketReceive;

        /// <summary>
        /// Connect to server
        /// </summary>
        /// <param name="iep">server</param>
        void Connect(IPEndPoint iep);

        /// <summary>
        /// Send bytes to server
        /// </summary>
        /// <param name="bytes"></param>
        void Send(byte[] bytes);

        /// <summary>
        /// Disconnect
        /// </summary>
        void DisConnect();
    }
}
