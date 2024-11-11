//
// (C) 2024 Takap.
//

using System.IO.Pipes;
using MessagePack;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;

using Table = System.Collections.Generic.Dictionary<int, System.Func<byte[][], System.Threading.Tasks.ValueTask<byte[]>>>;

namespace Takap.RpcIpc;

/// <summary>
/// サーバーの共通基底クラス
/// </summary>
public abstract class IpcRpcServer : IDisposable
{
    // 
    // Fields
    // - - - - - - - - - - - - - - - - - - - -
    #region... 

    //readonly string _pipeName;
    readonly ServiceCollection _sc;
    ServiceProvider _provider;
    private bool _disposed;
    readonly CancellationTokenSource _cts = new();

    #endregion

    // 
    // Constructors
    // - - - - - - - - - - - - - - - - - - - -
    #region...

    public IpcRpcServer(string pipeName, PipeSecurity pipeSecurity = null)
    {
        var sc = new ServiceCollection();

        // MessagePipeを作成
        IMessagePipeBuilder pb = sc.AddMessagePipe();
        pb.AddNamedPipeInterprocess(pipeName, options =>
        {
            options.HostAsServer = true;
            options.PipeSecurity = pipeSecurity;
        });
        pb.AddAsyncRequestHandler(typeof(InternalAsyncHandler));
        //pb.AddAsyncRequestHandler(typeof(InternalAsyncHandlerDummy));

        // メソッドの割り当てテーブルの初期化
        sc.AddSingleton<Table>();
        _sc = sc;

        // ダミーを登録しておかないとメッセージ処理が開始されないため仮登録
        _provider =_sc.BuildServiceProvider();
        _provider
            .GetRequiredService<IDistributedSubscriber<int, int>>()
                .SubscribeAsync(int.MinValue, _ =>
                {
                    Console.WriteLine("DummyHander");
                });
        
        RegisterMethods();
    }

    ~IpcRpcServer()
    {
        Dispose(false);
    }

    #endregion

    // 
    // IDisposabe impl
    // - - - - - - - - - - - - - - - - - - - -
    #region...

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            _provider?.Dispose();
            _cts?.Dispose();
        }
        _disposed = true;

    }

    #endregion

    //
    // Priavte Methods
    // - - - - - - - - - - - - - - - - - - - -
    #region...

    // 派生クラスでサーバーのメソッドを登録する
    protected abstract void RegisterMethods();

    // 単方向通信用のPubを取得する
    IDistributedSubscriber<int, byte[][]> GetSub()
    {
        _provider ??= _sc.BuildServiceProvider();
        return _provider.GetRequiredService<IDistributedSubscriber<int, byte[][]>>();
    }

    Table GetTable() => _provider.GetRequiredService<Table>();

    // 引数と戻り値の変換
    static T Conv<T>(byte[] value) => MessagePackSerializer.Deserialize<T>(value);
    static byte[] Conv<T>(T value) => MessagePackSerializer.Serialize(value);

    #endregion

    //
    // Protected Methods
    // - - - - - - - - - - - - - - - - - - - -
    #region...

    // IDistributedSubscriberを使用した結果を受け取らないメソッドの登録
    protected void RegisterOne(int key, Func<ValueTask> method)
    {
        GetSub().SubscribeAsync(key, args =>
        {
            method();
        });
    }
    protected void RegisterOne<T1>(int key, Func<T1, ValueTask> method)
    {
        GetSub().SubscribeAsync(key, async args =>
        {
            T1 a = Conv<T1>(args[0]);
            await method(a);
        });
    }

    // 戻り値無しのメソッドの登録
    protected void Register(int key, Func<ValueTask> method)
    {
        ArgumentNullException.ThrowIfNull(method);
        GetTable().Add(key, async args =>
        {
            await method();
            return null;
        });
    }
    protected void Register<T1>(int key, Func<T1, ValueTask> method)
    {
        ArgumentNullException.ThrowIfNull(method);
        GetTable().Add(key, async args =>
        {
            T1 a = Conv<T1>(args[0]);
            await method(a);
            return null;
        });
    }
    protected void Register<T1, T2>(int key, Func<T1, T2, ValueTask> method)
    {
        ArgumentNullException.ThrowIfNull(method);
        GetTable().Add(key, async args =>
        {
            T1 a = Conv<T1>(args[0]);
            T2 b = Conv<T2>(args[1]);
            await method(a, b);
            return null;
        });
    }
    protected void Register<T1, T2, T3>(int key, Func<T1, T2, T3, ValueTask> method)
    {
        ArgumentNullException.ThrowIfNull(method);
        GetTable().Add(key, async args =>
        {
            T1 a = Conv<T1>(args[0]);
            T2 b = Conv<T2>(args[1]);
            T3 c = Conv<T3>(args[2]);
            await method(a, b, c);
            return null;
        });
    }

    // 戻り値ありのメソッドの登録
    protected void Register<TResult>(int key, Func<ValueTask<TResult>> method)
    {
        ArgumentNullException.ThrowIfNull(method);
        GetTable().Add(key, async args =>
        {
            var ret = await method();
            return Conv(ret);
        });
    }
    protected void Register<T1, TResult>(int key, Func<T1, ValueTask<TResult>> method)
    {
        ArgumentNullException.ThrowIfNull(method);
        GetTable().Add(key, async args =>
        {
            T1 a = Conv<T1>(args[0]);
            var ret = await method(a);
            return Conv(ret);
        });
    }
    protected void Register<T1, T2, TResult>(int key, Func<T1, T2, ValueTask<TResult>> method)
    {
        ArgumentNullException.ThrowIfNull(method);
        GetTable().Add(key, async args =>
        {
            T1 a = Conv<T1>(args[0]);
            T2 b = Conv<T2>(args[1]);
            var ret = await method(a, b);
            return Conv(ret);
        });
    }
    protected void Register<T1, T2, T3, TResult>(int key, Func<T1, T2, T3, ValueTask<TResult>> method)
    {
        ArgumentNullException.ThrowIfNull(method);
        GetTable().Add(key, async args =>
        {
            T1 a = Conv<T1>(args[0]);
            T2 b = Conv<T2>(args[1]);
            T3 c = Conv<T3>(args[2]);
            var ret = await method(a, b, c);
            return Conv(ret);
        });
    }

    #endregion

    //
    // Private Types
    // - - - - - - - - - - - - - - - - - - - -
    #region...

    // 戻り値ありの要求を受け取るための内部の型
    public class InternalAsyncHandler(Table table) : IAsyncRequestHandler<RequestData, ResponseData>
    {
        public readonly Table Service = table;
        
        public async ValueTask<ResponseData> InvokeAsync(RequestData req, CancellationToken ct = default)
        {
            Console.WriteLine("[InvokeAsync] accepted. req.key=" + req.Key);

            // IDみて自分で振り分ける必要がありそう
            int key = req.Key;
            if (Service.TryGetValue(key, out Func<byte[][], ValueTask<byte[]>> value))
            {
                try
                {
                    var ret = await value(req.Args);
                    ret ??= [];
                    return new(true, ret);
                }
                catch (Exception ex)
                {
                    return new(false, Conv(ex.Message));
                }
            }
            return new(false, MessagePackSerializer.Serialize("Method not found", cancellationToken: ct));
        }
    }

    #endregion
}
