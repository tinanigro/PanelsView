using System.Collections.Generic;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using System.Threading.Tasks;
using VisualTreeHelper = PanelsView.Helpers.VisualTreeHelper;

namespace PanelsView
{
    [TemplatePart(Name = SideTransformName, Type = typeof(CompositeTransform))]
    [TemplatePart(Name = SidebarGridName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = FadeOutSidebarGridAnimationName, Type = typeof(DoubleAnimation))]
    [TemplatePart(Name = FadeInPropertyName, Type = typeof(Storyboard))]
    [TemplatePart(Name = FadeOutPropertyName, Type = typeof(Storyboard))]
    [TemplatePart(Name = ControlMainFrameName, Type = typeof(Frame))]
    [TemplatePart(Name = EdgeGridName, Type = typeof(Grid))]
    [TemplatePart(Name = ControlMainFrameThemeTransitionName, Type = typeof(EdgeUIThemeTransition))]
    public sealed class PanelsFrame : Control
    {
        private const string SideTransformName = "SideTransform";
        private const string SidebarGridName = "SidebarGrid";
        private const string FadeOutSidebarGridAnimationName = "FadeOutSidebarGridAnimation";
        private const string FadeInPropertyName = "FadeInProperty";
        private const string FadeOutPropertyName = "FadeOutProperty";
        private const string ControlMainFrameName = "ControlMainFrame";
        private const string EdgeGridName = "EdgeGrid";
        private const string ControlMainFrameThemeTransitionName = "ControlMainFrameThemeTransition";

        private CompositeTransform _sideTransform;
        private ContentPresenter _sidebarGrid;
        private DoubleAnimation _fadeOutSidebarGridAnimation;
        private Storyboard _fadeInProperty;
        private Storyboard _fadeOutProperty;
        private Frame _controlMainFrame;
        private Grid _edgeGrid;
        private EdgeUIThemeTransition _controlMainFrameThemeTransition;

        public bool IsSideBarVisible { get; private set; }

        /// <summary>
        /// 0 is sidebar visible
        /// 1 is sidebar collapsed
        /// 0.5 is sidebar at mid course
        /// </summary>
        public double Scrolling
        {
            get
            {
                try
                {
                    return (double)this.GetValue(ScrollingProperty);
                }
                catch
                {
                    return 1;
                }
            }
            set { SetValue(ScrollingProperty, value); }
        }

        internal static readonly DependencyProperty ScrollingProperty = DependencyProperty.Register(
            "Scrolling", typeof(double), typeof(PanelsFrame), new PropertyMetadata(default(double), ScrollingPropertyChangedCallback));

        private static void ScrollingPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {

        }

        public object SideBarContent
        {
            get { return (object)GetValue(SideBarContentProperty); }
            set { SetValue(SideBarContentProperty, value); }
        }

        internal static readonly DependencyProperty SideBarContentProperty = DependencyProperty.Register(
            "SideBarContent", typeof(object), typeof(PanelsFrame), new PropertyMetadata(default(object), SideBarContentPropertyChangedCallback));

        private static void SideBarContentPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (PanelsFrame) dependencyObject;
            if (that._sidebarGrid != null)
            {
                that._sidebarGrid.Content = dependencyPropertyChangedEventArgs.NewValue;
            }
        }

        public Frame MainFrame
        {
            get { return _controlMainFrame; }
        }

        public EdgeUIThemeTransition MainFrameThemeTransition
        {
            get { return _controlMainFrameThemeTransition; }
        }

        public PanelsFrame()
        {
            DefaultStyleKey = typeof (PanelsFrame);

            if (!DesignMode.DesignModeEnabled)
            {
                HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
                this.Loaded += OnLoaded;
            }
        }
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _sideTransform = GetTemplateChild(SideTransformName) as CompositeTransform;
            _sidebarGrid = GetTemplateChild(SidebarGridName) as ContentPresenter;
            _fadeOutSidebarGridAnimation = GetTemplateChild(FadeOutSidebarGridAnimationName) as DoubleAnimation;
            _fadeInProperty = GetTemplateChild(FadeInPropertyName) as Storyboard;
            _fadeOutProperty = GetTemplateChild(FadeOutPropertyName) as Storyboard;
            _controlMainFrame = GetTemplateChild(ControlMainFrameName) as Frame;
            _edgeGrid = GetTemplateChild(EdgeGridName) as Grid;
            _controlMainFrameThemeTransition = GetTemplateChild(ControlMainFrameThemeTransitionName) as EdgeUIThemeTransition;

