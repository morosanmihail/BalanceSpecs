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
using System.Windows.Shapes;

namespace BalanceSpecsGUI.Windows.Analysis
{
    /// <summary>
    /// Interaction logic for StatisticalTests.xaml
    /// </summary>
    public partial class StatisticalTests : Window
    {
        public StatisticalTests()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<double> vpop1 = new List<double>();
            List<double> vpop2 = new List<double>();

            try
            {
                //Try comma separated instead
                var TextScores = pop1.Text.Replace(" ", string.Empty).Split(',').ToList();
                foreach (var S in TextScores)
                {
                    vpop1.Add(double.Parse(S));
                }

                TextScores = pop2.Text.Replace(" ", string.Empty).Split(',').ToList();
                foreach (var S in TextScores)
                {
                    vpop2.Add(double.Parse(S));
                }
            }
            catch
            {
                return;
            }

            Accord.Statistics.Testing.TwoSampleHypothesis Hypo = Accord.Statistics.Testing.TwoSampleHypothesis.FirstValueIsGreaterThanSecond;
            string HypoString = ((Button)sender).Content.ToString();
            switch (HypoString)
            {
                case "Dif":
                    Hypo = Accord.Statistics.Testing.TwoSampleHypothesis.ValuesAreDifferent;
                    break;
                case "Less":
                    Hypo = Accord.Statistics.Testing.TwoSampleHypothesis.FirstValueIsSmallerThanSecond;
                    break;
                case "More":
                    Hypo = Accord.Statistics.Testing.TwoSampleHypothesis.FirstValueIsGreaterThanSecond;
                    break;
                default:
                    break;
            }

            dynamic test, testWilcoxon;

            if (vpop1.Count == vpop2.Count)
            {
                test = new Accord.Statistics.Testing.PairedTTest(vpop1.ToArray(), vpop2.ToArray(), Hypo);

                testWilcoxon = new Accord.Statistics.Testing.TwoSampleWilcoxonSignedRankTest(vpop1.ToArray(), vpop2.ToArray(), Hypo);
            }
            else
            {
                test = new Accord.Statistics.Testing.TTest(vpop1.ToArray(), vpop2[0], Accord.Statistics.Testing.OneSampleHypothesis.ValueIsSmallerThanHypothesis);

                testWilcoxon = new Accord.Statistics.Testing.WilcoxonSignedRankTest(vpop1.ToArray(), vpop2[0], Accord.Statistics.Testing.OneSampleHypothesis.ValueIsSmallerThanHypothesis);
            }

            results.Text = "T-Test:\n Significant: " + test.Significant + "\n p-value: " + test.PValue +
                "\nMannWhitneyWilcoxon Test:\n Significant: " + testWilcoxon.Significant + "\n p-value: " + testWilcoxon.PValue;
        }
    }
}
