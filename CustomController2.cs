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
        double[] hsv;
        double[] thetas;
        int left_circle_id;
        int right_circle_id;

        public CustomController2(MainWindow win) : base(win)
        {

            argb = new double[] { 0.5, 0.5, 0.5, 0.5};
            hsv = new double[3];
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
            
            byte byte_val = (byte)((int)(d * 255));
            Console.WriteLine("d: " + d + "byte: " + byte_val);
            return byte_val;
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
                    Console.WriteLine(change);
                    adjust_circles_color(i, change, targets);
                    targets[5].setTargetColor(Color.FromArgb(doubleToByte(argb[0]), doubleToByte(argb[1]), doubleToByte(argb[2]), doubleToByte(argb[3])));
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
            Console.Write("delta theta = " + delta_theta + " change = ");
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
            if (change < 0)
            {
                colorDelta = -0.01;
            }
            else
            {
                colorDelta = 0.01;
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
            RGBtoHSV(hsv, argb[1], argb[2], argb[3]);
            HSVtoRGB(bright_rgb, hsv[1], 0, hsv[2]);
            targets[4].setTargetColor(Color.FromArgb(doubleToByte(argb[0]), doubleToByte(bright_rgb[1]), doubleToByte(bright_rgb[2]), doubleToByte(bright_rgb[3])));
        }

        private void scale_brightness(double colorDelta)
        {
            RGBtoHSV(hsv, argb[1], argb[2], argb[3]);
            hsv[2] = change_color(hsv, 2, colorDelta);
            HSVtoRGB(argb, hsv[0], hsv[1], hsv[2]);
        }

        private double change_color(double[] array, int i, double colorDelta)
        {
            if (array[i] + colorDelta > 1)
            {
                return 1;
            }
            else if (array[i] + colorDelta < 0)
            {
                return 0;
            }
            else
            {
                return array[i] + colorDelta;
            }
        }


        private void RGBtoHSV(double[] hsv, double r, double g, double b)
        {
            double min, max, delta;
            min = Math.Min( Math.Min(r, g), Math.Min(g, b));
            max = Math.Max(Math.Max(r, g), Math.Max(g, b));
            delta = max - min;

            hsv[2] = max; //set v
            if (max != 0)
            {
                hsv[1]= delta / max; //set s
            }
            else
            {
                hsv[1] = 0;
                hsv[0] = -1;
                return;
            }

            if (r == max)
            {
                hsv[0] = (g - b) / delta;
            }
            else if (g == max)
            {
                hsv[0] = 2 + (b - r) / delta;
            }
            else
            {
                hsv[0] = 4 + (r - g) / delta;
            }

            hsv[0] *= 60;
            if (hsv[0] < 0) {
                hsv[0] += 360;
            }

        }

        private void HSVtoRGB(double[] argb, double h, double s, double v)
        {
            int i;
            double f, p, q, t;
            if (s == 0) //achromatic (grey)
            {
                argb[1] = argb[2] = argb[3] = v;
                return;
            }

            h /= 60;
            i = (int) Math.Floor(h);
            f = h - i;
            p = v * (1 - s);
            q = v * (1 - s * f);
            t = v * (1 - s * (1 - f));

            switch (i)
            {
                case 0:
                    argb[1] = v;
                    argb[2] = t;
                    argb[3] = p;
                    break;
                case 1:
                    argb[1] = q;
                    argb[2] = v;
                    argb[3] = p;
                    break;
                case 2:
                    argb[1] = p;
                    argb[2] = v;
                    argb[3] = t;
                    break;
                case 3:
                    argb[1] = p;
                    argb[2] = q;
                    argb[3] = v;
                    break;
                case 4:
                    argb[1] = t;
                    argb[2] = p;
                    argb[3] = v;
                    break;
                default:
                    argb[1] = v;
                    argb[2] = p;
                    argb[3] = q;
                    break;
            }
        }
        private double distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(x1 - x2), 2) + Math.Pow(Math.Abs(y1 - y2), 2));
        }

        public override void controllerActivated(Dictionary<int, Target> targets)
        {
            //1 is RED, 2 is GREEN, 3 is BLUE, 4 is BRIGHTNESS
            targets[1].setTargetColor(Color.FromArgb(127, doubleToByte(argb[1]), 0, 0));
            targets[2].setTargetColor(Color.FromArgb(127, 0, doubleToByte(argb[2]), 0));
            targets[3].setTargetColor(Color.FromArgb(127, 0, 0, doubleToByte(argb[3])));
            targets[4].setTargetColor(Color.FromArgb(127, 127, 127, 127));
            targets[5].setTargetColor(Color.FromArgb(127, doubleToByte(argb[1]), doubleToByte(argb[2]), doubleToByte(argb[3]) ));
        }
    }

}
