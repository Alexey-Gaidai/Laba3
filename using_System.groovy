using System;
using System.Windows.Forms;

namespace lab6
{
    public abstract class SLAE
    {
        public int N { get; set; }
        public double[,] Matrix { get; set; }
        public double[] B { get; set; }
        public int Iterations { get; set; }
        public SLAE(int n, double[,] matrix)
        {
            N = n;
            Matrix = matrix;
            B = CreateVectorB(Matrix);
            Iterations = 0;
        }
        public double[] CreateVectorB(double[,] matrix)
        {
            double[] B = new double[N];
            for (int i = 0; i < N; i++)
            {
                B[i] = matrix[i, N];
            }
            return B;
        }
        public double[,] Transpose(double[,] matrix)
        {
            double[,] T = (double[,])matrix.Clone();
            double tmp;
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    tmp = T[i, j];
                    T[i, j] = T[j, i];
                    T[j, i] = tmp;
                }
            }
            return T;
        }
        public double[] ReverseSolve(double[,] triangleMatrix, double[] B, bool DawnZero)
        {
            double[] X = new double[N];
            if (DawnZero)
            {
                X[0] = B[0] / triangleMatrix[0, 0];
                for (int i = 1; i < N; i++)
                {
                    double temp = B[i];
                    for (int j = 0; j < i + 1; j++)
                    {
                        temp -= X[j] * triangleMatrix[i, j];
                    }
                    X[i] = temp / triangleMatrix[i, i];
                }
            }
            else
            {
                X[N-1] = B[N-1] / triangleMatrix[N-1, N-1];
                for (int i = N - 2; i >= 0; i--)
                {
                    double temp = B[i];
                    for (int j = N - 1; j >= 0; j--)
                    {
                        temp -= X[j] * triangleMatrix[i, j];
                    }
                    X[i] = temp / triangleMatrix[i, i];
                }
            }
            return X;
        }
        public bool CheckMainDiagonal()
        {
            for (int i = 0; i < N; i++)
            {
                if (Matrix[i, i] == 0)
                {
                    MessageBox.Show("Нулевые элементы на главной диагонали", "Ошибка!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return true;
        }
      
        public abstract double[] FindX();
    }
    class Gauss : SLAE
    {
        public Gauss(int n, double[,] matrix) : base(n, matrix) { }
        public override double[] FindX()
        {
            double[,] Matrix_Clone = (double[,])Matrix.Clone();
            
            for (int k = 0; k < N; k++) 
            {
                for (int i = 0; i < N + 1; i++) 
                {
                    Matrix_Clone[k, i] = Matrix_Clone[k, i] / Matrix[k, k]; 
                }
                for (int i = k + 1; i < N; i++) 
                {
                    double K = Matrix_Clone[i, k] / Matrix_Clone[k, k]; 
                    for (int j = 0; j < N + 1; j++) 
                    {
                        Matrix_Clone[i, j] = Matrix_Clone[i, j] - Matrix_Clone[k, j] * K; 
                    }
                }
            }
            double[] b = CreateVectorB(Matrix_Clone);
            double[] X = ReverseSolve(Matrix_Clone, b, false);
            return X;
        }
    }
    //Метод квадратного корня
    class Cholesky : SLAE
    {
        public Cholesky(int n, double[,] matrix) : base(n, matrix) { }
        public override double[] FindX()
        {
            double[,] L = CholeskyDecomposition();
            double[] y = ReverseSolve(L, B, true);
            double[,] LT = Transpose(L);
            double[] X = ReverseSolve(LT, y, false);
            return X;
        }
        public double[,] CholeskyDecomposition()
        {
            double[,] L = new double[N, N];
            for (int i = 0; i < N; i++)
            {

                double temp;
                for (int j = 0; j < i; j++)
                {
                    temp = 0;
                    for (int k = 0; k < j; k++)
                    {
                        temp += L[i, k] * L[j, k];
                    }
                    L[i, j] = (Matrix[i, j] - temp) / L[j, j];
                }
                
                temp = Matrix[i, i];
                for (int k = 0; k < i; k++)
                {
                    temp -= L[i, k] * L[i, k];
                }
                L[i, i] = Math.Sqrt(temp);
            }
            return L;
        }
    }
    //Метод прогонки
    class RunThrough: SLAE
    {
        public RunThrough(int n, double[,] matrix) : base(n, matrix) { }
        public override double[] FindX()
        {
            if (checkMatrix())
            {
                //Прямой ход
                double[] v = new double[N];
                double[] u = new double[N];
                v[0] = Matrix[0, 1] / (-Matrix[0, 0]);
                u[0] = (-B[0]) / (-Matrix[0, 0]);
                for (int i = 1; i < N - 1; i++)
                {
                    v[i] = Matrix[i, i + 1] / (-Matrix[i, i] - Matrix[i, i - 1] * v[i - 1]);
                    u[i] = (Matrix[i, i - 1] * u[i - 1] - B[i]) / (-Matrix[i, i] - Matrix[i, i - 1] * v[N - 2]);
                }
                v[N - 1] = 0;
                u[N - 1] = (Matrix[N - 1, N - 2] * u[N - 2] - B[N - 1]) / (-Matrix[N - 1, N - 1] - Matrix[N - 1, N - 2] * v[N - 2]);
                //Обратный ход
                double[] X = new double[N];
                X[N - 1] = u[N - 1];
                for (int i = N - 1; i > 0; i--)
                {
                    X[i - 1] = v[i - 1] * X[i] + u[i - 1];
                }
                return X;
            }
            else
            {
                return null;
            }
        }
        public bool checkMatrix()
        {
            for(int i = 1; i < N - 1; i++)
            {
                if (Math.Abs(Matrix[i, i]) < Math.Abs(Matrix[i, i - 1]) + Math.Abs(Matrix[i, i + 1]))
                {
                    MessageBox.Show("Не выполнены условия достаточности", "Ошибка!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                if((Math.Abs(Matrix[0,0])<Math.Abs(Matrix[0,1])) || Math.Abs(Matrix[N - 1, N - 1]) < Math.Abs(Matrix[N - 1, N - 2]))
                {
                    MessageBox.Show("Не выполнены условия достаточности", "Ошибка!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return CheckMainDiagonal();
        }
    }
    class SimpleIteration : SLAE
    {
        public double eps { get; set; }
        public SimpleIteration(int n, double[,] matrix, double eps) : base(n, matrix) 
        {
            this.eps = eps;
        }
        public override double[] FindX()
        {
            if (CheckMainDiagonal())
            {
                double[] TempX = new double[N];
                double norm; 
                double[] X = new double[N];
                for (int i = 0; i < N; i++)
                {
                    X[i] = B[i] / Matrix[i, i];
                }
                do
                {
                    for (int i = 0; i < N; i++)
                    {
                        TempX[i] = B[i];
                        for (int g = 0; g < N; g++)
                        {
                            if (i != g)
                                TempX[i] -= Matrix[i, g] * X[g];
                        }
                        TempX[i] /= Matrix[i, i];
                    }
                    norm = Math.Abs(X[0] - TempX[0]);
                    for (int h = 0; h < N; h++)
                    {
                        if (Math.Abs(X[h] - TempX[h]) > norm)
                            norm = Math.Abs(X[h] - TempX[h]);
                        X[h] = TempX[h];
                    }
                    Iterations++;
                } while (norm > eps);
                return X;
            }
            else
            {
                return null;
            }
        }
    }
    class ConjugateGradient : SLAE
    {
        public double eps { get; set; }
        public ConjugateGradient(int n, double[,] matrix, double eps) : base(n, matrix) 
        {
            this.eps = eps;
        }
        public override double[] FindX()
        {
            int i, j;
            double sumSq = 0;
            double alpha, beta;
            double Spr1;
            double[] X = new double[N];
            double[] Rk = new double[N];
            double[] Zk = new double[N];
            double[] Sz = new double[N];
            for (i = 0; i < N; i++)
            {
                sumSq += B[i] * B[i];
            }
            for (i = 0; i < N; i++)
            {
                for (Sz[i] = 0, j = 0; j < N; j++)
                {
                    Sz[i] += Matrix[i, j] * X[j];
                }
                Rk[i] = B[i] - Sz[i];
                Zk[i] = Rk[i];
            }
            do
            {
                double Spz = 0;
                double Spr = 0;
                for (i = 0; i < N; i++)
                {
                    for (Sz[i] = 0, j = 0; j < N; j++)
                    {
                        Sz[i] += Matrix[i, j] * Zk[j];
                    }
                    Spz += Sz[i] * Zk[i];
                    Spr += Rk[i] * Rk[i];
                }
                alpha = Spr / Spz;
                Spr1 = 0;
                for (i = 0; i < N; i++)
                {
                    X[i] += alpha * Zk[i];
                    Rk[i] -= alpha * Sz[i];
                    Spr1 += Rk[i] * Rk[i];
                }
               
                beta = Spr1 / Spr;
              
                for (i = 0; i < N; i++)
                {
                    Zk[i] = Rk[i] + beta * Zk[i];
                }
                Iterations++;
            }
    
            while (Spr1 / sumSq > eps * eps);
            return X;
        }

    }
    //Метод наискорейшего спуска
    class GradientDescent : SLAE
    {
        public double eps { get; set; }
        public GradientDescent(int n, double[,] matrix, double eps) : base(n, matrix) {
            this.eps = eps;
        }
        public override double[] FindX()
        {
            double[] TempX = new double[N];
            double[] X = new double[N];
            double[] r = new double[N];
            double[] r1 = new double[N];
            double s, s1 = 0;
            do
            {
                for (int i = 0; i < N; i++)
                {
                    r[i] = B[i];
                    for (int j = 0; j < N; j++)
                    {
                        r[i] -= Matrix[i, j] * TempX[j];
                    }
                }
                s = 0;
                for (int i = 0; i < N; i++)
                {
                    s += r[i] * r[i];
                }
                for (int i = 0; i < N; i++)
                {
                    r1[i] = 0;
                    for (int j = 0; j < N; j++)
                    {
                        r1[i] += Matrix[i, j] * r[j];
                    }
                }
                s1 = 0;
                for (int i = 0; i < N; i++)
                {
                    s1 += r[i] * r1[i];
                }
                s /= s1;
                for (int i = 0; i < N; i++)
                {
                    X[i] += s * r[i];
                }
                s = 0;
                for (int i = 0; i < N; i++)
                {
                    s += (TempX[i] - X[i]) * (TempX[i] - X[i]);
                    TempX[i] = X[i];
                }
                Iterations++;
            }
            while (Math.Sqrt(s) > eps);
            return TempX;
        }
    }
}
