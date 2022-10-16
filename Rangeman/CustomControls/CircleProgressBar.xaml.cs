using System;

using Xamarin.Forms;
using Xamarin.Forms.Shapes;
using Xamarin.Forms.Xaml;

namespace Rangeman.CustomControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CircleProgressBar : ContentView
    {
        public CircleProgressBar()
        {
            InitializeComponent();

            Angle = (Percentage * 360) / 100;
            RenderArc();
        }

        public int Radius
        {
            get { return (int)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public Brush SegmentColor
        {
            get { return (Brush)GetValue(SegmentColorProperty); }
            set { SetValue(SegmentColorProperty, value); }
        }

        public int StrokeThickness
        {
            get { return (int)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public double Percentage
        {
            get { return (double)GetValue(PercentageProperty); }
            set { SetValue(PercentageProperty, value); }
        }

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        // Using a BindableProperty as the backing store for Percentage.  This enables animation, styling, binding, etc...
        public static readonly BindableProperty PercentageProperty =
            BindableProperty.Create("Percentage", typeof(double), typeof(CircleProgressBar),
             65d, propertyChanged: OnPercentageChanged);

        // Using a BindableProperty as the backing store for StrokeThickness.  This enables animation, styling, binding, etc...
        public static readonly BindableProperty StrokeThicknessProperty =
            BindableProperty.Create("StrokeThickness", typeof(int), typeof(CircleProgressBar),
                5, propertyChanged: OnThicknessChanged);

        // Using a BindableProperty as the backing store for SegmentColor.  This enables animation, styling, binding, etc...
        public static readonly BindableProperty SegmentColorProperty =
            BindableProperty.Create("SegmentColor", typeof(Brush), typeof(CircleProgressBar),
                new SolidColorBrush(Color.Red), propertyChanged: OnColorChanged);

        // Using a BindableProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
        public static readonly BindableProperty RadiusProperty =
            BindableProperty.Create("Radius", typeof(int), typeof(CircleProgressBar),
                25, propertyChanged: OnPropertyChanged);

        // Using a BindableProperty as the backing store for Angle.  This enables animation, styling, binding, etc...
        public static readonly BindableProperty AngleProperty =
            BindableProperty.Create("Angle", typeof(double), typeof(CircleProgressBar),
                120d, propertyChanged: OnPropertyChanged);

        private static void OnColorChanged(BindableObject sender, object old, object newValue)
        {
            CircleProgressBar circle = sender as CircleProgressBar;
            circle.SetColor((SolidColorBrush)newValue);
        }

        private static void OnThicknessChanged(BindableObject sender, object old, object newValue)
        {
            CircleProgressBar circle = sender as CircleProgressBar;
            circle.SetThickness((int)newValue);
        }

        private static void OnPercentageChanged(BindableObject sender, object old, object newValue)
        {
            CircleProgressBar circle = sender as CircleProgressBar;
            if (circle.Percentage > 100) circle.Percentage = 100;
            circle.Angle = (circle.Percentage * 360) / 100;
        }

        private static void OnPropertyChanged(BindableObject sender, object old, object newValue)
        {
            CircleProgressBar circle = sender as CircleProgressBar;
            circle.RenderArc();
        }

        public void SetThickness(int n)
        {
            pathRoot.StrokeThickness = n;
        }

        public void SetColor(SolidColorBrush n)
        {
            pathRoot.Stroke = n;
        }

        public void RenderSpecificArc(Path pathRoot, PathFigure pathFigure, ArcSegment arcSegment, double angle)
        {
            Point startPoint = new Point(Radius, 0);
            Point endPoint = ComputeCartesianCoordinate(angle, Radius);
            endPoint.X += Radius;
            endPoint.Y += Radius;

            pathRoot.WidthRequest = Radius * 2 + StrokeThickness;
            pathRoot.HeightRequest = Radius * 2 + StrokeThickness;
            pathRoot.Margin = new Thickness(StrokeThickness, StrokeThickness, 0, 0);


            bool largeArc = Angle > 180.0;

            Size outerArcSize = new Size(Radius, Radius);

            pathFigure.StartPoint = startPoint;

            if (startPoint.X == Math.Round(endPoint.X) && startPoint.Y == Math.Round(endPoint.Y))
                endPoint.X -= 0.01;

            arcSegment.Point = endPoint;
            arcSegment.Size = outerArcSize;
            arcSegment.IsLargeArc = largeArc;
        }

        public void RenderArc()
        {
            RenderSpecificArc(this.grayPath, this.grayPathFigure, this.grayArcSegment, angle: 360);
            RenderSpecificArc(this.pathRoot, this.pathFigure, this.arcSegment, this.Angle);
        }

        private Point ComputeCartesianCoordinate(double angle, double radius)
        {
            // convert to radians
            double angleRad = (Math.PI / 180.0) * (angle - 90);

            double x = radius * Math.Cos(angleRad);
            double y = radius * Math.Sin(angleRad);

            return new Point(x, y);
        }
    }
}