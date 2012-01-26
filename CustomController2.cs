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

        double[] argb;
        double[] bright_rgb;
        double[] thetas;
        int left_circle_id;
        int right_circle_id;

        public CustomController2(MainWindow win) : base(win)
        {

            argb = new double[] { 0.5, 0.5, 0.5, 0.5};
            bright_rgb = new double[] { 0.5, 0.5, 0.5, 0.5 };

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

        /*This function takes in a RGB value [0,1] and returns a byte [0, 255]*/
        private byte doubleToByte(double d)
        {           
            return (byte)((int)(d * 255));
        }

        private void trackHand(Joint hand, int lr, Dictionary<int, Target> targets) {
            bool inCircle = false;
            for (int i = 1; i <= 4; i++)
            {
                Target cur = targets[i];
                double paintcircleRadius = cur.getTargetRadius();
                if (distance(hand.Position.X, hand.Position.Y, cur.getXPosition() + paintcircleRadius / 2, cur.getYPosition() + paintcircleRadius / 2) <= paintcircleRadius)
                {
                    inCircle = true;
                    int change = increaseOrDecrease(hand, lr, cur);
                    adjust_circles_color(i, change, targets);
                    targets[5].setTargetColor(Color.FromArgb(255, doubleToByte(argb[1]), doubleToByte(argb[2]), doubleToByte(argb[3])));
                }
            }
            if (!inCircle)
            {
                thetas[lr] = -1;
            }
        }

        private int increaseOrDecrease(Joint hand, int lr, Target cur)
        {
            double deltaX = hand.Position.X - cur.getXPosition();
            double deltaY = - (hand.Position.Y - cur.getYPosition());
            double angle = Math.Atan(deltaY/deltaX);

            if (deltaX < 0)
            {
                angle += Math.PI;
            }
            if (angle < 0)
            {
                angle += (2*Math.PI);
            }
            if (thetas[lr] == -1)
            {
                thetas[lr] = angle;
                return 0;
            }
            double delta_theta = angle - thetas[lr];
            thetas[lr] = angle;
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

        private void adjust_circles_color(int i, int change, Dictionary<int, Target> targets)
        {
            double colorDelta;
            if (change == 0)
            {
                return;
            }
            else if (change < 0)
            {
                colorDelta = -0.02;
            } 
            else
            {
                colorDelta = 0.02;
            }

            switch (i) {
                case 1:
                    argb[i] = change_color(argb, i, colorDelta);
                    targets[i].setTargetColor(Color.FromArgb(doubleToByte(argb[0]), doubleToByte(argb[1]), 0, 0));
                    break;
                case 2:
                    argb[i] = change_color(argb, i, colorDelta);
                    targets[i].setTargetColor(Color.FromArgb(doubleToByte(argb[0]), 0, doubleToByte(argb[2]), 0));
                    break;
                case 3:
                    argb[i] = change_color(argb, i, colorDelta);
                    targets[i].setTargetColor(Color.FromArgb(doubleToByte(argb[0]), 0, 0, doubleToByte(argb[3]) ) );
                    break;
                case 4:
                    scale_brightness(colorDelta);
                    targets[1].setTargetColor(Color.FromArgb(doubleToByte(argb[0]), doubleToByte(argb[1]), 0, 0));
                    targets[2].setTargetColor(Color.FromArgb(doubleToByte(argb[0]), 0, doubleToByte(argb[2]), 0));
                    targets[3].setTargetColor(Color.FromArgb(doubleToByte(argb[0]), 0, 0, doubleToByte(argb[3])));
                    break;
            }
            byte grayScaleVal = doubleToByte(0.2989 * argb[1] + 0.5870 * argb[2] + 0.1140 * argb[3]);
            targets[4].setTargetColor(Color.FromArgb(doubleToByte(argb[0]), grayScaleVal, grayScaleVal, grayScaleVal));
        }

        private void scale_brightness(double colorDelta)
        {
            double val = 0;
            if (colorDelta !=0)
            {
                val = Math.Max(argb[1], Math.Max(argb[2], argb[3]));
            }

            double target = val + colorDelta;
            if (target > 1.0) target = 1;
            if (target < 0.001) target = 0.001;

            for (int i = 1; i < 4; i++)
            {
                argb[i] = Math.Round(argb[i] * target / val, 10);
  //              Console.Write(argb[i] + "\t");
            }
  //          Console.WriteLine();
        }


        private double change_color(double[] array, int i, double colorDelta)
        {
            if (array[i] + colorDelta > 1)
            {
                return 1;
            }
            else if (array[i] + colorDelta < 0.001)
            {
                return 0.001;
            }
            else
            {
                return array[i] + colorDelta;
            }
        }

        private double distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(x1 - x2), 2) + Math.Pow(Math.Abs(y1 - y2), 2));
        }

        public override void controllerActivated(Dictionary<int, Target> targets)
        {
            targets[5].showTarget();
            //1 is RED, 2 is GREEN, 3 is BLUE, 4 is BRIGHTNESS
            targets[1].setTargetColor(Color.FromArgb(127, doubleToByte(argb[1]), 0, 0));
            targets[2].setTargetColor(Color.FromArgb(127, 0, doubleToByte(argb[2]), 0));
            targets[3].setTargetColor(Color.FromArgb(127, 0, 0, doubleToByte(argb[3])));
            targets[4].setTargetColor(Color.FromArgb(127, 127, 127, 127));
            targets[5].setTargetColor(Color.FromArgb(255, doubleToByte(argb[1]), doubleToByte(argb[2]), doubleToByte(argb[3]) ));
/*
            for (int i = 1; i <= 5; i++)
            {
                targets[i].setTargetRadius(50.0);
            }
*/
            targets[1].setTargetPosition(140, 170);
            targets[2].setTargetPosition(180, 58);
            targets[3].setTargetPosition(311, 58);
            targets[4].setTargetPosition(351, 170);
            targets[5].setTargetPosition(27, 390);

            targets[1].setTargetText("R");
            targets[2].setTargetText("G");
            targets[3].setTargetText("B");
            targets[4].setTargetText("Brightness");
            targets[4].setFontSize(15);
            targets[5].setTargetText("Dance Floor");
            targets[5].setFontSize(20);

        }
    }
}
