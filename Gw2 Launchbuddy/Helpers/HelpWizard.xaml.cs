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

namespace Gw2_Launchbuddy.Helpers
{
    public partial class HelpWizard : Window
    {
        private HelpWizardStep currentStep
        {
            get
            {
                if (dp_stepview.DataContext == null) return null;
                return dp_stepview.DataContext as HelpWizardStep;
            }
        }

        List<HelpWizardStep> steps;

        public HelpWizard(List<HelpWizardStep> steps)
        {
            InitializeComponent();
            if (steps == null) Close();
            this.steps = steps;

            if (steps.Count == 0) this.Close();
            SetStep(steps[0]);
        }

        private void bt_prevstep_Click(object sender, RoutedEventArgs e)
        {
            PrevStep();
        }

        public bool NextStep()
        {
            var step = dp_stepview.DataContext as HelpWizardStep;
            if (step == null) return false;
            SetStep(steps[steps.IndexOf(step) + 1]);
            return true;
        }

        public bool PrevStep()
        {
            var step = dp_stepview.DataContext as HelpWizardStep;
            if (step == null) return false;
            SetStep(steps[steps.IndexOf(step) - 1]);
            return true;
        }

        private void SetStep(HelpWizardStep step)
        {
            currentStep?.CleanUp();
            int index = steps.IndexOf(step);

            bt_prevstep.IsEnabled = index > 0;
            bt_nextstep.IsEnabled = index < steps.Count - 1;
            dp_stepview.DataContext = step;

            UpdateProgressBar();
            currentStep.Highlight();
        }

        private void bt_nextstep_Click(object sender, RoutedEventArgs e)
        {
            NextStep();
        }

        private void bt_highlight_Click(object sender, RoutedEventArgs e)
        {
            currentStep.Highlight();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void bt_closewin_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void UpdateProgressBar()
        {
            pb_stepprogress.Value = ((double)steps.IndexOf(currentStep) / (steps.Count - 1)) * 100;
        }
    }

    public class HelpWizardStep
    {
        FrameworkElement element;
        string title;
        string description;
        bool skippable = false;

        public HelpWizardStep(FrameworkElement element, string title, string description, bool skippable = false)
        {
            this.element = element;
            this.description = description;
            this.title = title;
            this.skippable = skippable;
        }
        public string Title { get { return title; } }
        public string Description { get { return description; } }

        public void CleanUp()
        {
            switch (element.GetType().Name)
            {
                case nameof(ComboBox):
                    ((ComboBox)element).IsDropDownOpen = false;
                    break;

                case nameof(Expander):
                    ((Expander)element).IsExpanded = false;
                    break;
            }
        }

        public void Highlight()
        {
            if (element.Focusable)
            {
                element.Focus();
            }
            switch (element.GetType().Name)
            {
                case nameof(ComboBox):
                    ((ComboBox)element).IsDropDownOpen = true;
                    break;

                case nameof(Expander):
                    ((Expander)element).IsExpanded = true;
                    break;
            }
            try
            {
                Point pt = element.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0));
                pt = new Point(Application.Current.MainWindow.Left + pt.X + element.ActualWidth, Application.Current.MainWindow.Top + pt.Y + (element.ActualHeight / 2));
                UIPointer uipoint = new UIPointer(pt);
                uipoint.Show();
            }
            catch
            {

            }

        }
    }
}
