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
        double[] thetas;
        int right_circle_id;

        public CustomController2(MainWindow win) : base(win)
        {
            background_red = 0;
            background_blue = 0;
            background_green = 0;
            alpha = 127;
            left_circle_id = -1;
            thetas = new double[] { -1, -1 };
            right_circle_id = -1;
        }

        public override void processSkeletonFrame(SkeletonData skeleton, Dictionary<int, Target> targets)
        {
            //Scale the joints to the size of the window
            Joint leftHand = skeleton.Joints[JointID.HandLeft].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);
            Joint rightHand = skeleton.Joints[JointID.HandRight].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);

            trackHand(leftHand, 0, targets);
            trackHand(rightHand, 1, targets);
        }

        private double trackHand(Joint hand, int lr, Dictionary<int, Target> targets) {
            bool inCircle = false;
            for (int i = 1; i <= 4; i++)
            {
                Target cur = targets[i];
                double paintcircleRadius = cur.getTargetRadius();
                if (distance(hand.Position.X, hand.Position.Y, cur.getXPosition() + paintcircleRadius / 2, cur.getYPosition() + paintcircleRadius / 2) <= paintcircleRadius)
                {
                    inCircle = true;
                    int change = increaseOrDecrease(hand, lr, cur);

                    adjust_circle_color(i, change);
                    /*case 1: 
                        background_red += 50; 
                        targets[i].setTargetColor(Color.FromArgb(alpha, background_red, 0, 0);
                        break;
                    case 2: 
                        background_green += 50; 
                        targets[i].setTargetColor(Color.FromArgb(alpha, 0, background_green, 0));
                        break;
                    case 3: 
                        background_blue += 50; 
                        targets[i].setTargetColor(Color.FromArgb(alpha, 0, 0, background_blue));
                        break;
                    case 4: 
                        background_red /= 2; 
                        background_green /= 2; 
                        background_blue /= 2; 
                        targets[i].setTargetColor(Color.FromArgb(alpha, background_red, background_green, background_blue));
                        break; */
                }
            }
            if (!inCircle)
            {
                thetas[lr] = -1;
            }

            return 0;
        }

        private int increaseOrDecrease(Joint hand, int lr, Target cur)
        {
            double deltaX = hand.Position.X - cur.getXPosition();
            double deltaY = hand.Position.Y - cur.getYPosition();
            double angle = Math.Asin(deltaY / deltaX);
            if (deltaX < 0)
            {
                if (deltaY > 0)
                {
                    angle += Math.PI;
                }
                else
                {
                    angle -= Math.PI;

                }
            }
            if (angle < 0)
            {
                angle += Math.PI;
            }
            if (thetas[lr] == -1)
            {
                thetas[lr] = angle;
                return 0;
            }
            double delta_theta = angle - thetas[lr];
            if (delta_theta > Math.PI)
            {
                return 1;
            }
            if (delta_theta < -1 * Math.PI)
            {
                return -1;
            }
            if (delta_theta > 0)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }


        private void adjust_circle_color(int i, int change) 
        {
            switch (i)
            {
                // do the switch cases

            }
             
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
