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



