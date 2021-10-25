using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using org.mariuszgromada.math.mxparser;

namespace Laba3
{
    public partial class Form1 : Form
    {

        public static List<point> steps = new List<point>();//список точек
        public string func;

        private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private const string SpreadsheetId = "1Kcvpqi-I6wY0HSFGehgdVp_tS70Fk2KQroZT39Z8S5Q";
        private const string GoogleCredentialsFileName = "google-credentials.json";
        private const string ReadRange = "Лист1!A:B";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private double f(double x)//вынес подставления значения в функцию в отдельный метод
        {
            double result = 0;
            Function f = new Function(func);
            string sklt = "f()";
            string fx = sklt.Insert(2, x.ToString());
            fx = fx.Replace(",", ".");
            Expression fxx = new Expression(fx, f);
            result = fxx.calculate();
            return result;
        }

        public void MathPart()
        {
            double sumX = 0;
            double sumY = 0;
            double sumXY = 0;
            double X2 = 0;
            double Y2 = 0;
            double X3 = 0;
            double X4 = 0;
            double x2y = 0;
            double count = steps.Count;
            double a, a1, b, b1, c;


            foreach(var p in steps)
            {
                sumX += p.x;
                sumY += p.y;
                sumXY += p.x * p.y;
                X2 += p.x * p.x;
                Y2 += p.y * p.y;
                X3 += p.x * p.x * p.x;
                X4 += p.x * p.x * p.x * p.x;
                x2y += (p.x * p.x) * p.y;
            }
            a = Math.Round(LinearRegerssionA(sumX, sumY, sumXY, X2, Y2, count), 4);
            b = Math.Round(LinearRegerssionB(sumX, sumY, sumXY, X2, Y2, count), 4);
            label2.Text = Convert.ToString(delta(X2, sumX, count, X3, X4));

            label3.Text = Convert.ToString(deltaA(X2, sumX, count, X3, X4, sumY, sumXY, x2y));

            label4.Text = deltaB(X2, sumX, count, X3, X4, sumY, sumXY, x2y).ToString();

            func = "f(x) =" + a + "*x+" + b;
            func = func.Replace(",", ".");
            label1.Text = func;
            graph();
        }

        public double LinearRegerssionA(double sumX, double SumY, double SumXY, double X2, double Y2, double count)
        {
            double a;
            a = (sumX * SumY - count * SumXY) / ((sumX * sumX) - count * X2);
            return a;
        }

        public double LinearRegerssionB(double sumX, double SumY, double SumXY, double X2, double Y2, double count)
        {
            double b;
            b = (sumX * SumXY - X2 * SumY) / ((sumX * sumX) - count * X2);
            return b;
        }

        public double delta(double X2, double sumX, double count, double X3, double X4)
        {
            double[,] matrix = new double[3, 3] { { X2, sumX, count }, { X3, X2, sumX }, { X4, X3, X2 } };
            double det = matrix[0, 0] * matrix[1, 1] * matrix[2, 2] + matrix[0, 1] * matrix[1, 2] * matrix[2, 0] +
                matrix[0, 2] * matrix[1, 0] * matrix[2, 1] - matrix[0, 2] * matrix[2, 2] * matrix[2, 0] -
                matrix[0, 0] * matrix[1, 2] * matrix[2, 1] - matrix[0, 1] * matrix[1, 0] * matrix[2, 2];
            return det;
        }
        public double deltaA(double X2, double sumX, double count, double X3, double X4, double sumY, double sumXY, double x2y)
        {
            double[,] matrix = new double[3, 3] { { sumY, sumX, count }, { sumXY, X2, sumX }, { x2y, X3, X2 } };
            double det = matrix[0, 0] * matrix[1, 1] * matrix[2, 2] + matrix[0, 1] * matrix[1, 2] * matrix[2, 0] +
                matrix[0, 2] * matrix[1, 0] * matrix[2, 1] - matrix[0, 2] * matrix[2, 2] * matrix[2, 0] -
                matrix[0, 0] * matrix[1, 2] * matrix[2, 1] - matrix[0, 1] * matrix[1, 0] * matrix[2, 2];
            return det;
        }
        public double deltaB(double X2, double sumX, double count, double X3, double X4, double sumY, double sumXY, double x2y)
        {
            double[,] matrix = new double[3, 3] { { X2, sumY, count }, { X3, sumXY, sumX }, { X4, x2y, X2 } };
            double det = matrix[0, 0] * matrix[1, 1] * matrix[2, 2] + matrix[0, 1] * matrix[1, 2] * matrix[2, 0] +
                matrix[0, 2] * matrix[1, 0] * matrix[2, 1] - matrix[0, 2] * matrix[2, 2] * matrix[2, 0] -
                matrix[0, 0] * matrix[1, 2] * matrix[2, 1] - matrix[0, 1] * matrix[1, 0] * matrix[2, 2];
            return det;
        }
        public double deltaC(double X2, double sumX, double count, double X3, double X4, double sumY, double sumXY, double x2y)
        {
            double[,] matrix = new double[3, 3] { { X2, sumX, sumY }, { X3, X2, sumXY }, { X4, X3, x2y } };
            double det = matrix[0, 0] * matrix[1, 1] * matrix[2, 2] + matrix[0, 1] * matrix[1, 2] * matrix[2, 0] +
                matrix[0, 2] * matrix[1, 0] * matrix[2, 1] - matrix[0, 2] * matrix[2, 2] * matrix[2, 0] -
                matrix[0, 0] * matrix[1, 2] * matrix[2, 1] - matrix[0, 1] * matrix[1, 0] * matrix[2, 2];
            return det;
        }


