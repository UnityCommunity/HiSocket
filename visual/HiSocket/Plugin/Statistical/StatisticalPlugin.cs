﻿/***************************************************************
 * Description: 
 *
 * Documents: https://github.com/hiramtan/HiSocket
 * Author: hiramtan@live.com
***************************************************************/

namespace HiSocket
{
    /// <summary>
    /// For example
    /// </summary>
    public sealed class StatisticalPlugin : PluginBase
    {
        private int _howManyBytesSend;

        public StatisticalPlugin(string name, IConnection connection) : base(name, connection)
        {
            connection.OnSend += x => { _howManyBytesSend += x.Length; };
        }

        //class Test
        //{
        //    void Start()
        //    {
        //        var tcp = new TcpConnection(iPackage);
        //        var plugin = new StatisticalPlugin("Statistical", tcp);
        //    }
        //}
    }
}