            if (_sidebarGrid != null && SideBarContent != null)
            {
                _sidebarGrid.Content = SideBarContent;
            }

            RegisterManipulationEvents();
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

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Responsive();
        }
        private void Responsive()
        {
            if (_sideTransform != null)
            {
                _sideTransform.TranslateX = -_sidebarGrid.ActualWidth;
            }

            if (_fadeOutSidebarGridAnimation != null)
            {
                _fadeOutSidebarGridAnimation.To = -_sidebarGrid.ActualWidth;
            }
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

        public void ShowSidebar()
        {
            OnSideBarVisible();
        }

        public void HideSidebar()
        {
            OnSideBarCollapsed();
        }

        private void OnSideBarVisible()
        {
            _fadeInProperty.Begin();

            Page page = _controlMainFrame.Content as Page;
            if (page != null && page.BottomAppBar != null)
            {
                page.BottomAppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
            }
            Scrolling = 1;
            _controlMainFrame.IsEnabled = false;
            IsSideBarVisible = true;
            EnabledTextBox();
        }

        private void OnSideBarCollapsed()
        {
            _fadeOutProperty.Begin();

            Page page = _controlMainFrame.Content as Page;
            if (page != null && page.BottomAppBar != null)
            {
                page.BottomAppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
            }
            Scrolling = 0;
            _controlMainFrame.IsEnabled = true;
            IsSideBarVisible = false;
            DisableTextBox();
        }

        /// <summary>
        /// Enabled textboxes can make keybord appear even if textbox is out of the screen/not visible
        /// Disabling them prevent this bug
        /// </summary>
        private void DisableTextBox()
        {
            List<TextBox> textBoxs = new List<TextBox>();
            VisualTreeHelper.FindChildren(textBoxs, _sidebarGrid);
            foreach (TextBox textBox in textBoxs)
            {
                textBox.IsEnabled = false;
            }
        }

        private void EnabledTextBox()
        {
            List<TextBox> textBoxs = new List<TextBox>();
            VisualTreeHelper.FindChildren(textBoxs, _sidebarGrid);
            foreach (TextBox textBox in textBoxs)
            {
                textBox.IsEnabled = true;
            }
        }
        #region Manipulations Event Handlers

        private void RegisterManipulationEvents()
        {
            if (_edgeGrid != null)
            {
                _edgeGrid.ManipulationStarted += EdgeGrid_OnManipulationStarted;
                _edgeGrid.ManipulationDelta += EdgeGrid_OnManipulationDelta;
                _edgeGrid.ManipulationCompleted += EdgeGrid_OnManipulationCompleted;
            }

            if(_sidebarGrid != null)
            {
                _sidebarGrid.ManipulationStarted += Sidebar_OnManipulationStarted;
                _sidebarGrid.ManipulationDelta += Sidebar_OnManipulationDelta;
                _sidebarGrid.ManipulationCompleted += Sidebar_OnManipulationCompleted;
            }
        }

        private void EdgeGrid_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (_sideTransform.TranslateX > -240)
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
                _sideTransform.TranslateX += e.Delta.Translation.X;
                //MainFramePlaneProjection.GlobalOffsetZ -= e.Delta.Translation.X / 4;
            }
            Scrolling = (_sidebarGrid.ActualWidth + _sideTransform.TranslateX) / _sidebarGrid.ActualWidth;
        }

        private void Sidebar_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {

        }

        private void Sidebar_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Cumulative.Translation.X < 0)
            {
                //MainFramePlaneProjection.GlobalOffsetZ -= e.Delta.Translation.X / 4;
                _sideTransform.TranslateX += e.Delta.Translation.X;
            }
            Scrolling = (_sidebarGrid.ActualWidth + _sideTransform.TranslateX) / _sidebarGrid.ActualWidth;
        }

        private void Sidebar_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (_sideTransform.TranslateX < -50)
            {
                OnSideBarCollapsed();
            }
            else
            {
                OnSideBarVisible();
            }
        }

        #endregion
    }
}
