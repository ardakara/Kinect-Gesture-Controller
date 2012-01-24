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
            background_red = 0;
            background_blue = 0;
            background_green = 0;
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
            for (int i = 1; i <= 4; i++)
            {
                Target cur = targets[i];
                double paintcircleRadius = cur.getTargetRadius();
                if (distance(hand.Position.X, hand.Position.Y, cur.getXPosition() + paintcircleRadius/2, cur.getYPosition()+paintcircleRadius/2) <= paintcircleRadius)
                {
                    byte colorDelta = adjust_background_color(i, hand, cur);
                    adjust_circle_color(i, 10, cur);
                }
            }
            return 0;
        }

        private byte adjust_background_color(int i, Joint hand, Target cur)
        {
            return 0;
        }

        private void adjust_circle_color(int i, int colorDelta) {
            switch (i) {
                case 1:
                    background_red = change_color(background_red, colorDelta);
                    break;
                case 2:
                    background_green = change_color(background_green, colorDelta);
                    break;
                case 3:
                    background_blue = change_color(background_blue, colorDelta);
                    break;
                case 4: 
                    scale_brightness(background_red, background_green, background_blue);
                    break;
            }
        }

        private byte change_color(byte orig_color, int extra)
        {
            if ((int)orig_color + (int)extra > (int)255)
            {
                return 255;
            }
            else
            {
                return (byte) (orig_color + extra);
            }
        }

        private void scale_brightness(byte r, byte g, byte b) 
        {
            background_red /= 2;
            background_green /= 2;
            background_blue /= 2;
        }

        private double distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(x1 - x2), 2) + Math.Pow(Math.Abs(y1 - y2), 2));
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
