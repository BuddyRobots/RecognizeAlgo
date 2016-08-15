using UnityEngine;
using OpenCVForUnity;
using System.Collections.Generic;

namespace MagicCircuit
{
    public class LineDetector
    {
        private const int delta_small = 10;
        private const int delta_medium = 15;
        private const int delta_large = 20;
        private const int point_num = 2;

        public LineDetector()
        {
            
        }

        public List<List<Point>> vectorize(Mat lineImg, ref Mat skel)
        {
            List<MyPoint> myLine = new List<MyPoint>();
            List<Point> line = new List<Point>();
            List<List<Point>> listLine = new List<List<Point>>();
            Stack<MyPoint> pointStack = new Stack<MyPoint>();
            Stack<MyPoint> stack = new Stack<MyPoint>();

            // Skeletonize the input first.
            //Mat skel = new Mat(lineImg.size(), CvType.CV_8UC1);
            skel = skeletonization(lineImg);
            
            // Pick a init point on the line
            MyPoint firstPoint = findFirstPoint(skel);
            if (firstPoint.x != 0 && firstPoint.y != 0)
            {
                myLine.Add(firstPoint);
                line.Add(firstPoint.toPoint());
            }
            if (line.Count == 0) return listLine; // If we don't have any point

            //@
            do
            {
                // @stack : all cross points
                // @pointStack : cross points in one iteration
                findLine(skel, firstPoint, ref line, ref pointStack);

                while (pointStack.Count > 0)
                    stack.Push(pointStack.Pop());   

                // If we have more than point_num points on one line, add this line
                if(line.Count > point_num)
                    listLine.Add(line);
                line = new List<Point>();
                Debug.Log("stack.count : " + stack.Count);
                if (stack.Count == 0)
                    break;
                firstPoint = stack.Pop();
                line.Add(firstPoint.toPoint());



            } while (stack.Count > 0);
            
        
            return listLine;
        }

        private Mat skeletonization(Mat grayImg)
        {
            Mat skel = new Mat(grayImg.size(), CvType.CV_8UC1, new Scalar(0, 0, 0));
            Mat temp = new Mat();
            Mat eroded = new Mat();

            Mat element = Imgproc.getStructuringElement(Imgproc.MORPH_ELLIPSE, new Size(3, 3));

            for (var i = 0; i < 200; i++)
            {
                Imgproc.erode(grayImg, eroded, element);
                Imgproc.dilate(eroded, temp, element); // temp = open(grayImg)
                Core.subtract(grayImg, temp, temp);
                Core.bitwise_or(skel, temp, skel);
                grayImg = eroded.clone();

                if (Core.countNonZero(grayImg) == 0)   // done.
                    break;
            }

            //Imgproc.GaussianBlur(skel, skel, new Size(5, 5), 0);
            //pointFilter(skel, 2, 5);
            return skel;
        }

        private MyPoint findFirstPoint(Mat skel)
        {
            for (var i = 1; i < skel.rows(); i++)
                for (var j = 1; j < skel.cols(); j++)
                    if (skel.get(i, j)[0] > 125)
                    {                        
                        return new MyPoint(j, i, From.none);
                    }
            return new MyPoint(0, 0, From.none);
        }

