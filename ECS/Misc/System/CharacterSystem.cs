using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.CharacterController;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial struct CharacterInitializationSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged);
        BufferLookup<LinkedEntityGroup> linkedEntitiesLoopkup = SystemAPI.GetBufferLookup<LinkedEntityGroup>(true);

        foreach (var (character, stateMachine, entity) in SystemAPI.Query<RefRW<CharacterComponent>, RefRW<CharacterStateMachine>>().WithNone<CharacterInitialized>().WithEntityAccess())
        {
            if (linkedEntitiesLoopkup.HasBuffer(entity))
            {
                CharacterUtilities.SetEntityHierarchyEnabled(false, character.ValueRO.RollBallMeshEntity, ecb, linkedEntitiesLoopkup);
                ecb.AddComponent<CharacterInitialized>(entity);
            }
        }
    }
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
[BurstCompile]
public partial struct CharacterVariableUpdateSystem : ISystem
{
    private EntityQuery _characterQuery;
    private CharacterUpdateContext _context;
    private KinematicCharacterUpdateContext _baseContext;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _characterQuery = KinematicCharacterUtilities.GetBaseCharacterQueryBuilder()
            .WithAll<
                CharacterComponent,
                CharacterControl>()
            .Build(ref state);

        _context = new CharacterUpdateContext();
        _context.OnSystemCreate(ref state);
        _baseContext = new KinematicCharacterUpdateContext();
        _baseContext.OnSystemCreate(ref state);

        state.RequireForUpdate(_characterQuery);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _context.OnSystemUpdate(ref state, SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged));
        _baseContext.OnSystemUpdate(ref state, SystemAPI.Time, SystemAPI.GetSingleton<PhysicsWorldSingleton>());

        CharacterVariableUpdateJob job = new CharacterVariableUpdateJob
        {
            Context = _context,
            BaseContext = _baseContext
        };
        job.ScheduleParallel();
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct CharacterVariableUpdateJob : IJobEntity, IJobEntityChunkBeginEnd
    {
        public CharacterUpdateContext Context;
        public KinematicCharacterUpdateContext BaseContext;

        void Execute([ChunkIndexInQuery] int chunkIndex, CharacterAspect characterAspect)
        {
            Context.SetChunkIndex(chunkIndex);
            characterAspect.VariableUpdate(ref Context, ref BaseContext);
        }

        public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            BaseContext.EnsureCreationOfTmpCollections();
            return true;
        }

        public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted)
        { }
    }
}
    [UpdateInGroup(typeof(KinematicCharacterPhysicsUpdateGroup))]
[BurstCompile]
public partial struct CharacterPhysicsUpdateSystem : ISystem
{
    private EntityQuery characterQuery;
    private CharacterUpdateContext _context;
    private KinematicCharacterUpdateContext baseContext;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        characterQuery = KinematicCharacterUtilities.GetBaseCharacterQueryBuilder()
            .WithAll<
                CharacterComponent,
                CharacterControl,
                CharacterStateMachine>()
                .Build(ref state);

        _context = new CharacterUpdateContext();
        _context.OnSystemCreate(ref state);
        baseContext = new KinematicCharacterUpdateContext();
        baseContext.OnSystemCreate(ref state);

        state.RequireForUpdate(characterQuery);
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _context.OnSystemUpdate(ref state, SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged));
        baseContext.OnSystemUpdate(ref state, SystemAPI.Time, SystemAPI.GetSingleton<PhysicsWorldSingleton>());

        CharacterPhysicsUpdateJob job = new CharacterPhysicsUpdateJob
        {
            Context = _context,
            BaseContext = baseContext,
        };
        job.ScheduleParallel();
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct CharacterPhysicsUpdateJob : IJobEntity, IJobEntityChunkBeginEnd
    {
        public CharacterUpdateContext Context;
        public KinematicCharacterUpdateContext BaseContext;

        void Execute([ChunkIndexInQuery] int chunkIndex, CharacterAspect characterAspect)
        {
            Context.SetChunkIndex(chunkIndex); 
            characterAspect.PhysicsUpdate(ref Context, ref BaseContext);
        }

        public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            BaseContext.EnsureCreationOfTmpCollections();
            return true;
        }

        public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted)
        { }
    }
}