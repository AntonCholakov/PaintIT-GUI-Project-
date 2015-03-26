using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintIT
{
    class Figure
    {
        private string type;
        private Color colorLines;
        private Color colorFill;
        private string lineType;
        private float lineWidth;
        private Point startingPoint;
        private Point endingPoint;
        private int opacity;
        private bool convertToEllipse;

        private string text;
        private Font font;

        // Constructor
        public Figure() { }
        public Figure(string type)
        {
            this.Type = type;
        }

        // Properties
        public string Type
        {
            get { return this.type; }
            set { this.type = value; }
        }
        public Color ColorLines
        {
            get { return this.colorLines; }
            set { this.colorLines = value; }
        }
        public Color ColorFill
        {
            get { return this.colorFill; }
            set { this.colorFill = value; }
        }
        public string LineType
        {
            get { return this.lineType; }
            set { this.lineType = value; }
        }
        public float LineWidth
        {
            get { return this.lineWidth; }
            set { this.lineWidth = value; }
        }
        public Point StartingPoint
        {
            get { return this.startingPoint; }
            set { this.startingPoint = value; }
        }
        public Point EndingPoint
        {
            get { return this.endingPoint; }
            set { this.endingPoint = value; }
        }
        public int Opacity
        {
            get { return this.opacity; }
            set { this.opacity = value; }
        }
        public bool ConvertToEllipse
        {
            get { return this.convertToEllipse; }
            set { this.convertToEllipse = value; }
        }
        public string Text
        {
            get { return this.text; }
            set { this.text = value; }
        }
        public Font Font
        {
            get { return this.font; }
            set { this.font = value; }
        }


        public string GetInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Type: " + this.Type);
            sb.AppendLine("Color Lines: " + this.ColorLines);
            sb.AppendLine("Color Fill: " + this.ColorFill);
            sb.AppendLine("Line Type: " + this.LineType);
            sb.AppendLine("Line Width: " + this.LineWidth);
            sb.AppendLine("Starting Point X: " + this.StartingPoint.X + "; Starting Point Y: " + this.StartingPoint.Y);
            sb.AppendLine("Ending Point X: " + this.EndingPoint.X + "; Ending Point Y: " + this.EndingPoint.Y);
            sb.AppendLine("Convert to Ellipse: " + this.ConvertToEllipse);
            sb.AppendLine("Text: " + this.Text);
            sb.AppendLine("Font: " + this.Font);


            return sb.ToString();
        }
    }
}
