using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStuckDetector : NetworkBehaviour
{
    [Header("スタック検知設定")]
    [SerializeField] private float stuckDetectionTime = 3.0f; // スタック判定時間
    [SerializeField] private float minMoveDistance = 0.5f; // 最小移動距離
    [SerializeField] private float checkInterval = 0.5f; // チェック間隔
    
    [Header("代替ルート設定")]
    [SerializeField] private bool useAlternativeRoute = true; // 代替ルート使用フラグ
    [SerializeField] private float alternativeRouteRadius = 10.0f; // 代替ルート探索半径
    [SerializeField] private int maxAlternativeAttempts = 3; // 代替ルート試行回数
    [SerializeField] private float waypointReachDistance = 2.0f; // 経由点到達判定距離
    
    [Header("テレポート設定")]
    [SerializeField] private bool useTeleportOnStuck = false; // スタック時テレポート使用フラグ
    [SerializeField] private float teleportRadius = 8.0f; // テレポート範囲
    [SerializeField] private int maxTeleportAttempts = 5; // テレポート試行回数
    [SerializeField] private float minTeleportDistance = 3.0f; // 最小テレポート距離
    
    // 必要なコンポーネント
    private DiscoveryPlayerController discovery_player_controller;
    private EnemySoundSensor enemy_sound_sensor;
    private FollowObjectByNav follow_object_nav;
    private EnemyRoutingController enemy_routing_controller;
    private EnemyStatus enemy_status;
    private NavMeshAgent nav_agent;
    
    // スタック検知用変数
    private Vector3 lastPosition;
    private float stuckTimer;
    private bool isStuckDetectionActive;
    private Coroutine stuckCheckCoroutine;
    
    // 代替ルート用変数
    private bool isUsingAlternativeRoute;
    private List<Vector3> alternativeWaypoints;
    private int currentWaypointIndex;
    private Vector3 originalTarget;
    private Coroutine alternativeRouteCoroutine;
    
    [Header("デバッグ情報")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private float currentStuckTime;
    [SerializeField] private float distanceMoved;
    [SerializeField] private bool isCurrentlyStuck;
    [SerializeField] private bool isUsingAltRoute;
    [SerializeField] private int currentWaypoint;
    
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        
        // コンポーネント取得
        discovery_player_controller = GetComponent<DiscoveryPlayerController>();
        enemy_sound_sensor = GetComponent<EnemySoundSensor>();
        follow_object_nav = GetComponent<FollowObjectByNav>();
        enemy_routing_controller = GetComponent<EnemyRoutingController>();
        enemy_status = GetComponent<EnemyStatus>();
        nav_agent = GetComponent<NavMeshAgent>();
        
        // 初期化
        lastPosition = transform.position;
        stuckTimer = 0f;
        isStuckDetectionActive = false;
        isUsingAlternativeRoute = false;
        alternativeWaypoints = new List<Vector3>();
        currentWaypointIndex = 0;
        
        // スタック検知コルーチン開始
        stuckCheckCoroutine = StartCoroutine(StuckCheckCoroutine());
    }
    
    private void Update()
    {
        if (!IsServer) return;
        
        // デバッグ情報更新
        if (showDebugInfo)
        {
            currentStuckTime = stuckTimer;
            distanceMoved = Vector3.Distance(transform.position, lastPosition);
            isCurrentlyStuck = stuckTimer >= stuckDetectionTime;
            isUsingAltRoute = isUsingAlternativeRoute;
            currentWaypoint = currentWaypointIndex;
        }
    }
    
    private IEnumerator StuckCheckCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            
            if (!IsServer) continue;
            
            // 敵が動いているべき状況かチェック
            bool shouldBeMoving = ShouldEnemyBeMoving();
            
            if (shouldBeMoving)
            {
                CheckForStuck();
            }
            else
            {
                // 動いていなくてもよい状況ならタイマーリセット
                ResetStuckTimer();
            }
        }
    }
    
    private bool ShouldEnemyBeMoving()
    {
        // 敵が動いているべき状況を判定
        bool isFollowing = follow_object_nav.ChaseToTargetMode && !follow_object_nav.ArrivedTarget;
        
        return isFollowing;
    }
    
    private void CheckForStuck()
    {
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        
        if (distanceMoved < minMoveDistance)
        {
            // 十分に動いていない場合
            stuckTimer += checkInterval;
            
            if (stuckTimer >= stuckDetectionTime)
            {
                // スタック判定
                HandleStuckSituation();
            }
        }
        else
        {
            // 十分に動いている場合はタイマーリセット
            ResetStuckTimer();
        }
        
        lastPosition = transform.position;
    }
    
    private void ResetStuckTimer()
    {
        stuckTimer = 0f;
        lastPosition = transform.position;
    }
    
    private void HandleStuckSituation()
    {
        if (showDebugInfo)
        {
            Debug.LogWarning($"[{gameObject.name}] スタック検知！");
        }

        // RouteMovePermitがtrueで代替ルートが有効な場合
        if (useAlternativeRoute && enemy_routing_controller != null &&
            enemy_routing_controller.RoutingMovePermit && follow_object_nav.ChaseToTargetMode)
        {
            // 代替ルートを試行
            if (TryAlternativeRoute())
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[{gameObject.name}] 代替ルートを開始しました。");
                }
                ResetStuckTimer();
                return;
            }

        }

        // 代替ルートが使用できない場合は通常のリセット処理
        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] 通常のリセット処理を実行します。");
        }
        
        ResetEnemyBehavior();
        ResetStuckTimer();
    }
    
    private bool TryAlternativeRoute()
    {
        // 既に代替ルートを使用している場合は失敗
        if (isUsingAlternativeRoute) return false;
        
        // 現在の目標地点を取得
        Vector3 targetPosition = follow_object_nav.ChaseTargetPosition;
        
        // 代替ルートを生成
        List<Vector3> waypoints = GenerateAlternativeWaypoints(transform.position, targetPosition);
        
        if (waypoints.Count > 0)
        {
            // 代替ルート開始
            StartAlternativeRoute(waypoints, targetPosition);
            return true;
        }
        
        return false;
    }
    
    private List<Vector3> GenerateAlternativeWaypoints(Vector3 startPos, Vector3 targetPos)
    {
        List<Vector3> waypoints = new List<Vector3>();
        
        // 複数の代替ルートを試行
        for (int attempt = 0; attempt < maxAlternativeAttempts; attempt++)
        {
            // ランダムな中間地点を生成
            Vector3 randomDirection = Random.insideUnitSphere * alternativeRouteRadius;
            randomDirection.y = 0; // Y軸は固定
            
            Vector3 intermediatePoint = startPos + randomDirection;
            
            // NavMesh上の有効な位置を探す
            NavMeshHit hit;
            if (NavMesh.SamplePosition(intermediatePoint, out hit, alternativeRouteRadius, NavMesh.AllAreas))
            {
                Vector3 validPoint = hit.position;
                
                // 開始地点から中間地点へのパスをチェック
                NavMeshPath pathToIntermediate = new NavMeshPath();
                if (NavMesh.CalculatePath(startPos, validPoint, NavMesh.AllAreas, pathToIntermediate))
                {
                    // 中間地点から目標地点へのパスをチェック
                    NavMeshPath pathToTarget = new NavMeshPath();
                    if (NavMesh.CalculatePath(validPoint, targetPos, NavMesh.AllAreas, pathToTarget))
                    {
                        // 両方のパスが有効な場合、中間地点を追加
                        waypoints.Add(validPoint);
                        
                        if (showDebugInfo)
                        {
                            Debug.Log($"[{gameObject.name}] 代替ルート中間地点を生成: {validPoint}");
                        }
                        
                        break; // 一つの中間地点が見つかったら終了
                    }
                }
            }
        }
        
        return waypoints;
    }
    
    private bool TryTeleportToRandomPosition()
    {
        Vector3 currentPosition = transform.position;
        
        // 複数回試行してテレポート先を探す
        for (int attempt = 0; attempt < maxTeleportAttempts; attempt++)
        {
            // ランダムな方向を生成
            Vector3 randomDirection = Random.insideUnitSphere * teleportRadius;
            randomDirection.y = 0; // Y軸は固定
            
            Vector3 targetPosition = currentPosition + randomDirection;
            
            // NavMesh上の有効な位置を探す
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, teleportRadius, NavMesh.AllAreas))
            {
                Vector3 teleportPosition = hit.position;
                
                // 最小距離チェック
                if (Vector3.Distance(currentPosition, teleportPosition) >= minTeleportDistance)
                {
                    // テレポート先からターゲットへのパスが存在するかチェック
                    if (follow_object_nav.ChaseToTargetMode)
                    {
                        Vector3 targetPos = follow_object_nav.ChaseTargetPosition;
                        NavMeshPath pathToTarget = new NavMeshPath();
                        
                        if (NavMesh.CalculatePath(teleportPosition, targetPos, NavMesh.AllAreas, pathToTarget))
                        {
                            // テレポート実行
                            ExecuteTeleport(teleportPosition);
                            return true;
                        }
                    }
                    else
                    {
                        // 追跡モードでない場合はそのままテレポート
                        ExecuteTeleport(teleportPosition);
                        return true;
                    }
                }
            }
        }
        
        return false; // テレポート失敗
    }
    
    private void ExecuteTeleport(Vector3 teleportPosition)
    {
        // NavMeshAgentを一時的に無効化
        if (nav_agent != null && nav_agent.isActiveAndEnabled)
        {
            nav_agent.enabled = false;
        }
        
        // 位置を変更
        transform.position = teleportPosition;
        
        // NavMeshAgentを再有効化
        if (nav_agent != null)
        {
            nav_agent.enabled = true;
            
            // 目標地点が設定されている場合は再設定
            if (follow_object_nav.ChaseToTargetMode)
            {
                nav_agent.SetDestination(follow_object_nav.ChaseTargetPosition);
            }
        }
        
        // 位置を更新
        lastPosition = transform.position;
        
        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] テレポート完了: {teleportPosition}");
        }
    }
    
    private void StartAlternativeRoute(List<Vector3> waypoints, Vector3 finalTarget)
    {
        isUsingAlternativeRoute = true;
        alternativeWaypoints = waypoints;
        currentWaypointIndex = 0;
        originalTarget = finalTarget;
        
        // 代替ルート実行コルーチン開始
        if (alternativeRouteCoroutine != null)
        {
            StopCoroutine(alternativeRouteCoroutine);
        }
        alternativeRouteCoroutine = StartCoroutine(AlternativeRouteCoroutine());
    }
    
    private IEnumerator AlternativeRouteCoroutine()
    {
        while (isUsingAlternativeRoute && currentWaypointIndex < alternativeWaypoints.Count)
        {
            Vector3 currentWaypoint = alternativeWaypoints[currentWaypointIndex];
            
            // 現在の経由点に向かって移動
            if (nav_agent != null && nav_agent.isActiveAndEnabled)
            {
                nav_agent.SetDestination(currentWaypoint);
            }
            
            // 経由点に到達するまで待機
            while (Vector3.Distance(transform.position, currentWaypoint) > waypointReachDistance)
            {
                yield return new WaitForSeconds(0.1f);
                
                // 代替ルートがキャンセルされた場合は終了
                if (!isUsingAlternativeRoute) yield break;
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[{gameObject.name}] 経由点 {currentWaypointIndex} に到達しました。");
            }
            
            currentWaypointIndex++;
        }
        
        // 全ての経由点を通過したら最終目標に向かう
        if (isUsingAlternativeRoute)
        {
            if (nav_agent != null && nav_agent.isActiveAndEnabled)
            {
                nav_agent.SetDestination(originalTarget);
            }
            
            // 通常の追跡モードに戻す
            follow_object_nav.ChaseTargetPosition = originalTarget;
            
            if (showDebugInfo)
            {
                Debug.Log($"[{gameObject.name}] 代替ルートを完了し、最終目標に向かいます。");
            }
        }
        
        // 代替ルート終了
        EndAlternativeRoute();
    }
    
    private void EndAlternativeRoute()
    {
        isUsingAlternativeRoute = false;
        alternativeWaypoints.Clear();
        currentWaypointIndex = 0;
        
        if (alternativeRouteCoroutine != null)
        {
            StopCoroutine(alternativeRouteCoroutine);
            alternativeRouteCoroutine = null;
        }
    }
    
    private void ResetEnemyBehavior()
    {
        // 代替ルートを停止
        if (isUsingAlternativeRoute)
        {
            EndAlternativeRoute();
        }
        
        // 1. プレイヤー発見状態をリセット
        if (discovery_player_controller.DiscoveryPlayerNowCheck())
        {
            var discoveredPlayers = new List<GameObject>(discovery_player_controller.DiscoveriedPlayers);
            foreach (var player in discoveredPlayers)
            {
                discovery_player_controller.RemovePlayerOfDiscoveryPlayerLists(player);
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[{gameObject.name}] プレイヤー発見状態をリセットしました。");
            }
        }
        
        // 2. 音センサー状態をリセット
        if (enemy_sound_sensor.MoveToSoundPlaceNow)
        {
            enemy_sound_sensor.MoveToSoundPlaceNow = false;
            enemy_sound_sensor.sound_by_obj = false;
            
            if (showDebugInfo)
            {
                Debug.Log($"[{gameObject.name}] 音センサー状態をリセットしました。");
            }
        }
        
        // 3. 追跡状態をリセット
        if (follow_object_nav.ChaseToTargetMode)
        {
            follow_object_nav.ChaseToTargetMode = false;
            follow_object_nav.ResetArrivedTarget();
            
            if (showDebugInfo)
            {
                Debug.Log($"[{gameObject.name}] 追跡状態をリセットしました。");
            }
        }
        
        // 4. 徘徊ルートに復帰
        if (enemy_routing_controller != null)
        {
            enemy_routing_controller.NearRouteSet();
            
            if (showDebugInfo)
            {
                Debug.Log($"[{gameObject.name}] 徘徊ルートに復帰しました。");
            }
        }
        
        // 5. 敵の状態を歩行に設定
        if (enemy_status != null)
        {
            enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Walk;
        }
    }
    
    // 外部から手動でリセットを呼び出す関数
    public void ForceResetEnemyBehavior()
    {
        if (!IsServer)
        {
            Debug.LogError("この関数はサーバーからでしか実行できません！");
            return;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] 手動リセットが実行されました。");
        }
        
        ResetEnemyBehavior();
        ResetStuckTimer();
    }
    
    // 代替ルートを手動でキャンセルする関数
    public void CancelAlternativeRoute()
    {
        if (!IsServer) return;
        
        if (isUsingAlternativeRoute)
        {
            EndAlternativeRoute();
            
            if (showDebugInfo)
            {
                Debug.Log($"[{gameObject.name}] 代替ルートをキャンセルしました。");
            }
        }
    }
    
    // スタック検知設定を変更する関数
    public void SetStuckDetectionSettings(float detectionTime, float minDistance, float interval)
    {
        stuckDetectionTime = detectionTime;
        minMoveDistance = minDistance;
        checkInterval = interval;
    }
    
    // 代替ルート設定を変更する関数
    public void SetAlternativeRouteSettings(bool useAltRoute, float radius, int maxAttempts)
    {
        useAlternativeRoute = useAltRoute;
        alternativeRouteRadius = radius;
        maxAlternativeAttempts = maxAttempts;
    }
    
    // テレポート設定を変更する関数
    public void SetTeleportSettings(bool useTeleport, float radius, int maxAttempts, float minDistance)
    {
        useTeleportOnStuck = useTeleport;
        teleportRadius = radius;
        maxTeleportAttempts = maxAttempts;
        minTeleportDistance = minDistance;
    }
    
    // 手動テレポート実行関数
    public bool ForceRandomTeleport()
    {
        if (!IsServer)
        {
            Debug.LogError("この関数はサーバーからでしか実行できません！");
            return false;
        }
        
        if (TryTeleportToRandomPosition())
        {
            if (showDebugInfo)
            {
                Debug.Log($"[{gameObject.name}] 手動テレポートが実行されました。");
            }
            ResetStuckTimer();
            return true;
        }
        
        return false;
    }
    
    private void OnDestroy()
    {
        if (stuckCheckCoroutine != null)
        {
            StopCoroutine(stuckCheckCoroutine);
        }
        
        if (alternativeRouteCoroutine != null)
        {
            StopCoroutine(alternativeRouteCoroutine);
        }
    }
    
    // デバッグ用Gizmo
    private void OnDrawGizmos()
    {
        if (!showDebugInfo) return;
        
        // 現在位置
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // 最後の位置
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lastPosition, 0.3f);
        
        // スタック状態なら赤色で表示
        if (isCurrentlyStuck)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1.0f);
        }
        
        // 代替ルート使用中の場合
        if (isUsingAlternativeRoute && alternativeWaypoints != null)
        {
            // 代替ルートの経由点を表示
            Gizmos.color = Color.cyan;
            for (int i = 0; i < alternativeWaypoints.Count; i++)
            {
                Gizmos.DrawWireSphere(alternativeWaypoints[i], 0.8f);
                
                // 経由点番号を表示
                if (i == currentWaypointIndex)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireSphere(alternativeWaypoints[i], 1.2f);
                    Gizmos.color = Color.cyan;
                }
            }
            
            // 最終目標を表示
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(originalTarget, 1.0f);
        }
        
        // 代替ルート探索範囲を表示
        if (useAlternativeRoute)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, alternativeRouteRadius);
        }
        
        // テレポート範囲を表示
        if (useTeleportOnStuck)
        {
            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(transform.position, teleportRadius);
            
            // 最小テレポート距離を表示
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, minTeleportDistance);
        }
    }
}