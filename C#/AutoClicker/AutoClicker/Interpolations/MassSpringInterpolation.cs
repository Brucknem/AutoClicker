﻿using System.Collections.Generic;
using System.Windows;

namespace AutoClicker
{
    public class MassSpringInterpolation : ICursorInterpolation
    {
        private static readonly double DELAY = 5000;
        private static readonly double DT = 1.0 / DELAY;
        private static readonly double STIFFNESS = 0.5;
        private static readonly double DAMPING = 1.2;

        protected List<MassPoint> Points { get; private set; } = new List<MassPoint>();
        protected List<Spring> springs = new List<Spring>();


        public MassSpringInterpolation(System.Drawing.Point end, double speed = 1) : base(end, speed)
        {
            Speed = speed;
            Initialize();
        }

        private void Initialize()
        {
            Vector direction = End - Cursor.Vector;
            //Vector midpoint = Cursor + direction * random.NextDouble();

            Points.Add(new MassPoint(End, new Vector(0, 0), -1, 0, Speed));
            //Points.Add(new MassPoint(midpoint, RandomNormal(End, Cursor, 50), random.NextDouble() * 2 + 1, DAMPING / 2));// random.NextDouble() * 0.8 + 0.6));
            Points.Add(new MassPoint(Cursor.Vector, RandomVector(50), random.NextDouble() * 2 + 1, DAMPING, Speed));

            springs.Add(new Spring(this, 0, 1, STIFFNESS));
            //springs.Add(new Spring(this, 1, 2, STIFFNESS, TargetRadius));
        }

        public override void Interpolate()
        {
            double length = (End - Cursor.Vector).Length;

            while (!Finished)
            {
                foreach (MassPoint p in Points)
                {
                    p.ClearForce();
                }

                foreach (Spring s in springs)
                {
                    s.ComputeElasticForce();
                }

                foreach (MassPoint p in Points)
                {
                    p.IntegratePosition();
                    p.IntegrateVelocity();
                }

                Cursor.Vector = Points[Points.Count - 1].Position;
                //Thread.Sleep((int)Math.Max(1, DELAY / 50));
            }
        }

        protected class Spring
        {
            public int Start { get; private set; }
            public int End { get; private set; }
            public double Stiffness { get; private set; }
            public double InitialLength { get; private set; }
            private MassSpringInterpolation outer;

            public Spring(MassSpringInterpolation outer, int start, int end, double stiffness)
            {
                this.outer = outer;
                Start = start;
                End = end;
                Stiffness = stiffness;
                InitialLength = outer.Points[Start].Distance(outer.Points[End]);
            }

            public void ComputeElasticForce()
            {
                double length = outer.Points[Start].Distance(outer.Points[End]);

                Vector force = -Stiffness * length * (outer.Points[End].Position - outer.Points[Start].Position) / length;

                outer.Points[Start].Force += -force;
                outer.Points[End].Force += force;

                int randomLength = 100;
                if (length < 0.2 * InitialLength)
                {
                    randomLength = 50;
                }
                else if (length < 50)
                {
                    randomLength = 5;
                }
                outer.Points[End].RandomVelocity += RandomNormal(outer.Points[Start].Position, outer.Points[End].Position, randomLength);
                outer.Points[Start].RandomVelocity += RandomNormal(outer.Points[Start].Position, outer.Points[End].Position, randomLength);
            }
        }

        protected class MassPoint
        {
            public Vector Position { get; private set; }
            public Vector Velocity { get; private set; }
            public Vector RandomVelocity { get; set; }
            public Vector Force { get; set; }
            public double Mass { get; private set; }
            public double Damping { get; private set; }
            public double Speed { get; private set; }

            public MassPoint(Vector position, Vector velocity, double mass, double damping, double speed)
            {
                Position = position;
                Velocity = velocity;
                Force = new Vector(0, 0);
                Mass = mass;
                Damping = damping;
                Speed = speed;
            }

            public void IntegratePosition()
            {
                if (Mass > 0)
                {
                    Position += DT * Speed * Velocity;
                }
            }

            public void IntegrateVelocity()
            {
                Velocity += DT * Speed * ((Force - Damping * Velocity) / Mass + RandomVelocity);
            }

            public double Distance(MassPoint other)
            {
                return (Position - other.Position).Length;
            }

            public void ClearForce()
            {
                Force = RandomVelocity = new Vector(0, 0);
            }
        }
    }
}