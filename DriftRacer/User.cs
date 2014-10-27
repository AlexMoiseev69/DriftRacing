using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics.Vehicle;
using BEPUutilities;
using Fusion;
using Fusion.Graphics;
using Fusion.Input;

namespace DriftRacer
{
    class User
    {
        private readonly Car car;
        private String name;
        private Gamepad gp;

        public User(string name, Car car, Gamepad gp)
        {
            this.name = name;
            this.car = car;
            this.gp = gp;
        }

        public Boolean Update(GameTime gameTime)
        {
            if (gp.IsConnected)
            {
                Vector2 v = car.Update();
                gp.SetVibration(v.X, v.Y);
                updateCarControllers(gameTime.ElapsedSec);

                return true;
            }
            return false;
        }

        public void Draw(SpriteBatch sb)
        {
            car.Draw(sb);
        }

        private void updateCarControllers(float delta)
        {
            if (gp.IsKeyPressed(GamepadButtons.A))
            {
                //Drive
                car.Vehicle.Wheels[0].DrivingMotor.TargetSpeed = car.ForwardSpeed;
                car.Vehicle.Wheels[2].DrivingMotor.TargetSpeed = car.ForwardSpeed;
            }
            else if (gp.IsKeyPressed(GamepadButtons.B))
            {
                //Reverse
                car.Vehicle.Wheels[0].DrivingMotor.TargetSpeed = car.BackwardSpeed;
                car.Vehicle.Wheels[2].DrivingMotor.TargetSpeed = car.BackwardSpeed;
            }
            else
            {
                //Idle
                car.Vehicle.Wheels[0].DrivingMotor.TargetSpeed = 0;
                car.Vehicle.Wheels[2].DrivingMotor.TargetSpeed = 0;
            }
            if (gp.IsKeyPressed(GamepadButtons.RightShoulder))
            {
                //Brake
                foreach (Wheel wheel in car.Vehicle.Wheels)
                {
                    wheel.Brake.IsBraking = true;
                }
            }
            else
            {
                //Release brake
                foreach (Wheel wheel in car.Vehicle.Wheels)
                {
                    wheel.Brake.IsBraking = false;
                }
            }
            //Use smooth steering; while held down, move towards maximum.
            //When not pressing any buttons, smoothly return to facing forward.
            float angle;
            bool steered = false;
            if (gp.IsKeyPressed(GamepadButtons.DPadLeft))
            {
                steered = true;
                angle = Math.Max(car.Vehicle.Wheels[1].Shape.SteeringAngle - car.TurnSpeed * delta, -car.MaximumTurnAngle);
                car.Vehicle.Wheels[1].Shape.SteeringAngle = angle;
                car.Vehicle.Wheels[3].Shape.SteeringAngle = angle;
            }
            if (gp.IsKeyPressed(GamepadButtons.DPadRight))
            {
                steered = true;
                angle = Math.Min(car.Vehicle.Wheels[1].Shape.SteeringAngle + car.TurnSpeed * delta, car.MaximumTurnAngle);
                car.Vehicle.Wheels[1].Shape.SteeringAngle = angle;
                car.Vehicle.Wheels[3].Shape.SteeringAngle = angle;
            }
            if (!steered)
            {
                //Neither key was pressed, so de-steer.
                if (car.Vehicle.Wheels[1].Shape.SteeringAngle > 0)
                {
                    angle = Math.Max(car.Vehicle.Wheels[1].Shape.SteeringAngle - car.TurnSpeed * delta, 0);
                    car.Vehicle.Wheels[1].Shape.SteeringAngle = angle;
                    car.Vehicle.Wheels[3].Shape.SteeringAngle = angle;
                }
                else
                {
                    angle = Math.Min(car.Vehicle.Wheels[1].Shape.SteeringAngle + car.TurnSpeed * delta, 0);
                    car.Vehicle.Wheels[1].Shape.SteeringAngle = angle;
                    car.Vehicle.Wheels[3].Shape.SteeringAngle = angle;
                }
            }
        }
    }
}