        private Stack<MyPoint> findNextPoints(Mat skel, MyPoint current, int delta)
        {
            Stack<MyPoint> temp = new Stack<MyPoint>();
            MyPoint temPoint_1 = new MyPoint();
            MyPoint temPoint_2 = new MyPoint();
            Stack<MyPoint> result = new Stack<MyPoint>();

            // Point Range[0, rows() - 1][0, cols() - 1]          
            int _xl = Mathf.Max((int)current.x - delta, 0);
            int _xr = Mathf.Min((int)current.x + delta, skel.cols() - 1);
            int _yu = Mathf.Max((int)current.y - delta, 0);
            int _yd = Mathf.Min((int)current.y + delta, skel.rows() - 1);

            // left
            for (var y = _yu + 1; y < _yd; y++)
                if (skel.get(y, _xl)[0] > 125)
                    temp.Push(new MyPoint(_xl, y, From.right));
            if (temp.Count > 0)
            {
                temPoint_1 = temp.Pop();
                result.Push(temPoint_1);
                while(temp.Count > 0)
                {
                    temPoint_2 = temp.Pop();
                    if (temPoint_1.y - temPoint_2.y > 1) // if the new point is not connected to the old point
                        result.Push(temPoint_2);
                    temPoint_1 = temPoint_2;
                }
            }
            temp.Clear();
            // right
            for (var y = _yu + 1; y < _yd; y++)
                if (skel.get(y, _xr)[0] > 125)
                    temp.Push(new MyPoint(_xr, y, From.left));
            if (temp.Count > 0)
            {
                temPoint_1 = temp.Pop();
                result.Push(temPoint_1);
                while (temp.Count > 0)
                {
                    temPoint_2 = temp.Pop();
                    if (temPoint_1.y - temPoint_2.y > 1) // if the new point is not connected to the old point
                        result.Push(temPoint_2);
                    temPoint_1 = temPoint_2;
                }
            }
            temp.Clear();
            // up
            for (var x = _xl; x <= _xr; x++)
                if (skel.get(_yu, x)[0] > 125)
                    temp.Push(new MyPoint(x, _yu, From.down));
            if (temp.Count > 0)
            {
                temPoint_1 = temp.Pop();
                result.Push(temPoint_1);
                while (temp.Count > 0)
                {
                    temPoint_2 = temp.Pop();
                    if (temPoint_1.x - temPoint_2.x > 1) // if the new point is not connected to the old point
                        result.Push(temPoint_2);
                    temPoint_1 = temPoint_2;
                }
            }
            temp.Clear();
            // down
            for (var x = _xl; x <= _xr; x++)
                if (skel.get(_yd, x)[0] > 125)
                    temp.Push(new MyPoint(x, _yd, From.up));
            if (temp.Count > 0)
            {
                temPoint_1 = temp.Pop();
                result.Push(temPoint_1);
                while (temp.Count > 0)
                {
                    temPoint_2 = temp.Pop();
                    if (temPoint_1.x - temPoint_2.x > 1) // if the new point is not connected to the old point
                        result.Push(temPoint_2);
                    temPoint_1 = temPoint_2;
                }
            }
            temp.Clear();

            // Delete detected region
            removeBox(skel, _xl, _xr, _yu, _yd);
            return result;
        }

        private void findLine(Mat skel, MyPoint current, ref List<Point> line, ref Stack<MyPoint> pointStack)
        {
            while(true)
            {
                Stack<MyPoint> myPoint = findNextPoints(skel, current, delta_small);

                if (myPoint.Count != 1)
                {
                    // increase radius
                    myPoint = findNextPoints(skel, current, delta_medium);

                    if (myPoint.Count != 1)
                    {
                        // increase radius
                        myPoint = findNextPoints(skel, current, delta_large);

                        if (myPoint.Count != 1)
                        {
                            // cross point or deadend
                            Debug.Log("=====return : " + myPoint.Count);

                            pointStack = myPoint; // Save pointStack
                            return; 
                        }
                    }                    
                }

                MyPoint next = myPoint.Pop();

                line.Add(current.toPoint());

                current = next;
            }
        }



        private void removeBox(Mat img, int xl, int xr, int yu, int yd)
        {
            for(var i = yu; i <= yd; i++)
                for(var j = xl; j <= xr; j++)
                {
                    img.put(i, j, 0);
                }
        }
    }

    public class Line
    {
        public int number;
        public List<int> nextComponent;
        public List<int> nextLine;
        public List<Point> points;
    }

    public enum From { left, right, up, down, none };

    public class MyPoint
    {
        public From from;
        public int x;
        public int y;

        public MyPoint()
        {
            x = 0;
            y = 0;
            from = From.none;
        }

        public MyPoint(int _x, int _y, From _from)
        {
            x = _x;
            y = _y;
            from = _from;
        }

        public Point toPoint()
        {
            return new Point(x, y);
        }
    }
}


