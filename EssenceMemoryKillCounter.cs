using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Vector2 = System.Numerics.Vector2;

namespace EssenceMemoryKillCounter
{
    public class EssenceMemoryKillCounter : BaseSettingsPlugin<Settings>
    {

        Dictionary<string, List<Essence>> essenceCounter = new Dictionary<string, List<Essence>>();
        Dictionary<string, List<Essence>> essenceKilled = new Dictionary<string, List<Essence>>();
        private bool _canTick = true;
        private Camera Camera => GameController.IngameState.Camera;
        private CachedValue<bool> _ingameUiCheckVisible;
        private IngameUIElements IngameUi => GameController.IngameState.IngameUi;

        public override bool Initialise()
        {

            _ingameUiCheckVisible = new TimeCache<bool>(() =>IngameUi.FullscreenPanels.Any(x => x.IsVisibleLocal) || IngameUi.LargePanels.Any(x => x.IsVisibleLocal), 250);
            return true;
        }

        public override void AreaChange(AreaInstance area)
        {
            if (_ingameUiCheckVisible?.Value == true || Camera == null || GameController.Area.CurrentArea.IsTown || GameController.Area.CurrentArea.IsHideout)
            {
                _canTick = false;
                return;
            }
            if (essenceCounter.Count > 0)
            {
                var toRemove = essenceCounter.Where(pair => pair.Key != area.Name)
                                         .Select(pair => pair.Key)
                                         .ToList();

                foreach (var key in toRemove)
                {
                    essenceKilled.Remove(key);
                }
            }

            if (essenceKilled.Count > 0)
            {
                var toRemove = essenceKilled.Where(pair => pair.Key != area.Name)
                                         .Select(pair => pair.Key)
                                         .ToList();

                foreach (var key in toRemove)
                {
                    essenceKilled.Remove(key);
                }
            }
            if (!essenceCounter.TryGetValue(area.Name, out var valueCounter))
            {
                essenceCounter.Add(area.Name, []);
            }
            if (!essenceKilled.TryGetValue(area.Name, out var valueKilled))
            {
                essenceKilled.Add(area.Name, []);
            }
        }

        public override Job Tick()
        {
            _canTick = true;
            if (_ingameUiCheckVisible?.Value == true ||
                Camera == null || GameController.Area.CurrentArea.IsTown || GameController.Area.CurrentArea.IsHideout)
            {
                _canTick = false;
                return null;
            }

            var areaName = GameController.Area.CurrentArea.Name;

            foreach (var validEntity in GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Monster]
                .Concat(GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Player]))
            {
                var essence = validEntity.GetHudComponent<Essence>();
                if (essence == null) continue;
                try
                {
                    if (essenceKilled[areaName].Where(ess=> ess.Id.Equals(essence.Id)).Count() == 0 && essence.IsCannotBeDamaged && !essence.IsDead)
                    {
                        
                        essenceCounter[areaName].Add(essence);
                    }
                    
                    if ((essenceKilled[areaName].Where(ess => ess.Id.Equals(essence.Id)).Count() == 0 && essence.IsDead))
                    {
                        essenceKilled[areaName].Add(essence);
                        essenceCounter[areaName].Remove(essence);
                    }
                }
                catch (Exception e)
                {
                    DebugWindow.LogError(e.Message);
                }
            }
            return null;
        }

        public override void Render()
        {
            if (!_canTick) return;
            var area = GameController.Area;
            var essenceText = $"Essence Killed: {essenceKilled[area.CurrentArea.Name].Count()}";
            if (Settings.ShowKilledEssenceExileName && essenceKilled.Count(se => se.Key.Equals(area.CurrentArea.Name)) > 0)
            {
                essenceText += $"\nExile killed:";
                    foreach (var exile in essenceKilled[area.CurrentArea.Name])
                    {
                       essenceText += $"\n * {exile.EssenceName}";
                    }
            }

                var textSize = Graphics.MeasureText(essenceText) * 45;
                var pos = new Vector2(Settings.PositionX.Value, Settings.PositionY.Value);
                var bgSize = Settings.BackgroundSize.Value;
                var box = Graphics.DrawText(essenceText, pos, Settings.TextColor);
                Graphics.DrawBox(pos - new Vector2(bgSize, bgSize), pos + new Vector2(box.X, box.Y) + new Vector2(bgSize, bgSize), Settings.BackgroundColor, 2);
            
        }

        public override void EntityAdded(Entity entity)
        {

            if (entity.Type != EntityType.Player && entity.Metadata.Contains("Exiles") && CannotBeDamaged(entity))
            {
                var essence = new Essence(entity);
                 entity.SetHudComponent(essence);
            }
        }

        public bool CannotBeDamaged(Entity entity)
        {
            var entityStat = entity.TryGetComponent<Stats>(out var exileStat) ? exileStat?.StatDictionary : null;
            var isExileCannotBeDamaged = entityStat.TryGetValue(GameStat.CannotBeDamaged, out var cannotBeDamaged);
            return isExileCannotBeDamaged;
        }
    }
}
