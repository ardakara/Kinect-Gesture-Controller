using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkeletalTracking
{

    class BallModels
    {
        private MainWindow _window;
        private Dictionary<int, BallModel> _balls = new Dictionary<int, BallModel>();
        private List<BallModelsListener> _listeners = new List<BallModelsListener>();
        private int _nextId = 0;

        public BallModels(MainWindow window)
        {
            this._window = window;
            this.addBallModelsListener(window);
        }

        public void clear()
        {
            
            foreach (BallModel ball in this._balls.Values)
            {
                this.fireBallModelRemoved(ball);
            }
            this._balls.Clear();
        }

        public int generateNextId()
        {
            int id = this._nextId;
            this._nextId++;
            return id;

        }

        public void addBallModelsListener(BallModelsListener listener)
        {
            this._listeners.Add(listener);
        }
        public void removeBallModelsListener(BallModelsListener listener)
        {
            this._listeners.Remove(listener);
        }

        public void fireBallModelAdded(BallModel ball)
        {
            foreach (BallModelsListener listener in this._listeners)
            {
                listener.handleBallAdded(ball);
            }

        }

        public void fireBallModelRemoved(BallModel ball)
        {
            foreach (BallModelsListener listener in this._listeners)
            {
                listener.handleBallRemoved(ball);
            }

        }

        public BallModel createBall(double x, double y, double r, double angle, double velocity)
        {
            int id = this.generateNextId();
            BallModel ball = new BallModel(id, x, y, r, angle, velocity);
            ball.addBallModelListener(this._window);
            this._balls.Add(id, ball);
            this.fireBallModelAdded(ball);
            return ball;
        }
        public BallModel createBall(double x, double y, double r)
        {
            return this.createBall(x, y, r, 0, 0);
        }

        public Boolean removeBall(int id)
        {
            if (this._balls.ContainsKey(id))
            {
                BallModel ball = this._balls[id];
                if (ball != null)
                {
                    ball.removeBallModelListener(this._window);
                }

                Boolean isBallRemoved = this._balls.Remove(id);
                if (isBallRemoved)
                {
                    this.fireBallModelRemoved(ball); ;
                }

                return isBallRemoved;
            }

            return true;
        }

        public Boolean removeBall(BallModel ball)
        {
            int id = ball.ID;
            return this.removeBall(id);
        }


        public BallModel getBall(int id)
        {
            return this._balls[id];
        }
        public void animateBalls()
        {
            foreach (BallModel ball in this._balls.Values)
            {
                if (ball.Velocity > 0)
                {
                    double dx = Math.Cos(ball.Angle) * ball.Velocity;
                    double dy = Math.Sin(ball.Angle) * ball.Velocity;
                    double x = ball.X;
                    double y = ball.Y;
                    double newX = x + dx;
                    double newY = y + dy;
                    ball.setPosition(newX, newY);
                }
            }
        }

        public void removeOutOfBoundsBalls()
        {
            List<BallModel> removed = new List<BallModel>();
            foreach (BallModel ball in this._balls.Values)
            {
                if (ball.Velocity > 0 && !isInBounds(ball))
                {
                    removed.Add(ball);

                }
            }
            foreach (BallModel ball in removed)
            {
                this.removeBall(ball);
            }

        }

        public void removeIntersectingBalls(Dictionary<int, Target> targets)
        {
            List<BallModel> removed = new List<BallModel>();
            foreach (BallModel ball in this._balls.Values)
            {
                foreach (Target target in targets.Values)
                {
                    if (ball.Velocity > 0 && ballIntersectsTarget(ball, target))
                    {
                        removed.Add(ball);

                        break;
                    }
                }
            }
            foreach (BallModel ball in removed)
            {
                this.removeBall(ball);
            }

        }
        private static Boolean ballIntersectsTarget(BallModel ball, Target target)
        {
            double ballX = ball.X;
            double ballY = ball.Y;
            double ballR = ball.R;
            double targetX = target.getXPosition();
            double targetY = target.getYPosition();
            double targetR = target.getTargetRadius();
            return true;
        }
        /**
         * Determines whether the ball is in bounds of canvas
        */
        private Boolean isInBounds(BallModel ball)
        {
            double width = this._window.Width;
            double height = this._window.Height;

            if (ball.Velocity > 0)
            {
                double x = ball.X;
                double y = ball.Y;
                if (x < 0 || x > width)
                {
                    return false;
                }
                if (y < 0 || y > height)
                {
                    return false;
                }
            }
            return true;
        }




    }
}
