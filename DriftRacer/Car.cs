using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Vehicle;
using BEPUutilities;
using Fusion;
using Fusion.Graphics;
using SharpDX;
using Matrix = BEPUutilities.Matrix;
using Quaternion = BEPUutilities.Quaternion;
using Vector2 = BEPUutilities.Vector2;
using Vector3 = BEPUutilities.Vector3;

namespace DriftRacer
{
    public class CarConfig
    {
        [Category("Passport")]
        [Description("Maximum forward speed of the car")]
        public float ForwardSpeed { get; set; }

        [Category("Passport")]
        [Description("Maximum backward speed of the car")]
        public float BackwardSpeed { get; set; }

        [Category("Passport")]
        [Description("Maximum turn angle of the wheels")]
        public float MaximumTurnAngle { get; set; }

        [Category("Passport")]
        [Description("Turning speed of the wheels in radians per second")]
        public float TurnSpeed { get; set; }

        [Command]
        public void SuperCommand()
        {
            Log.LogInfo("Super Command!");
        }

        public CarConfig()
        {
            ForwardSpeed  = 100f;
            BackwardSpeed = -23f;
            MaximumTurnAngle = MathHelper.Pi / 6;
            TurnSpeed = MathHelper.Pi;
        }
    }

    class Car
    {
        [Config]
        public CarConfig config {get;set;}

        private bool wasGras = false;
        
        
        public readonly Vehicle Vehicle;
        public readonly float CarWidth = 25;
        public readonly float CarLength = 45;
        public readonly float CarHeight = 10;

        private Texture2D texture;

        public Car(Vector3 position, Space space, Texture2D carTex)
        {
            config = new CarConfig();
            this.texture = carTex;
            var bodies = new List<CompoundShapeEntry>
                {
                    new CompoundShapeEntry(new BoxShape(CarWidth, CarHeight, CarLength), new Vector3(0, 0, 0), 60),
                };
            var body = new CompoundBody(bodies, 61);
            body.CollisionInformation.LocalPosition = new Vector3(0, .5f, 0);
            body.Position = position; //At first, just keep it out of the way.

            // prevent car turn over
            Matrix3x3 m = body.LocalInertiaTensorInverse;
            m.M11 = 0;
            m.M33 = 0;
            body.LocalInertiaTensorInverse = m;
            Vehicle = new Vehicle(body);

            body.OrientationMatrix = new Matrix3x3(0, 0, 1, 
                                                   0, 0, 0, 
                                                   0, 0, 0);
            

            var localWheelRotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), MathHelper.PiOver2);

            //The wheel model used is not aligned initially with how a wheel would normally look, so rotate them.
            Matrix wheelGraphicRotation = Matrix.CreateFromAxisAngle(Vector3.Forward, MathHelper.PiOver2);
            Vehicle.AddWheel(new Wheel(
                new CylinderCastWheelShape(.375f, 0.2f, localWheelRotation, wheelGraphicRotation, false),
                new WheelSuspension(2000, 100f, Vector3.Down, 0.325f, new Vector3(-1.1f * 10f, -5f, 1.8f * 10f)),
                new WheelDrivingMotor(5f, 50000, 10000),
                new WheelBrake(5f, 2, .02f),
                new WheelSlidingFriction(4, 5)));
            Vehicle.AddWheel(new Wheel(
                new CylinderCastWheelShape(.375f, 0.2f, localWheelRotation, wheelGraphicRotation, false),
                new WheelSuspension(2000, 100f, Vector3.Down, 0.325f, new Vector3(-1.1f * 10f, -5f, -1.8f * 10f)),
                new WheelDrivingMotor(5f, 50000, 10000),
                new WheelBrake(5f, 2, .02f),
                new WheelSlidingFriction(4, 5)));
            Vehicle.AddWheel(new Wheel(
                new CylinderCastWheelShape(.375f, 0.2f, localWheelRotation, wheelGraphicRotation, false),
                new WheelSuspension(2000, 100f, Vector3.Down, 0.325f, new Vector3(1.1f * 10f, -5f, 1.8f * 10f)),
                new WheelDrivingMotor(5f, 50000, 10000),
                new WheelBrake(5f, 2, .02f),
                new WheelSlidingFriction(4, 5)));
            Vehicle.AddWheel(new Wheel(
                new CylinderCastWheelShape(.375f, 0.2f, localWheelRotation, wheelGraphicRotation, false),
                new WheelSuspension(2000, 100f, Vector3.Down, 0.325f, new Vector3(1.1f * 10f, -5f, -1.8f * 10f)),
                new WheelDrivingMotor(5f, 50000, 10000),
                new WheelBrake(5f, 2, .02f),
                new WheelSlidingFriction(4, 5)));


