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
using System.Text.RegularExpressions;

//using Excel = Microsoft.Office.Interop.Excel;

namespace Recomenmovies2
{
    public partial class MainWindow : Window
    {

        /*//zmienne potrzebne do załadowania Excela
        /Excel.Application myApp;
        Excel.Workbook myWorkBook;
        Excel.Worksheet myWorkSheet;
        Excel.Range myRange;
        int rows;
        int cols;*/

        List<int> ListOfYears = new List<int>();
        List<string> ListOfTitles = new List<string>();
        List<string> ListOfGenres = new List<string>();
        List<string> ListOfCountries = new List<string>();
        List<string> ListOfLanguages = new List<string>();
        List<int> ListOfDurations = new List<int>();
        List<double> ListOfRatings = new List<double>();
        List<double> ListOfPopularity = new List<double>();

        int YearFrom;
        int YearTo;
        float rating;
        float popularity;
        int DurationFrom;
        int DurationTo;

        bool Years = false;
        bool Genre = false;
        bool Country = false;
        bool Languages = false;
        bool Duration = false;
        bool Rating = false;
        bool Popularity = false;

        //Lists with elements to view and choose
        List<string> countries_items_origin;
        List<string> languages_items_origin;
        List<string> genres_items_origin;
        List<string> countries_items;
        List<string> languages_items;
        List<string> genres_items;


        public MainWindow()
        {
            InitializeComponent();

            //Initialization of components
            string line;
            countries_items_origin = new List<string>();
            System.IO.StreamReader countries_file = new System.IO.StreamReader("countries.txt");
            while ((line = countries_file.ReadLine()) != null)
                countries_items_origin.Add(line);
            countries_file.Close();

            languages_items_origin = new List<string>();
            System.IO.StreamReader languages_file = new System.IO.StreamReader("languages.txt");
            while ((line = languages_file.ReadLine()) != null)
                languages_items_origin.Add(line);
            languages_file.Close();

            genres_items_origin = new List<string>();
            System.IO.StreamReader genres_file = new System.IO.StreamReader("genres.txt");
            while ((line = genres_file.ReadLine()) != null)
                genres_items_origin.Add(line);
            genres_file.Close();

            countries_items = countries_items_origin;
            languages_items = languages_items_origin;
            genres_items = genres_items_origin;

            // Refresh
            RefreshView();

        }

        private void RefreshView()
        {
            CountryListBox.ItemsSource = countries_items;
            LanguageListBox.ItemsSource = languages_items;
            GenresListBox.ItemsSource = genres_items;

            countries_items = countries_items_origin;
            languages_items = languages_items_origin;
            genres_items = genres_items_origin;
        }

        // Loading excel
        private void OnClickStart(object sender, RoutedEventArgs e)
        {
            //Zanim cokolwiek zrobisz musisz dać to Click w Start bo to ładuje Excela z danymi
            /* myApp = new Excel.Application();
             //TUTAJ zmien sobie ścieżkę 
             myWorkBook = myApp.Workbooks.Open(@"C:\temp\dane.xlsx", 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
             myWorkSheet = (Excel.Worksheet)myWorkBook.Worksheets.get_Item(1);
             myRange = myWorkSheet.UsedRange;
             rows = myRange.Rows.Count;
             cols = myRange.Columns.Count;*/

            string line;
            System.IO.StreamReader countriy_file = new System.IO.StreamReader("country.txt");
            while ((line = countriy_file.ReadLine()) != null)
                ListOfCountries.Add(line);
            countriy_file.Close();


            System.IO.StreamReader title_file = new System.IO.StreamReader("title.txt");
            while ((line = title_file.ReadLine()) != null)
                ListOfTitles.Add(line);
            title_file.Close();


            System.IO.StreamReader genre_file = new System.IO.StreamReader("genre.txt");
            while ((line = genre_file.ReadLine()) != null)
                ListOfGenres.Add(line);
            genre_file.Close();


            System.IO.StreamReader lan_file = new System.IO.StreamReader("language.txt");
            while ((line = lan_file.ReadLine()) != null)
                ListOfLanguages.Add(line);
            lan_file.Close();



            int x;
            System.IO.StreamReader year_file = new System.IO.StreamReader("year.txt");
            while ((line = year_file.ReadLine()) != null)
            {
                Int32.TryParse(line, out x);
                ListOfYears.Add(x);
            }
            year_file.Close();


            System.IO.StreamReader dur_file = new System.IO.StreamReader("duration.txt");
            while ((line = dur_file.ReadLine()) != null)
            {
                Int32.TryParse(line, out x);
                ListOfDurations.Add(x);
            }
            dur_file.Close();


            System.IO.StreamReader rating_file = new System.IO.StreamReader("rating.txt");
            while ((line = rating_file.ReadLine()) != null)
                ListOfRatings.Add(Convert.ToDouble(line));
            rating_file.Close();


            System.IO.StreamReader pop_file = new System.IO.StreamReader("popularity.txt");
            while ((line = pop_file.ReadLine()) != null)
                ListOfPopularity.Add(Convert.ToDouble(line));
            pop_file.Close();

        }

