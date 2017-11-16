using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using ZedGraph;
namespace Integration
{
    public partial class Form1 : Form
    {
        private double e;
        private double n;
        private double tao;
        private double uh0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            groupBox1.Visible = true;
        }

        public double function(double x)
        {
            switch (comboBoxTask.SelectedIndex)
            {
                case 0: //Задача #2
                    return (2.0 + (x * x) / (2.0 * e)) * Math.Exp(-x * x / e);
                case 1: //Задача #8
                    return 1.0 / (Math.Sqrt(1.0 + 3.0 * Math.Exp(x * x / e)));
                case 2: //Task #12.1
                    return 1.0/ (x - e + (1.0 + e) * Math.Exp(-x /e));
                case 3: //Задача 13.3
                    return (-3.0 + Math.Exp(x / e)) / (3.0 + Math.Exp(x / e));
                default:
                    return 0;
            }
        }

        private double f(double u, double t)
        {
            switch (comboBoxTask.SelectedIndex)
            {
                case 0: //Task #2
                    return (t * Math.Exp(-t * t / e) - 2.0 * t * u) / e;
                case 1: //Task #8
                    return (t * Math.Pow(u,3) - t * u) / e;
                case 2: //Task #12.1
                    return (u - t * u * u) / e;
                case 3: //Task #13.3
                    return ((u + 3.0) * (1.0 - u)) / (4.0 * e);
                default:
                    return 0;
            }
        }

      
       private double firststage(double x) //Одностадийный метод
        {
            if (x == 0)
            {
                return uh0;
            }
            else
            {
                uh0 = uh0 + tao * f(uh0, x);
                return uh0;
            }
        }

        private double secondstage(double x) //Двухстадийный метод
        {

            if (x == 0)
            {
                return uh0;
            }
            else
            {
                //лямбда равна одному
                double temp = f(uh0, x);
                double yh = uh0 + tao * temp;
                double uhnext = uh0 + tao * (0.5 * temp + 0.5 * f(yh, (x + tao)));
                uh0 = uhnext;
                return uh0;
            }
        }

        private double thirdstage(double x) //Трехстадийный метод
        {
            if (x == 0)
            {
                return uh0;
            }
            else
            {
                //тета равна одному
                double temp = f(uh0, x);
                double yh1 = uh0 + ((2.0 * tao) / 3.0) * temp;
                double temp2 = f(yh1, x + (2.0 * tao) / 3.0);
                double yh2 = uh0 + ((2.0 * tao) / 3.0) * ((1 - (3.0 / 8.0)) * temp + (3.0 / 8.0) * temp2);
                double uhnext = uh0 + tao * ((0.25 * temp + (3.0 / 4.0 - 1) * temp2 + f(yh2, x + (2.0 * tao / 3.0))));
                uh0 = uhnext;
                return uh0;
            }
        }


        private void maxError()
        {
            double num = 0;
            double denum = 0;
            double temp;
            for(double x = 0; x < 1; x+=tao)
            {
                temp = Math.Abs(function(x));

                if (temp > denum) denum = temp;
                if(comboBoxMethod.SelectedIndex == 0)
                    temp = Math.Abs(temp - firststage(x));
                else if(comboBoxMethod.SelectedIndex == 1)
                    temp = Math.Abs(temp - secondstage(x));
                else
                    temp = Math.Abs(temp - thirdstage(x));

                if (temp > num) num = temp;
                
            }
           
            textBox1.Text = Convert.ToString(Math.Abs(num / denum * 100.0));

        }


        public void Draw()
        {
            ctrl.GraphPane.YAxis.IsVisible = true;
            PointPairList list1 = new PointPairList();
            GraphPane myPane = ctrl.GraphPane;
            myPane.CurveList.Clear();

            e = Convert.ToDouble(comboBoxE.Text); //параметр E
            n = Convert.ToDouble(comboBoxNodes.Text); //число узлов
            tao = 1.0 / (n - 1.0); // шаг сетки
           

            for (double x = 0.0; x <= 1; x += 0.0001)
            {
                list1.Add(x, function(x));
            }

            LineItem line1 = myPane.AddCurve("Тестовая функция", list1, Color.Black, SymbolType.None);
            line1.Line.IsVisible = true;
            line1.Symbol.Fill.Color = Color.Black;
            line1.Symbol.Fill.Type = FillType.Solid;
            line1.Symbol.Size = 2.0f;
            ctrl.AxisChange();
            ctrl.Invalidate();
            myPane.Title.Text = " ";


            
            PointPairList list2 = new PointPairList();


            if (comboBoxTask.SelectedIndex == 0) uh0 = 2; //Задача #2
            else if (comboBoxTask.SelectedIndex == 1) uh0 = 0.5; //Задача #8
            else if (comboBoxTask.SelectedIndex == 2) uh0 = 1; //Task #12.1
            else if (comboBoxTask.SelectedIndex == 3) uh0 = -0.5; //Задача 13.3


            if (comboBoxMethod.SelectedIndex == 0) {
                for (double x = 0; x <= 1; x += tao)
                {
                    list2.Add(x, firststage(x));
                }
            }
            else if(comboBoxMethod.SelectedIndex == 1)
            {
                for (double x = 0; x <= 1; x += tao)
                {
                    list2.Add(x, secondstage(x));
                }
            }
            else
            {
                for (double x = 0; x <= 1; x += tao)
                {
                    list2.Add(x, thirdstage(x));
                }
            }

            LineItem line2 = myPane.AddCurve("Метод Рунге-Кутта", list2, Color.Red, SymbolType.None);
            line2.Line.IsVisible = true;
            line2.Symbol.Fill.Color = Color.Red;
            line2.Symbol.Fill.Type = FillType.Solid;
            line2.Symbol.Size = 6.0f;
            line2.IsSelectable = true;
            line2.IsOverrideOrdinal = true;
            ctrl.AxisChange();
            ctrl.Invalidate();


            maxError();

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = true;
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(60);
                this.Opacity = this.Opacity - 0.1;
            }
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(60);
                this.Opacity = this.Opacity + 0.1;
            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Draw();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void ctrl_Load(object sender, EventArgs e)
        {

        }

        
    }
}