            foreach (Wheel wheel in Vehicle.Wheels)
            {
                //This is a cosmetic setting that makes it looks like the car doesn't have antilock brakes.
                wheel.Shape.FreezeWheelsWhileBraking = true;

                //By default, wheels use as many iterations as the space.  By lowering it,
                //performance can be improved at the cost of a little accuracy.
                //However, because the suspension and friction are not really rigid,
                //the lowered accuracy is not so much of a problem.
                wheel.Suspension.SolverSettings.MaximumIterationCount = 1;
                wheel.Brake.SolverSettings.MaximumIterationCount = 1;
                wheel.SlidingFriction.SolverSettings.MaximumIterationCount = 1;
                wheel.DrivingMotor.SolverSettings.MaximumIterationCount = 1;

                wheel.DrivingMotor.GripFriction = 1;
            }

            space.Add(Vehicle);
        }
        Color color;

        public Vector2 Update()
        {
            // dependent on surfece type, change wheel friction
            int i = 0;
            float totalWheelsOnGras = 0f;
            float totalWheelsOnSurf = 0f;
            bool allOnGras = true;
            foreach (var wheel in Vehicle.Wheels)
            {
                Vector2 wheelPos = new Vector2(wheel.SupportLocation.X, wheel.SupportLocation.Z);

                var ds = DriftRacer.Instance.GetService<DebugStrings>();

                //ds.Add(Color.Orange, "Wheel {0}:  x={1}, y={2}", i, wheelPos.X, wheelPos.Y);

                try
                {
                    switch (DriftRacer.Surfece.GetPixel((int) Math.Round(wheelPos.X), (int) Math.Round(wheelPos.Y)).R)
                    {
                            //grass
                        case 255:
                            //back wheels
                            wheel.DrivingMotor.GripFriction         = 1;
                            wheel.Brake.RollingFrictionCoefficient  = .2f;
                            totalWheelsOnGras                       += .1f;
                            break;
                            //roadside
                        case 179:
                        case 185:
                            wheel.DrivingMotor.GripFriction         = 2;
                            wheel.Brake.RollingFrictionCoefficient  = .08f;
                            allOnGras                               = false;
                            totalWheelsOnSurf                       += .05f;
                            break;
                            //good road
                        case 0:
                            wheel.DrivingMotor.GripFriction         = 5;
                            wheel.Brake.RollingFrictionCoefficient  = .02f;
                            allOnGras                               = false;
                            break;
                    }
                }
                catch (Exception e)
                {
                    Log.Error("error: x=" + (wheelPos.X) + " y=" + (wheelPos.Y), e);
                }
                i++;
            }

            if (allOnGras && allOnGras != wasGras) {
                Vehicle.Body.LinearVelocity = Vehicle.Body.LinearVelocity / 3;
                Vehicle.Body.LinearMomentum = Vehicle.Body.LinearMomentum / 2;
            }
            else {
                color = Color.White;
                
            }
            wasGras = allOnGras;
            return new Vector2(totalWheelsOnSurf,totalWheelsOnGras);
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Begin();
            var cos1 = (Vehicle.Body.Orientation.Y < 0)
                     ? -Vehicle.Body.Orientation.W
                     : Vehicle.Body.Orientation.W;
            sb.DrawSprite(texture, Vehicle.Body.Position.X, Vehicle.Body.Position.Z, CarLength, CarWidth, (float) ( Math.Acos(cos1) * 2 + Math.PI / 2 ), color);
            sb.End();
        }

        public void MoveToPosition(Vector2 position)
        {
            Vehicle.Body.Position = new Vector3(position.X, 30, position.Y);
            Vehicle.Body.OrientationMatrix = new Matrix3x3(0, 0, 1,
                                                   0, 0, 0,
                                                   0, 0, 0);
            Vehicle.Body.LinearVelocity = Vector3.Zero;
            Vehicle.Body.LinearMomentum = Vector3.Zero;
            Vehicle.Body.AngularVelocity = Vector3.Zero;
            Vehicle.Body.AngularMomentum = Vector3.Zero;

            foreach (var wheel in Vehicle.Wheels) {
                wheel.Brake.IsBraking = true;
            }
        }
    }
}
