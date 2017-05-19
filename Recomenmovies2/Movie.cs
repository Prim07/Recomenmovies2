using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recomenmovies2
{
    class Movie
    {
        //public variables in Movie clas defining a movie
        public int year;
        public string title;
        public string[] genre;
        public string country;
        public string language;
        public int duration;
        public double rating;
        public double popularity;

        //membership degree of a movie
        public double membs_degree;

       public Movie()
        {
            membs_degree = 0.0;
        }
    }
}
