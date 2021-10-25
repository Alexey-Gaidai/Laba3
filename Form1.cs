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
using ExcelDataReader;
using System.Data.OleDb;

namespace Laba3
{
    public partial class Form1 : Form
    {
        //регион с предустановками необходимыми для работы программы
        #region SettingsFields
        public static List<point> steps = new List<point>();//список точек
        public string func;
        public string func2;

        private string fileName = string.Empty;
        private DataTableCollection tableCollection = null;

        private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private const string SpreadsheetId = "1Kcvpqi-I6wY0HSFGehgdVp_tS70Fk2KQroZT39Z8S5Q";
        private const string GoogleCredentialsFileName = "google-credentials.json";
        private const string ReadRange = "Лист1!A:B";
        #endregion
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        //регион математической стороны программы
        #region MathSide
        public void MathPart()
        {
            //переменные
            double sumX = 0;
            double sumY = 0;
            double sumXY = 0;
            double X2 = 0;
            double Y2 = 0;
            double X3 = 0;
            double X4 = 0;
            double x2y = 0;
            double count = steps.Count;
            
            double a, deltta, deltaa, b, deltab, deltac;


            foreach(var p in steps)//заполнение переменных в соотвествии со значениями
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

            a = Math.Round(LinearRegerssionA(sumX, sumY, sumXY, X2, Y2, count), 4);//коэфицент а для линейной регрессии
            b = Math.Round(LinearRegerssionB(sumX, sumY, sumXY, X2, Y2, count), 4);//коэфицент b для линейной регрессии

            deltta = delta(X2, sumX, count, X3, X4);//вызываем метод расчитывающий детерминант основной матрицы
            deltaa = Math.Round(deltaA(X2, sumX, count, X3, X4, sumY, sumXY, x2y) / deltta, 4);//вызываем метод детерминант матрицы дельта А и считаем коэфицент не отходя от кассы
            //далее тоже самое но по другому(почему бы и нет)
            deltab = Math.Round(((X2 * sumXY * X2) + (sumY * sumX * X4) + (count * X3 * x2y) - (count * sumXY * X4) - (X2 * sumX * x2y) - (sumY * X3 * X2)) / deltta, 4);
            deltac = Math.Round(((X2 * X2 * x2y) + (sumX * sumXY * X4) + (sumY * X3 * X3) - (sumY * X2 * X4) - (X2 * sumXY * X3) - (sumX * X3 * x2y)) / deltta, 4);


            //Конструируем функцию для линейной регрессии
            func = "f(x) =" + a + "*x+" + b;
            func = func.Replace(",", ".");

            //Конструируем функцию для квадратичной регрессии
            func2 = "f(x) =" + deltaa + "*x^2+(" + deltab + ")*x+" + deltac;
            func2 = func2.Replace(",", ".");

            //Выводим коэфиценты
            label1.Text = func;
            label2.Text = func2;

            //рисуем граф
            graph();
        }

        public double LinearRegerssionA(double sumX, double SumY, double SumXY, double X2, double Y2, double count)//расчет коэфицента А для линейной регрессии
        {
            double a;
            a = (sumX * SumY - count * SumXY) / ((sumX * sumX) - count * X2);
            return a;
        }

        public double LinearRegerssionB(double sumX, double SumY, double SumXY, double X2, double Y2, double count)//расчет коэфицента B для линейной регрессии 
        {
            double b;
            b = (sumX * SumXY - X2 * SumY) / ((sumX * sumX) - count * X2);
            return b;
        }