        public void graph()
        {
            double min = Double.MaxValue;
            double max = Double.MinValue;
            double step = 0.1;

            for (int i = 0; i < steps.Count; i++)
            {
                if (steps[i].x < min)
                    min = steps[i].x;
                if (steps[i].x > max)
                    max = steps[i].x;
            }
            
            int count = (int)Math.Ceiling((max - min) / step) + 1;

            double[] x = new double[count];
            double[] y = new double[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = min + step * i;
                y[i] = f(x[i]);
                Console.WriteLine(x[i]+ " "+y[i]);
            }
            chart1.Series[1].Points.DataBindXY(x, y);
        }

        public void randompoints(double n)
        {
            Random rnd = new Random();
            for (int i = 0; i < n; i++)
            {
                int value1 = rnd.Next(0, 100);
                int value2 = rnd.Next(0, 100);
                point abc = new point(value1, value2);
                steps.Add(abc);

            }

        }
        public void addgraphpoints()
        {

        }
        private void button2_Click(object sender, EventArgs e)
        {
            double count = Convert.ToDouble(textBox1.Text);
            randompoints(count);
            foreach (var p in steps)
            {
                dataGridView1.Rows.Add(p.x, p.y);
                chart1.Series[0].Points.AddXY(p.x, p.y);
            }

        }

        async private void button3_Click(object sender, EventArgs e)
        {
            var serviceValues = GetSheetsService().Spreadsheets.Values;
            await ReadAsync(serviceValues);
            foreach (var p in steps)
            {
                dataGridView1.Rows.Add(p.x, p.y);
                chart1.Series[0].Points.AddXY(p.x, p.y);
            }
        }

        private static SheetsService GetSheetsService()
        {
            using (var stream = new FileStream(GoogleCredentialsFileName, FileMode.Open, FileAccess.Read))
            {
                var serviceInitializer = new BaseClientService.Initializer
                {
                    HttpClientInitializer = GoogleCredential.FromStream(stream).CreateScoped(Scopes)
                };
                return new SheetsService(serviceInitializer);
            }
        }

        private async Task ReadAsync(SpreadsheetsResource.ValuesResource valuesResource)
        {
            var response = await valuesResource.Get(SpreadsheetId, ReadRange).ExecuteAsync();
            var values = response.Values;
            if (values == null || !values.Any())
            {
                 Console.WriteLine("No data found.");
                 return;
            }
            for (int i = 0; i < values.Count; i++)
            {
                double val0 = Convert.ToDouble(values[i][0]);
                double val1 = Convert.ToDouble(values[i][1]);
                point aaa = new point(val0, val1);
                steps.Add(aaa);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            MathPart();
        }
    }

    public class point//класс для сохранения точек
    {
        public double x, y;
        public point(double X, double Y)
        {
            this.x = X;
            this.y = Y;
        }
    }
}
