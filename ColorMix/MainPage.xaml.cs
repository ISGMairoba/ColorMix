using System.Collections.ObjectModel;

namespace ColorMix
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Palette> Palettes { get; set; } = new();
        public MainPage()
        {
            InitializeComponent();
            InitializePallete();
            BindingContext = this;
        }

        private void InitializePallete()
        {
            // 9. Earth Tones (Brown, Sienna, Tan, Olive, Yellow, White, ratio 2:2:1:1:1:1 -> total 8, percentages: 25%, 25%, 13%, 13%, 13%, 13%)
            var palette9 = new Palette("Earth Tones", "#A52A2A", Colors.Brown);
            palette9.PaletteColors.Add(new MixColor("Brown", Colors.Brown, 25, 2));
            palette9.PaletteColors.Add(new MixColor("Sienna", Colors.Sienna, 25, 2));
            palette9.PaletteColors.Add(new MixColor("Tan", Colors.Tan, 13, 1));
            palette9.PaletteColors.Add(new MixColor("Olive", Colors.Olive, 13, 1));
            palette9.PaletteColors.Add(new MixColor("Yellow", Colors.Yellow, 13, 1));
            palette9.PaletteColors.Add(new MixColor("White", Colors.White, 13, 1));
            Palettes.Add(palette9);

            // 1. Pink palette with red and white (1:1 ratio -> each 50%)
            var palette1 = new Palette("Pink", "#FFC0CB", Colors.Pink);
            palette1.PaletteColors.Add(new MixColor("Red", Colors.Red, 50, 1));
            palette1.PaletteColors.Add(new MixColor("White", Colors.White, 50, 1));
            Palettes.Add(palette1);

            // 7. Coral Beach (Red, Orange, Yellow, Pink, White, ratio 3:2:1:1:1 -> total 8, percentages: 38%, 25%, 13%, 13%, 13%)
            var palette7 = new Palette("Coral Beach", "#FA8072", Colors.Salmon);
            palette7.PaletteColors.Add(new MixColor("Red", Colors.Red, 38, 3));
            palette7.PaletteColors.Add(new MixColor("Orange", Colors.Orange, 25, 2));
            palette7.PaletteColors.Add(new MixColor("Yellow", Colors.Yellow, 13, 1));
            palette7.PaletteColors.Add(new MixColor("Pink", Colors.Pink, 13, 1));
            palette7.PaletteColors.Add(new MixColor("White", Colors.White, 13, 1));
            Palettes.Add(palette7);


            // 2. Yellowish Pink with red, yellow and white (3:1:1 ratio -> red=60%, yellow=20%, white=20%)
            var palette2 = new Palette("Yellowish Pink", "#FFB6C1", Colors.LightPink);
            palette2.PaletteColors.Add(new MixColor("Red", Colors.Red, 60, 3));
            palette2.PaletteColors.Add(new MixColor("Yellow", Colors.Yellow, 20, 1));
            palette2.PaletteColors.Add(new MixColor("White", Colors.White, 20, 1));
            Palettes.Add(palette2);

            // 3. Sunset (Orange & Red, 1:1 ratio -> each 50%)
            var palette3 = new Palette("Sunset", "#FF4500", Colors.OrangeRed);
            palette3.PaletteColors.Add(new MixColor("Orange", Colors.Orange, 50, 1));
            palette3.PaletteColors.Add(new MixColor("Red", Colors.Red, 50, 1));
            Palettes.Add(palette3);

            // 4. Teal Glow (Teal, Cyan, White, 2:1:1 ratio -> teal=50%, cyan=25%, white=25%)
            var palette4 = new Palette("Teal Glow", "#008080", Colors.Teal);
            palette4.PaletteColors.Add(new MixColor("Teal", Colors.Teal, 50, 2));
            palette4.PaletteColors.Add(new MixColor("Cyan", Colors.Cyan, 25, 1));
            palette4.PaletteColors.Add(new MixColor("White", Colors.White, 25, 1));
            Palettes.Add(palette4);

            // 5. Royal Purple (Purple, Violet, White, 3:1:2 ratio -> purple=50%, violet≈17%, white≈33%)
            var palette5 = new Palette("Royal Purple", "#800080", Colors.Purple);
            palette5.PaletteColors.Add(new MixColor("Purple", Colors.Purple, 50, 3));
            palette5.PaletteColors.Add(new MixColor("Violet", Colors.Violet, 17, 1));
            palette5.PaletteColors.Add(new MixColor("White", Colors.White, 33, 2));
            Palettes.Add(palette5);

            // 6. Green Field (Green, Lime, Yellow, 2:1:1 ratio -> green=50%, lime=25%, yellow=25%)
            var palette6 = new Palette("Green Field", "#32CD32", Colors.LimeGreen);
            palette6.PaletteColors.Add(new MixColor("Green", Colors.Green, 50, 2));
            palette6.PaletteColors.Add(new MixColor("Lime", Colors.Lime, 25, 1));
            palette6.PaletteColors.Add(new MixColor("Yellow", Colors.Yellow, 25, 1));
            Palettes.Add(palette6);

           
            // 8. Deep Ocean (Navy, Blue, Cyan, Teal, Gray, White, ratio 2:2:1:1:1:1 -> total 8, percentages: 25%, 25%, 13%, 13%, 13%, 13%)
            var palette8 = new Palette("Deep Ocean", "#000080", Colors.Navy);
            palette8.PaletteColors.Add(new MixColor("Navy", Colors.Navy, 25, 2));
            palette8.PaletteColors.Add(new MixColor("Blue", Colors.Blue, 25, 2));
            palette8.PaletteColors.Add(new MixColor("Cyan", Colors.Cyan, 13, 1));
            palette8.PaletteColors.Add(new MixColor("Teal", Colors.Teal, 13, 1));
            palette8.PaletteColors.Add(new MixColor("Gray", Colors.Gray, 13, 1));
            palette8.PaletteColors.Add(new MixColor("White", Colors.White, 13, 1));
            Palettes.Add(palette8);

            

            // 10. Rainbow Spectrum (Red, Orange, Yellow, Green, Blue, Indigo, Violet, White; equal ratios -> each ≈13%)
            var palette10 = new Palette("Rainbow Spectrum", "#FF0000", Colors.Red);
            palette10.PaletteColors.Add(new MixColor("Red", Colors.Red, 13, 1));
            palette10.PaletteColors.Add(new MixColor("Orange", Colors.Orange, 13, 1));
            palette10.PaletteColors.Add(new MixColor("Yellow", Colors.Yellow, 13, 1));
            palette10.PaletteColors.Add(new MixColor("Green", Colors.Green, 13, 1));
            palette10.PaletteColors.Add(new MixColor("Blue", Colors.Blue, 13, 1));
            palette10.PaletteColors.Add(new MixColor("Indigo", Colors.Indigo, 13, 1));
            palette10.PaletteColors.Add(new MixColor("Violet", Colors.Violet, 13, 1));
            palette10.PaletteColors.Add(new MixColor("White", Colors.White, 13, 1));
            Palettes.Add(palette10);

            // 11. Lavender Dream (Lavender, Thistle, Plum, White, ratio 3:2:1:1 -> total 7, percentages: 43%, 29%, 14%, 14%)
            var palette11 = new Palette("Lavender Dream", "#E6E6FA", Colors.Lavender);
            palette11.PaletteColors.Add(new MixColor("Lavender", Colors.Lavender, 43, 3));
            palette11.PaletteColors.Add(new MixColor("Thistle", Colors.Thistle, 29, 2));
            palette11.PaletteColors.Add(new MixColor("Plum", Colors.Plum, 14, 1));
            palette11.PaletteColors.Add(new MixColor("White", Colors.White, 14, 1));
            Palettes.Add(palette11);

            // 12. Forest Mix (DarkGreen, ForestGreen, Olive, LimeGreen, YellowGreen, ratio 4:3:2:1:1 -> total 11, percentages: DarkGreen=37%, ForestGreen=27%, Olive=18%, LimeGreen=9%, YellowGreen=9%)
            var palette12 = new Palette("Forest Mix", "#006400", Colors.DarkGreen);
            palette12.PaletteColors.Add(new MixColor("DarkGreen", Colors.DarkGreen, 37, 4));
            palette12.PaletteColors.Add(new MixColor("ForestGreen", Colors.ForestGreen, 27, 3));
            palette12.PaletteColors.Add(new MixColor("Olive", Colors.Olive, 18, 2));
            palette12.PaletteColors.Add(new MixColor("LimeGreen", Colors.LimeGreen, 9, 1));
            palette12.PaletteColors.Add(new MixColor("YellowGreen", Colors.YellowGreen, 9, 1));
            Palettes.Add(palette12);

            // 13. Ocean Breeze (DeepSkyBlue, LightSeaGreen, Aqua, White, ratio 2:1:1:1 -> total 5, percentages: DeepSkyBlue=40%, others=20%)
            var palette13 = new Palette("Ocean Breeze", "#00BFFF", Colors.DeepSkyBlue);
            palette13.PaletteColors.Add(new MixColor("DeepSkyBlue", Colors.DeepSkyBlue, 40, 2));
            palette13.PaletteColors.Add(new MixColor("LightSeaGreen", Colors.LightSeaGreen, 20, 1));
            palette13.PaletteColors.Add(new MixColor("Aqua", Colors.Aqua, 20, 1));
            palette13.PaletteColors.Add(new MixColor("White", Colors.White, 20, 1));
            Palettes.Add(palette13);

            // 14. Fire Blaze (Firebrick, OrangeRed, Gold, ratio 3:2:1 -> total 6, percentages: Firebrick=50%, OrangeRed≈33%, Gold≈17%)
            var palette14 = new Palette("Fire Blaze", "#B22222", Colors.Firebrick);
            palette14.PaletteColors.Add(new MixColor("Firebrick", Colors.Firebrick, 50, 3));
            palette14.PaletteColors.Add(new MixColor("OrangeRed", Colors.OrangeRed, 33, 2));
            palette14.PaletteColors.Add(new MixColor("Gold", Colors.Gold, 17, 1));
            Palettes.Add(palette14);

            // 15. Mystic Night (MidnightBlue, DarkSlateBlue, Indigo, Purple, Black, Gray, ratio 2:2:1:1:1:1 -> total 8
            //    We'll use: MidnightBlue=25%, DarkSlateBlue=25%, Indigo=12%, Purple=12%, Black=13%, Gray=13%)
            var palette15 = new Palette("Mystic Night", "#191970", Colors.MidnightBlue);
            palette15.PaletteColors.Add(new MixColor("MidnightBlue", Colors.MidnightBlue, 25, 2));
            palette15.PaletteColors.Add(new MixColor("DarkSlateBlue", Colors.DarkSlateBlue, 25, 2));
            palette15.PaletteColors.Add(new MixColor("Indigo", Colors.Indigo, 12, 1));
            palette15.PaletteColors.Add(new MixColor("Purple", Colors.Purple, 12, 1));
            palette15.PaletteColors.Add(new MixColor("Black", Colors.Black, 13, 1));
            palette15.PaletteColors.Add(new MixColor("Gray", Colors.Gray, 13, 1));
            Palettes.Add(palette15);

            // 1. Pink palette with red and white (1:1 ratio -> each 50%)
            var palette16 = new Palette("Pink", "#FFC0CB", Colors.Pink);
            palette16.PaletteColors.Add(new MixColor("Red", Colors.Red, 50, 1));
            palette16.PaletteColors.Add(new MixColor("White", Colors.White, 50, 1));
            Palettes.Add(palette16);
        }


    }

}
