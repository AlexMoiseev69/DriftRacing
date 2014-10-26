using System;
using System.Collections.Generic;
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

namespace DriftRacer
{
    class Car
    {
        public readonly Vehicle Vehicle;

        public Car(Vector3 position, Space space)
        {
            var vWidth = 2.5f * 10f;
            var vHeight = 4.5f * 10f;
            float yHeight = 10f;
            var bodies = new List<CompoundShapeEntry>
                {
                    new CompoundShapeEntry(new BoxShape(vWidth, yHeight, vHeight), new Vector3(0, 0, 0), 60),
                    //new CompoundShapeEntry(new BoxShape(2.5f, .3f, 2f), new Vector3(0, .75f / 2 + .3f / 2, .5f), 1)
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

            var localWheelRotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), MathHelper.PiOver2);

            //The wheel model used is not aligned initially with how a wheel would normally look, so rotate them.
            Matrix wheelGraphicRotation = Matrix.CreateFromAxisAngle(Vector3.Forward, MathHelper.PiOver2);
            Vehicle.AddWheel(new Wheel(
                new CylinderCastWheelShape(.375f, 0.2f, localWheelRotation, wheelGraphicRotation, false),
                new WheelSuspension(2000, 100f, Vector3.Down, 0.325f, new Vector3(-1.1f * 10f, -5f, 1.8f * 10f)),
                new WheelDrivingMotor(2.5f, 30000, 10000),
                new WheelBrake(1.5f, 2, .02f),
                new WheelSlidingFriction(4, 5)));
            Vehicle.AddWheel(new Wheel(
                new CylinderCastWheelShape(.375f, 0.2f, localWheelRotation, wheelGraphicRotation, false),
                new WheelSuspension(2000, 100f, Vector3.Down, 0.325f, new Vector3(-1.1f * 10f, -5f, -1.8f * 10f)),
                new WheelDrivingMotor(2.5f, 30000, 10000),
                new WheelBrake(1.5f, 2, .02f),
                new WheelSlidingFriction(4, 5)));
            Vehicle.AddWheel(new Wheel(
                new CylinderCastWheelShape(.375f, 0.2f, localWheelRotation, wheelGraphicRotation, false),
                new WheelSuspension(2000, 100f, Vector3.Down, 0.325f, new Vector3(1.1f * 10f, -5f, 1.8f * 10f)),
                new WheelDrivingMotor(2.5f, 30000, 10000),
                new WheelBrake(1.5f, 2, .02f),
                new WheelSlidingFriction(4, 5)));
            Vehicle.AddWheel(new Wheel(
                new CylinderCastWheelShape(.375f, 0.2f, localWheelRotation, wheelGraphicRotation, false),
                new WheelSuspension(2000, 100f, Vector3.Down, 0.325f, new Vector3(1.1f * 10f, -5f, -1.8f * 10f)),
                new WheelDrivingMotor(2.5f, 30000, 10000),
                new WheelBrake(1.5f, 2, .02f),
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

        public Vector2[] GetWheelsPosition()
        {
            Vector2[] vector2 = new Vector2[4];
            //Vehicle.Wheels[0].SupportLocation.X;
            int i = 0;
            foreach (var wheel in Vehicle.Wheels)
            {
                vector2[i] = new Vector2(wheel.SupportLocation.X, wheel.SupportLocation.Z);
            }
            return vector2;
        }

        public void Update()
        {
            foreach (var wheel in Vehicle.Wheels)
            {
                Vector2 wheelPos = new Vector2(wheel.SupportLocation.X, wheel.SupportLocation.Z);
                try
                {
                    switch (DriftRacer.Surfece.GetPixel((int)Math.Round(wheelPos.X), (int)Math.Round(wheelPos.Y)).R)
                    {
                            //grass
                        case 255:
                            wheel.DrivingMotor.GripFriction = 1;
                            break;
                            //roadside
                        case 160:
                            wheel.DrivingMotor.GripFriction = 2;
                            break;
                            //good road
                        case 0:
                            wheel.DrivingMotor.GripFriction = 5;
                            break;
                    }
                }
                catch (Exception e)
                {
                    Log.Error("error: x=" + (wheelPos.X) + " y=" + (wheelPos.Y), e);
                }
            }
        }
    }
}
