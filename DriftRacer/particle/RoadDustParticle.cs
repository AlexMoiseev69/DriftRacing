using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Graphics;
using SharpDX;

namespace DriftRacer.particle
{
    class RoadDustParticle : Particle
    {
        public RoadDustParticle(List<Particle> particlePool, Texture2D texture) : base(particlePool, texture)
        {
        }

        public override void SetDefaults()
        {
            lifetime = 2f;
            scale = 0.1f;
            maxScale = 0.5f;
            color = Color.Black;

            rotation = new Random().NextFloat(0, (float)(Math.PI * 2));
        }
    }
}
