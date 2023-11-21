using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static PlantClicker.MainWindow;
using Microsoft.VisualBasic;

namespace PlantClicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int clickIncome = 1;
        double passiveIncome = 0;
        static double score = 0;

        static Color activeUpgrade = new Color() { A = 100, R = 85, G = 117, B = 63 };
        static Color inactiveUpgrade = new Color() { A = 50, R = 65, G = 54, B = 23 };

        List<Upgrade> upgrades = new List<Upgrade>();

        DispatcherTimer gameClock = new DispatcherTimer();

        int time = 0;

        int sizeWidth = 0;
        int sizeHeight = 0;

        public class Upgrade
        {
            public string Name { get; set; }
            public int Price { get; set; }
            public int ClickValue { get; set; }
            public double PassiveValue { get; set; }
            public int Amount { get; set; }
            public bool IsUnique { get; }
            public BitmapImage Image { get; set; }
            public string ToolTip { get { return PassivePerSecond + " raindrops per second"; } }
            public bool IsAvailable { get { return CalcPrice(Price); } }
            public SolidColorBrush Background { get { return GetBackGround(IsAvailable); } }
            public double PassivePerSecond {  get { return PassiveValue * 100; } }
        }

        private static SolidColorBrush GetBackGround(bool isAvailable)
        {
            Color color = isAvailable ? activeUpgrade : inactiveUpgrade;
            SolidColorBrush brush = new SolidColorBrush(color);

            return brush;
        }

        public MainWindow()
        {
            InitializeComponent();
        }
        private static bool CalcPrice(int price)
        {
            return score >= price;
        }
        private void InitializeShop()
        {
            upgrades.Add(new Upgrade() { Name = "Watering can", Price = 15, ClickValue = 1, PassiveValue = 0.001, Image = LoadNewImage("watering_can") });
            upgrades.Add(new Upgrade() { Name = "Gardener", Price = 100, ClickValue = 10, PassiveValue = 0.01, Image = LoadNewImage("gardener") });
            upgrades.Add(new Upgrade() { Name = "Farm", Price = 1100, ClickValue = 100, PassiveValue = 0.08, Image = LoadNewImage("farm") });
            upgrades.Add(new Upgrade() { Name = "Raincloud", Price = 12000, ClickValue = 250, PassiveValue = 0.47, Image = LoadNewImage("rain") });
            upgrades.Add(new Upgrade() { Name = "Weatherstation", Price = 130000, ClickValue = 500, PassiveValue = 2.60, Image = LoadNewImage("system") });
            upgrades.Add(new Upgrade() { Name = "DebugUpgrade", Price = 1, ClickValue = 500000, PassiveValue = 100, Image = LoadNewImage("system") });
            LstShop.ItemsSource = upgrades;
        }

        Canvas myCanvas = new Canvas();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeShop();
            InitializeClock();
            SetSize();
            myCanvas.Name = "animationCanvas";
            this.RegisterName(myCanvas.Name, myCanvas);
            MakeItRain();
        }
        private void GameClock_Tick(object sender, EventArgs e)
        {
            time += 1;
            score += passiveIncome;
            Draw();
        }
        private void SetSize()
        {
            sizeWidth = (int)ImgIncrement.ActualWidth;
            sizeHeight = (int)ImgIncrement.ActualHeight;
        }

        private void InitializeClock()
        {
            gameClock.Tick += new EventHandler(GameClock_Tick);
            gameClock.Interval = TimeSpan.FromMilliseconds(10);
            gameClock.Start();
        }



        private BitmapImage LoadNewImage(string fileName = "user", string fileExtension = "png")
        {
            // TO-DO: use caching instead
            return new BitmapImage(new Uri($"/PlantClicker;component/Image/{fileName}.{fileExtension}", UriKind.Relative));
        }

        private void PurchaseUpgrade(Upgrade upgrade)
        {
            if (score < upgrade.Price)
            {
                MessageBox.Show("Not enough money!");
                return;
            }
            else
            {
                score -= upgrade.Price;
                clickIncome += upgrade.ClickValue;
                passiveIncome += upgrade.PassiveValue;
                ++upgrade.Amount;
            }
            LstShop.Items.Refresh();
        }

        private void ShopItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int index = LstShop.Items.IndexOf(LstShop.SelectedItem);
            Upgrade item = upgrades[index];

            PurchaseUpgrade(item);
        }

        private void Increment_Click(object sender, RoutedEventArgs e)
        {
            score += clickIncome;
            LstShop.Items.Refresh();
        }

        private void Draw()
        {
            DrawScore();
            DrawIncome();
            //DrawClock();
            DrawDebug();
        }

        private void DrawScore()
        {
            TxtScore.Text = Math.Truncate(score).ToString();
        }
        private void DrawIncome()
        {
            TxtClickIncome.Text = clickIncome.ToString();
            TxtPassiveIncome.Text = (100 * passiveIncome).ToString() + "/s";
        }
        private void DrawClock()
        {
            //TxtTime.Text = FormatNumber(time);
        }

        private void DrawDebug()
        {
            DebugClickIncome.Text = clickIncome.ToString();
            DebugPassiveIncome.Text = (100*passiveIncome).ToString();
            DebugScore.Text = score.ToString();
        }

        private void Increment_MouseUp(object sender, MouseButtonEventArgs e)
        {
            score += clickIncome;
            LstShop.Items.Refresh();
            ImgIncrement.Width = sizeWidth + 25;
        }

        private void Increment_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ImgIncrement.Width = sizeWidth - 25;
        }

        private void Increment_MouseLeave(object sender, MouseEventArgs e)
        {
            ImgIncrement.Width = sizeWidth;
        }

        private string FormatNumber(double numberToFormat)
        {
            string stringToFormat = ((int)numberToFormat).ToString();
            string formattedString = stringToFormat;

            /*if (formattedString.Length <= 0)
            {
                return formattedString;
            };

            string suffix = "";
            int divider = 1;

            if(numberToFormat >= 1000 && numberToFormat < 1000000)
            {
                divider = 1000;
                suffix = "K";
                //formattedString = (numberToFormat / 1000).ToString() + "K";
                //formattedString = stringToFormat.Substring(0, stringToFormat.Length - 3) + "K";
            }
            else if (numberToFormat >= 1000000 && numberToFormat < 999999999)
            {
                divider = 1000000;
                suffix = "M";
                //formattedString = stringToFormat.Substring(0, stringToFormat.Length - 6) + "M";
            }
            else if (numberToFormat >= 1000000000)
            {
                divider = 1000000000;
                suffix = "B";
                //formattedString = (numberToFormat / 1000000000).ToString() + "B";
                //formattedString = stringToFormat.Substring(0, stringToFormat.Length - 6) + "B";
            }

            // formattedString = (numberToFormat / divider).ToString();
            // formattedString = formattedString.Length > 3 ? formattedString.Substring(0, 4) : formattedString;

            switch (suffix)
            {
                case "K":
                    break;
                case "M":
                    break;
                case "B":
                    break;
                default:
                    break;
            }*/

            //formattedString = formattedString.Length > 3 ? formattedString.Substring(0, formattedString.Length - 3) : formattedString;
            return numberToFormat.ToString("N0");
        }

        private Storyboard rainStoryboard;
        private Storyboard fallingStoryboard;


        Rectangle originalRectangle = new Rectangle();

        Image raindrop = new Image();
        private void MakeItRain()
        {
            // https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/animation-overview?view=netframeworkdesktop-4.8
            var myPanel = new StackPanel();
            myPanel.Margin = new Thickness(10);

            raindrop = new Image();
            raindrop.Source = LoadNewImage("drop");
            raindrop.Name = "fallingRaindrop";
            this.RegisterName(raindrop.Name, raindrop);
            raindrop.Width = 100;
            raindrop.Height = 100;
            myCanvas.Children.Add(raindrop);


            // spawn an object
            originalRectangle = new Rectangle();
            originalRectangle.Name = "originalRectangle";
            this.RegisterName(originalRectangle.Name, originalRectangle);
            originalRectangle.Width = 200;
            originalRectangle.Height = 50;
            originalRectangle.Fill = Brushes.Yellow;

            myCanvas.Children.Add(originalRectangle);

            // spawn the drop
            // Raindrop drop = new Raindrop() { Image = LoadNewImage("drop"), VerticalPosition = 0, HorizontalPosition = 0, FallingSpeed = 1 };
            // let it fall
            //int topPosition = 0;
            //int bottomPosition = 0;

            // make an animation
            var rainAnimation = new DoubleAnimation();
            rainAnimation.From = 1.0;
            rainAnimation.To = 0.0;
            rainAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
            rainAnimation.AutoReverse = true;
            rainAnimation.RepeatBehavior = RepeatBehavior.Forever;

            // storyboard for the animation
            rainStoryboard = new Storyboard();
            rainStoryboard.Children.Add(rainAnimation);

            // assign the storyboard to the spawned object
            Storyboard.SetTargetName(rainAnimation, originalRectangle.Name);
            Storyboard.SetTargetProperty(rainAnimation, new PropertyPath(Rectangle.OpacityProperty));

            // add handler on spawn of object
            originalRectangle.Loaded += new RoutedEventHandler(myRectangleLoaded);
            // myPanel.Children.Add(originalRectangle);

           // myCanvas.Loaded += new RoutedEventHandler(myCanvasLoaded);

           // this.Content = myCanvas;

        }

        private void myRectangleLoaded(object sender, RoutedEventArgs e)
        {
            var fallingAnimation = new DoubleAnimation();

            fallingAnimation.From = 0;

            fallingAnimation.To = 1000;
            fallingAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
            fallingAnimation.AutoReverse = true;
            //fallingAnimation.RepeatBehavior = RepeatBehavior.Forever;

            fallingStoryboard = new Storyboard();
            fallingStoryboard.Children.Add(fallingAnimation);

            Storyboard.SetTargetName(fallingAnimation, raindrop.Name);
            
            Storyboard.SetTargetProperty(fallingAnimation, new PropertyPath(Canvas.TopProperty));
            fallingStoryboard.Begin(this);
            rainStoryboard.Begin(this);
        }

        private void RadialAnimation()
        {
            // radial transform
        }
    

        private void TranslateTransformSample()
        {
            Canvas myCanvas = new Canvas();
            var rainAnimation = new DoubleAnimation();
            double fromValue = 0;
            double toValue = 0;
            double.TryParse(Canvas.TopProperty.ToString(), out fromValue);
            double.TryParse(Canvas.BottomProperty.ToString(), out toValue);
            rainAnimation.From = fromValue;
            rainAnimation.To = toValue;
            rainAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
            // storyboard for the animation
            rainStoryboard = new Storyboard();
            rainStoryboard.Children.Add(rainAnimation);

            Rectangle movedRectangle = new Rectangle();
            movedRectangle.Name = "movingBox";
            this.RegisterName(movedRectangle.Name, movedRectangle);
            movedRectangle.Width = 200;
            movedRectangle.Height = 50;
            movedRectangle.Fill = Brushes.Blue;
            movedRectangle.Opacity = 0.5;
            TranslateTransform translateTransform1 = new TranslateTransform(50, 20);
            movedRectangle.RenderTransform = translateTransform1;

            myCanvas.Children.Add(movedRectangle);

            Storyboard.SetTargetName(rainAnimation, movedRectangle.Name);
            Storyboard.SetTargetProperty(rainAnimation, new PropertyPath(Canvas.TopProperty));
            rainStoryboard.Begin();

            Rectangle originalRectangle = new Rectangle();
            originalRectangle.Width = 200;
            originalRectangle.Height = 50;
            originalRectangle.Fill = Brushes.Yellow;
            //AnimationCanvas.Children.Add(originalRectangle);

            movedRectangle = new Rectangle();
            movedRectangle.Width = 200;
            movedRectangle.Height = 50;
            movedRectangle.Fill = Brushes.Blue;
            movedRectangle.Opacity = 0.5;
            //TranslateTransform translateTransform1 = new TranslateTransform(50, 20);
            movedRectangle.RenderTransform = translateTransform1;

            //AnimationCanvas.Children.Add(movedRectangle);
        }

        private void MoveStuff(Rectangle thingToMove)
        {
            TranslateTransform translateTransform1 = new TranslateTransform(50, 20);
            thingToMove.RenderTransform = translateTransform1;
        }

        public class Raindrop
        {
            public int HorizontalPosition { get; set; }
            public int VerticalPosition { get; set; }
            public BitmapImage Image { get; set; }
            public int FallingSpeed {  get; set; }
        }
    }
}
