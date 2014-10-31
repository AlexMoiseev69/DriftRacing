using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Graphics;
using SharpDX;

namespace DriftRacer.particle
{
    class DustParticle : Particle
    {
        public DustParticle(List<Particle> particlePool, Texture2D texture) : base(particlePool, texture)
        {
        }

        public override void SetDefaults()
        {
            lifetime = 3f;
            scale = 0.1f;
            maxScale = 1.5f;
            color = Color.White;

            rotation = new Random().NextFloat(0, (float)(Math.PI * 2));
        }
    }
}
