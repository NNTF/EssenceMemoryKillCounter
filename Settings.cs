using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace EssenceMemoryKillCounter
{
    public class Settings : ISettings
    {
        public ToggleNode Enable { get; set; } = new ToggleNode(false);
        public RangeNode<int> PositionX { get; set; } = new(100, 0, 5000);
        public RangeNode<int> PositionY { get; set; } = new(100, 0, 5000);
        public RangeNode<int> BackgroundSize { get; set; } = new(6, 0, 15);
        public ColorNode TextColor { get; set; } = new ColorNode(SharpDX.Color.Black);
        public ColorNode BackgroundColor { get; set; } = new ColorNode(SharpDX.Color.White);

        public ToggleNode ShowKilledEssenceExileName { get; set; } = new ToggleNode(false);

    }
}