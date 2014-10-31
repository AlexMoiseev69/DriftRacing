using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUutilities;
using Fusion;

namespace DriftRacer
{

    public class TrackConfig
    {
        public Track[] Tracks { get; set; }

        public TrackConfig()
        {
            Tracks = new Track[2];

            Tracks[0] = new Track()
            {

                TrackName = "SuperTrack",
                TrackTexture = "images/route_1.png",
                TrackSurfaceTexture = "\\Content\\images\\route_1_mh.png",

                CarsStartPositions = new Vector2[4]{
                    new Vector2(525, 40),
                    new Vector2(500, 80),
                    new Vector2(425, 40),
                    new Vector2(400, 80),
                },
            };

            Tracks[1] = new Track()
            {
                TrackName = "SuperTrack",
                TrackTexture = "images/route_2.png",
                TrackSurfaceTexture = "\\Content\\images\\route_2_mh.png",

                CarsStartPositions = new Vector2[4]{
                    new Vector2(655, 52),
                    new Vector2(573, 95),
                    new Vector2(495, 52),
                    new Vector2(423, 95),
                },
            };
        }
    }

    public class Track
    {
        public String TrackName { get; set; }

        public String TrackTexture { get; set; }

        public String TrackSurfaceTexture { get; set; }

        public Vector2[] CarsStartPositions { get; set; }
    }

    public class ConfigService : GameService
    {

        [Config]
        public TrackConfig TrackConfig { get; set; }

        public ConfigService(Game game) : base(game)
        {
            TrackConfig = new TrackConfig();
        }
    }
}
