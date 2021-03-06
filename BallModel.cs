﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Collections;

namespace SkeletalTracking
{
    class BallModel
    {

        public static double MAX_V = 8;
        public static double MIN_V = 2;
        public static double MAX_R = 40;


        public static double MIN_R = 10;
        public static double MIN_VALUE = 1;
        public static double MAX_VALUE = 10;


        private int _id;
        public int ID
        {
            get
            {
                return _id;
            }
        }
        private Boolean _exists;
        public Boolean Exists
        {
            set
            {
                _exists = value;
            }

            get
            {
                return _exists;
            }

        }
        private double _angle;
        public double Angle
        {
            get
            {
                return _angle;
            }
            set
            {
                _angle = value;
            }
        }
        private double _value;
        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                _r = convertValueToRadius(value);
                this.fireBallModelChanged();
            }
        }
        private double _x;
        public double X
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
                this.fireBallModelChanged();
            }
        }
        private double _y;
        public double Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
                this.fireBallModelChanged();
            }

        }
        private double _r;
        public double R
        {

            get
            {
                return this._r;
            }
            set
            {
                this._r = value;
                this._value = convertRadiusToValue(value);
                this.fireBallModelChanged();
            }
        }
        private double _velocity;
        public double Velocity
        {
            get
            {
                return _velocity;
            }
            set
            {
                _velocity = value;
            }
        }
        private Brush _targetColor;
        public Brush TargetColor
        {
            get
            {
                return _targetColor;
            }
        }

        private static double convertRadiusToValue(double radius)
        {
            double percentMax = (radius - MIN_R) / (MAX_R - MIN_R);
            double value = MIN_VALUE + percentMax * (MAX_VALUE - MIN_VALUE);
            return value;
        }
        private static double convertValueToRadius(double value)
        {
            double percentMax = (value - MIN_VALUE) / (MAX_VALUE - MIN_VALUE);
            double radius = MIN_R + percentMax * (MAX_R - MIN_R);
            return radius;
        }
        private List<BallModelListener> _listeners = new List<BallModelListener>();
        public BallModel(int id, double x, double y, double r, double angle, double velocity)
        {
            _targetColor = new SolidColorBrush(Colors.Red);

            _id = id;

            this.Y = y;
            this.X = x;
            this.R = r;

            this.Velocity = velocity;
            this.Angle = angle;
        }
        public void addBallModelListener(BallModelListener listener)
        {
            this._listeners.Add(listener);
        }
        public void removeBallModelListener(BallModelListener listener)
        {
            this._listeners.Remove(listener);
        }
        public void fireBallModelChanged()
        {
            foreach (BallModelListener listener in this._listeners)
            {
                listener.handleBallModelChanged(this);
            }
        }
        public void setPosition(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }


    }
}