        // Event on change of slider - from year
        private void Slider_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                YearFrom = (int)slider.Value + 1915;
                FromYearTextBlock.Text = YearFrom.ToString();
            }
        }

        // Event on change of slider - to year
        private void Slider_DragDelta_1(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                YearTo = (int)slider.Value + 1915;
                ToYearTextBlock.Text = YearTo.ToString();
                if (YearTo < YearFrom)
                {
                    ToYearTextBlock.Background = Brushes.OrangeRed;
                    YearTo = YearFrom;
                }
                else
                {
                    ToYearTextBlock.Background = Brushes.White;
                }
            }
        }

        private void FromDuration_SelectionChanged(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text != "")
            {
                DurationFrom = Int32.Parse(textBox.Text);
            }
        }

        private void ToDuration_SelectionChanged(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text != "")
            {
                DurationTo = Int32.Parse(textBox.Text);
                if (DurationTo < DurationFrom)
                {
                    textBox.Background = Brushes.OrangeRed;
                    DurationTo = DurationFrom;
                }
                else
                {
                    textBox.Background = Brushes.White;
                }
            }

        }

        // Event on change of slider - to rating
        private void Slider_DragDelta_2(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                rating = (float)slider.Value;
                AverageRating.Text = rating.ToString("0.#");
            }
        }

        // Event on change of slider - to rating
        private void Slider_DragDelta_3(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                popularity = (int)slider.Value;
                PopularitySlider.Text = popularity.ToString();
            }
        }

        // Action that triggers on every change of check box from Summary group box
        private void AnyCheckBoxAction(object sender, RoutedEventArgs e)
        {

            Years = YearsCheckBox.IsChecked.GetValueOrDefault();
            Genre = GenreCheckBox.IsChecked.GetValueOrDefault();
            Country = CountryCheckBox.IsChecked.GetValueOrDefault();
            Languages = LanguageCheckBox.IsChecked.GetValueOrDefault();
            Duration = DurationCheckBox.IsChecked.GetValueOrDefault();
            Rating = RatingCheckBox.IsChecked.GetValueOrDefault();
            Popularity = PopularityCheckBox.IsChecked.GetValueOrDefault();

            if (Years)
            {
                YearsGroupBox.IsEnabled = true;
            }
            else
            {
                YearsGroupBox.IsEnabled = false;
            }

            if (Genre)
            {
                GenreGroupBox.IsEnabled = true;
            }
            else
            {
                GenreGroupBox.IsEnabled = false;
            }

            if (Country)
            {
                CountryGroupBox.IsEnabled = true;
            }
            else
            {
                CountryGroupBox.IsEnabled = false;
            }

            if (Languages)
            {
                LanguageGroupBox.IsEnabled = true;
            }
            else
            {
                LanguageGroupBox.IsEnabled = false;
            }

            if (Duration)
            {
                DurationGroupBox.IsEnabled = true;
            }
            else
            {
                DurationGroupBox.IsEnabled = false;
            }

            if (Rating)
            {
                RatingGroupBox.IsEnabled = true;
            }
            else
            {
                RatingGroupBox.IsEnabled = false;
            }

            if (Popularity)
            {
                PopularityGroupBox.IsEnabled = true;
            }
            else
            {
                PopularityGroupBox.IsEnabled = false;
            }


        }

        // Sprawdza czy wpisane są liczby
        private void PreviewOnlyNumbers(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //Start recommendations oin click
        //jedyna funkcja którą zmieniłam
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //ta zmienna mówi ile pól w excelu przeszukujemy - przy 10000 jest już baaardzo długo
            //uwaga! excel jest posortowany popularnoscia, wiec bierzemy te najbardziej znane filmy
            int rangeOfSearch = ListOfYears.Count;
            //tablica prawdopodobienstw
            double[] TabOfMembership = new double[rangeOfSearch];
            //zerujemy nasz "prawodopodobieństwo"
            for (int i = 0; i < rangeOfSearch; i++)
            {
                TabOfMembership[i] = 0.0;
            }
            //rok prosukcji
            if (Years)
            {
                //tutaj mamy pętle, która spradza zawartość komurki w excelu w różnych rzędach i w 2 kolumnie
                //czyli właśnie lata
                int year;
                for (int rw = 0; rw < rangeOfSearch; rw++)
                {
                    year = ListOfYears[rw];
                    //jeśli rok mieści się w podanych założeniach
                    if (year <= YearTo && year >= YearFrom)
                    {
                        //to robimy średnia srytmetyczna z jego membership dagree i 1.0
                        //to jest taki tylko moj pomysl na szybko i jak najbardziej mozesz to modyfikować
                        TabOfMembership[rw] = (TabOfMembership[rw] + 1.0) / 2.0;
                        //tu sprawdzamy czy rok jest odrobinę za niski lub odrobinę za wysoki
                        //musza byc takie dlugie warunki bo się sypie inaczej
                    }
                    else if ((year <= (YearTo + 10) && year > YearTo) || (year >= (YearFrom - 10) && year < YearFrom))
                    {
                        //tutaj obliczamy różnice czyli ile naszej komurce z excela brakuje do poprawnego wyniku
                        //czyli jak na przyklad mamy lata 1920-1930, a rok jest 1932, 
                        //to mu przypiszemy coś trochę mniejszego niż jeden
                        double dif;
                        if ((year >= (YearFrom - 10) && year < YearFrom))
                        {
                            dif = YearFrom - year;
                            dif = 1.0 - dif / 10.0;
                        }
                        else
                        {
                            dif = year - YearTo;
                            dif = 1.0 - dif / 10.0;
                        }
                        TabOfMembership[rw] = (TabOfMembership[rw] + dif) / 2.0;
                    }
                }
            }
            //rodzaj
            if (Genre)
            {

            }
            //kraj produkcji
            if (Country)
            {

            }
            //jezyk
            if (Languages)
            {

            }
            //czas trwania
            if (Duration)
            {
                //tutaj mamy pętle, która spradza zawartość komórki w excelu w różnych rzędach i w 3 kolumnie
                //czyli właśnie czas trawania
                int dur;
                for (int rw = 0; rw < rangeOfSearch; rw++)
                {
                    dur = ListOfDurations[rw];
                    //jeśli czas mieści się w podanych założeniach
                    if (dur <= DurationTo && dur >= DurationFrom)
                    {
                        //to robimy średnia srytmetyczna z jego membership dagree i 1.0
                        //to jest taki tylko moj pomysl na szybko i jak najbardziej mozesz to modyfikować
                        TabOfMembership[rw] = (TabOfMembership[rw] + 1.0) / 2.0;
                        //tu sprawdzamy czy czas jest odrobinę za niski lub odrobinę za wysoki
                        //musza byc takie dlugie warunki bo się sypie inaczej
                    }
                    else if ((dur <= (DurationTo + 15) && dur > DurationTo) || (dur >= (DurationFrom - 15) && dur < DurationFrom))
                    {
                        //tutaj obliczamy różnice czyli ile naszej komórce z excela brakuje do poprawnego wyniku
                        //czyli jak na przyklad mamy czas 90-120, a czas jest 85, 
                        //to mu przypiszemy coś trochę mniejszego niż jeden
                        double dif;
                        if (dur >= (DurationFrom - 15) && dur < DurationFrom)
                        {
                            dif = DurationFrom - dur;
                            dif = 1.0 - dif / 15.0;
                        }
                        else
                        {
                            dif = dur - DurationTo;
                            dif = 1.0 - dif / 15.0;
                        }
                        TabOfMembership[rw] = (TabOfMembership[rw] + dif) / 2.0;
                    }
                }
            }
            //ocena
            if (Rating)
            {
                //tutaj mamy pętle, która spradza zawartość komórki w excelu w różnych rzędach i w 7 kolumnie
                //czyli właśnie ocene
                double rat;
                for (int rw = 0; rw < rangeOfSearch; rw++)
                {
                    rat = ListOfRatings[rw];
                    //jeśli ocena mieści się w podanych założeniach
                    if (rat == rating)
                    {
                        //to robimy średnia arytmetyczna z jego membership dagree i 1.0
                        //to jest taki tylko moj pomysl na szybko i jak najbardziej mozesz to modyfikować
                        TabOfMembership[rw] = (TabOfMembership[rw] + 1.0) / 2.0;
                        //tu sprawdzamy czy ocena jest odrobinę za niska lub odrobinę za wysoka
                        //musza byc takie dlugie warunki bo się sypie inaczej
                    }
                    else if ((rat <= (rating + 1.0) && rat > rating) || (rat >= (rating - 1.0) && rat < rating))
                    {
                        //tutaj obliczamy różnice czyli ile naszej komórce z excela brakuje do poprawnego wyniku
                        //czyli jak na przyklad mamy ocene 8.0, a ocena jest 7.8, 
                        //to mu przypiszemy coś trochę mniejszego niż jeden
                        double dif;
                        if (rat <= (rating + 1.0) && rat > rating)
                        {
                            dif = (rating + 1.0) - rat;
                        }
                        else
                        {
                            dif = rat - (rating - 1.0);
                        }
                        TabOfMembership[rw] = (TabOfMembership[rw] + dif) / 2.0;
                    }
                }
            }
            //popularnosc
            if (Popularity)
            {
                //szukamy maksymalnej warości popularnosci
                //to jest potrzebne bo robimy z tych liczb procenty
                double max = 0.0;
                double tmp;
                for (int rw = 0; rw < rangeOfSearch; rw++)
                {
                    tmp = ListOfPopularity[rw];
                    if (tmp > max) max = tmp;
                }

                //tutaj mamy pętle, która spradza zawartość komórki w excelu w różnych rzędach i w 8 kolumnie
                //czyli właśnie popularność
                double pop;
                for (int rw = 0; rw < rangeOfSearch; rw++)
                {
                    pop = ListOfPopularity[rw];
                    pop = (pop / max) * 100.0;
                    //jeśli popularnosc mieści się w podanych założeniach
                    if (pop == popularity)
                    {
                        //to robimy średnia srytmetyczna z jego membership dagree i 1.0
                        //to jest taki tylko moj pomysl na szybko i jak najbardziej mozesz to modyfikować
                        TabOfMembership[rw] = (TabOfMembership[rw] + 1.0) / 2.0;
                        //tu sprawdzamy czy popularnosc jest odrobinę za niska lub odrobinę za wysoka
                        //musza byc takie dlugie warunki bo się sypie inaczej
                    }
                    else if ((pop <= (popularity + 20.0) && pop > popularity) || (pop >= (popularity - 20.0) && pop < popularity))

                    {
                        //tutaj obliczamy różnice czyli ile naszej komurce z excela brakuje do poprawnego wyniku
                        //czyli jak na przyklad mamy popularnosc 80%, a pop jest 79%, 
                        //to mu przypiszemy coś trochę mniejszego niż jeden
                        double dif;
                        if (pop <= (popularity + 20.0) && pop > popularity)
                        {
                            dif = (popularity + 20.0) - pop;
                            dif = dif / 20.0;
                        }
                        else
                        {
                            dif = pop - (popularity - 20.0);
                            dif = dif / 20.0;
                        }
                        TabOfMembership[rw] = (TabOfMembership[rw] + dif) / 2.0;
                    }
                }
            }
            //wypisywanie informacji o wynikach
            //to jest do zmiany, ale chcę wiedzieć, że się dobrze oblicza i są różne wyniki w granicach [0,1]
            string toShow = "";
            for (int i = 0; i < rangeOfSearch; i++)
            {
                toShow += TabOfMembership[i].ToString("0.##") + "  ";
            }
            Output.Text = toShow;
        }

        //Searcher for genres
        private void GenresTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<string> new_genres_items = new List<string>();

            TextBox textBox = sender as TextBox;
            string written_str = textBox.Text;
            for (int i = 0; i < genres_items.Count; i++)
            {
                if (genres_items[i].Contains(written_str))
                {
                    new_genres_items.Add(genres_items[i]);
                }
            }
            genres_items = new_genres_items;
            RefreshView();
        }

        //Searcher for countries
        private void CountryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<string> new_countries_items = new List<string>();

            TextBox textBox = sender as TextBox;
            string written_str = textBox.Text;
            for (int i = 0; i < countries_items.Count; i++)
            {
                if (countries_items[i].Contains(written_str))
                {
                    new_countries_items.Add(countries_items[i]);
                }
            }
            countries_items = new_countries_items;
            RefreshView();
        }

        //Searcher for languages
        private void LanguageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<string> new_languages_items = new List<string>();

            TextBox textBox = sender as TextBox;
            string written_str = textBox.Text;
            for (int i = 0; i < countries_items.Count; i++)
            {
                if (languages_items[i].Contains(written_str))
                {
                    new_languages_items.Add(languages_items[i]);
                }
            }
            languages_items = new_languages_items;
            RefreshView();
        }
    }
}