        public double delta(double X2, double sumX, double count, double X3, double X4) //замудренный расчет детерминанта
        {
            double[,] matrix = new double[3, 3] { { X2, sumX, count }, { X3, X2, sumX }, { X4, X3, X2 } };
            double det = matrix[0, 0] * matrix[1, 1] * matrix[2, 2] + matrix[0, 1] * matrix[1, 2] * matrix[2, 0] +
                matrix[0, 2] * matrix[1, 0] * matrix[2, 1] - matrix[0, 2] * matrix[2, 2] * matrix[2, 0] -
                matrix[0, 0] * matrix[1, 2] * matrix[2, 1] - matrix[0, 1] * matrix[1, 0] * matrix[2, 2];
            return det;
        }
        public double deltaA(double X2, double sumX, double count, double X3, double X4, double sumY, double sumXY, double x2y)//замудренный расчет дельты
        {
            double[,] matrix = new double[3, 3] { { sumY, sumX, count }, { sumXY, X2, sumX }, { x2y, X3, X2 } };
            double det = matrix[0, 0] * matrix[1, 1] * matrix[2, 2] + matrix[0, 1] * matrix[1, 2] * matrix[2, 0] + matrix[0, 2] * matrix[1, 0] * matrix[2, 1] - matrix[0, 2] * matrix[2, 2] * matrix[2, 0] - matrix[0, 0] * matrix[1, 2] * matrix[2, 1] - matrix[0, 1] * matrix[1, 0] * matrix[2, 2];
            return det;
        }

        #endregion

        //регион логической стороны программы
        #region Logic
        public void graph()//отрисовка графа
        {
            double min = Double.MaxValue;
            double max = Double.MinValue;
            double step = 1;

            for (int i = 0; i < steps.Count; i++)//поиск минимума и максимума
            {
                if (steps[i].x < min)
                    min = steps[i].x;
                if (steps[i].x > max)
                    max = steps[i].x;
            }
            
            int count = (int)Math.Ceiling((max - min) / step) + 1;

            double[] x = new double[count];
            double[] y = new double[count];
            double[] y1 = new double[count];

            for (int i = 0; i < count; i++)
            {
                x[i] = min + step * i;
                y[i] = Math.Round(f(x[i]), 5);
                y1[i] = Math.Round(f2(x[i]), 5);
            }
            chart1.Series[1].Points.DataBindXY(x, y);//отрисовка
            chart1.Series[2].Points.DataBindXY(x, y1);
        }

        public void randompoints(double n)//рандомное заполнение точек
        {
            Random rnd = new Random();
            for (int i = 0; i < n; i++)
            {
                int value1 = rnd.Next(0, 10);
                int value2 = rnd.Next(0, 10);
                point abc = new point(value1, value2);
                steps.Add(abc);

            }

        }
        private double f(double x)//подставления значения в функцию для линейной регрессии
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

        private double f2(double x)//подставления значения в функцию для квадратичной регрессии
        {
            double result = 0;
            Function f = new Function(func2);
            string sklt = "f()";
            string fx = sklt.Insert(2, x.ToString());
            fx = fx.Replace(",", ".");
            Expression fxx = new Expression(fx, f);
            result = fxx.calculate();
            return result;
        }
        #endregion

        //регион взаимодействия с формой
        #region FormsDevices
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
        private void button1_Click(object sender, EventArgs e)
        {
            MathPart();
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

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
               openExcelFile();
                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void openExcelFile()
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "Excel (*.XLSX)|*.XLSX";
            opf.ShowDialog();
            DataTable tb = new DataTable();
            string filename = opf.FileName;
            string ConStr = String.Format("Provider=Microsoft.ACE.OLEDB.12.0; Data Source={0}; Extended Properties=Excel", filename);
            System.Data.DataSet ds = new System.Data.DataSet("EXCEL");
            OleDbConnection cn = new OleDbConnection(ConStr);
            cn.Open();
            DataTable schemaTable = cn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            string sheet1 = (string)schemaTable.Rows[0].ItemArray[2];
            string select = String.Format("SELECT * FROM [{0}]", sheet1);
            OleDbDataAdapter ad = new OleDbDataAdapter(select, cn);
            ad.Fill(ds);
            tb = ds.Tables[0];
            cn.Close();
            dataGridView1.DataSource = tb;

        }
        #endregion

        //регион работы гугл сервиса
        #region Sheets
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
        #endregion

        
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
