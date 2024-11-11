//
// (C) 2024 Takap.
//

using MessagePack;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;

namespace Takap.RpcIpc;

/// <summary>
/// クライアントの共通基底クラス
/// </summary>
public abstract class IpcRpcClient : IDisposable
{
    // 
    // Fields
    // - - - - - - - - - - - - - - - - - - - -
    #region...

    ServiceCollection _sc;
    ServiceProvider _provider;
    bool _disposed;
    readonly CancellationTokenSource _cts = new CancellationTokenSource();

    #endregion

    // 
    // Constructors
    // - - - - - - - - - - - - - - - - - - - -
    #region...

    public IpcRpcClient(string pipeName)
    {
        var sc = new ServiceCollection();
        IMessagePipeBuilder pb = sc.AddMessagePipe();
        pb.AddNamedPipeInterprocess(pipeName);
        _sc = sc;
        _provider = _sc.BuildServiceProvider();
    }

    ~IpcRpcClient()
    {
        Dispose(false);
    }

    #endregion

    // 
    // IDisposabe impl
    // - - - - - - - - - - - - - - - - - - - -
    #region...

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _provider?.Dispose();
                _provider = null;
                _cts.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    //
    // Priavte Methods
    // - - - - - - - - - - - - - - - - - - - -
    #region...

    ServiceProvider GetService()
    {
        _provider ??= _sc.BuildServiceProvider();
        return _provider;
    }

    // 単方向の送信
    IDistributedPublisher<int, byte[][]> GetPublisher()
    {
        return GetService().GetRequiredService<IDistributedPublisher<int, byte[][]>>();
    }

    // 双方向通信で戻り値を受け取る
    IRemoteRequestHandler<RequestData, ResponseData> GetPublisher2Way<TResponse>()
    {
        return GetService().GetRequiredService<IRemoteRequestHandler<RequestData, ResponseData>>();
    }

    byte[] Conv<T>(T value)
    {
        return MessagePackSerializer.Serialize(value);
    }

    async Task PublishCore(RequestData req, TimeSpan timeOut, CancellationToken ct)
    {
        var pub = GetService().GetRequiredService<IRemoteRequestHandler<RequestData, ResponseData>>();
        Task<ResponseData> mainTask = pub.InvokeAsync(req, ct).AsTask();
        
        using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        Task delayTask = Task.Delay(timeOut, cts.Token);
        Task completedTask = await Task.WhenAny(mainTask, delayTask);

        if (completedTask == delayTask)
        {
            _provider?.Dispose();
            _provider = null;
            throw new RpcIpcBadRequestException("Request timed out. Service not found. or The process is taking a long time.");
        }
        else
        {
            cts.Cancel(); // ちゃんと終わらせる
            ResponseData ret = await mainTask;
            if (!ret.IsCompletedNormally)
            {
                throw new RpcIpcApiFailedException(MessagePackSerializer.Deserialize<string>(ret.Value, cancellationToken: ct));
            }
        }
    }

    async Task<TResponse> PublishCore<TResponse>(RequestData req, TimeSpan timeOut, CancellationToken ct)
    {
        var pub = GetService().GetRequiredService<IRemoteRequestHandler<RequestData, ResponseData>>();
        Task<ResponseData> mainTask = pub.InvokeAsync(req, ct).AsTask();
        Task delayTask = Task.Delay(timeOut, ct);

        // どちらか早く完了した方を待つ
        Task completedTask = await Task.WhenAny(mainTask, delayTask);
        if (completedTask == delayTask)
        {
            _provider?.Dispose();
            _provider = null;

            // 以下のような原因でエラーが発生するが区別がつかないのは仕様
            // - サーバーが存在しない
            // - サーバーが開始していない
            // - サーバーで処理に時間がかかってる
            throw new RpcIpcBadRequestException("Request timed out. Service not found. or The process is taking a long time.");
        }
        else
        {
            ResponseData ret = await mainTask;
            if (ret.IsCompletedNormally)
            {
                return MessagePackSerializer.Deserialize<TResponse>(ret.Value, cancellationToken: ct);
            }
            else
            {
                throw new RpcIpcApiFailedException(MessagePackSerializer.Deserialize<string>(ret.Value, cancellationToken: ct));
            }
        }
    }

    #endregion

    //
    // Protected Methods
    // - - - - - - - - - - - - - - - - - - - -
    #region...

    protected ValueTask PublishOne(int key)
    {
        return GetPublisher().PublishAsync(key, null, _cts.Token);
    }
    protected ValueTask PublishOne<T1>(int key, T1 a)
    {
        return GetPublisher().PublishAsync(key, [Conv(a)], _cts.Token);
    }

    // 戻り値なし
    protected async ValueTask Publish<T1>(int key, T1 a, double timeoutSecond, CancellationToken ct = default)
    {
        await PublishCore(new RequestData(key, [Conv(a)]), TimeSpan.FromSeconds(timeoutSecond), ct);
    }
    protected async ValueTask Publish<T1, T2>(int key, T1 a, T2 b, double timeoutSecond, CancellationToken ct = default)
    {
        await PublishCore(new RequestData(key, [Conv(a), Conv(b)]), TimeSpan.FromSeconds(timeoutSecond), ct);
    }
    protected async ValueTask Publish<T1, T2, T3>(int key, T1 a, T2 b, T3 c, double timeoutSecond, CancellationToken ct = default)
    {
        await PublishCore(new RequestData(key, [Conv(a), Conv(b), Conv(c)]), TimeSpan.FromSeconds(timeoutSecond), ct);
    }

    // 戻り値あり
    protected async ValueTask<TResponse> Publish<T1, TResponse>(int key, T1 a, double timeoutSecond, CancellationToken ct = default)
    {
        return await PublishCore<TResponse>(new(key, [Conv(a)]), TimeSpan.FromSeconds(timeoutSecond), ct);
    }
    protected async ValueTask<TResponse> Publish<T1, T2, TResponse>(int key, T1 a, T2 b, double timeoutSecond, CancellationToken ct = default)
    {
        return await PublishCore<TResponse>(new(key, [Conv(a), Conv(b)]), TimeSpan.FromSeconds(timeoutSecond), ct);
    }
    protected async ValueTask<TResponse> Publish<T1, T2, T3, TResponse>(int key, T1 a, T2 b, T3 c, double timeoutSecond, CancellationToken ct = default)
    {
        return await PublishCore<TResponse>(new(key, [Conv(a), Conv(b), Conv(c)]), TimeSpan.FromSeconds(timeoutSecond), ct);
    }

    #endregion
}

