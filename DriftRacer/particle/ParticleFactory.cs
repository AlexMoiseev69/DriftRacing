using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Fusion;
using Fusion.Graphics;

namespace DriftRacer.particle
{
    public class ParticleFactory
    {
        public enum ParticleType
        {
            Dust, Grass, RoadDust
        }
        private List<Particle> dustParticlePool;
        private List<Particle> grassParticlePool;
        private List<Particle> roadDustParticlePool;

        private Texture2D grassTexture;
        private Texture2D dustTexture;



        private static ParticleFactory instance;

        private ParticleFactory()
        {
            dustParticlePool = new List<Particle>();
            grassParticlePool = new List<Particle>();
            roadDustParticlePool = new List<Particle>();

            grassTexture = Game.Instance.Content.Load<Texture2D>("images/grassParticle.png");
            dustTexture = Game.Instance.Content.Load<Texture2D>("images/dustParticle.png");
        }

        public static ParticleFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ParticleFactory();
                }
                return instance;
            }
        }

        public Particle GetDustParticle()
        {
            if (dustParticlePool.Count > 0)
            {
                Particle particle = dustParticlePool.First();
                particle.SetDefaults();
                dustParticlePool.Remove(particle);
                return particle;
            }
            else
            {
                return new DustParticle(dustParticlePool, dustTexture);
            }
        }

        public Particle GetGrassParticle()
        {
            if (grassParticlePool.Count > 0)
            {
                Particle particle = grassParticlePool.First();
                particle.SetDefaults();
                grassParticlePool.Remove(particle);
                return particle;
            }
            else
            {
                return new GrassParticle(grassParticlePool, grassTexture);
            }
        }

        public Particle GetRoadDustParticle()
        {
            if (roadDustParticlePool.Count > 0)
            {
                Particle particle = roadDustParticlePool.First();
                particle.SetDefaults();
                roadDustParticlePool.Remove(particle);
                return particle;
            }
            else
            {
                return new RoadDustParticle(roadDustParticlePool, dustTexture);
            }
        }
    }
}
