using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UniRx;
using Cysharp.Threading.Tasks;
using System;
using Random = Unity.Mathematics.Random;
using FPS.MVP;
using UnityEngine.Rendering;
using System.Diagnostics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class EnemySpawnerSystem : SystemBase
{
    private Random random;
    private bool IsSpawn = true;

    protected override void OnUpdate()
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (spawner, entity) in
            SystemAPI.Query<RefRW<EnemySpanwer>>()
            .WithNone<Round>()
            .WithEntityAccess())
        {
            commandBuffer.AddComponent<Round>(entity);

            MessageBroker.Default.Receive<RoundStartEvent>().Subscribe(_ =>
            {
                var ecb = new EntityCommandBuffer(Allocator.Temp);

                Entities.WithAll<Enemy>().ForEach((Entity entity, int entityInQueryIndex) =>
                {
                    ecb.DestroyEntity(entity);
                }).Run();

                ecb.Playback(EntityManager);
                ecb.Dispose();

                HandleRoundStart(entity).Forget();
            });

            MessageBroker.Default.Receive<EnemyDieEvent>().Subscribe(_ =>
            {
                EntityQuery remainEnemyQuery = SystemAPI.QueryBuilder().WithAll<CharacterHP, Enemy>().Build();
                NativeArray<Enemy> remainEnemyLtWs = remainEnemyQuery.ToComponentDataArray<Enemy>(Allocator.Temp);
                var currentRound = SystemAPI.GetComponent<Round>(entity);

                if (remainEnemyLtWs.Length <= 1 && IsSpawn == false)
                {
                    PopupManager.Instance.Get<ShopPresenter>(new ShopPresenter.Args { Round = currentRound.CurrentRound }).Forget();
                    IsSpawn = true;
                }
            });

            MessageBroker.Default.Publish(new RoundStartEvent { Round = 1 });

            uint randomSeed = (uint)DateTime.Now.Millisecond;
            random = Random.CreateFromIndex(randomSeed);
        }

        foreach (var (spawner, round, entity) in SystemAPI
                 .Query<RefRW<EnemySpanwer>, RefRW<Round>>().WithEntityAccess())
        {
            if (round.ValueRW.Count > 0)
            {
                round.ValueRW.SpawnTimer -= SystemAPI.Time.DeltaTime;

                if (round.ValueRO.SpawnTimer < 0f)
                {
                    EntityQuery spawnPointsQuery = SystemAPI.QueryBuilder().WithAll<EnemySpawnPoint, LocalToWorld>().Build();
                    NativeArray<LocalToWorld> spawnPointLtWs = spawnPointsQuery.ToComponentDataArray<LocalToWorld>(Allocator.Temp);

                    if (spawnPointLtWs.Length > 0)
                    {
                        int index = random.NextInt(spawnPointLtWs.Length);


                        if (spawner.ValueRO.LastSpawnPoint.x == spawnPointLtWs[index].Position.x
                            && spawner.ValueRO.LastSpawnPoint.y == spawnPointLtWs[index].Position.y
                            && spawner.ValueRO.LastSpawnPoint.z == spawnPointLtWs[index].Position.z)
                            continue;

                        var enemy = commandBuffer.Instantiate(spawner.ValueRO.Enemy);
                        commandBuffer.SetComponent(enemy, new LocalTransform
                        {
                            Position = spawnPointLtWs[index].Position,
                            Rotation = quaternion.identity,
                            Scale = 1f
                        });

                        PathRequest pathRequest = new PathRequest();
                        pathRequest.RequestPath = true;

                        //commandBuffer.AddBuffer<PathBufferElement>(enemy);
                        //commandBuffer.AddComponent(enemy, pathRequest);

                        round.ValueRW.Count--;
                        round.ValueRW.SpawnTimer = round.ValueRO.Time;
                        spawner.ValueRW.LastSpawnPoint = spawnPointLtWs[index].Position;
                    }
                }
            }
            else
            {
                IsSpawn = false;
            }
        }

        commandBuffer.Playback(EntityManager);
        commandBuffer.Dispose();
    }

    private async UniTask HandleRoundStart(Entity entity)
    {
        await UniTask.Delay(5000);

        if (SystemAPI.HasComponent<Round>(entity))
        {
            var currentRound = SystemAPI.GetComponent<Round>(entity);
            if (DataContainer.RoundData.TryGetValue(currentRound.CurrentRound + 1, out var roundData))
            {
                currentRound.CurrentRound = roundData.Round;
                currentRound.Time = roundData.Time;
                currentRound.Count = roundData.Count;
                currentRound.StartHeId = true;

                SystemAPI.SetComponent(entity, currentRound);
            }
        }
    }
}
