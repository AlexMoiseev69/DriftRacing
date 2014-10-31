using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.Graphics;
using SharpDX;
using Vector2 = BEPUutilities.Vector2;

namespace DriftRacer.particle
{
    public abstract class Particle
    {
        private List<Particle> particlePool;
        protected float lifetime;
        protected float scale;
        protected float rotation;
        protected float maxScale;
        protected Color color;
        public Vector2 position;
        private Texture2D texture;


        public Particle(List<Particle> particlePool, Texture2D texture)
        {
            this.particlePool = particlePool;
            this.texture = texture;
            SetDefaults();
        }

        public abstract void SetDefaults();

        public bool Update(float delta)
        {
            color.A = (byte) (lifetime / 3f * 255);
            if (scale < maxScale)
            {
                scale += delta;
            }
            if ((lifetime -= delta) < 0)
            {
                particlePool.Add(this);
                return true;
            }
            return false;
        }

        public float GetScale()
        {
            return scale;
        }

        public Color GetColor()
        {
            return color;
        }

        public float GetRotation()
        {
            return rotation;
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Begin();
            sb.DrawSprite(texture,
                    position.X,
                    position.Y,
                    texture.Width * scale,
                    texture.Height * scale,
                    rotation,
                    color);
            sb.End();
        }
    }
}
