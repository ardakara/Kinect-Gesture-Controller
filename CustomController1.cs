using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using Microsoft.Research.Kinect.Nui;
using Coding4Fun.Kinect.Wpf;


namespace SkeletalTracking
{
    class CustomController1 : SkeletonController
    {
        private static int BALL_LOC_HIST_SIZE = 40;
        private static double R_GROWN_PER_FRAME = 0.3;
        private BallModels _balls;
        private BallModel curBall = null;
        private Dictionary<int, Target> validTargets = new Dictionary<int, Target>();
        private List<KeyValuePair<double, double>> _pastBallLocations = new List<KeyValuePair<double, double>>();
        public CustomController1(MainWindow win)
            : base(win)
        {
            _balls = new BallModels(this.window);
        }

        public void removeAllBalls()
        {
            this._balls.clear();
        }

        public override void processSkeletonFrame(SkeletonData skeleton, Dictionary<int, Target> targets)
        {
            // x,y launch angle, 
            if (areWristsTogether(skeleton) &&
                isCloseToBody(skeleton))            // charging
            {
                // here is where we should put a non-moving ball at X,Y
                double[] ballCoords = getJointForBall(skeleton);

                double ballX = ballCoords[0];
                double ballY = ballCoords[1];
                double ballR = BallModel.MIN_R;
                if (curBall == null)
                {

                    ballX -= ballR;
                    ballY -= ballR;
                    curBall = _balls.createBall(ballX, ballY, ballR);
                }
                else
                {
                    ballR = computeGrownRadius(curBall.R);
                    ballX -= ballR;
                    ballY -= ballR;
                    curBall.setPosition(ballX, ballY);
                    curBall.R = ballR;
                }
            }
            else if (areArmsStraight(skeleton) &&
                areWristsTogether(skeleton) &&
                !isCloseToBody(skeleton))           // launching
            {
                // get angle

                double angle = computeLaunchAngle(skeleton);
                double velocity = BallModel.MAX_V;
                if (curBall != null)
                {
                    curBall.Velocity = velocity;
                    curBall.Angle = angle;
                    curBall = null;
                    _pastBallLocations.Clear();
                }
            }
            else if (areWristsTogether(skeleton))   // moving around
            {
                if (curBall != null)
                {
                    // here is where we should put a non-moving ball at X,Y
                    double[] ballCoords = getJointForBall(skeleton);
                    double ballR = curBall.R;
                    double ballX = ballCoords[0] - ballR;
                    double ballY = ballCoords[1] - ballR;

                    curBall.setPosition(ballX, ballY);
                }
            }
            else                                    // no ball / cancel
            {
                // if there is a curball cancel it
                if (curBall != null)
                {
                    _balls.removeBall(curBall);
                    curBall = null;
                    _pastBallLocations.Clear();
                }
                // if no ball, keep having no ball
            }
            if (curBall != null)
            {
                this.addPastBallLocation(curBall.X, curBall.Y);
            }


            _balls.animateBalls();
            _balls.removeOutOfBoundsBalls();
            _balls.removeIntersectingBalls(this.validTargets, new ProcessTargetIntersectedDelegate(handleTargetIntersected));
     }

        private void addPastBallLocation(double x, double y)
        {
            KeyValuePair<double, double> loc = new KeyValuePair<double, double>(x, y);
            this._pastBallLocations.Insert(0, loc);
            if (this._pastBallLocations.Count > BALL_LOC_HIST_SIZE)
            {
                this._pastBallLocations.RemoveRange(BALL_LOC_HIST_SIZE, this._pastBallLocations.Count - BALL_LOC_HIST_SIZE);
            }
        }

        private double[] computeBallDirection()
        {
            double[] direction = new double[4];
            if (this._pastBallLocations.Count >= 2)
            {
                KeyValuePair<double, double> start = this._pastBallLocations.ElementAt<KeyValuePair<double, double>>(this._pastBallLocations.Count - 1);
                KeyValuePair<double, double> finish = this._pastBallLocations.ElementAt<KeyValuePair<double, double>>(0);
                direction[0] = start.Key;
                direction[1] = start.Value;
                direction[2] = finish.Key;
                direction[3] = finish.Value;
            }
            return direction;

        }

        private void handleTargetIntersected(Target t, double value)
        {
            int currentValue = Convert.ToInt16(t.getTargetText());
            currentValue += (int)value;

            if (currentValue >= 100)
            {
                currentValue = 100;
                t.setTargetSelected();
            }
            t.setTargetText(currentValue.ToString());
        }
        private double computeGrownRadius(double currentR)
        {
            double grownR = currentR + R_GROWN_PER_FRAME;
            if (grownR >= BallModel.MAX_R)
            {
                grownR = BallModel.MAX_R;
            }
            return grownR;
        }

        public override void controllerActivated(Dictionary<int, Target> targets)
        {
            targets[1].setTargetPosition(0, 0);
            targets[1].setTargetText("0");
            targets[1].setTargetUnselected();
            targets[1].setFontSize(40);

            targets[2].setTargetPosition(524, 0);
            targets[2].setTargetText("0");
            targets[2].setTargetUnselected();
            targets[2].setFontSize(40);

            targets[3].setTargetPosition(0, 342);
            targets[3].setTargetText("0");
            targets[3].setTargetUnselected();
            targets[3].setFontSize(40);

            targets[4].setTargetPosition(524, 342);
            targets[4].setTargetText("0");
            targets[4].setTargetUnselected();
            targets[4].setFontSize(40);

            targets[5].hideTarget();
            this.validTargets.Clear();
            this.validTargets.Add(1, targets[1]);
            this.validTargets.Add(2, targets[2]);
            this.validTargets.Add(3, targets[3]);
            this.validTargets.Add(4, targets[4]);
        }

