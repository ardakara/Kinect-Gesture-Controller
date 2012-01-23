using System;
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
        private int _id;
        private int _x
        {
            set
            {
                this._x = value;
                this.fireBallModelChanged();
            }
            get
            {
                return this._x;
            }
        }
        private int _y
        {
            set
            {
                this._y = value;
                this.fireBallModelChanged();
            }
            get
            {
                return this._y;
            }
        }
        private int _r
        {
            set
            {
                this._r = value;
                this.fireBallModelChanged();
            }
            get
            {
                return this._r;
            }
        }
        private Boolean _isInHands;
        private Ellipse _elipse;
        private Brush _target_color;
        private List<BallModelListener> _listeners;
        public BallModel(int id, int x, int y, int r, Boolean isInHands)
        {
            _target_color = new SolidColorBrush(Colors.Red);
            _id = id;
            _x = x;
            _y = y;
            _isInHands = true;
            _r = r;
            _elipse = this.generateEllipse();

        }
        public void addBallModelListener(BallModelListener listener)
        {
            this._listeners.Add(listener);
        }
        public void fireBallModelChanged()
        {
            foreach (BallModelListener listener in this._listeners){
                listener.handleBallModelChanged(this);
            }
        }
        public void setPosition(int x, int y)
        {
            this._x = x;
            this._y = y;
        }            

        public int getX()
        {
            return this._x;
        }

        public int getY()
        {
            return this._y;
        }

        public void setRadius(int r)
        {
            this._r = r;
            this._elipse = this.generateEllipse();
        }

        public int getRadius()
        {
            return _r;
        }

        public Ellipse getEllipse()
        {
            return this._elipse;
        }
        private Ellipse generateEllipse()
        {
            var circle = new Ellipse();
            circle.Width = this._r * 2;
            circle.Height = this._r * 2;
            circle.Stroke = new SolidColorBrush(Colors.Black);
            circle.StrokeThickness = 1;
            circle.Fill = this._target_color;
            return circle;
        }

    }
}
