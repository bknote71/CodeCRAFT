using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Engine
{
    public struct Arc2D
    {
        public double AngleStart { get; set; }
        public double AngleExtent { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        
        public (double, double, double, double) Frame
        {
            get => (X, Y, Width, Height);
            set
            {
                X = value.Item1;
                Y = value.Item2;
                Width = value.Item3;
                Height = value.Item4;
            }
        }

        public Arc2D(double angleStart, double angleExtent, double x, double y, double width, double height)
        {
            AngleStart = angleStart;
            AngleExtent = angleExtent;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