        private bool areWristsTogether(SkeletonData skeleton)
        {
            Joint leftWrist = skeleton.Joints[JointID.WristLeft].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);
            Joint rightWrist = skeleton.Joints[JointID.WristRight].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);

            Joint leftHand = skeleton.Joints[JointID.HandLeft].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);
            Joint rightHand = skeleton.Joints[JointID.HandRight].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);

            double deltaX = Math.Min(Math.Abs(leftWrist.Position.X - rightWrist.Position.X), Math.Abs(leftHand.Position.X - rightHand.Position.X));
            double deltaY = Math.Min(Math.Abs(leftWrist.Position.Y - rightWrist.Position.Y), Math.Abs(leftHand.Position.Y - rightHand.Position.Y));
            double deltaZ = Math.Min(Math.Abs(leftWrist.Position.Z - rightWrist.Position.Z), Math.Abs(leftHand.Position.Z - rightHand.Position.Z));

            const int maximumWristDistance = 50;
            if (deltaX < maximumWristDistance &&
                deltaY < maximumWristDistance &&
                deltaZ < maximumWristDistance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool areArmsStraight(SkeletonData skeleton)
        {
            Joint centerShoulder = skeleton.Joints[JointID.ShoulderCenter].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);

            Joint leftHand = skeleton.Joints[JointID.HandLeft].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);
            Joint rightHand = skeleton.Joints[JointID.HandRight].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);

            double deltaX = Math.Max(Math.Abs(leftHand.Position.X - centerShoulder.Position.X), Math.Abs(rightHand.Position.X - centerShoulder.Position.X));
            double deltaY = Math.Max(Math.Abs(leftHand.Position.Y - centerShoulder.Position.Y), Math.Abs(rightHand.Position.Y - centerShoulder.Position.Y));
            double deltaZ = Math.Max(Math.Abs(leftHand.Position.Z - centerShoulder.Position.Z), Math.Abs(rightHand.Position.Z - centerShoulder.Position.Z));

            const int minimumArmDistance = 65;

            double armDistance = getDistance(deltaX, deltaY, deltaZ);

            if (armDistance > minimumArmDistance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool isCloseToBody(SkeletonData skeleton)
        {
            Joint centerHip = skeleton.Joints[JointID.HipCenter].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);

            Joint leftHand = skeleton.Joints[JointID.HandLeft].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);
            Joint rightHand = skeleton.Joints[JointID.HandRight].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);

            double deltaX = Math.Min(Math.Abs(leftHand.Position.X - centerHip.Position.X), Math.Abs(rightHand.Position.X - centerHip.Position.X));
            double deltaY = Math.Min(Math.Abs(leftHand.Position.Y - centerHip.Position.Y), Math.Abs(rightHand.Position.Y - centerHip.Position.Y));
            double deltaZ = Math.Min(Math.Abs(leftHand.Position.Z - centerHip.Position.Z), Math.Abs(rightHand.Position.Z - centerHip.Position.Z));

            const int minimumBodyDistance = 30;

            double bodyDistance = getDistance(deltaX, deltaY, deltaZ);

            if (bodyDistance < minimumBodyDistance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private double getDistance(double x, double y, double z)
        {
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
        }

        private double computeLaunchAngle(SkeletonData skeleton)
        {


            if (curBall != null)
            {
                double ballX = curBall.X;

                double[] ballJoint = getJointForBall(skeleton);

                Joint hipCenter = skeleton.Joints[JointID.HipCenter].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);
                Joint shoulderJoint = skeleton.Joints[JointID.ShoulderRight].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);
                if (ballX < hipCenter.Position.X)
                {
                    shoulderJoint = skeleton.Joints[JointID.ShoulderLeft].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);
                }

                double[] startJoint = new double[2] { hipCenter.Position.X, hipCenter.Position.Y };
                double[] finishJoint = ballJoint;
                double[] direction = this.computeBallDirection();
                startJoint[0] = direction[0];
                startJoint[1] = direction[1];
                finishJoint[0] = direction[2];
                finishJoint[1] = direction[3];
                double launchSize = Math.Sqrt(Math.Pow(finishJoint[0] - startJoint[0], 2) + Math.Pow(finishJoint[1] - startJoint[1], 2));
                if (launchSize == 0)
                {
                    return 0;
                }
                double absoluteLaunchAngle = Math.Acos((finishJoint[0] - startJoint[0]) / launchSize);
                double launchSign = Math.Sign(Math.Asin((finishJoint[1] - startJoint[1]) / launchSize));
                double launchAngle = absoluteLaunchAngle * launchSign;
                return launchAngle;
            }
            else
            {
                return 0;
            }



        }

        private double[] getJointForBall(SkeletonData skeleton)
        {
            Joint rightHand = skeleton.Joints[JointID.HandRight].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);
            Joint leftHand = skeleton.Joints[JointID.HandLeft].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);

            double[] ballCoords = new double[2];

            ballCoords[0] = (rightHand.Position.X + leftHand.Position.X) / 2.0;
            ballCoords[1] = (rightHand.Position.Y + leftHand.Position.Y) / 2.0;

            return ballCoords;
        }


    }
}
