using SpacePirates.Console.UI.Controls;
using SpacePirates.Console.UI.Styles;
using SpacePirates.Console.Core.Models.Movement;
using SpacePirates.Console.Core.Interfaces;

namespace SpacePirates.Console.UI.Views
{
    public class GameView : BaseView
    {
        public PanelView? Panel { get; set; }
        public MapView? Map { get; set; }
        public CommandLineView? CommandLine { get; set; }
        public ShipTrail? ShipTrail { get; set; }

        public GameView(GameControls controls, BaseStyleProvider styleProvider)
        {
            Controls = controls;
            StyleProvider = styleProvider;
        }

        public override void Render()
        {
            Panel?.Render();
            Map?.Render();
            CommandLine?.Render();
        }

        public void Render(IBufferWriter buffer)
        {
            Panel?.Render(buffer);
            Map?.Render(buffer);
            CommandLine?.Render(buffer);
        }

        public override void HandleInput(ConsoleKeyInfo key)
        {
            // Always forward hjkl to the map view for cursor movement
            if ("hjkl".Contains(char.ToLower(key.KeyChar)) && Map != null)
            {
                Map.HandleInput(key);
            }
            else
            {
                Controls.HandleInput(key, this);
            }
        }

        public void SetMapView(MapView mapView)
        {
            Map = mapView;
        }

        public void SetShipTrail(ShipTrail? trail)
        {
            ShipTrail = trail;
        }
    }
} 