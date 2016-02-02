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
using OxyPlot;
using OxyPlot.Series;
using FDL.FuzzyLogic.Membership;

namespace WPFTest
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            this.MyModel = new PlotModel { Title = "Membership" };
            this.MyModel.Series.Add(new FunctionSeries(new Func<double, double>(MembershipFactory.MakeTriangular(-2, 0, 2)), -Math.PI, Math.PI, 0.01, "Triangular function"));
            this.MyModel.Series.Add(new FunctionSeries(new Func<double, double>(MembershipFactory.MakeTrapezoidal(-2,-1,1,2)), -Math.PI, Math.PI, 0.01, "Trapezoidal function"));
            var lineS = new LineSeries();
            lineS.Points.Add(new DataPoint(-1, 0.5));
            lineS.Points.Add(new DataPoint(0, 1));
            lineS.Points.Add(new DataPoint(1, 0.5));
            lineS.Title = "Point";
            lineS.Color = OxyColor.FromArgb(0, 1, 1, 1);
            lineS.MarkerFill = OxyColor.FromRgb(0, 0, 255);
            lineS.MarkerType = MarkerType.Circle;
            this.MyModel.Series.Add(lineS);
        }

        public PlotModel MyModel { get; private set; }
    }
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
