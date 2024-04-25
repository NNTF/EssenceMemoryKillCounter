using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.PoEMemory.Components;

namespace EssenceMemoryKillCounter
{

    public class Essence
    {
        public Essence(Entity entity)
        {
            Entity = entity;
            EssenceName = entity.RenderName;
            Id = entity.Id;

        }
        public string EssenceName { get; set; }
        public Entity Entity { get; }

        public uint Id { get; set; }

        public bool IsDead {
            get
            {
                return Entity.IsDead;
            }
        }

        public bool IsCannotBeDamaged
        {
            get
            {
                return CannotBeDamaged(Entity);
            }
        }

        public bool CannotBeDamaged(Entity entity)
        {
                var entityStat = entity.TryGetComponent<Stats>(out var exileStat) ? exileStat?.StatDictionary : null;
                var isExileCannotBeDamaged = entityStat.TryGetValue(ExileCore.Shared.Enums.GameStat.CannotBeDamaged, out var cannotBeDamaged) ? cannotBeDamaged == 1 : false;
                return isExileCannotBeDamaged;
        }
    }
}
