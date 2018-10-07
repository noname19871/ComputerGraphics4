using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Graphics4
{
    public partial class Form1 : Form
    {
        struct line
        {
            public double a, b, c;
        }
        struct point
        {
            public double x, y;
        }

        private Bitmap bmp;

        private Graphics g;

        private double [,] transformation_matrix;

        private Point rotation_point = new Point(-1,-1);

        //
        private string mode = "draw";

        private string current_transformation = "shift";

        //Chosen primitive
        public string current_primitive = "";

        //Number of edges in polygon
        public int number_of_edges = 3;


        public Form1()
        {
            InitializeComponent();
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(bmp);
            pictureBox1.Image = bmp;

            TransformationButton.Enabled = false;
            DefinitionButton.Enabled = false;
            ResultLabel.Hide();
            comboBox1.Hide();
            textBox1.Hide();
            textBox2.Hide();
            label1.Hide();
            label2.Hide();
            OkButton.Hide();
        }

        private double[,] matrix_multiplication(double[,] m1, double[,] m2)
        {
            double[,] res = new double[m1.GetLength(0), m2.GetLength(1)];

            for (int i = 0; i < m1.GetLength(0); ++i)
                for (int j = 0; j < m2.GetLength(1); ++j)
                    for (int k = 0; k < m2.GetLength(0); k++)
                    {
                        res[i, j] += m1[i, k] * m2[k, j];
                    }

            return res;
        }

        private void DrawButton_Click(object sender, EventArgs e)
        {
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(bmp);
            pictureBox1.Image = bmp;
            Points.Clear();
            mode = "draw";
            TransformationButton.Enabled = false;
            DefinitionButton.Enabled = false;

            ResultLabel.Hide();
            comboBox1.Hide();
            textBox1.Hide();
            textBox2.Hide();
            label1.Hide();
            label2.Hide();
            OkButton.Hide();

            GetPrimitive form = new GetPrimitive();
            form.Owner = this;
            form.ShowDialog();
        }

        private void TransformationButton_Click(object sender, EventArgs e)
        {
            ResultLabel.Hide();
            comboBox1.Show();
            comboBox1.SelectedItem = comboBox1.Items[0];
            textBox1.Show();
            textBox2.Show();
            label1.Show();
            label2.Show();
            OkButton.Show();
            mode = "transform";
        }

        private void DefinitionButton_Click(object sender, EventArgs e)
        {
            if (current_primitive == "Point")
            {
                MessageBox.Show("Выберите другой примитив", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ResultLabel.Show();
            comboBox1.Hide();
            textBox1.Hide();
            textBox2.Hide();
            label1.Hide();
            label2.Hide();
            OkButton.Hide();
            mode = "define";
        }

        //Для хранения примитивов
        List<Point> Points = new List<Point>();

        private void draw_point(int x, int y, Color c)
        {
            g.DrawEllipse(new Pen(c), x, y, 2, 2);
            pictureBox1.Image = bmp;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (mode == "draw")
            {
                if (current_primitive == "")
                {
                    MessageBox.Show("Примитив не выбран", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (current_primitive == "Point")
                {
                    if (Points.Count != 0)
                    {
                        MessageBox.Show("Сцена уже занята", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    draw_point(e.Location.X, e.Location.Y, Color.Black);
                    Points.Add(e.Location);

                    TransformationButton.Enabled = true;
                }

                if (current_primitive == "Edge")
                {
                    if (Points.Count > 1)
                    {
                        MessageBox.Show("Сцена уже занята", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (Points.Count == 1)
                    {
                        draw_point(e.Location.X, e.Location.Y, Color.Black);
                        Points.Add(e.Location);
                        g.DrawLine(new Pen(Color.Black, 3), Points[0], Points[1]);
                        pictureBox1.Image = bmp;

                        TransformationButton.Enabled = true;
                        DefinitionButton.Enabled = true;
                    }
                    else if (Points.Count == 0)
                    {
                        Points.Add(e.Location);
                        draw_point(e.Location.X, e.Location.Y, Color.Black);
                    }

                }

                if (current_primitive == "Polygon")
                {
                    if (Points.Count == number_of_edges)
                    {
                        MessageBox.Show("Сцена уже занята", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    draw_point(e.Location.X, e.Location.Y, Color.Black);
                    if (Points.Count > 0)
                    {
                        g.DrawLine(new Pen(Color.Black, 3), Points[Points.Count - 1], e.Location);
                        pictureBox1.Image = bmp;
                    }
                    if (Points.Count + 1 == number_of_edges)
                    {
                        g.DrawLine(new Pen(Color.Black, 3), e.Location, Points[0]);
                        pictureBox1.Image = bmp;

                        TransformationButton.Enabled = true;
                        DefinitionButton.Enabled = true;
                    }
                    pictureBox1.Image = bmp;
                    Points.Add(e.Location);
                }
            }

            if (mode == "define")
            {
                if (current_primitive == "Edge")
                {
                    if (Points.Count == 3)
                    {
                        draw_point(Points[2].X, Points[2].Y, this.pictureBox1.BackColor);
                        Points.Remove(Points[2]);
                    }
                    draw_point(e.Location.X,e.Location.Y, Color.Red);
                    Points.Add(e.Location);

                    Point tmp = new Point(e.Location.X, e.Location.Y);
                    Point tmp1 = new Point(e.Location.X + 1, e.Location.Y);

                    if (wherePoint(tmp, tmp1, Points[0], Points[1]))
                        ResultLabel.Text = "Точка находится слева";
                    else
                        ResultLabel.Text = "Точка находится справа";
                }

                if (current_primitive == "Polygon")
                {
                    if (Points.Count == number_of_edges + 1)
                    {
                        draw_point(Points[number_of_edges].X, Points[number_of_edges].Y, this.pictureBox1.BackColor);
                        Points.Remove(Points[number_of_edges]);
                    }
                    int cntIntersect = 0;
                    draw_point(e.Location.X, e.Location.Y, Color.Red);


                    for (int i = 0; i < number_of_edges; i++)
                    {
                        int ii = (i + 1) % number_of_edges;
                        Point tmp = new Point(e.Location.X, e.Location.Y);
                        //если заданная совпадает с вершиной. 
                        if (Points.Contains(tmp))
                        {
                            cntIntersect++;
                            break;
                        }
                        Point tmp1 = new Point(e.Location.X + 1, e.Location.Y);
                        if (intersect(tmp, tmp1, Points[i], Points[ii]))
                        {
                            cntIntersect++;
                        }
                    }
                    if ((cntIntersect % 2) == 0)
                        ResultLabel.Text = "Точка не принадлежит многоугольнику";
                    else
                        ResultLabel.Text = "Точка принадлежит многоугольнику";
                    Points.Add(e.Location);
                }
            }

            if (mode == "transform")
            {
                if (current_transformation == "rotation")
                {
                    rotation_point = e.Location;
                }
            }
        }

        line getLinebyPoints(Point a, Point b)
        {
            double A, B, C;
            A = a.Y - b.Y;
            B = b.X - a.X;
            C = a.X * b.Y - b.X * a.Y;
            line f;
            f.a = A;
            f.b = B;
            f.c = C;
            return f;                          //(y1 - y2)x + (x2 - x1)y + (x1y2 - x2y1) = 0;
        }

        const double EPS = 1e-9;

        double det(double a1, double b1, double a2, double b2)
        {
            return a1 * b2 - b1 * a2;
        }
        
        bool wherePoint(Point px, Point py, Point px1, Point py1)
        {
            Point pp;
            if (px1.Y > py1.Y)
            {
                pp = px1;
                px1 = py1;
                py1 = pp;
            }
            else if (px1.Y == py1.Y)
                if (px1.X > py1.X)
                {
                    pp = px1;
                    px1 = py1;
                    py1 = pp;
                }

            line m = getLinebyPoints(px, py);
            line n = getLinebyPoints(px1, py1);
            point res;
            double zn = det(m.a, m.b, n.a, n.b);
            if (Math.Abs(zn) < EPS)//параллельность
                return true;
            res.x = -det(m.c, m.b, n.c, n.b) / zn;
            res.y = -det(m.a, m.c, n.a, n.c) / zn;
            //проверка, что Point пересечения между координатами концов отрезка.
            if (res.x > px.X) //С нужной стороны от луча
                return true;
            else return false;;
        }

        bool intersect(Point px, Point py, Point px1, Point py1)
        {
            point res;
            line m = getLinebyPoints(px, py);
            line n = getLinebyPoints(px1, py1);

            double zn = det(m.a, m.b, n.a, n.b);
            if (Math.Abs(det(m.a, m.b, n.a, n.b)) < EPS
                    && Math.Abs(det(m.a, m.c, n.a, n.c)) < EPS
                    && Math.Abs(det(m.b, m.c, n.b, n.c)) < EPS) //Проверка продолжения луча на совпадение с Edgeм, совпадение с Edgeм = 1 пересечению
                return true;
            if (Math.Abs(zn) < EPS)//параллельность
                return false;
            res.x = -det(m.c, m.b, n.c, n.b) / zn;
            res.y = -det(m.a, m.c, n.a, n.c) / zn;
            //проверка, что Point пересечения между координатами концов отрезка.
            if (((res.x > px1.X && res.x < py1.X) || (res.x < px1.X && res.x > py1.X)) && ((res.y > px1.Y && res.y < py1.Y) || (res.y < px1.Y && res.y > py1.Y)))
                if (res.x > px.X) //С нужной стороны от луча
                    return true;
                else return false;
            else return false;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(comboBox1.SelectedItem.ToString())
            {
                case "Смещение":
                    {
                        current_transformation = "shift";
                        OkButton.Show();
                        textBox1.Show();
                        textBox2.Show();
                        label1.Show();
                        label2.Show();
                        label1.Text = "Введите сдвиг по X: ";
                        label2.Text = "Введите сдвиг по Y: ";
                        break;
                    }
                case "Поворот примитива":
                    {
                        current_transformation = "rotation";
                        OkButton.Show();
                        textBox1.Show();
                        textBox2.Hide();
                        label1.Show();
                        label2.Hide();
                        label1.Text = "Введите угол поворота: ";
                        break;
                    }
                case "Поворот ребра":
                    {
                        break;
                    }
                case "Масштабирование":
                    {
                        current_transformation = "scale";
                        OkButton.Show();
                        textBox1.Show();
                        textBox2.Show();
                        label1.Show();
                        label2.Show();
                        label1.Text = "Введите коэффициент по X: ";
                        label2.Text = "Введите коэффициент по Y: ";
                        break;
                    }
                case "Поиск точки пересечения":
                    {
                        break;
                    }
            }
        }

        private void find_center(ref double x, ref double y)
        {
            foreach (Point p in Points)
            {
                x += p.X;
                y += p.Y;
            }

            x /= Points.Count;
            y /= Points.Count;
        }


        private bool create_transformation_matrix()
        {
            if (current_transformation == "shift")
            {
                double tX = System.Convert.ToDouble(textBox1.Text);
                double tY = System.Convert.ToDouble(textBox2.Text);
                transformation_matrix = new double[,] { { 1.0, 0, 0 }, { 0, 1.0, 0 }, { tX, tY, 1.0 } };
            }
            else if (current_transformation == "rotation")
            {
                if (rotation_point == new Point(-1,-1))
                {
                    MessageBox.Show("Не задана точка вращения", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                double c = rotation_point.X;
                double d = rotation_point.Y;
                double p = System.Convert.ToDouble(textBox1.Text) * Math.PI / 180;
                double cos = Math.Cos(p);
                double sin = Math.Sin(p);
                transformation_matrix = new double[,] { {cos, sin, 0}, {-sin, cos, 0},
                        {cos*(-c)+d*sin+c, (-c)*sin-d*cos+d, 1}};
            }
            else if (current_transformation == "scale")
            {
                double cx = System.Convert.ToDouble(textBox1.Text); 
                double cy = System.Convert.ToDouble(textBox2.Text); 

                if (current_primitive == "Point")
                    transformation_matrix = new double[3, 3] { { 1.0, 0, 0 }, { 0, 1.0, 0 }, { 0, 0, 1.0 } };
                else
                {
                    double a = 0, b = 0;
                    find_center(ref a, ref b);
                    transformation_matrix = new double[3, 3] { { cx, 0, 0 }, { 0, cy, 0 }, { (1 - cx) * a, (1 - cy) * b, 1 } };
                }

            }

            return true;
        }

        private void redraw(List<Point> new_points)
        {
            if (new_points.Count == 1)
            {
                draw_point(Points[0].X, Points[0].Y, this.pictureBox1.BackColor);
                draw_point(new_points[0].X, new_points[0].Y, Color.Black);
                Points[0] = new_points[0];
            }
            else if (new_points.Count == 2)
            {
                draw_point(Points[0].X, Points[0].Y, this.pictureBox1.BackColor);
                draw_point(Points[1].X, Points[1].Y, this.pictureBox1.BackColor);
                g.DrawLine(new Pen(this.pictureBox1.BackColor, 3), Points[0], Points[1]);

                draw_point(new_points[0].X, new_points[0].Y, Color.Black);
                draw_point(new_points[1].X, new_points[1].Y, Color.Black);
                g.DrawLine(new Pen(Color.Black, 3), new_points[0], new_points[1]);
                Points[0] = new_points[0];
                Points[1] = new_points[1];
            }
            else
            {
                Point start = Points[0];
                draw_point(Points[0].X, Points[0].Y, this.pictureBox1.BackColor);

                foreach (Point p in Points)
                {
                    if (p != start)
                    {
                        draw_point(p.X, p.Y, this.pictureBox1.BackColor);
                        g.DrawLine(new Pen(this.pictureBox1.BackColor, 3), start, p);
                        start = p;
                    }
                }
                g.DrawLine(new Pen(this.pictureBox1.BackColor, 3), start, Points[0]);

                start = new_points[0];
                Points[0] = start;
                draw_point(new_points[0].X, new_points[0].Y, Color.Black);

                int cur = 1;
                foreach (Point p in new_points)
                {
                    if (p != start)
                    {
                        Points[cur] = p;
                        cur++;
                        draw_point(p.X, p.Y, Color.Black);
                        g.DrawLine(new Pen(Color.Black, 3), start, p);
                        start = p;
                    }
                }
                g.DrawLine(new Pen(Color.Black, 3), start, new_points[0]);
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (!create_transformation_matrix())
            {
                return;
            }
            List<Point> new_points = new List<Point>();
            foreach(Point p in Points)
            {
                double[,] point = new double[,] { { p.X, p.Y, 1.0 } };
                double[,] res = matrix_multiplication(point, transformation_matrix);
                new_points.Add(new Point(Convert.ToInt32(Math.Round(res[0, 0])), Convert.ToInt32(Math.Round(res[0, 1]))));
            }

            redraw(new_points);
        }
    }
}
;