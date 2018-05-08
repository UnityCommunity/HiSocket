# HiSocket

![Packagist](https://img.shields.io/packagist/l/doctrine/orm.svg)   [![Build Status](https://travis-ci.org/hiramtan/HiSocket.svg?branch=master)](https://travis-ci.org/hiramtan/HiSocket)   [![GitHub release](https://img.shields.io/github/release/hiramtan/HiSocket.svg)](https://github.com/hiramtan/HiSocket/releases)

-----

### 如何使用
- 如果用在c#项目中,可以从此下载 HiSocket.dll: [HiSocket_xx.zip](https://github.com/hiramtan/HiSocket/releases)
- 如果用在unity3d项目中,可以从此下载 HiSocket.unitypackage: [HiSocket_xx.unitypackage](https://github.com/hiramtan/HiSocket/releases)

  (ps. HiSocket.unitypackage 包含HiSocket.dll和一些示例)

 快速开始:
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

### 总览
项目包含:
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


### 功能
- Tcp socket
- Udp socket
- 可伸缩字节表
- 高性能字节块缓冲区
- 消息注册和回调
- 二进制字节消息封装
- Protobuf消息封装
- AES消息加密

### 详情
- Tcp和Udp都是采用主线程异步连接的方式(避免主线程阻塞).
- 启动发送线程和接收线程处理数据传输(提高性能).
- 高性能字节缓冲区避免内存空间重复申请,减少GC.
- 可以添加一系列的事件监听获取当前的连接状态.
- 如果使用Tcp协议需要实现IPackage接口处理粘包拆包.
- 如果使用Udp协议需要声明缓冲区大小.
- Ping: 源码包含一个Ping插件可以使用,但是如果用在unity3d工程中会报错(因为mono的问题,在.net2.0会报错.net4.6可以正常使用)


### 介绍
- Tcp 
[Transmission Control Protocol](https://en.wikipedia.org/wiki/Transmission_Control_Protocol)

Tcp 协议提供可靠有序的流字节传输,用户需要自己分割数据,在这个框架中可以继承IPackage接口来实现.

Tcp协议传输字节流,用户需要分割字节流获得正确的数据包,当创建一个tcp协议的socket时,需要传入一个Package对象来封包和解包.

最初创建连接时我们定义了一个packer来分割数据包,当发送消息时我们在数据头部插入消息长度/当接收到消息时我们根据头部的消息长度获得数据包的大小.
        
- Udp
[User Datagram Protocol](https://www.assetstore.unity3d.com/en/#!/content/104658) 

Udp协议提供不可靠的报文消息,用户无法知道当前连接状态,但是消息包时完整的.

如果创建upd连接,需要指定发送接收缓冲区大小.

- Ping :
    因为mono在.net2.0和2.0 subset的bug,可以在unity3d使用如下逻辑获取ping值.
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
- 消息注册
- Protobuf
- 字节消息
- 加密

### 高级功能
- 如果对Socket很熟悉,也可以使用TcpSocket(UdpSocket)来实现功能,但是还是推荐使用TcpConnection(UdpConnection)的方式.
- 可以向TcpConnection(UdpConnection)添加不同的插件完成所需的功能,
- 注册基类可以方便快速注册消息(基于反射)
- Byte block buffer 采用有序链表实现,当有区块空闲时会重用区块.
- .etc
---------

### Example
在**HiSocketExample** 和 **HiSocket.unitypackage**有很多示例, 其中有一些如下:

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



