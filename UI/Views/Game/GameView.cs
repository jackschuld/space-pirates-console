using SpacePirates.Console.UI.Controls;
using SpacePirates.Console.UI.Styles;
using SpacePirates.Console.Core.Models.Movement;
using SpacePirates.Console.Core.Interfaces;
using SpacePirates.Console.UI.Views.Map;

namespace SpacePirates.Console.UI.Views
{
    public class GameView : BaseView
    {
        public PanelView? Panel { get; set; }
        public MapView? Map { get; set; }
        public CommandLineView? CommandLine { get; set; }
        public ShipTrail? ShipTrail { get; set; }

        public GameView(GameControls controls, BaseStyle styleProvider)
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
            if (Map is SpacePirates.Console.UI.Views.SolarSystemMapView ssmv)
            {
                ssmv.SetShipTrail(trail);
            }
        }

        public void SwitchToSolarSystem(SpacePirates.API.Models.SolarSystem system, ShipTrail? trail = null)
        {
            int gameViewX = SpacePirates.Console.Core.Models.State.ConsoleConfig.StatusAreaWidth;
            var bounds = (gameViewX, 0, SpacePirates.Console.Core.Models.State.ConsoleConfig.GAME_AREA_WIDTH, SpacePirates.Console.Core.Models.State.ConsoleConfig.MainAreaHeight);
            var ssmv = new SpacePirates.Console.UI.Views.SolarSystemMapView(system, bounds);
            ssmv.ParentGameView = this;
            SetMapView(ssmv);
            Controls = new SpacePirates.Console.UI.Controls.SolarSystemControls();
            if (trail != null) ssmv.SetShipTrail(trail);
        }

        public void SwitchToGalaxy(SpacePirates.API.Models.Galaxy galaxy, ShipTrail? trail = null)
        {
            int gameViewX = SpacePirates.Console.Core.Models.State.ConsoleConfig.StatusAreaWidth;
            var bounds = (gameViewX, 0, SpacePirates.Console.Core.Models.State.ConsoleConfig.GAME_AREA_WIDTH, SpacePirates.Console.Core.Models.State.ConsoleConfig.MainAreaHeight);
            var gmv = new SpacePirates.Console.UI.Views.Map.GalaxyMapView(galaxy, bounds);
            SetMapView(gmv);
            Controls = new SpacePirates.Console.UI.Controls.GalaxyControls();
        }
    }
} 