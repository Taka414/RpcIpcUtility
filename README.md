# RpcIpcUtility

プロセス間通信でクラス/メソッドベースでAPIをRCP（リモートプロシージャーコール）を実現するライブラリです。





### ライブラリを使用する利点

このライブラリを利用すると以下の機能的利点があります。

* プロセス間通信でクライアント - サーバー間の通信をC#のメソッド呼び出しと同等に呼び出すことができます。

* パイプを使用したプロセス間通信のPublishで戻り値を受け取ることができます。
  * MessagePipeのIDistributablePublisherでは戻り値を受け取ることができません。
  * IRemoteRequestHandlerは通信が失敗するとハングアップする不具合を回避できます。
* サーバー側で発生したエラー情報をクライアントで限定的に受け取ることができます。
* MessagePipeが利用するDIシステムと分離しているため単独のクラスとして使用可能です。



## 依存ライブラリ

[MessagePipe-ForWindowsOnly](https://github.com/Taka414/MessagePipe-ForWindowsOnly)

forked from: [Cysharp/MessagePipe](https://github.com/Cysharp/MessagePipe)



※本ライブラリは、Windows専用です。



## 利用方法

- プロジェクトをCloneします。
- ソリューションをビルドし、`RpcIpcUtility`プロジェクト > RpcIpcUtility.dll を使用するプロジェクト参照に加えます。



動作に必要なライブラリは以下の通りです。RpcIpcUtility.dllと同じフォルダーに一緒に配置してください。

* RpcIpcUtility.dll
* MessagePipe.dll
* MessagePipe.Interprocess.dll



また、プロセス間通信に[MessagePack-CSharp](https://github.com/MessagePack-CSharp/MessagePack-CSharp)を使用しています。ユーザー定義型を交換する場合は、作成したクラスに「MessagePackObject」属性を正しく指定し引数として使用できます。



### コード例



#### クライアント側

```csharp
public class ClientSample(string pipeName) : IpcRpcClient(pipeName)
{
    // 戻り値なし
    public async ValueTask Calc0(Sample s, CancellationToken ct = default)
    {
        await Publish(0, s, 3.0, ct);
    }
    public async ValueTask Calc1(int a, CancellationToken ct = default)
    {
        await Publish(1, a, 3.0, ct);
    }
    public async ValueTask Calc2(int a, int b, int c, CancellationToken ct = default)
    {
        await Publish(2, a, b, c, 3.0, ct);
    }

    // 戻り値あり
    public async Task<int> Calc3(int a, CancellationToken ct = default)
    {
        return await Publish<int, int>(100, a, 999.0, ct);
    }
}
```

`IpcRpcClient`クラスを継承し、async ValueTaskを戻り値に持つメソッド内で基底クラスの対応するPublishメソッドを呼び出します。第1引数のkeyはサーバー側の対応するメソッドにリクエストが転送するために使用されます。



利用時は以下のように通常のメソッド呼び出しでサーバーにリクエストが転送されます。

```csharp
static async Task Main(string[] _)
{
    string pipeName = "pipe-name";
    using ClientSample client = new ClientSample(pipeName);

    Sample s = new Sample()
    {
        Code = 100,
        Message = "asdfasdf",
    };

    await client.Calc0(s);
    await client.Calc1(1);
    await client.Calc2(1, 2, 3);

    int ret = await client.Calc3(10);
    Console.WriteLine("ret=" + ret);
}
```





#### サーバー側

```csharp
public class ServerSample(string pipeName, PipeSecurity pipeSecurity = null)
    : IpcRpcServer(pipeName, pipeSecurity)
{
    protected override void RegisterMethods()
    {
        // メソッドを登録する
        Register<Sample>(0, Calc0);
        Register<int>(1, Calc1);
        Register<int, int, int>(2, Calc2);
        Register<int, int>(100, Calc3);
    }

    public async ValueTask Calc0(Sample s)
    {
        Console.WriteLine("Calc0");
    }
    public async ValueTask Calc1(int a)
    {
        Console.WriteLine("Calc1");
    }
    public async ValueTask Calc2(int a, int b, int c)
    {
        Console.WriteLine("Calc2");
    }
    public async ValueTask<int> Calc3(int a)
    {
        Console.WriteLine("Calc3");
        return a;
    }
}
```

`IpcRpcServer`を継承したサーバークラスを作成します。公開するメソッドは`RegisterMethods`メソッド内で基底クラスのRegisterメソッドを使用すると登録できます。



利用方法は以下の通りです。

```csharp
static void Main(string[] _)
{
    Console.WriteLine("[Start] Server");

    string pipeName = "pipe-name";

    PipeSecurity pipeSecurity = new PipeSecurity();
    pipeSecurity.AddAccessRule(
        new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                           PipeAccessRights.FullControl,
                           AccessControlType.Allow));


    ServerSample s = new(pipeName, pipeSecurity);

    Console.WriteLine("Start");
    Console.ReadLine();

    Console.WriteLine("[END] Server");

    return;
}
```

基本的にワーカーサービスなどでDIを組み合わせて使用することになると思うので一般的には以下の通りDIに登録してもよいでしょう。



```csharp
public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();

        PipeSecurity pipeSecurity = new PipeSecurity();
        pipeSecurity.AddAccessRule(
            new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                               PipeAccessRights.FullControl,
                               AccessControlType.Allow));
        ServerSample s = new(pipeName, pipeSecurity);
        builder.Services.AddSingleton(typeof(ServerSample), s);

        var host = builder.Build();
        host.Run();
    }
}

public class Worker : BackgroundService
{
    private readonly ServerSample _s;
    public Worker(ServerSample s)
    {
        _s = s;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        _s?.Dispose();
        _s = null;
        base.Dispose();
    }
}
```



Windowsサービスでサーバーを開始する場合のサンプルはソリューション内の`WindowsService`を参照ください。



以上

