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

        /*private double f(double x)//вынес подставления значения в функцию в отдельный метод
        {
            double result = 0;
            Function f = new Function("f(x) = " + textBox1.Text);
            string sklt = "f()";
            string fx = sklt.Insert(2, x.ToString());
            fx = fx.Replace(",", ".");
            Expression fxx = new Expression(fx, f);
            result = fxx.calculate();
            return result;
        }*/

        public void MathPart()
        {
            double sumX = 0;
            double SumY = 0;
            double SumXY = 0;
            double X2 = 0;
            double Y2 = 0;
            double count = steps.Count;
            double a, b;

            foreach(var p in steps)
            {
                sumX += p.x;
                SumY += p.y;
                SumXY += p.x * p.y;
                X2 += p.x * p.x;
                Y2 += p.y * p.y;
            }
            a = Math.Round(LinearRegerssionA(sumX, SumY, SumXY, X2, Y2, count), 4);
            b = Math.Round(LinearRegerssionB(sumX, SumY, SumXY, X2, Y2, count), 4);

            string func = "f(x) = " + a + "*x+" + b;
            Function f = new Function(func);
            label1.Text = func;
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

        public void graph()
        {

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
