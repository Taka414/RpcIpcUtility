//
// (C) 2024 Takap.
//

using MessagePack;

namespace Takap.RpcIpc.Samples;

// サーバーとクライアント間で共通の型をやり取りしたい場合の定義
[MessagePackObject]
public class Sample
{
    [Key(0)]
    public int Code { get; set; }
    [Key(1)]
    public string Message { get; set; } = "";
}
