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
        public CustomController1(MainWindow win) : base(win){}

        public override void processSkeletonFrame(SkeletonData skeleton, Dictionary<int, Target> targets)
        {

            /* YOUR CODE HERE*/
            if (areWristsTogether(skeleton) &&
                isCloseToBody(skeleton))
            {
                targets[1].setTargetSelected();
            }
            else
            {
                targets[1].setTargetUnselected();
            }

            if (areArmsStraight(skeleton) &&
                areWristsTogether(skeleton) &&
                !isCloseToBody(skeleton))
            {
                targets[2].setTargetSelected();
            }
            else
            {
                targets[2].setTargetUnselected();
            }
            
            if (areWristsTogether(skeleton))
            {
                targets[5].setTargetSelected();
            }
            else
            {
                targets[5].setTargetUnselected();
            }

        }

        public override void controllerActivated(Dictionary<int, Target> targets)
        {

            /* YOUR CODE HERE */

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
    }
}
