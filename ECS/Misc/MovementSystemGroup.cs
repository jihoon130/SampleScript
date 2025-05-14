using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial class MovementSystemGroup : ComponentSystemGroup
{
}