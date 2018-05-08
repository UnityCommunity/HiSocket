# HiSocket

![Packagist](https://img.shields.io/packagist/l/doctrine/orm.svg)   [![Build Status](https://travis-ci.org/hiramtan/HiSocket.svg?branch=master)](https://travis-ci.org/hiramtan/HiSocket)   [![GitHub release](https://img.shields.io/github/release/hiramtan/HiSocket.svg)](https://github.com/hiramtan/HiSocket/releases)

-----
[中文说明](https://github.com/hiramtan/HiSocket/blob/master/README_zh.md) 

### How to use
- If you want to used in c# project, you can download HiSocket.dll from here: [HiSocket_xx.zip](https://github.com/hiramtan/HiSocket/releases)
- If you want to used in unity3d, you can download HiSocket.unitypackage from here: [HiSocket_xx.unitypackage](https://github.com/hiramtan/HiSocket/releases)

  (ps. HiSocket.unitypackage contains HiSocket.dll and some example)

 Quick Start:
```csharp
        private IPackage _package = new PackageExample();
        private TcpConnection _tcp;
        void Init()
        {
            _tcp = new TcpConnection(_package);
            _tcp.OnConnected += OnConnected;
            _tcp.OnReceive += OnReceive;
            //_tcp.OnError
            //_tcp.OnDisconnected
        }
        void OnConnected()
        {
            //connect success
            _tcp.Send(new byte[10]);//send message
            _tcp.DisConnect();//disconnect
        }

        void OnReceive(byte[] bytes)
        {
            //get message from server
        }
```

-----

### General
This project contains:
- Connection
    - TcpConnection
        - TcpSocket
        - Package
    - UdpConnection
        - UdpSocket
    - Plugin
- Message
    - Message register
    - Aes encryption
    - Byte message
    - Protobuf message


### Features
- Support Tcp socket
- Support Udp socket
- Scalable byte Array
- High-performance byte block buffer
- Message registration and call back
- Support byte message
- Support protobuf message
- AES encryption


### Details
- Tcp and Udp are all use async connection in main thread(avoid thread blocking).
- There are send thread and receive thread in background to process bytes(use high-performance block).
- High-performance buffer avoid memory allocation every time, and reduce garbage collection.
- You can get current connect state and message by adding listener of event.
- If you use Tcp socket, you should implement IPackage interface to pack or unpack message.
- If you use Udp socket, you should declaring buffer size.
- Ping: there is a ping plugin you can used, but if you are used in unity3d because of the bug of mono, it will throw an error on .net2.0(.net 4.6 will be fine, also you can use unity's api to get ping time)


### Instructions
- Tcp 
[Transmission Control Protocol](https://en.wikipedia.org/wiki/Transmission_Control_Protocol)

Tcp provides reliable, ordered, and error-checked delivery of a stream of bytes. you have to split bytes by yourself, in this framework you can implement IPackage interface to achieve this.

Because Tcp is a a stream of bytes protocol, user should split the bytes to get correct message package. when create a tcp socket channel there must be a package instance to pack and unpack message.

Pack and Unpack message: In the beginning we define a packager to split bytes, when send message we add length in the head of every message and when receive message we use this length to get how long our message is.
        
- Udp
[User Datagram Protocol](https://www.assetstore.unity3d.com/en/#!/content/104658) 

Udp provides checksums for data integrity, and port numbers for addressing different functions at the source and destination of the datagram. that means you don't know current connect state, but package is integrated.

If use Udp connection shold define send and receive's buffer size.

- Ping :
    Because there is a bug with mono on .net 2.0 and subset in unity3d, you can use logic as below.
    ```csharp
    public int PingTime;
    private Ping p;
    private float timeOut = 1;
    private float lastTime;
    void Start()
    {
        StartCoroutine(Ping());
    }
    IEnumerator Ping()
    {
        p = new Ping("127.0.0.1");
        lastTime = Time.realtimeSinceStartup;
        while (!p.isDone && Time.realtimeSinceStartup - lastTime < 1)
        {
            yield return null;
        }
        PingTime = p.time;
        p.DestroyPing();
        yield return new WaitForSeconds(1);
        StartCoroutine(Ping());
    }
    ```
- Message Register
- Protobuf
- Bytes message
- Encription

### Advanced
- If you are clear about socket, you also can use TcpSocket(UdpSocket) to achieve your logic, anyway the recommend is TcpConnection(UdpConnection).
- You can add many different plugins based on TcpConnection(UdpConnection) to achieve different functions.
- There are a message register base class help user to quick register id and callback(based on reflection)
- Byte block buffer use linked list and reuse block when some block is free.
- .etc
---------

### Example
There are many example in **HiSocketExample** project or in **HiSocket.unitypackage**, here is some of them:

Package example:
```csharp
/// <summary>
    /// Example: Used to pack or unpack message
    /// You should inheritance IPackage interface and implement your own logic
    /// </summary>
    class PackageExample : IPackage
    {  /// <summary>
       /// Pack your message here(this is only an example)
       /// </summary>
       /// <param name="source"></param>
       /// <param name="unpackedHandler"></param>
        public void Unpack(IByteArray source, Action<byte[]> unpackedHandler)
        {
            // Unpack your message(use int, 4 byte as head)
            while (source.Length >= 4)
            {
                var head = source.Read(4);
                int bodyLength = BitConverter.ToInt32(head, 0);// get body's length
                if (source.Length >= bodyLength)
                {
                    var unpacked = source.Read(bodyLength);// get body
                    unpackedHandler(unpacked);
                }
                else
                {
                    source.Insert(0, head);// rewrite in, used for next time
                }
            }
        }

        /// <summary>
        /// Unpack your message here(this is only an example)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="packedHandler"></param>
        public void Pack(IByteArray source, Action<byte[]> packedHandler)
        {
            // Add head length to your message(use int, 4 byte as head)
            var length = source.Length;
            var head = BitConverter.GetBytes(length);
            source.Insert(0, head);// add head bytes
            var packed = source.Read(source.Length);
            packedHandler(packed);
        }
    }
```

```csharp
private IPackage _package = new PackageExample();
        private TcpConnection _tcp;
        static void Main(string[] args)
        {

        }
        void Init()
        {
            _tcp = new TcpConnection(_package);
            _tcp.OnConnected += OnConnected;
            _tcp.OnReceive += Receive;
            //_tcp.OnError
            //_tcp.OnDisconnected
        }
        void OnConnected()
        {
            //connect success
            _tcp.Send(new byte[10]);//send message
            _tcp.DisConnect();//disconnect
        }

        void Receive(byte[] bytes)
        {
            //get message from server
        }
```

```csharp
 void Init()
        {
            var tcp = new TcpConnection(new PackageExample());
            tcp.AddPlugin(new PingPlugin("ping", tcp));
            //tcp.GetPlugin("ping");
        }
```


support: hiramtan@live.com

-------------
MIT License

Copyright (c) [2017] [Hiram]

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.



