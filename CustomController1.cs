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
        private static double R_GROWN_PER_FRAME = 0.3;
        private BallModels _balls;
        private BallModel curBall = null;
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
                }
                // if no ball, keep having no ball
            }

            _balls.animateBalls();
            _balls.removeOutOfBoundsBalls();
            _balls.removeIntersectingBalls(targets,new ProcessTargetIntersectedDelegate(handleTargetIntersected));
        }


        private void handleTargetIntersected(Target t, double value)
        {
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

            targets[2].setTargetPosition(524, 0);
            targets[2].setTargetText("0");

            targets[3].setTargetPosition(0, 342);
            targets[3].setTargetText("0");

            targets[4].setTargetPosition(524, 342);
            targets[4].setTargetText("0");

            targets[5].hideTarget();
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

            double[] ballJoint = getJointForBall(skeleton);

            Joint hipCenter = skeleton.Joints[JointID.HipCenter].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);

            double launchSize = Math.Sqrt(Math.Pow(ballJoint[0] - hipCenter.Position.X, 2) + Math.Pow(ballJoint[1] - hipCenter.Position.Y, 2));
            double absoluteLaunchAngle = Math.Acos((ballJoint[0] - hipCenter.Position.X) / launchSize);
            double launchSign = Math.Sign(Math.Asin((ballJoint[1] - hipCenter.Position.Y) / launchSize));
            double launchAngle = absoluteLaunchAngle * launchSign;
            return launchAngle;

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
