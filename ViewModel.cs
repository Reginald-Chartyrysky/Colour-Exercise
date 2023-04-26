using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Media;
using System.Reflection;

namespace ColourExercise
{
    class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }




        private ColorWithName _selectedColor;

        public ColorWithName SelectedColor { get => _selectedColor; 
            set { _selectedColor = value; if (!ContrastColors.Contains(SelectedContrastColor)) { try { SelectedContrastColor = ContrastColors[0]; } catch { SelectedContrastColor = WrapWithName(Colors.Black); } }; UpdateAllRelevantProperties(); } }

        void UpdateAllRelevantProperties()
        {
            OnPropertyChanged("SelectedColor"); 
            OnPropertyChanged("SelectedColorBrush"); 
            OnPropertyChanged("ComplimentaryColor"); 
            OnPropertyChanged("ContrastColors");
            UpdateFormula();
        }

        private ColorWithName _selectedContrastColor;

        public ColorWithName SelectedContrastColor { get => _selectedContrastColor; set { _selectedContrastColor = value; OnPropertyChanged("SelectedContrastColor"); OnPropertyChanged("ComplimentaryColor"); UpdateFormula(); } }




     
        public SolidColorBrush SelectedColorBrush { get => new SolidColorBrush(SelectedColor.Color);  }

     
        public SolidColorBrush ComplimentaryColor { get => new SolidColorBrush(SelectedContrastColor.Color); }


        private List<ColorWithName> _allColors = ColorStructToList();
        public  List<ColorWithName> AllColors { get=> _allColors; }
        public List<ColorWithName> ContrastColors { get => GetContrastColors(SelectedColor.Color); }

        private string _formula;

        public string Formula { get => _formula; set { _formula = value; OnPropertyChanged("Formula");  } }

        double _mark = 7;

        public double Mark { get => _mark; set { _mark = value; OnPropertyChanged("Mark"); OnPropertyChanged("ContrastColors"); } }


        public static List<ColorWithName> ColorStructToList()
        {

            var colors = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public)
                                .Select(c => (Color)c.GetValue(null, null))
                                .ToList();
            colors.Count();

            List<ColorWithName> getColorsWithName = new List<ColorWithName>();

            foreach (var col in colors)
            {
                getColorsWithName.Add( WrapWithName(col) );
            }

            return getColorsWithName;


        }




        public void UpdateFormula()
        {
            Func<Color, double> Algorithm = GetLuminance3;

            Formula = $" {DoGetLuminance(SelectedColor.Color, Algorithm)} + 0.05 / {DoGetLuminance(SelectedContrastColor.Color, Algorithm)} +0.05 = ";
            Formula += ((DoGetLuminance(SelectedColor.Color, Algorithm) + 0.05) / (DoGetLuminance(SelectedContrastColor.Color, Algorithm) + 0.05)).ToString("0.000");
            Formula += " ("+ ((DoGetLuminance(SelectedContrastColor.Color, Algorithm) + 0.05) / (DoGetLuminance(SelectedColor.Color, Algorithm) + 0.05)).ToString("0.000") +")";

        }
        

        public List<ColorWithName> GetContrastColors(Color oldColor)
        {

            Func<Color, double> Algorithm = GetLuminance3;

            double oldLuminance = DoGetLuminance(oldColor, Algorithm);
            List<ColorWithName> getColors = new List<ColorWithName>();

            foreach (var col in AllColors)
            {
                if (   ( (oldLuminance +0.05 ) / (DoGetLuminance(col.Color, Algorithm) + 0.05) ) > _mark || ((DoGetLuminance(col.Color, Algorithm) + 0.05) / ( oldLuminance + 0.05)) > _mark)
                {
                    getColors.Add(col);
                    
                }
            }
            

            
            return getColors;
        }


        public double DoGetLuminance(Color color, Func<Color,double> getLum)
        {
            return getLum(color);
        }



        public double GetLuminance1(Color color)
        {

            double vR = color.R / (double)255;
            double vG = color.G / (double)255;
            double vB = color.B / (double)255;

            return 0.2126 * sRGBtoLin(vR) + 0.7152 * sRGBtoLin(vG) + 0.0722 * sRGBtoLin(vB);
        }


       public double sRGBtoLin( double colorChannel)
        {
            // Send this function a decimal sRGB gamma encoded color value
            // between 0.0 and 1.0, and it returns a linearized value.

            if (colorChannel <= 0.04045)
            {
                return colorChannel / 12.92;
            }
            else
            {
                return Math.Pow(((colorChannel + 0.055) / 1.055), 2.4);
            }
        }


        public double GetLuminance2(Color color)
        {

            double vR = color.R / (double)255;
            double vG = color.G / (double)255;
            double vB = color.B / (double)255;

            return 0.299 * sRGBtoLin(vR) + 0.587 * sRGBtoLin(vG) + 0.114 * sRGBtoLin(vB);
        }

        public double GetLuminance3(Color color)
        {
            double vR = color.R / (double)255;
            double vG = color.G / (double)255;
            double vB = color.B / (double)255;

            return Math.Sqrt( 0.299 * Math.Pow(sRGBtoLin(vR), 2) + 0.587 * Math.Pow(sRGBtoLin(vG), 2) + 0.114 * Math.Pow(sRGBtoLin(vB), 2));
        }


        public static ColorWithName WrapWithName(Color color)
        {
            return new ColorWithName
            {
                Color = color,
                Name = GetColorName(color)
            };
        }


        public static string GetColorName(Color color)
        {
            Type colors = typeof(System.Windows.Media.Colors);
            foreach (var prop in colors.GetProperties())
            {
                if (((System.Windows.Media.Color)prop.GetValue(null, null)) == color)
                    return prop.Name;
            }

            return "The provided Color is not named.";
        }


        public struct ColorWithName
        {
           public Color Color { get; set; }
           public string Name { get; set; }
        }

    }
}
