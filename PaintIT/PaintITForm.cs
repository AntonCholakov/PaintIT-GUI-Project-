using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaintIT
{
    #region Enums
    public enum LineType
    {
        Straight,
        Dashed,
        Pointed
    }

    public enum FillType
    {
        Transparent,
        Fill
    }
    #endregion
    public partial class PaintITForm : Form
    {

        #region Variables
        Bitmap changedImg;
        Bitmap coppiedBitmap;
        Pen pen = new Pen(Color.Black, 1);
        Brush brush = new SolidBrush(Color.Black);
        Font font = new Font("Arial", 16);
        LineType lineType = LineType.Straight;
        FillType fillType = FillType.Transparent;

        Graphics graphics;
        Color colorPrimary, colorSecondary;
        Point sp = new Point(0, 0); // starting point
        Point ep = new Point(0, 0); // ending point
        Point tempEndPoint = new Point(0, 0);
        List<Rectangle> rectangels = new List<Rectangle>();

        List<Figure> figures = new List<Figure>();
        List<Figure> deletedFigures = new List<Figure>();

        bool borders = true;
        bool draw = false;
        bool paste = false;
        bool conturDraw = false;
        bool startInitialising = true;
        Rectangle Rect = new Rectangle();
        Point RectStartingPoint = new Point();

        #endregion

        #region Constructor
        public PaintITForm()
        {
            InitializeComponent();
        }
        #endregion

        private void PaintITForm_Load(object sender, EventArgs e)
        {
            colorPrimary = Color.Black;
            colorSecondary = Color.White;

            toolStrip_tools.Visible = false;

            // Filling Pen Size Combobox
            for (int i = 1; i <= 10; i++)
            {
                toolStripComboBox_penSize.Items.Add(i);
            }
            toolStripComboBox_penSize.SelectedItem = toolStripComboBox_penSize.Items[0];

            EnableOptions(true);
            // Hiding detailed choices
            UncheckTools();
            toolStripButton_toolPen.Checked = true;

            New();

            toolStripComboBox_penSize.SelectedIndex = 0;
        }

        #region File Options

        // Tool button New
        private void toolStripButton_newFile_Click(object sender, EventArgs e)
        {
            listBox_layers.Items.Clear();
            figures.Clear();
            deletedFigures.Clear();
            New();
        }

        // Tool button Open
        private void toolStripButton_openFile_Click(object sender, EventArgs e)
        {
            Open();
        }

        // Tool button Save
        private void toolStripButton_saveFile_Click(object sender, EventArgs e)
        {
            Save();
        }

        // Tool button Exit
        private void toolStripButton_exitFile_Click(object sender, EventArgs e)
        {
            Exit();
        }

        // File / New
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox_layers.Items.Clear();
            figures.Clear();
            deletedFigures.Clear();
            New();
        }

        // File / Open
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Open();
        }

        // File / Save
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        // File / exit


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Exit();
        }
        #endregion

        #region File Options Implementation
        // Create an image in blank
        public void New()
        {
            Bitmap bmp = new Bitmap(pictureBox_canvas.Width, pictureBox_canvas.Height);
            graphics = Graphics.FromImage(bmp);
            NewImage(bmp);
            // Drawing Background
            Figure bgRect = new Figure("rectangle");
            bgRect.ColorFill = Color.White;
            bgRect.StartingPoint = new Point(0, 0);
            bgRect.EndingPoint = new Point(pictureBox_canvas.Width, pictureBox_canvas.Height);
            graphics.FillRectangle(new SolidBrush(Color.White), bgRect.StartingPoint.X, bgRect.StartingPoint.Y, pictureBox_canvas.Width, pictureBox_canvas.Height);
            if (startInitialising)
            {
                figures.Add(bgRect);
                listBox_layers.Items.Add("Layer 1 - Background");
                startInitialising = false;
            }
            if (figures.Count==0)
            {
                figures.Add(bgRect); 
                listBox_layers.Items.Add("Layer 1 - Background");
            }
        }

        public void Open()
        {
            EnableOptions(true);
            string currentDirectory = Directory.GetCurrentDirectory();
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "JPEG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|Bitmap Files (*.bmp)|*.bmp";
                dialog.RestoreDirectory = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    NewImage((Bitmap)Bitmap.FromFile(dialog.FileName));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetType().ToString());
            }
        }

        public void Save()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|PNG (*.png)|*.png|Bitmap (*.bmp)|*.bmp";
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                switch (dialog.FilterIndex)
                {
                    case 1:
                        changedImg.Save(dialog.FileName, ImageFormat.Jpeg);
                        break;
                    case 2:
                        changedImg.Save(dialog.FileName, ImageFormat.Png);
                        break;
                    case 3:
                        changedImg.Save(dialog.FileName, ImageFormat.Bmp);
                        break;
                }
            }
        }

        public void Exit()
        {
            DialogResult dialog = MessageBox.Show("Do you really want to close? All unsaved changes will be deleted!", "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialog == DialogResult.Yes)
            {
                this.Close();
            }
        }
        #endregion

        private void NewImage(Bitmap bmp)
        {
            Bitmap b = new Bitmap(bmp, pictureBox_canvas.Size);
            changedImg = (Bitmap)b.Clone();
            pictureBox_canvas.Image = changedImg;
            graphics = Graphics.FromImage(changedImg);
        }

        private void EnableOptions(bool view)
        {
            pictureBox_canvas.Visible =
                toolStrip_tools.Visible =
                panel_colors.Visible =
                saveToolStripMenuItem.Enabled =
                toolStripButton_saveFile.Enabled = view;
        }

        #region Canvas Events
        private void pictureBox_canvas_MouseDown(object sender, MouseEventArgs e)
        {
            sp = e.Location;
            draw = true; // for pen
            conturDraw = true;

            RectStartingPoint = e.Location;
            Invalidate();
        }

        private void pictureBox_canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (toolStripButton_toolBucket.Checked)
            {
                for (int i = figures.Count - 1; i >= 0; i--)
                {
                    Figure figure = figures[i];
                    if (e.Location.X < figure.EndingPoint.X && e.Location.X > figure.StartingPoint.X && e.Location.Y < figure.EndingPoint.Y && e.Location.Y > figure.StartingPoint.Y)
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            figure.ColorFill = colorPrimary;
                        }
                        else if (e.Button == MouseButtons.Right)
                        {
                            figure.ColorFill = colorSecondary;
                        }

                        if (checkBox_opacity.Checked)
                        {
                            figure.Opacity = trackBar_opacity.Value;
                        }
                        else
                        {
                            figure.Opacity = 255;
                        }
                        break;
                    }
                }
                ClearCanvas(sender, e);
                pen.Color = colorPrimary;
                ReDrawCanvas();
            }

            if (toolStripButton_toolColorPicker.Checked)
            {
                Bitmap bitmap = (Bitmap)pictureBox_canvas.Image;
                Color color = bitmap.GetPixel(ep.X, ep.Y);
                if (e.Button == MouseButtons.Left)
                {
                    colorPrimary = color;
                    pictureBox_colorPrimary.BackColor = colorPrimary;
                }
                else if (e.Button == MouseButtons.Right)
                {
                    colorSecondary = color;
                    pictureBox_colorSecondary.BackColor = colorSecondary;
                }
            }

            if (toolStripButton_toolSelect.Checked)
            {
                if (paste)
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        using (Graphics g = Graphics.FromImage(changedImg))
                        {
                            g.DrawImage(coppiedBitmap, ep);
                        }
                    }
                    else if (e.Button == MouseButtons.Left)
                    {
                        paste = false;
                        this.Cursor = Cursors.Default;
                    }
                }
                else
                {
                    if ((Math.Abs(sp.X - ep.X) != 0) && (Math.Abs(sp.Y - ep.Y) != 0))
                    {
                        Rectangle cloneRect = new Rectangle(Math.Min(sp.X, ep.X), Math.Min(sp.Y, ep.Y), Math.Abs(sp.X - ep.X), Math.Abs(sp.Y - ep.Y));
                        System.Drawing.Imaging.PixelFormat format =
                        changedImg.PixelFormat;
                        coppiedBitmap = changedImg.Clone(cloneRect, format);
                        paste = true;
                        this.Cursor = Cursors.Hand;
                    }
                }
            }

            conturDraw = false;
            draw = false; // for pen
            pen.Color = colorPrimary;
            DrawOnCanvas();
        }

        private void pictureBox_canvas_MouseMove(object sender, MouseEventArgs e)
        {
            ep = e.Location;

            tempEndPoint = e.Location;
            Rect.Location = new Point(Math.Min(RectStartingPoint.X, tempEndPoint.X),
        Math.Min(RectStartingPoint.Y, tempEndPoint.Y));
            Rect.Size = new Size(Math.Abs(RectStartingPoint.X - tempEndPoint.X), 
                Math.Abs(RectStartingPoint.Y - tempEndPoint.Y));

            if (e.Button == MouseButtons.Right)
            {
                if (Rect.Width > Rect.Height)
                {
                    
                    //Rect.Height = Rect.Width;
                    Rect.Size = new Size(Math.Abs(RectStartingPoint.X - tempEndPoint.X), Math.Abs(RectStartingPoint.X - tempEndPoint.X));
                }
                else
                {
                    //Rect.Width = Rect.Height;
                    Rect.Size = new Size(Math.Abs(RectStartingPoint.Y - tempEndPoint.Y), Math.Abs(RectStartingPoint.Y - tempEndPoint.Y));
                }
            }

            if (draw)
            {
                if (toolStripButton_toolPen.Checked || toolStripButton_toolEraser.Checked || toolStripButton_toolColoredPen.Checked)
                {
                    DrawOnCanvas();
                }
            }

            pictureBox_canvas.Invalidate();
        }

        private void pictureBox_canvas_Paint(object sender, PaintEventArgs e)
        {
            // Draw the rectangle...
            if (pictureBox_canvas.Image != null)
            {
                if (conturDraw)
                {
                    if (toolStripButton_toolRectangle.Checked)
                    {
                        e.Graphics.DrawRectangle(pen, Rect);
                    } 
                    if (toolStripButton_toolSelect.Checked)
                    {
                        Brush b = new SolidBrush(Color.Black);
                        Pen pen = new Pen(b, 1f);
                        float[] patt = { 2, 2, 2, 2 };
                        pen.DashPattern = patt;
                        e.Graphics.DrawRectangle(pen, Rect);
                    }
                    if (toolStripButton_toolEllipse.Checked)
                    {
                        e.Graphics.DrawEllipse(pen, Rect);
                    }
                    if (toolStripButton_toolLine.Checked)
                    {
                        e.Graphics.DrawLine(pen, sp, ep);
                    }
                }
            }
        }

        private void pictureBox_canvas_MouseEnter(object sender, EventArgs e)
        {
            if (toolStripButton_toolPen.Checked)
            {
                //this.Cursor = new Cursor("pen.cur");
                this.Cursor = Cursors.UpArrow;
            }
            else if (toolStripButton_toolRectangle.Checked)
            {
                this.Cursor = Cursors.Cross;
            }
            else if (toolStripButton_toolEllipse.Checked)
            {
                this.Cursor = Cursors.Cross;
            }
            else if (toolStripButton_toolLine.Checked)
            {
                this.Cursor = Cursors.UpArrow;
            }
            else if (toolStripButton_toolText.Checked)
            {
                this.Cursor = Cursors.Cross;
            }
        }
        private void pictureBox_canvas_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }
        #endregion

        private void DrawOnCanvas()
        {
            if (toolStripButton_toolPen.Checked)
            {
                graphics.DrawLine(pen, sp, ep);
                sp = ep;
            }
            else if (toolStripButton_toolColoredPen.Checked)
            {
                Random rand = new Random();
                graphics.DrawLine(pen, sp, ep);
                pen.Color = Color.FromArgb(rand.Next(0, 256), rand.Next(0, 256), rand.Next(0, 256));
                sp = ep;
            }
            else if (toolStripButton_toolEraser.Checked)
            {
                pen.Color = Color.White;
                graphics.DrawLine(pen, sp, ep);
                sp = ep;
                pen.Color = colorPrimary;
            }
            else if (toolStripButton_toolRectangle.Checked)
            {
                DrawFigure("rectangle");
            }
            else if (toolStripButton_toolEllipse.Checked)
            {
                DrawFigure("ellipse");
            }
            else if (toolStripButton_toolLine.Checked)
            {
                Figure line = new Figure("line");
                line.StartingPoint = sp;
                line.EndingPoint = ep;
                line.ColorLines = pen.Color;
                line.LineWidth = pen.Width;
                
                if (lineType == LineType.Straight)
                {
                    graphics.DrawLine(pen, sp, ep);
                    line.LineType = "straight";
                }
                else if (lineType == LineType.Dashed)
                {
                    float[] dashValues = { 5, 15, 5};
                    pen.DashPattern = dashValues;
                    graphics.DrawLine(pen, sp, ep);
                    pen = new Pen(colorPrimary, float.Parse(toolStripComboBox_penSize.SelectedItem.ToString()));
                    line.LineType = "dashed";
                }
                else if (lineType == LineType.Pointed)
                {
                    float[] dashValues = { 2, 2, 2, 2 };
                    pen.DashPattern = dashValues;
                    graphics.DrawLine(pen, sp, ep);
                    pen = new Pen(colorPrimary, float.Parse(toolStripComboBox_penSize.SelectedItem.ToString()));
                    line.LineType = "pointed";
                }

                figures.Add(line);
                listBox_layers.Items.Add("Layer " + figures.Count + " - " + line.Type);
            }
            else if (toolStripButton_toolText.Checked)
            {
                string str = toolStripTextBox_text.Text;
                brush = new SolidBrush(colorPrimary);
                Figure text = new Figure("text");
                text.Font = font;
                text.ColorLines = colorPrimary;
                text.Text = str;
                text.EndingPoint = ep;
                graphics.DrawString(str, font, brush, ep);
                figures.Add(text);
                listBox_layers.Items.Add("Layer " + figures.Count + " - " + text.Type);
                brush = new SolidBrush(colorSecondary);
            }
        }

        private void DrawFigure(string figure)
        {
            Figure rectangle = new Figure(figure);
            rectangle.LineType = "solid";
            rectangle.StartingPoint = Rect.Location;
            rectangle.EndingPoint = new Point(Rect.Location.X + Rect.Size.Width, Rect.Location.Y + Rect.Size.Height);
            rectangle.LineWidth = pen.Width;

            if (lineType == LineType.Dashed)
            {
                float[] dashValues = { 5, 2, 15, 4 };
                pen.DashPattern = dashValues;
                rectangle.LineType = "dashed";
            }
            else if (lineType == LineType.Pointed)
            {
                float[] dashValues = { 2, 2, 2, 2 };
                pen.DashPattern = dashValues;
                rectangle.LineType = "pointed";
            }

            if (checkBox_opacity.Checked)
            {
                rectangle.Opacity = trackBar_opacity.Value;
                brush = new SolidBrush(Color.FromArgb(trackBar_opacity.Value, colorSecondary.R, colorSecondary.G, colorSecondary.B));
            }
            else
            {
                rectangle.Opacity = 0;
                brush = new SolidBrush(colorSecondary);
            }
            rectangle.ColorLines = pen.Color;

            switch (figure)
            {
                case "rectangle":
                    rectangle.ConvertToEllipse = false;

                    if (fillType == FillType.Fill)
                    {
                        rectangle.ColorFill = Color.FromArgb(trackBar_opacity.Value, colorSecondary.R, colorSecondary.G, colorSecondary.B);
                        graphics.FillRectangle(brush, Rect);
                    }
                    if (borders)
                    {
                        graphics.DrawRectangle(pen, Rect);
                    }
                    break;

                case "ellipse":
                    rectangle.ConvertToEllipse = true;

                    if (fillType == FillType.Fill)
                    {
                        graphics.FillEllipse(brush, Rect);
                        rectangle.ColorFill = colorSecondary;
                    }
                    if (borders)
                    {
                        graphics.DrawEllipse(pen, Rect);
                    }
                    break;
            }

            figures.Add(rectangle);
            listBox_layers.Items.Add("Layer " + figures.Count + " - " + rectangle.Type);

            pen = new Pen(colorPrimary, float.Parse(toolStripComboBox_penSize.SelectedItem.ToString()));
        }

        #region Tools Switch
        private void UncheckTools()
        {
            toolStripButton_toolPen.Checked = false;
            toolStripButton_toolRectangle.Checked = false;
            toolStripButton_toolEllipse.Checked = false;
            toolStripButton_toolLine.Checked = false;
            toolStripButton_toolText.Checked = false;
            toolStripButton_toolEraser.Checked = false;
            toolStripButton_toolSelect.Checked = false;
            toolStripButton_toolBucket.Checked = false;
            toolStripButton_toolColoredPen.Checked = false;
            toolStripButton_toolColorPicker.Checked = false;
            toolStripTextBox_text.Visible = false;
        }


        #endregion

        #region Pen/Line selection
        private void toolStripComboBox_lineType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (toolStripComboBox_lineType.SelectedIndex)
            {
                case 0:
                    lineType = LineType.Straight;
                    break;
                case 1:
                    lineType = LineType.Dashed;
                    break;
                case 2:
                    lineType = LineType.Pointed;
                    break;
            }

        }

        private void toolStripComboBox_penSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            pen.Width = float.Parse(toolStripComboBox_penSize.SelectedItem.ToString());
        }
        #endregion

        private void pictureBox_colorDialog_Click(object sender, EventArgs e)
        {
            ColorDialog dialog = new ColorDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox_colorPrimary.BackColor = dialog.Color;
                colorPrimary = dialog.Color;
                pen.Color = colorPrimary;
            }
        }

        private void button_switchColors_Click(object sender, EventArgs e)
        {
            pictureBox_colorPrimary.BackColor = colorSecondary;
            pictureBox_colorSecondary.BackColor = colorPrimary;

            colorPrimary = pictureBox_colorPrimary.BackColor;
            colorSecondary = pictureBox_colorSecondary.BackColor;
            pen.Color = colorPrimary;
        }

        private void colorPlum_MouseDown(object sender, MouseEventArgs e)
        {
            Color newColor = ((PictureBox)sender).BackColor;
            if (e.Button == MouseButtons.Left)
            {
                pictureBox_colorPrimary.BackColor = newColor;
                colorPrimary = newColor;
                pen.Color = colorPrimary;
            }
            else if (e.Button == MouseButtons)
            {
                pictureBox_colorSecondary.BackColor = newColor;
                colorSecondary = newColor;
                brush = new SolidBrush(colorSecondary);
            }

        }
        private void tools_Click(object sender, EventArgs e)
        {
            UncheckTools();
            ((ToolStripButton)sender).Checked = true;
            if (toolStripButton_toolText.Checked)
            {
                toolStripTextBox_text.Visible = true;

                FontDialog dialog = new FontDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    font = dialog.Font;
                }
            }
        }

        private void checkBox_fill_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_fill.Checked)
            {
                fillType = FillType.Fill;
            }
            else
            {
                fillType = FillType.Transparent;
                checkBox_borders.Checked = true;
            }
        }

        private void checkBox_borders_CheckedChanged(object sender, EventArgs e)
        {
            borders = checkBox_borders.Checked;
            if (!borders)
            {
                checkBox_fill.Checked = true;
            }
        }

        private void checkBox_opacity_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_opacity.Checked)
            {
                trackBar_opacity.Show();
                label_opacity.Show();
            }
            else
            {
                trackBar_opacity.Hide();
                label_opacity.Hide();
            }
        }

        private void trackBar_opacity_Scroll(object sender, EventArgs e)
        {
            label_opacity.Text = trackBar_opacity.Value.ToString();
        }

        private void listBox_layers_SelectedIndexChanged(object sender, EventArgs e)
        {
            // to do
        }
        private void listBox_layers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            MessageBox.Show(figures[listBox_layers.SelectedIndex].GetInfo());
        }

        private void ClearCanvas(object sender, EventArgs e)
        {
            New();
            pictureBox_canvas.Invalidate();   
        }

        private void ReDrawCanvas()
        {
            foreach (Figure fig in figures)
            {
                // Pen Settings
                Pen p = new Pen(fig.ColorLines, fig.LineWidth);

                if (fig.LineType == "dashed")
                {
                    float[] dashValues = { 5, 2, 15, 4 };
                    p.DashPattern = dashValues;
                }
                else if (fig.LineType == "pointed")
                {
                    float[] dashValues = { 2, 2, 2, 2 };
                    p.DashPattern = dashValues;
                }

                // Figures Types
                if (fig.Type == "line")
                {
                    graphics.DrawLine(p, fig.StartingPoint, fig.EndingPoint);
                }
                else if (fig.Type == "rectangle")
                {
                    if (fig.ColorFill != Color.Empty)
                    {
                        Brush b = new SolidBrush(Color.FromArgb(fig.Opacity, fig.ColorFill));

                        graphics.FillRectangle(b, fig.StartingPoint.X, fig.StartingPoint.Y, fig.EndingPoint.X - fig.StartingPoint.X, fig.EndingPoint.Y - fig.StartingPoint.Y);
                    }
                    graphics.DrawRectangle(p, fig.StartingPoint.X, fig.StartingPoint.Y, fig.EndingPoint.X - fig.StartingPoint.X, fig.EndingPoint.Y - fig.StartingPoint.Y);

                }
                else if (fig.Type == "ellipse")
                {
                    if (fig.ColorFill != Color.Empty)
                    {
                        Brush b = new SolidBrush(Color.FromArgb(fig.Opacity, fig.ColorFill.R, fig.ColorFill.G, fig.ColorFill.B));
                        graphics.FillEllipse(b, fig.StartingPoint.X, fig.StartingPoint.Y, fig.EndingPoint.X - fig.StartingPoint.X, fig.EndingPoint.Y - fig.StartingPoint.Y);
                    }
                    graphics.DrawEllipse(p, fig.StartingPoint.X, fig.StartingPoint.Y, fig.EndingPoint.X - fig.StartingPoint.X, fig.EndingPoint.Y - fig.StartingPoint.Y);
                }
                else if (fig.Type == "text")
                {
                    graphics.DrawString(fig.Text, fig.Font, new SolidBrush(fig.ColorLines), fig.EndingPoint);
                }
            }

            pictureBox_canvas.Invalidate();
        }

        private void button_undo_Click(object sender, EventArgs e)
        {
            try
            {
                if (figures.Count > 1)
                {
                    deletedFigures.Add(figures[figures.Count - 1]);
                    figures.RemoveAt(figures.Count - 1);
                    listBox_layers.Items.RemoveAt(listBox_layers.Items.Count - 1);
                    ClearCanvas(sender, e);
                    ReDrawCanvas();
                }
            }
            catch (Exception)
            {
                
            }
        }

        private void button_redo_Click(object sender, EventArgs e)
        {
            try
            {
                figures.Add(deletedFigures[deletedFigures.Count - 1]);
                deletedFigures.RemoveAt(deletedFigures.Count - 1);
                listBox_layers.Items.Add("Layer " + figures.Count + " - " + figures[figures.Count - 1].Type);
                ClearCanvas(sender, e);
                ReDrawCanvas();
            }
            catch (Exception)
            {
                
            }
        }

        private void listBox_layers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {

                deletedFigures.Add(figures[listBox_layers.SelectedIndex]);
                figures.RemoveAt(listBox_layers.SelectedIndex);
                listBox_layers.Items.RemoveAt(listBox_layers.SelectedIndex);
                New();
                ReDrawCanvas();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout formAbout = new FormAbout();
            formAbout.ShowDialog();
            formAbout.Activate();
        }
    }
}
