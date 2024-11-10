//
// (C) 2024 Takap.
//

using MessagePack;

namespace Takap.RpcIpc;

/// <summary>
/// クライアント → サーバーへの送信データを表します。
/// </summary>
[MessagePackObject]
public readonly struct RequestData(int key, byte[][] args)
{
    [Key(0)]
    public readonly int Key = key;
    [Key(1)]
    public readonly byte[][] Args = args;
}

/// <summary>
/// サーバー → クライアントへの戻り値を表します。
/// </summary>
[MessagePackObject]
public readonly struct ResponseData(bool isCompletedNormally, byte[] value)
{
    [Key(0)]
    public readonly bool IsCompletedNormally = isCompletedNormally;
    [Key(1)]
    public readonly byte[] Value = value;
}

/// <summary>
/// ライブラリーで発生する例外を表します。
/// </summary>
[Serializable]
public class RpcIpcApiFailedException : Exception
{
	public RpcIpcApiFailedException() { }
	public RpcIpcApiFailedException(string message) : base(message) { }
	public RpcIpcApiFailedException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// クライアントで発生する例外を表します。
/// </summary>
[Serializable]
public class RpcIpcBadRequestException : Exception
{
    public RpcIpcBadRequestException() { }
    public RpcIpcBadRequestException(string message) : base(message) { }
    public RpcIpcBadRequestException(string message, Exception inner) : base(message, inner) { }
}