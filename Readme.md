# これは何？

DearDeerは、低速度及び高速度の侵徹について、一次元の解析的なモデルに基づいて計算を行うソフトウェアです。

C#レファレンスはawlib/docを、pythonレファレンスはpenepy.doc参照のこと

モデルの理論的なところはE. Dekel、Z. RosenbergのTerminal ballistics、P. J. HazellのArmourがおすすめです。

計算の中核となるawlib.dllは現在.Net standard 2.0をターゲットにしています。
Windows環境であればpenepyモジュールはそのまま動作しますが、その他のOSでは正常に動作しません。
.net core 3.0であれば問題なくpenepyモジュールでインポートすることができるので、
.net core SDKをインストールし、.net standard 2.0をターゲットにしたプロジェクトを作製後、awlib内の*.csファイルをコピーしてビルドすることで動作すると考えられます。
penepyの実行にはpythonnet2.5.2を使用しています。pipでインストールしてください。