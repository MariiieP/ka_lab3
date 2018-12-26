using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Diagnostics;
using System.Threading;

namespace ka_lb3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int countGraph = 2;
        Grid[] graphs;
        Cities cities;
        Stack<TextBox> errorTB;
        CancellationTokenSource[] cts;
        Queue<Task> tasks;

        public MainWindow()
        {
            InitializeComponent();
            graphs = new Grid[countGraph];
            graphs[0] = graph1;
            graphs[1] = graph2;
            errorTB = new Stack<TextBox>();
            tasks = new Queue<Task>();
            cts = new CancellationTokenSource[countGraph];
            for (int i = 0; i < cts.Length; i++)
            {
                cts[i] = new CancellationTokenSource();
            }
        }

        // отмена операций
        private void CancelCalculation()
        {
            foreach (var c in cts)
            {
                c.Cancel();
            }
            tasks.Clear();
        }


        public void DrawPoints(object sender, RoutedEventArgs e)
        {
            CancelCalculation();
            ClearTextBox();//если уже что-то писали, замазываем белым
            int nCities;
            if (!Int32.TryParse(textB_countCities.Text, out nCities))//если не число
            {
                textB_countCities.Background = Brushes.Coral;
                errorTB.Push(textB_countCities);
                return;
            }
            cities = new Cities(nCities);
            cities.Generate((int)graph1.Width);
            GeometryGroup cityGroup = new GeometryGroup();
            for (int i = 0; i < cities.NumCities; i++)
            {
                Location l = cities.GetLocation(i);
                // формирование точек на карте
                EllipseGeometry city = new EllipseGeometry();
                city.Center = new Point(l.X, l.Y);
                city.RadiusX = 4;
                city.RadiusY = 4;
                cityGroup.Children.Add(city);
            }
            //отрисовка точек на карте
            Path[] myPath = new Path[countGraph];
            for (int i = 0; i < countGraph; i++)
            {
                myPath[i] = new Path();
                myPath[i].Fill = Brushes.Red;
                myPath[i].Stroke = Brushes.Black;
                myPath[i].Data = cityGroup;

                graphs[i].Children.Clear();
                graphs[i].Children.Add(myPath[i]);
            }
            button_CalcBF.IsEnabled = true;
            button_CalcAC.IsEnabled = true;

        }

        public void DrawLines(int[] trail, Grid graph)
        {
            GeometryGroup linesGroup = new GeometryGroup();
            LineGeometry line = new LineGeometry();
            int city = 1;
            //берем координаты каждoго города
            for (; city < trail.Length; city++)
            {
                line = new LineGeometry();
                line.StartPoint = new Point(cities.GetLocation(trail[city - 1]).X, cities.GetLocation(trail[city - 1]).Y);
                line.EndPoint = new Point(cities.GetLocation(trail[city]).X, cities.GetLocation(trail[city]).Y);
                linesGroup.Children.Add(line);
            }
            //координаты последнего города
            line = new LineGeometry();
            line.StartPoint = new Point(cities.GetLocation(trail[city - 1]).X, cities.GetLocation(trail[city - 1]).Y);
            line.EndPoint = new Point(cities.GetLocation(trail[0]).X, cities.GetLocation(trail[0]).Y);
            linesGroup.Children.Add(line);

            Path myPath = new Path();
            myPath.Stroke = Brushes.Black;
            myPath.Data = linesGroup;
            if (graph.Children.Count > 1)
                graph.Children.Remove(graph.Children[1]);
            graph.Children.Add(myPath);
        }

        public void ClearTextBox()
        {
            while (errorTB.Count > 0)
            {
                errorTB.Pop().Background = Brushes.White;
            }
        }

        public async Task CalculateBust()
        {
            ClearTextBox();//если уже что-то писали, замазываем белым
            cts[0].Cancel();
            cts[0] = new CancellationTokenSource();
            progressBF.Visibility = Visibility.Visible;
            
            Bust algorithm = null;
            Stopwatch time = null;
            int[] solve = null;//решение
            Task thisTask = (Task.Run(() =>
            {
                algorithm = new Bust();
                time = new Stopwatch();
                time.Start();
                solve = algorithm.Solution(cities);
                time.Stop();
            }));
            tasks.Enqueue(thisTask);//синхронная связка
            await Task.Run(() =>
            {
                while (true)
                {

                    if (cts[0].Token.IsCancellationRequested || thisTask.IsCompleted)
                    {
                        break;
                    }
                }
            });
            if (solve != null)
            {
                DrawLines(solve, graphs[0]);
                timeBF.Content = time.ElapsedMilliseconds.ToString();
                lengthBF.Content = algorithm.TotalDistance.ToString("F2");
            }
            progressBF.Visibility = Visibility.Hidden;
        }

        public async Task CalculateAnt()//ant
        {
            ClearTextBox();//если уже что-то писали, замазываем белым
            cts[1].Cancel();
            cts[1] = new CancellationTokenSource();
            progressAC.Visibility = Visibility.Visible;
            int alpha;
            if (!Int32.TryParse(textB_alpha.Text, out alpha))
            {
                textB_alpha.Background = Brushes.Coral;
                errorTB.Push(textB_alpha);
                return;
            }
            int beta;
            if (!Int32.TryParse(textB_beta.Text, out beta))
            {
                textB_beta.Background = Brushes.Coral;
                errorTB.Push(textB_beta);
                return;
            }
            double rho;
            if (!Double.TryParse(textB_rho.Text, out rho))
            {
                textB_rho.Background = Brushes.Coral;
                errorTB.Push(textB_rho);
                return;
            }
            double Q;
            if (!Double.TryParse(textB_Q.Text, out Q))
            {
                textB_Q.Background = Brushes.Coral;
                errorTB.Push(textB_Q);
                return;
            }
            int numAnts;
            if (!Int32.TryParse(textB_nAnts.Text, out numAnts))
            {
                textB_nAnts.Background = Brushes.Coral;
                errorTB.Push(textB_nAnts);
                return;
            }
            int maxTime;
            if (!Int32.TryParse(textB_time.Text, out maxTime))
            {
                textB_time.Background = Brushes.Coral;
                errorTB.Push(textB_time);
                return;
            }
            Ant algorithm = null;
            Stopwatch time = null;
            int[] solve = null;
            Task thisTask = (Task.Run(() =>
            {
                algorithm = new Ant(alpha, beta, rho, Q, numAnts, maxTime);
                time = new Stopwatch();
                time.Start();
                solve = algorithm.Solution(cities);
                time.Stop();
            }));
            tasks.Enqueue(thisTask);
            await Task.Run(() =>
            {
                while (true)
                {

                    if (cts[1].Token.IsCancellationRequested || thisTask.IsCompleted)
                    {
                        break;
                    }
                }
            });
            if (solve != null)
            {
                DrawLines(solve, graphs[1]);
                timeAC.Content = time.ElapsedMilliseconds.ToString();
                lengthAC.Content = algorithm.TotalDistance.ToString("F2");
            }
            progressAC.Visibility = Visibility.Hidden;
        }


        private async void button_CalcAC_Click(object sender, RoutedEventArgs e)
        {
            await CalculateAnt();
        }

        private async void button_CalcBF_Click(object sender, RoutedEventArgs e)
        {
            await CalculateBust();
        }
    }
}
