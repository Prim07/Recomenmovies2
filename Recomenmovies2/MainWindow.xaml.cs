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
        // public variables
        int YearFrom = 1915;
        int YearTo = 1915;
        double ratingFrom = 1.0;
        double ratingTo = 1.0;
        double popularityFrom = 1.0;
        double popularityTo = 1.0;
        int DurationFrom;
        int DurationTo;

        //bools connected with checkboxes
        bool Years = false;
        bool Genre = false;
        bool Country = false;
        bool Languages = false;
        bool Duration = false;
        bool Rating = false;
        bool Popularity = false;
        bool BindRating = false;
        bool BindPopularity = false;

        //weights of choices
        int weight_of_years = 1;
        int weight_of_genres = 1;
        int weight_of_countries = 1;
        int weight_of_languages = 1;
        int weight_of_duration = 1;
        int weight_of_rating = 1;
        int weight_of_popularity = 1;

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

        //List of all movies
        List<Movie> ListOfMovies = new List<Movie>();
        List<Movie> SortedList;
        
        // list of realtions
        double[,] ArrayOfRelations;

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
                    fields[3] = TrimAllWithInplaceCharArray(fields[3]);
                    tmpMovie.genre = fields[3].Split(',');
                    tmpMovie.language = fields[4];
                    tmpMovie.country = fields[5];
                    tmpMovie.rating = Convert.ToDouble(fields[6]);
                    tmpMovie.popularity = Convert.ToDouble(fields[7]);
                    ListOfMovies.Add(tmpMovie);
                }
            }

            using (TextFieldParser parser = new TextFieldParser(@"c:\temp\relations_in_genres.csv"))
            {
                int size = genres_items_origin.Count;
                ArrayOfRelations = new double[size, size];

                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(";");
                int row = 0;
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    if (row != 0)
                    {
                        for (int col = 1; col <= row; col++)
                        {
                            if (fields[col] != "")
                            {
                                ArrayOfRelations[row - 1, col - 1] = Convert.ToDouble(fields[col]);
                                ArrayOfRelations[col - 1, row - 1] = Convert.ToDouble(fields[col]);
                            }
                        }
                    }
                    row++;
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

        // Event on change of slider - to ratingFrom
        private void Slider_DragDelta_2(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                ratingFrom = Math.Round((double)slider.Value, 1);
                AverageRatingFromTextBlock.Text = ratingFrom.ToString("0.#");
                if (BindRating)
                {
                    AverageRatingToTextBlock.Background = Brushes.White;
                    AverageRatingToTextBlock.Text = ratingFrom.ToString("0.#");
                }

                RefreshChoices();
            }
        }



        private void Slider_DragDelta_22(object sender, RoutedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                ratingTo = Math.Round((double)slider.Value, 1);
                AverageRatingToTextBlock.Text = ratingTo.ToString("0.#");
                if (ratingTo < ratingFrom)
                {
                    AverageRatingToTextBlock.Background = Brushes.OrangeRed;
                    ratingTo = ratingFrom;
                    RefreshChoices();
                }
                else
                {
                    AverageRatingToTextBlock.Background = Brushes.White;
                    RefreshChoices();
                }
            }

        }

        // Event on change of slider - to popularityFrom
        private void Slider_DragDelta_3(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                popularityFrom = (int)slider.Value;
                PopularitySliderFromTextBlock.Text = popularityFrom.ToString();
                if(BindPopularity)
                {
                    PopularitySliderToTextBlock.Background = Brushes.White;
                    PopularitySliderToTextBlock.Text = popularityFrom.ToString();
                }
                    

                RefreshChoices();
            }
        }


        // Event on change of slider - to popularityTo
        private void Slider_DragDelta_33(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                popularityTo = (int)slider.Value;
                PopularitySliderToTextBlock.Text = popularityTo.ToString();
                if (popularityTo < popularityFrom)
                {
                    PopularitySliderToTextBlock.Background = Brushes.OrangeRed;
                    popularityTo = popularityFrom;
                    RefreshChoices();
                }
                else
                {
                    PopularitySliderToTextBlock.Background = Brushes.White;
                    RefreshChoices();
                }
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
                double yearMem = 0.0;
                double genreMem = 0.0;
                double languageMem = 0.0;
                double countryMem = 0.0;
                double ratingMem = 0.0;
                double popularityMem = 0.0;
                double durationMem = 0.0;
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
                        yearMem = (double)weight_of_years * 1.0;
                        //ListOfMovies[rw].membs_degree = firstMemChange ? 1.0 : (ListOfMovies[rw].membs_degree + 1.0) / 2.0;
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
                        yearMem = weight_of_years * diff;
                        //ListOfMovies[rw].membs_degree = firstMemChange ? diff : (ListOfMovies[rw].membs_degree + weight_of_years*diff) /(1.0 + (double)weight_of_years);
                    }

                    firstMemChange = false;
                }
                //rodzaj
                if (Genre)
                {
                    double sumOfMembs = 0.0;
                    double sumOfMembsAdd = 0.0;

                    for (int j = 0; j < selected_genres.Count; j++)
                    {
                        string currentSelectedGenre = selected_genres[j];
                        for (int i = 0; i < ListOfMovies[rw].genre.Length; i++)
                        {
                            string currentGenreInMovie = ListOfMovies[rw].genre[i];
                            int row = genres_items_origin.IndexOf(currentSelectedGenre);
                            int col = genres_items_origin.IndexOf(currentGenreInMovie);
                            sumOfMembsAdd = (sumOfMembsAdd > ArrayOfRelations[row, col]) ? sumOfMembsAdd : ArrayOfRelations[row, col];
                        }
                        sumOfMembs += sumOfMembsAdd;
                        sumOfMembsAdd = 0.0;
                    }
                    genreMem = (double)weight_of_genres * (sumOfMembs / selected_genres.Count);
                    //ListOfMovies[rw].membs_degree = firstMemChange ? (sumOfMembs / selected_genres.Count) : (ListOfMovies[rw].membs_degree + (double)weight_of_genres * (sumOfMembs / selected_genres.Count)) / (1.0 + (double)weight_of_genres);
                    firstMemChange = false;

                }
                //kraj produkcji
                if (Country)
                {

                    for (int i = 0; i < selected_countries.Count; i++)
                    {
                        if (ListOfMovies[rw].country.Contains(selected_countries[i]))
                        {
                            countryMem = (double)weight_of_countries * 1.0;
                            //ListOfMovies[rw].membs_degree = firstMemChange ? 1.0 : (ListOfMovies[rw].membs_degree + (double)weight_of_countries*1.0) / (1.0 + (double)weight_of_countries);
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
                            languageMem = (double)weight_of_languages * 1.0;
                            //ListOfMovies[rw].membs_degree = firstMemChange ? 1.0 : (ListOfMovies[rw].membs_degree + (double)weight_of_languages*1.0) / (1.0 + (double)weight_of_languages);
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
                        durationMem = (double)weight_of_duration * 1.0;
                       // ListOfMovies[rw].membs_degree = firstMemChange ? 1.0 : (ListOfMovies[rw].membs_degree + 1.0) / 2.0;
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
                        durationMem = (double)weight_of_duration * diff;
                        //ListOfMovies[rw].membs_degree = firstMemChange ? diff : (ListOfMovies[rw].membs_degree + (double)weight_of_duration*diff) / (1.0 + (double)weight_of_duration);
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
                    if (rat <= ratingTo && rat >= ratingFrom)
                    {
                        //to robimy średnia arytmetyczna z jego membership dagree i 1.0
                        //to jest taki tylko moj pomysl na szybko i jak najbardziej mozesz to modyfikować
                        ratingMem = (double)weight_of_rating * 1.0;
                        //ListOfMovies[rw].membs_degree = firstMemChange ? 1.0 : (ListOfMovies[rw].membs_degree + 1.0) / 2.0;
                        //tu sprawdzamy czy ocena jest odrobinę za niska lub odrobinę za wysoka
                        //musza byc takie dlugie warunki bo się sypie inaczej
                    }
                    else if ((rat <= (ratingTo + 1.0) && rat > ratingTo) || (rat >= (ratingFrom - 1.0) && rat < ratingFrom))
                    {
                        //tutaj obliczamy różnice czyli ile naszej komórce z excela brakuje do poprawnego wyniku
                        //czyli jak na przyklad mamy ocene 8.0, a ocena jest 7.8, 
                        //to mu przypiszemy coś trochę mniejszego niż jeden
                        double diff;
                        if (rat <= (ratingTo + 1.0) && rat > ratingTo)
                        {
                            diff = (ratingTo + 1.0) - rat;
                        }
                        else
                        {
                            diff = rat - (ratingFrom - 1.0);
                        }
                        ratingMem = (double)weight_of_rating * diff;
                        //ListOfMovies[rw].membs_degree = firstMemChange ? diff : (ListOfMovies[rw].membs_degree + (double)weight_of_rating*diff) / (1.0 + (double)weight_of_rating);
                    }
                    firstMemChange = false;
                }
                //popularnosc
                if (Popularity)
                {
                    //tutaj mamy pętle, która spradza zawartość komórki w excelu w różnych rzędach i w 8 kolumnie
                    //czyli właśnie popularność
                    //zakładamy, że tabela jest posortowana malejąco wg popularności
                    double pop = rangeOfSearch - rw - 1;
                    pop = (((double)(pop / (double)rangeOfSearch)) * 100.0);
                    //jeśli popularnosc mieści się w podanych założeniach
                    if ((int)pop + 1 <= (int)popularityFrom && (int)pop + 1 >= (int)popularityTo)
                    {
                        //to robimy średnia srytmetyczna z jego membership dagree i 1.0
                        //to jest taki tylko moj pomysl na szybko i jak najbardziej mozesz to modyfikować
                        popularityMem = 1.0 * (double)weight_of_popularity;
                        //ListOfMovies[rw].membs_degree = firstMemChange ? 1.0 : (ListOfMovies[rw].membs_degree + 1.0) / 2.0;
                        //tu sprawdzamy czy popularnosc jest odrobinę za niska lub odrobinę za wysoka
                        //musza byc takie dlugie warunki bo się sypie inaczej
                    }
                    else if ((pop <= (popularityTo + 10.0) && pop > popularityTo) || (pop >= (popularityFrom - 10.0) && pop < popularityFrom))

                    {
                        //tutaj obliczamy różnice czyli ile naszej komurce z excela brakuje do poprawnego wyniku
                        //czyli jak na przyklad mamy popularnosc 80%, a pop jest 79%, 
                        //to mu przypiszemy coś trochę mniejszego niż jeden
                        double diff;
                        if (pop <= (popularityTo + 10.0) && pop > popularityTo)
                        {
                            diff = (popularityTo + 10.0) - pop;
                            diff = diff / 10.0;
                        }
                        else
                        {
                            diff = pop - (popularityFrom - 10.0);
                            diff = diff / 10.0;
                        }
                        popularityMem = diff * (double)weight_of_popularity;
                        //ListOfMovies[rw].membs_degree = firstMemChange ? diff : (ListOfMovies[rw].membs_degree + (double)weight_of_popularity*diff) / (1.0 + (double)weight_of_popularity);
                    }
                    firstMemChange = false;
                }

                double weight = 0.0;
                if (Years)
                {
                    weight += weight_of_years;
                }

                if (Genre)
                {
                    weight += weight_of_genres;
                }

                if (Country)
                {
                    weight += weight_of_countries;
                }

                if (Languages)
                {
                    weight += weight_of_languages;
                }

                if (Duration)
                {
                    weight += weight_of_duration;
                }

                if (Rating)
                {
                    weight += weight_of_rating;
                }

                if (Popularity)
                {
                    weight += weight_of_popularity;
                }

                ListOfMovies[rw].membs_degree = (double)(yearMem + genreMem + countryMem + languageMem + durationMem + ratingMem + popularityMem) / weight;

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
                               + "\nDuration: " + SortedList[i].duration + " mins";
            info_string += "\nGenre: ";
            
            
            for (int j = 0; j < SortedList[i].genre.Length - 1; j++)
                info_string += SortedList[i].genre[j] + ", ";
            if (SortedList[i].genre.Length >= 1)
                info_string += SortedList[i].genre[SortedList[i].genre.Length - 1] + ".";

            info_string += "\nLanguage: " + SortedList[i].language
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

            StackPanelForChoices.Children.Clear();
            
            if (Genre)
            {
                StackPanel myStackPanel = new StackPanel();
                myStackPanel.Orientation = Orientation.Horizontal;
                // 1 - Genre
                ProduceWeightPanel(1, myStackPanel);
                
                string myString = "Genres: ";
                for (int i = 0; i < selected_genres.Count - 1; i++)
                {
                    myString += selected_genres[i] + ", ";
                }
                if (selected_genres.Count >= 1)
                    myString += selected_genres[selected_genres.Count - 1] + ".";

                TextBlock myTextBlock = new TextBlock();
                myTextBlock.Text = myString;
                myStackPanel.Children.Add(myTextBlock);
                
                StackPanelForChoices.Children.Add(myStackPanel);
            }

            if (Country)
            {
                StackPanel myStackPanel = new StackPanel();
                myStackPanel.Orientation = Orientation.Horizontal;
                // 3 - Country
                ProduceWeightPanel(3, myStackPanel);

                string myString = "Countries: ";
                for (int i = 0; i < selected_countries.Count - 1; i++)
                {
                    myString += selected_countries[i] + ", ";
                }
                if (selected_countries.Count >= 1)
                    myString += selected_countries[selected_countries.Count - 1] + ".";

                TextBlock myTextBlock = new TextBlock();
                myTextBlock.Text = myString;
                myStackPanel.Children.Add(myTextBlock);
                
                StackPanelForChoices.Children.Add(myStackPanel);
            }

            if (Languages)
            {
                StackPanel myStackPanel = new StackPanel();
                myStackPanel.Orientation = Orientation.Horizontal;
                // 5 - Language
                ProduceWeightPanel(5, myStackPanel);

                string myString = "Languages: ";
                for (int i = 0; i < selected_languages.Count - 1; i++)
                {
                    myString += selected_languages[i] + ", ";
                }
                if (selected_languages.Count >= 1)
                    myString += selected_languages[selected_languages.Count - 1] + ".";
                
                TextBlock myTextBlock = new TextBlock();
                myTextBlock.Text = myString;
                myStackPanel.Children.Add(myTextBlock);

                StackPanelForChoices.Children.Add(myStackPanel);
            }

            if (Duration)
            {
                StackPanel myStackPanel = new StackPanel();
                myStackPanel.Orientation = Orientation.Horizontal;
                // 7 - Duration
                ProduceWeightPanel(7, myStackPanel);

                string myString = "Duration from " + FromDuration.Text + " to " + ToDuration.Text + " minutes.";

                TextBlock myTextBlock = new TextBlock();
                myTextBlock.Text = myString;
                myStackPanel.Children.Add(myTextBlock);

                StackPanelForChoices.Children.Add(myStackPanel);
            }

            if (Years)
            {
                StackPanel myStackPanel = new StackPanel();
                myStackPanel.Orientation = Orientation.Horizontal;
                // 9 - Duration
                ProduceWeightPanel(9, myStackPanel);

                string myString = "Year from " + FromYearTextBlock.Text + " to " + ToYearTextBlock.Text + ".";

                TextBlock myTextBlock = new TextBlock();
                myTextBlock.Text = myString;
                myStackPanel.Children.Add(myTextBlock);

                StackPanelForChoices.Children.Add(myStackPanel);
            }

            if (Rating)
            {
                StackPanel myStackPanel = new StackPanel();
                myStackPanel.Orientation = Orientation.Horizontal;
                // 11 - Rating
                ProduceWeightPanel(11, myStackPanel);

                string myString = "Average rating: " + AverageRatingFromTextBlock.Text + " - " + AverageRatingToTextBlock.Text + ".";

                TextBlock myTextBlock = new TextBlock();
                myTextBlock.Text = myString;
                myStackPanel.Children.Add(myTextBlock);

                StackPanelForChoices.Children.Add(myStackPanel);
            }

            if (Popularity)
            {
                StackPanel myStackPanel = new StackPanel();
                myStackPanel.Orientation = Orientation.Horizontal;
                // 13 - Popularity
                ProduceWeightPanel(13, myStackPanel);

                string myString = "Popularity: " + PopularitySliderFromTextBlock.Text + "% - " + PopularitySliderToTextBlock.Text + " %.";

                TextBlock myTextBlock = new TextBlock();
                myTextBlock.Text = myString;
                myStackPanel.Children.Add(myTextBlock);

                StackPanelForChoices.Children.Add(myStackPanel);
            }
        }

        // Producing weight Panel
        private void ProduceWeightPanel(int i, StackPanel myStackPanel)
        {
            // 1 - Genre; 3 - Country; 5 - Language
            // 7 - Duration; 9 - Years; 11 - Rating; 13 - Popularity

            StackPanel weightStackPanel = new StackPanel();
            weightStackPanel.Orientation = Orientation.Horizontal;
            weightStackPanel.Margin = new Thickness(0, 0, 10, 5);

            TextBlock weightTextBlock = new TextBlock();
            weightTextBlock.Margin = new Thickness(0, 0, 5, 0);
            weightTextBlock.Width = 20;
            weightTextBlock.TextAlignment = TextAlignment.Center;
            var bc = new BrushConverter();
            weightTextBlock.Background = (Brush)bc.ConvertFrom("#d9ff32");
            string weightString = weight_of_genres.ToString();
            weightTextBlock.Text = weightString;
            weightStackPanel.Children.Add(weightTextBlock);

            Button btnPlus = new Button();
            Button btnMinus = new Button();
            btnPlus.Tag = i;
            btnMinus.Tag = i + 1;
            btnPlus.Click += BtnPlus_Click;
            btnMinus.Click += BtnMinus_Click;
            btnPlus.Content = "+";
            btnMinus.Content = "-";
            btnPlus.Padding = btnMinus.Padding = new Thickness(0);
            btnPlus.HorizontalContentAlignment = btnMinus.HorizontalContentAlignment = HorizontalAlignment.Center;
            btnPlus.VerticalContentAlignment = btnMinus.VerticalContentAlignment = VerticalAlignment.Center;
            btnPlus.HorizontalAlignment = btnMinus.HorizontalAlignment = HorizontalAlignment.Center;
            btnPlus.VerticalAlignment = btnMinus.VerticalAlignment = VerticalAlignment.Center;
            btnPlus.Height = btnMinus.Height = btnPlus.Width = btnMinus.Width = 18;
            btnPlus.Margin = btnMinus.Margin = new Thickness(1, 0, 1, 0);
            weightStackPanel.Children.Add(btnPlus);
            weightStackPanel.Children.Add(btnMinus);

            //Binding
            Binding myBinding = new Binding();
            switch (i)
            {
                case 1:
                    myBinding.Source = weight_of_genres;
                    break;
                case 3:
                    myBinding.Source = weight_of_countries;
                    break;
                case 5:
                    myBinding.Source = weight_of_languages;
                    break;
                case 7:
                    myBinding.Source = weight_of_duration;
                    break;
                case 9:
                    myBinding.Source = weight_of_years;
                    break;
                case 11:
                    myBinding.Source = weight_of_rating;
                    break;
                case 13:
                    myBinding.Source = weight_of_popularity;
                    break;
            }
            weightTextBlock.SetBinding(TextBlock.TextProperty, myBinding);

            myStackPanel.Children.Add(weightStackPanel);
        }

        // Clicked on plus weight button
        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            // 1 - Genre; 3 - Country; 5 - Language
            // 7 - Duration; 9 - Years; 11 - Rating; 13 - Popularity
            Button btnPlus = sender as Button;
            if(btnPlus != null)
            {
                int i = (int)btnPlus.Tag;
                switch(i)
                {
                    case 1:
                        weight_of_genres = (weight_of_genres < 7) ? ++weight_of_genres : 7;
                        break;
                    case 3:
                        weight_of_countries = (weight_of_countries < 7) ? ++weight_of_countries : 7;
                        break;
                    case 5:
                        weight_of_languages = (weight_of_languages < 7) ? ++weight_of_languages : 7;
                        break;
                    case 7:
                        weight_of_duration = (weight_of_duration < 7) ? ++weight_of_duration : 7;
                        break;
                    case 9:
                        weight_of_years = (weight_of_years < 7) ? ++weight_of_years : 7;
                        break;
                    case 11:
                        weight_of_rating = (weight_of_rating < 7) ? ++weight_of_rating : 7;
                        break;
                    case 13:
                        weight_of_popularity = (weight_of_popularity < 7) ? ++weight_of_popularity : 7;
                        break;
                }
            }
            RefreshChoices();
        }

        // Clicked on minus weight button
        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            // 2 - Genre; 4 - Country; 6 - Language
            // 8 - Duration; 10 - Years; 12 - Rating; 14 - Popularity
            Button btnMinus = sender as Button;
            if (btnMinus != null)
            {
                int i = (int)btnMinus.Tag;
                switch (i)
                {
                    case 2:
                        weight_of_genres = (weight_of_genres > 1) ? --weight_of_genres : 1;
                        break;
                    case 4:
                        weight_of_countries = (weight_of_countries > 1) ? --weight_of_countries : 1;
                        break;
                    case 6:
                        weight_of_languages = (weight_of_languages > 1) ? --weight_of_languages : 1;
                        break;
                    case 8:
                        weight_of_duration = (weight_of_duration > 1) ? --weight_of_duration : 1;
                        break;
                    case 10:
                        weight_of_years = (weight_of_years > 1) ? --weight_of_years : 1;
                        break;
                    case 12:
                        weight_of_rating = (weight_of_rating > 1) ? --weight_of_rating : 1;
                        break;
                    case 14:
                        weight_of_popularity = (weight_of_popularity > 1) ? --weight_of_popularity : 1;
                        break;
                }
            }
            RefreshChoices();
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
            BindPopularity = BindPopularityCheckBox.IsChecked.GetValueOrDefault();
            BindRating = BindRatingCheckBox.IsChecked.GetValueOrDefault();

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

            if (BindPopularity)
            {
                PopularityToSlider.IsEnabled = false;
            }
            else
            {
                PopularityToSlider.IsEnabled = true;
            }

            if (BindRating)
            {
                AverageRatingToSlider.IsEnabled = false;
            }
            else
            {
                AverageRatingToSlider.IsEnabled = true;
            }
        }




        // Sprawdza czy wpisane są liczby
        private void PreviewOnlyNumbers(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        
        // Cut all whitespaces from given string and return this string
        public static string TrimAllWithInplaceCharArray(string str)
        {
            var len = str.Length;
            var src = str.ToCharArray();
            int dstIdx = 0;
            for (int i = 0; i < len; i++)
            {
                var ch = src[i];
                switch (ch)
                {
                    case '\u0020':
                    case '\u00A0':
                    case '\u1680':
                    case '\u2000':
                    case '\u2001':
                    case '\u2002':
                    case '\u2003':
                    case '\u2004':
                    case '\u2005':
                    case '\u2006':
                    case '\u2007':
                    case '\u2008':
                    case '\u2009':
                    case '\u200A':
                    case '\u202F':
                    case '\u205F':
                    case '\u3000':
                    case '\u2028':
                    case '\u2029':
                    case '\u0009':
                    case '\u000A':
                    case '\u000B':
                    case '\u000C':
                    case '\u000D':
                    case '\u0085':
                        continue;
                    default:
                        src[dstIdx++] = ch;
                        break;
                }
            }
            return new string(src, 0, dstIdx);
        }
    }
}
