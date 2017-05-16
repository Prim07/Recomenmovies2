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

using Microsoft.VisualBasic.FileIO;

namespace Recomenmovies2
{
    public partial class MainWindow : Window
    {
        int YearFrom = 1915;
        int YearTo = 1915;
        double rating = 0.0;
        double popularity = 0.0;
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

        //Lists to keep selected things
        List<string> selected_countries = new List<string>();
        List<string> selected_languages = new List<string>();
        List<string> selected_genres = new List<string>();

        // HashSet of Movies
        //HashSet<Movie> moviesHashSet = new HashSet<Movie>();
        List<Movie> ListOfMovies = new List<Movie>();
        List<Movie> SortedList;


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

            //Load files
            LoadFiles();

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

        // Loading files
        private void LoadFiles()
        {
            using (TextFieldParser parser = new TextFieldParser(@"c:\temp\dane.csv"))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(";");
                while (!parser.EndOfData)
                {
                    Movie tmpMovie = new Movie();
                    //Processing row
                    string[] fields = parser.ReadFields();
                    tmpMovie.title = fields[0];
                    tmpMovie.year = Int32.Parse(fields[1]);
                    tmpMovie.duration = Int32.Parse(fields[2]);
                    tmpMovie.genre = fields[3];
                    tmpMovie.language = fields[4];
                    tmpMovie.country = fields[5];
                    tmpMovie.rating = Convert.ToDouble(fields[6]);
                    tmpMovie.popularity = Convert.ToDouble(fields[7]);
                    foreach (string field in fields)
                    {
                        //TODO: Process field
                    }
                    ListOfMovies.Add(tmpMovie);
                }
            }

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
                    RefreshChoices();
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
                    RefreshChoices();
                }
            }

        }

        // Event on change of slider - to rating
        private void Slider_DragDelta_2(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                rating = Math.Round((double)slider.Value, 1);
                AverageRating.Text = rating.ToString("0.#");
                RefreshChoices();
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
                RefreshChoices();
            }
        }



        //Start recommendations on click
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //check where to put first changes and set membership to 1.0, not the average
            bool firstMemChange = true;

            //ta zmienna mówi ile pól w excelu przeszukujemy - przy 10000 jest już baaardzo długo
            //uwaga! excel jest posortowany popularnoscia, wiec bierzemy te najbardziej znane filmy
            int rangeOfSearch = ListOfMovies.Count;

            //clear stack panel with recommendations
            StackPanelForRecommendations.Children.Clear();

            //========== Część z popularnością
            //szukamy maksymalnej warości popularnosci
            //to jest potrzebne bo robimy z tych liczb procenty
            double max = ListOfMovies.Max(o => o.popularity);
            
            for (int rw = 0; rw < rangeOfSearch; rw++)
            {
                firstMemChange = true;
                //zerujemy nasz "prawodopodobieństwo"
                ListOfMovies[rw].membs_degree = 0.0;
                //rok produkcji
                if (Years)
                {
                    //tutaj mamy pętle, która spradza zawartość komórki w excelu w różnych rzędach i w 2 kolumnie
                    //czyli właśnie lata
                    int year = ListOfMovies[rw].year;
                    //jeśli rok mieści się w podanych założeniach
                    if ((year <= YearTo && year >= YearFrom))
                    {
                        //to robimy średnia arytmetyczna z jego membership dagree i 1.0
                        //to jest taki tylko moj pomysl na szybko i jak najbardziej mozesz to modyfikować
                        // jeśli pierwsza zmiana, to 1.0, a jeśli nie, to średnia arytmetyczna
                        ListOfMovies[rw].membs_degree = firstMemChange ? 1.0 : (ListOfMovies[rw].membs_degree + 1.0) / 2.0;
                        //tu sprawdzamy czy rok jest odrobinę za niski lub odrobinę za wysoki
                        //musza byc takie dlugie warunki bo się sypie inaczej
                    }
                    else if ((year <= (YearTo + 10) && year > YearTo) || (year >= (YearFrom - 10) && year < YearFrom))
                    {
                        //tutaj obliczamy różnice czyli ile naszej komórce z excela brakuje do poprawnego wyniku
                        //czyli jak na przyklad mamy lata 1920-1930, a rok jest 1932, 
                        //to mu przypiszemy coś trochę mniejszego niż jeden
                        double diff;
                        if ((year >= (YearFrom - 10) && year < YearFrom))
                        {
                            diff = YearFrom - year;
                            diff = 1.0 - diff / 10.0;
                        }
                        else
                        {
                            diff = year - YearTo;
                            diff = 1.0 - diff / 10.0;
                        }
                        ListOfMovies[rw].membs_degree = firstMemChange ? diff : (ListOfMovies[rw].membs_degree + diff) / 2.0;
                    }

                    firstMemChange = false;
                }
                //rodzaj
                if (Genre)
                {
                    int counterOfEquals = 0;
                    for (int i = 0; i < selected_genres.Count; i++)
                    {
                        if (ListOfMovies[rw].genre.Contains(selected_genres[i]))
                        {
                            counterOfEquals++;
                        }
                    }
                    ListOfMovies[rw].membs_degree = firstMemChange ? (double)counterOfEquals / selected_genres.Count : (ListOfMovies[rw].membs_degree + 1.0 * ((double)(counterOfEquals / selected_genres.Count))) / 2.0;
                    firstMemChange = false;
                }
                //kraj produkcji
                if (Country)
                {

                    for (int i = 0; i < selected_countries.Count; i++)
                    {
                        if (ListOfMovies[rw].country.Contains(selected_countries[i]))
                        {
                            ListOfMovies[rw].membs_degree = firstMemChange ? 1.0 : (ListOfMovies[rw].membs_degree + 1.0) / 2.0;
                        }
                    }
                    firstMemChange = false;
                }
                //language
                if (Languages)
                {
                    for (int i = 0; i < selected_languages.Count; i++)
                    {
                        if (ListOfMovies[rw].language.Contains(selected_languages[i]))
                        {
                            ListOfMovies[rw].membs_degree = firstMemChange ? 1.0 : (ListOfMovies[rw].membs_degree + 1.0) / 2.0;
                        }
                    }
                    firstMemChange = false;
                }
                //movie duration 
                if (Duration)
                {
                    //tutaj mamy pętle, która spradza zawartość komórki w excelu w różnych rzędach i w 3 kolumnie
                    //czyli właśnie czas trawania
                    int dur = ListOfMovies[rw].duration;
                    //jeśli czas mieści się w podanych założeniach
                    if (dur <= DurationTo && dur >= DurationFrom)
                    {
                        //to robimy średnia srytmetyczna z jego membership dagree i 1.0
                        //to jest taki tylko moj pomysl na szybko i jak najbardziej mozesz to modyfikować
                        ListOfMovies[rw].membs_degree = firstMemChange ? 1.0 : (ListOfMovies[rw].membs_degree + 1.0) / 2.0;
                        //tu sprawdzamy czy czas jest odrobinę za niski lub odrobinę za wysoki
                        //musza byc takie dlugie warunki bo się sypie inaczej
                    }
                    else if ((dur <= (DurationTo + 15) && dur > DurationTo) || (dur >= (DurationFrom - 15) && dur < DurationFrom))
                    {
                        //tutaj obliczamy różnice czyli ile naszej komórce z excela brakuje do poprawnego wyniku
                        //czyli jak na przyklad mamy czas 90-120, a czas jest 85, 
                        //to mu przypiszemy coś trochę mniejszego niż jeden
                        double diff;
                        if (dur >= (DurationFrom - 15) && dur < DurationFrom)
                        {
                            diff = DurationFrom - dur;
                            diff = 1.0 - diff / 15.0;
                        }
                        else
                        {
                            diff = dur - DurationTo;
                            diff = 1.0 - diff / 15.0;
                        }
                        ListOfMovies[rw].membs_degree = firstMemChange ? diff : (ListOfMovies[rw].membs_degree + diff) / 2.0;
                    }
                    firstMemChange = false;
                }
                //ocena
                if (Rating)
                {
                    //tutaj mamy pętle, która spradza zawartość komórki w excelu w różnych rzędach i w 7 kolumnie
                    //czyli właśnie ocene
                    double rat = ListOfMovies[rw].rating;
                    //jeśli ocena mieści się w podanych założeniach
                    if (rat == rating)
                    {
                        //to robimy średnia arytmetyczna z jego membership dagree i 1.0
                        //to jest taki tylko moj pomysl na szybko i jak najbardziej mozesz to modyfikować
                        ListOfMovies[rw].membs_degree = firstMemChange ? 1.0 : (ListOfMovies[rw].membs_degree + 1.0) / 2.0;
                        //tu sprawdzamy czy ocena jest odrobinę za niska lub odrobinę za wysoka
                        //musza byc takie dlugie warunki bo się sypie inaczej
                    }
                    else if ((rat <= (rating + 1.0) && rat > rating) || (rat >= (rating - 1.0) && rat < rating))
                    {
                        //tutaj obliczamy różnice czyli ile naszej komórce z excela brakuje do poprawnego wyniku
                        //czyli jak na przyklad mamy ocene 8.0, a ocena jest 7.8, 
                        //to mu przypiszemy coś trochę mniejszego niż jeden
                        double diff;
                        if (rat <= (rating + 1.0) && rat > rating)
                        {
                            diff = (rating + 1.0) - rat;
                        }
                        else
                        {
                            diff = rat - (rating - 1.0);
                        }
                        ListOfMovies[rw].membs_degree = firstMemChange ? diff : (ListOfMovies[rw].membs_degree + diff) / 2.0;
                    }
                    firstMemChange = false;
                }
                //popularnosc
                if (Popularity)
                {
                    //tutaj mamy pętle, która spradza zawartość komórki w excelu w różnych rzędach i w 8 kolumnie
                    //czyli właśnie popularność
                    double pop = ListOfMovies[rw].popularity;
                    pop = (pop / max) * 100.0;
                    //jeśli popularnosc mieści się w podanych założeniach
                    if (pop == popularity)
                    {
                        //to robimy średnia srytmetyczna z jego membership dagree i 1.0
                        //to jest taki tylko moj pomysl na szybko i jak najbardziej mozesz to modyfikować
                        ListOfMovies[rw].membs_degree = firstMemChange ? 1.0 : (ListOfMovies[rw].membs_degree + 1.0) / 2.0;
                        //tu sprawdzamy czy popularnosc jest odrobinę za niska lub odrobinę za wysoka
                        //musza byc takie dlugie warunki bo się sypie inaczej
                    }
                    else if ((pop <= (popularity + 20.0) && pop > popularity) || (pop >= (popularity - 20.0) && pop < popularity))

                    {
                        //tutaj obliczamy różnice czyli ile naszej komurce z excela brakuje do poprawnego wyniku
                        //czyli jak na przyklad mamy popularnosc 80%, a pop jest 79%, 
                        //to mu przypiszemy coś trochę mniejszego niż jeden
                        double diff;
                        if (pop <= (popularity + 20.0) && pop > popularity)
                        {
                            diff = (popularity + 20.0) - pop;
                            diff = diff / 20.0;
                        }
                        else
                        {
                            diff = pop - (popularity - 20.0);
                            diff = diff / 20.0;
                        }
                        ListOfMovies[rw].membs_degree = firstMemChange ? diff : (ListOfMovies[rw].membs_degree + diff) / 2.0;
                    }
                    firstMemChange = false;
                }
            }


            SortedList = ListOfMovies.OrderByDescending(o => o.membs_degree).ToList();
            for (int i = 0; i < SortedList.Count; i++)
            {
                if (i >= 5000 || SortedList[i].membs_degree == 0.0)
                    break;
                else
                {
                    TextBlock tmpBlock = new TextBlock();
                    tmpBlock.Text = (i + 1).ToString() + ". "
                                  + SortedList[i].title + ": "
                                  + SortedList[i].membs_degree.ToString("0.##");
                    tmpBlock.Margin = new System.Windows.Thickness(5, 5, 5, 0);
                    tmpBlock.Tag = i;
                    tmpBlock.MouseLeftButtonUp += TmpBlock_MouseLeftButtonUp;
                    StackPanelForRecommendations.Children.Add(tmpBlock);
                }
            }
        }

        //Clicked in block with a movie
        private void TmpBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            int i = (int)textBlock.Tag;

            StackPanelForMovieInfo.Children.Clear();

            string info_title = SortedList[i].title;
            TextBlock titleTextBlock = new TextBlock();
            titleTextBlock.Text = info_title;
            titleTextBlock.FontWeight = FontWeights.Bold;
            titleTextBlock.TextAlignment = TextAlignment.Center;
            StackPanelForMovieInfo.Children.Add(titleTextBlock);


            string info_string = "Year: " + SortedList[i].year
                               + "\nDuration: " + SortedList[i].duration + " mins"
                               + "\nGenre: " + SortedList[i].genre
                               + "\nLanguage: " + SortedList[i].language
                               + "\nCountry: " + SortedList[i].country
                               + "\nRating: " + SortedList[i].rating
                               + "\nPopularity: " + SortedList[i].popularity;

            TextBlock infoTextBlock = new TextBlock();
            infoTextBlock.Text = info_string;
            StackPanelForMovieInfo.Children.Add(infoTextBlock);

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

        //Refreshing 'Choices' textblock
        private void RefreshChoices()
        {
            string myString = "";

            if (Years)
                myString += "Year from " + FromYearTextBlock.Text + " to " + ToYearTextBlock.Text + ".\n";

            if(Genre)
            {
                myString += "Genres: ";
                for (int i = 0; i < selected_genres.Count - 1; i++)
                {
                    myString += selected_genres[i] + ", ";
                }
                if (selected_genres.Count >= 1)
                    myString += selected_genres[selected_genres.Count - 1] + ".";
                myString += "\n";
            }

            if (Country)
            {
                myString += "Countries: ";
                for (int i = 0; i < selected_countries.Count - 1; i++)
                {
                    myString += selected_countries[i] + ", ";
                }
                if(selected_countries.Count >= 1)
                    myString += selected_countries[selected_countries.Count - 1] + ".";
                myString += "\n";
            }

            if (Languages)
            {
                myString += "Languages: ";
                for (int i = 0; i < selected_languages.Count - 1; i++)
                {
                    myString += selected_languages[i] + ", ";
                }
                if (selected_languages.Count >= 1)
                    myString += selected_languages[selected_languages.Count - 1] + ".";
                myString += "\n";
            }

            if (Duration)
                myString += "Duration from " + FromDuration.Text + " to " + ToDuration.Text + " minutes.\n";

            if(Rating)
                myString += "Average rating: " + AverageRating.Text + ".\n";

            if (Popularity)
                myString += "Popularity: " + PopularitySlider.Text + " %.\n";

            ChoicesTextBlock.Text = myString;
        }

        private void GenresListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selected_genres.Clear();

            foreach (Object selecteditem in GenresListBox.SelectedItems)
                selected_genres.Add(selecteditem as String);

            RefreshChoices();
        }

        private void CountryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selected_countries.Clear();

            foreach (Object selecteditem in CountryListBox.SelectedItems)
                selected_countries.Add(selecteditem as String);

            RefreshChoices();
        }

        private void LanguageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selected_languages.Clear();

            foreach (Object selecteditem in LanguageListBox.SelectedItems)
                selected_languages.Add(selecteditem as String);

            RefreshChoices();
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
    }
}
