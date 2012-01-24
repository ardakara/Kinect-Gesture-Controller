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
    class CustomController2 : SkeletonController
    {

        byte background_red;
        byte background_blue;
        byte background_green;
        byte alpha;
        int left_circle_id;
        double left_theta;
        int right_circle_id;
        double right_theta;

        public CustomController2(MainWindow win) : base(win)
        {
            background_red = 255;
            background_blue = 127;
            background_green = 127;
            alpha = 127;
            left_circle_id = -1;
            left_theta = -1;
            right_circle_id = -1;
            right_theta = -1;
        }

        public override void processSkeletonFrame(SkeletonData skeleton, Dictionary<int, Target> targets)
        {
            //Scale the joints to the size of the window
            Joint leftHand = skeleton.Joints[JointID.HandLeft].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);
            Joint rightHand = skeleton.Joints[JointID.HandRight].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);

            trackLeftHand(leftHand, targets);
        }

        private double trackLeftHand(Joint hand, Dictionary<int, Target> targets) {
            foreach (var target in targets)
            {
                Target cur = target.Value;
                double handEllipseRadius = 17.5; //hardcoded from MainWindow.xaml
                double paintcircleRadius = cur.getTargetRadius();
                /* if (hand_hit_target(hand, cur))
                {
                    target.
                }*/
                //we need HEIGHT and WIDTH of hand ellipse AND circle!
                //otherwise I hard-coded with width of the leftEllipse.... ugh.
                //cur.getXPosition()
            }
            return -1;
        }

        public override void controllerActivated(Dictionary<int, Target> targets)
        {
            //1 is RED, 2 is GREEN, 3 is BLUE, 4 is BRIGHTNESS
            targets[1].setTargetColor(Color.FromArgb(127, background_red, 0, 0));
            targets[2].setTargetColor(Color.FromArgb(127, 0, background_green, 0));
            targets[3].setTargetColor(Color.FromArgb(127, 0, 0, background_blue));
            targets[4].setTargetColor(Color.FromArgb(127, 127, 127, 127));
            targets[5].setTargetColor(Color.FromArgb(127, background_red, background_green, background_blue));
        }
    }

}
