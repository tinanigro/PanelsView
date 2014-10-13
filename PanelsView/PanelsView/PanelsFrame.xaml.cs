using System.Threading.Tasks;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace PanelsView
{
    public sealed partial class PanelsFrame : UserControl
    {
        public PanelsFrame()
        {
            this.InitializeComponent();
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
            this.DataContext = this;
            this.Loaded += OnLoaded;
        }

        private async void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            if (Scrolling > 0)
            {
                backPressedEventArgs.Handled = true;
                await Task.Delay(200);
                HideSidebar();
            }
        }

        public bool IsSideBarVisible { get; set; }

        public double Scrolling
        {
            //0 is sidebar visible
            //1 is sidebar collapsed
            // 0.5 is sidebar at mid course
            get
            {
                return (double)this.GetValue(ScrollingProperty);
            }
            set
            {
                this.SetValue(ScrollingProperty, value);
            }
        }

        DependencyProperty ScrollingProperty = DependencyProperty.Register("Scrolling", typeof(double),
            typeof(PanelsFrame), new PropertyMetadata(0, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {

        }


        void Responsive()
        {
            SideTransform.TranslateX = -SidebarGrid.ActualWidth;
            FadeOutSidebarGridAnimation.To = -SidebarGrid.ActualWidth;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Responsive();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Unloaded += OnUnloaded;
            this.SizeChanged += OnSizeChanged;
            Responsive();
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.SizeChanged -= OnSizeChanged;
        }

        public object SideBarContent
        {
            get { return SidebarGrid.Content; }
            set { SidebarGrid.Content = value; }
        }

        public Frame MainFrame
        {
            get { return ControlMainFrame; }
        }

        public EdgeUIThemeTransition MainFrameThemeTransition
        {
            get { return ControlMainFrameThemeTransition; }
        }

        private void EdgeGrid_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (SideTransform.TranslateX > -240)
            {
                OnSideBarVisible();
            }
            else
            {
                OnSideBarCollapsed();
            }
        }

        private void EdgeGrid_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {

        }

        private void EdgeGrid_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Cumulative.Translation.X < 340)
            {
                SideTransform.TranslateX += e.Delta.Translation.X;
                MainFramePlaneProjection.GlobalOffsetZ -= e.Delta.Translation.X / 4;
            }
            Scrolling = (SidebarGrid.ActualWidth + SideTransform.TranslateX) / SidebarGrid.ActualWidth;
        }

        private void Sidebar_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {

        }

        private void Sidebar_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Cumulative.Translation.X < 0)
            {
                MainFramePlaneProjection.GlobalOffsetZ -= e.Delta.Translation.X / 4;
                SideTransform.TranslateX += e.Delta.Translation.X;
            }
            Scrolling = (SidebarGrid.ActualWidth + SideTransform.TranslateX) / SidebarGrid.ActualWidth;
        }

        private void Sidebar_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (SideTransform.TranslateX < -50)
            {
                OnSideBarCollapsed();
            }
            else
            {
                OnSideBarVisible();
            }
        }

        void OnSideBarVisible()
        {
            FadeInProperty.Begin();
            if ((ControlMainFrame.Content as Page).BottomAppBar != null)
            {
                (ControlMainFrame.Content as Page).BottomAppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
            }
            Scrolling = 1;
            ControlMainFrame.IsEnabled = false;
            IsSideBarVisible = true;
        }

        void OnSideBarCollapsed()
        {
            FadeOutProperty.Begin();
            if ((ControlMainFrame.Content as Page).BottomAppBar != null)
            {
                (ControlMainFrame.Content as Page).BottomAppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
            }
            Scrolling = 0;
            ControlMainFrame.IsEnabled = true;
            IsSideBarVisible = false;
        }

        public void ShowSidebar()
        {
            OnSideBarVisible();
        }

        public void HideSidebar()
        {
            OnSideBarCollapsed();
        }
    }
}
