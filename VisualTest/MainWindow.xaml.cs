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
using OxyPlot;
using OxyPlot.Series;

namespace VisualTest
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.MyModel = new PlotModel
            {
                Title = "My graph"
            };
            this.MyModel.Series.Add(new FunctionSeries(Math.Sin, 0, Math.PI, 0.1));
            InitializeComponent();
        }

        public PlotModel MyModel { get; private set; }
    }
}
