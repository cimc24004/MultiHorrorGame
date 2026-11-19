# MultiHorrorGame

Unity / C# で制作した、Steamのリレー通信を利用するオンラインマルチプレイのホラーゲームです。ロビーからゲーム本編に入り、ステージを巡回する敵を避けながら行動します。

## 実装の見どころ：敵の巡回・追跡

敵の行動はサーバー側で処理しています。巡回先を決める処理と、指定した位置まで移動する処理を別のコンポーネントに分けています。巡回では決められた地点を順にたどり、各地点で一定時間待機します。終点に着いたら逆順に戻ります。

| ソースコード | 役割 |
| --- | --- |
| [EnemyRoutingController.cs](Assets/Scripts/Enemy/EnemyRoutingController.cs) | 次の巡回地点の選択、到着後の待機、巡回方向の切り替え |
| [DiscoveryPlayerController.cs](Assets/Scripts/Enemy/DiscoveryPlayerController.cs) | プレイヤーの検知と追跡に関する処理 |
| [FollowObjectByNav.cs](Assets/Scripts/Enemy/FollowObjectByNav.cs) | NavMeshを使った目的地への移動と到着判定 |

## 使用技術

- Unity 6.1（6000.1.2f1） / C#
- Netcode for GameObjects 2.4.4
- Steamworks.NET / SteamNetworkingSockets Transport
- Unity AI Navigation（NavMesh）
- Universal Render Pipeline（URP）
