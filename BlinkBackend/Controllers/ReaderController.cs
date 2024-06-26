﻿using BlinkBackend.Models;
using BlinkBackend.Classes;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web;
using System.Web.Http.Cors;
using Newtonsoft.Json;
using System.Text;

namespace BlinkBackend.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ReaderController : ApiController
    {
        static int GenerateId()
        {

            long timestamp = DateTime.Now.Ticks;
            Random random = new Random();
            int randomComponent = random.Next();

            int userId = (int)(timestamp ^ randomComponent);

            return Math.Abs(userId);
        }
        [HttpPost]

        public HttpResponseMessage AddReaderFavorites(Favorites f)
        {

            BlinkMovieEntities db = new BlinkMovieEntities();
            try
            {


                var favorites = new Favorites()
                {
                    Favorites_ID = GenerateId(),
                    Reader_ID = f.Reader_ID,
                    Writer_ID = f.Writer_ID,
                    Movie_ID = f.Movie_ID,
                };

                db.Favorites.Add(favorites);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Added To Favorites");


            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }





        [HttpPost]

        public HttpResponseMessage AddReaderHistory(History h)
        {

            BlinkMovieEntities db = new BlinkMovieEntities();
            try
            {

                var history = new History()
                {
                    History_ID = GenerateId(),
                    Reader_ID = h.Reader_ID,
                    Writer_ID = h.Writer_ID,
                    Movie_ID = h.Movie_ID,
                };

                db.History.Add(history);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Added To History");


            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage RechargeBalance(int Reader_ID, int newBalance)
        {
            using (BlinkMovieEntities db = new BlinkMovieEntities())
            {
                try
                {
                    var reader = db.Reader.FirstOrDefault(r => r.Reader_ID == Reader_ID);
                    if (reader == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Reader not found");
                    }

                    reader.Subscription = "Paid";
                    reader.Balance = (reader.Balance ?? 0) + newBalance;
                    db.SaveChanges();

                    // Start a background task to reset the balance and subscription after 30 days
                    Task.Run(async () => await ResetBalanceAndSubscription(reader.Reader_ID));

                    return Request.CreateResponse(HttpStatusCode.OK, "Balance recharged successfully");
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        /*
                [HttpPost]
                public HttpResponseMessage UpdateMovieRating(int movieId, double rating)
                {

                    using (var db = new BlinkMovieEntities())
                    {
                        db.Configuration.LazyLoadingEnabled = false;
                        db.Configuration.ProxyCreationEnabled = false;
                        var movie = db.Movie.FirstOrDefault(w => w.Movie_ID == movieId);

                        if (movie == null)
                        {
                            return Request.CreateResponse(HttpStatusCode.NotFound, "Movie not found");
                        }


                        if (rating < 0 || rating > 5)
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Rating should be between 0 and 5");
                        }

                        int? totalRatings = movie.TotalRatings + 1;
                        double? totalRatingSum = movie.TotalRatingSum + rating;
                        double? averageRating = (double)totalRatingSum / totalRatings;


                        movie.TotalRatings = totalRatings;
                        movie.TotalRatingSum = (int)totalRatingSum;
                        movie.AverageRating = averageRating;


                        db.SaveChanges();

                        return Request.CreateResponse(HttpStatusCode.OK, "Movie rating updated successfully");
                    }
                }
        */

        [HttpPost]
        public HttpResponseMessage UpdateMovieRating(int movieId, double rating)
        {
            using (var db = new BlinkMovieEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                var movie = db.Movie.FirstOrDefault(w => w.Movie_ID == movieId);
                if (movie == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Movie not found");
                }

                if (rating < 0 || rating > 5)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Rating should be between 0 and 5");
                }

                // Ensure movie.TotalRatings and movie.TotalRatingSum are not null before incrementing
                int totalRatings = (movie.TotalRatings ?? 0) + 1;
                double totalRatingSum = (movie.TotalRatingSum ?? 0) + rating;
                double averageRating = totalRatingSum / totalRatings;

                movie.TotalRatings = totalRatings;
                movie.TotalRatingSum = (int)totalRatingSum;
                movie.AverageRating = averageRating;

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Movie rating updated successfully");
            }
        }

        /*
                [HttpGet]
                public HttpResponseMessage GetAllMovies(string genre = null, string searchTerm = null)
                {
                    using (BlinkMovieEntities db = new BlinkMovieEntities())
                    {
                        db.Configuration.LazyLoadingEnabled = false;

                        var moviesQuery = from gm in db.GetMovie
                                          join m in db.Movie on gm.Movie_ID equals m.Movie_ID
                                          where m.Type == "Movie"
                                          select new
                                          {
                                              Movie_ID = m.Movie_ID,
                                              Name = m.Name,
                                              Type = m.Type,
                                              Image = m.Image,
                                              Rating = m.AverageRating,
                                              Category = m.Category
                                          };


                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            if (moviesQuery.Any(m => m.Name.Contains(searchTerm)))
                            {
                                moviesQuery = moviesQuery.Where(m => m.Name.Contains(searchTerm));
                            }
                            else
                            {

                                moviesQuery = moviesQuery;
                            }
                        }
                        else if (!string.IsNullOrEmpty(genre))
                        {
                            moviesQuery = moviesQuery.Where(m => m.Category.Contains(genre));

                        }

                        else if (!string.IsNullOrEmpty(searchTerm) && !string.IsNullOrEmpty(genre))
                        {
                            moviesQuery = moviesQuery.Where(m => m.Name.Contains(searchTerm) || m.Category.Contains(genre));
                        }

                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            moviesQuery = moviesQuery.Where(m => m.Name.Contains(searchTerm));
                        }

                        var movies = moviesQuery.Distinct().ToList();

                        if (movies.Any())
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, movies);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found");
                        }
                    }
                }
        */

        [HttpGet]
        public HttpResponseMessage GetAllMovies(string genre = null, string searchTerm = null)
        {
            using (BlinkMovieEntities db = new BlinkMovieEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;

                var moviesQuery = from gm in db.GetMovie
                                  join m in db.Movie on gm.Movie_ID equals m.Movie_ID
                                  where m.Type == "Movie"
                                  select new
                                  {
                                      Movie_ID = m.Movie_ID,
                                      Name = m.Name,
                                      Type = m.Type,
                                      Image = m.Image,
                                      Rating = m.AverageRating,
                                      Category = m.Category
                                  };


                if (!string.IsNullOrEmpty(searchTerm))
                {
                    if (moviesQuery.Any(m => m.Name.Contains(searchTerm)))
                    {
                        moviesQuery = moviesQuery.Where(m => m.Name.Contains(searchTerm));
                    }
                    else
                    {

                        moviesQuery = moviesQuery;
                    }
                }
                else if (!string.IsNullOrEmpty(genre))
                {
                    moviesQuery = moviesQuery.Where(m => m.Category.Contains(genre));

                }

                /* else if (!string.IsNullOrEmpty(searchTerm) && !string.IsNullOrEmpty(genre))
                  {
                      moviesQuery = moviesQuery.Where(m => m.Name.Contains(searchTerm) || m.Category.Contains(genre));
                  }*/

                /*   if (!string.IsNullOrEmpty(searchTerm))
                   {
                       moviesQuery = moviesQuery.Where(m => m.Name.Contains(searchTerm));
                   }
   */
                var movies = moviesQuery.Distinct().ToList();

                if (movies.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, movies);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found");
                }
            }
        }


        [HttpGet]
        public HttpResponseMessage GetAllDramas(string genre = null, string searchTerm = null)
        {
            using (BlinkMovieEntities db = new BlinkMovieEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;

                var moviesQuery = from gm in db.GetMovie
                                  join m in db.Movie on gm.Movie_ID equals m.Movie_ID
                                  where m.Type == "Drama"
                                  select new
                                  {
                                      Movie_ID = m.Movie_ID,
                                      Name = m.Name,
                                      Type = m.Type,
                                      Image = m.Image,
                                      Rating = m.AverageRating,
                                  };


                if (!string.IsNullOrEmpty(searchTerm))
                {
                    moviesQuery = moviesQuery.Where(m => m.Name.Contains(searchTerm));
                }

                var movies = moviesQuery.Distinct().ToList();

                if (movies.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, movies);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found");
                }
            }
        }


        [HttpGet]
        public HttpResponseMessage GetAllMovieWriter(int movieId, string searchTerm = null)
        {
            using (BlinkMovieEntities db = new BlinkMovieEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;

                var writersQuery = from gm in db.GetMovie
                                   join w in db.Writer on gm.Writer_ID equals w.Writer_ID
                                   where gm.Movie_ID == movieId
                                   select new
                                   {
                                       Writer_ID = w.Writer_ID,
                                       UserName = w.UserName,
                                       Email = w.Email,
                                       Image = w.Image,
                                       Rating = w.AverageRating
                                   };

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    string lowerCaseSearchTerm = searchTerm.ToLower();
                    writersQuery = writersQuery.Where(w => w.UserName.ToLower().Contains(lowerCaseSearchTerm));
                }

                var writers = writersQuery.Distinct().ToList();

                if (writers.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, writers);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found");
                }
            }
        }






        [HttpPut]
        public HttpResponseMessage SendBalanceRequest(int Reader_ID, int Amount)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();

            DateTime currentDate = new DateTime();

            var balance = new BalanceRequests
            {
                Balance_ID = GenerateId(),
                Reader_ID = Reader_ID,
                Balance = Amount,
                Status = "Sent",
                RequestDate = currentDate.ToString(),
                adminNotifications = true
            };
            db.BalanceRequests.Add(balance);
            db.SaveChanges();


            return Request.CreateResponse(HttpStatusCode.OK, "Subscription updated successfully");


        }


        private async Task ResetBalanceAndSubscription(int Reader_ID)
        {
            using (BlinkMovieEntities db = new BlinkMovieEntities())
            {
                await Task.Delay(TimeSpan.FromDays(1));
                try
                {
                    var reader = db.Reader.FirstOrDefault(r => r.Reader_ID == Reader_ID);
                    if (reader != null)
                    {
                        reader.Balance = 0;
                        reader.Subscription = "Free";
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    
                }
            }
        }

        [HttpPost]
        public HttpResponseMessage RechargeBalanceFree(int Reader_ID)
        {
            using (BlinkMovieEntities db = new BlinkMovieEntities())
            {
                try
                {
                    var reader = db.Reader.FirstOrDefault(r => r.Reader_ID == Reader_ID);
                    if (reader == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Reader not found");
                    }

                    reader.Subscription = "Free";
                    reader.Balance = (reader.Balance ?? 0) + 0;
                    db.SaveChanges();

                    // Start a background task to reset the balance and subscription after 30 days
                    Task.Run(async () => await ResetBalanceAndSubscription(reader.Reader_ID));

                    return Request.CreateResponse(HttpStatusCode.OK, "Free Version Subscribed successfully");
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }



        /* [HttpPost]
         public HttpResponseMessage UpdateWriterRating(int writerId, int rating)
         {

             using (var db = new BlinkMovieEntities())
             {
                 db.Configuration.LazyLoadingEnabled = false;
                 db.Configuration.ProxyCreationEnabled = false;
                 var writer = db.Writer.FirstOrDefault(w => w.Writer_ID == writerId);

                 if (writer == null)
                 {
                     return Request.CreateResponse(HttpStatusCode.NotFound, "Writer not found");
                 }


                 if (rating < 0 || rating > 5)
                 {
                     return Request.CreateResponse(HttpStatusCode.BadRequest, "Rating should be between 0 and 5");
                 }

                 int? totalRatings = writer.TotalRatings + 1;
                 int? totalRatingSum = writer.TotalRatingSum + rating;
                 double? averageRating = (double)totalRatingSum / totalRatings;


                 writer.TotalRatings = totalRatings;
                 writer.TotalRatingSum = totalRatingSum;
                 writer.AverageRating = averageRating;


                 db.SaveChanges();

                 return Request.CreateResponse(HttpStatusCode.OK, "Writer rating updated successfully");
             }
         }
 */

        [HttpPost]
        public HttpResponseMessage UpdateWriterRating(int writerId, double rating)
        {
            try
            {
                using (var db = new BlinkMovieEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;
                    var writer = db.Writer.FirstOrDefault(w => w.Writer_ID == writerId);

                    if (writer == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Writer not found");
                    }

                    if (rating < 0 || rating > 5)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Rating should be between 0 and 5");
                    }

                    int? totalRatings = (writer.TotalRatings ?? 0) + 1;
                    double? totalRatingSum = (writer.TotalRatingSum ?? 0) + rating;
                    double? averageRating = totalRatingSum / totalRatings;

                    writer.TotalRatings = totalRatings;
                    writer.TotalRatingSum = (int)totalRatingSum;
                    writer.AverageRating = averageRating;

                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.OK, "Writer rating updated successfully");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }




        [HttpPut]
        public HttpResponseMessage UpdateSubscription(int Reader_ID, string subscription)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();

            var reader = db.Reader.FirstOrDefault(r => r.Reader_ID == Reader_ID);

            if (reader != null)
            {
                reader.Subscription = subscription;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Subscription updated successfully");
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "User not found");
            }

        }

        [HttpPut]
        public HttpResponseMessage UpdateInterests(int Reader_ID, string newInterests)
        {
            using (BlinkMovieEntities db = new BlinkMovieEntities())
            {
                var reader = db.Reader.FirstOrDefault(r => r.Reader_ID == Reader_ID);

                if (reader != null)
                {
                    reader.Interest = newInterests;
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.OK, "Interests updated successfully");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "User not found");
                }
            }
        }


        /*[HttpGet]

        public HttpResponseMessage GetAllMovies()
        {
            BlinkMovieEntities db = new BlinkMovieEntities();
            db.Configuration.LazyLoadingEnabled = false;

            var movies = db.Movie.Where(m => m.Type == "Movie").ToList();
            if (movies != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, movies);
            }

            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found");
            }

        }*/

        [HttpGet]
        public HttpResponseMessage GetAllDramas()
        {
            BlinkMovieEntities db = new BlinkMovieEntities();
            db.Configuration.LazyLoadingEnabled = false;

            var movies = db.Movie.Where(m => m.Type == "Drama").ToList();
            if (movies != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, movies);
            }

            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found");
            }
        }

        [HttpGet]

        public HttpResponseMessage GetSpecificMovie(int Movie_ID)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();
            db.Configuration.LazyLoadingEnabled = false;

            var movies = db.Movie.FirstOrDefault(m => m.Movie_ID == Movie_ID );
            if (movies != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, movies);
            }

            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found");
            }

        }

        [HttpGet]
        public HttpResponseMessage GetMovieDetails(int Movie_ID)
        {
            using (BlinkMovieEntities db = new BlinkMovieEntities())
            {

                db.Configuration.LazyLoadingEnabled = false;
                var movieIds = db.GetMovie.Where(getMovie => getMovie.Movie_ID == Movie_ID).Select(getMovie => getMovie.Movie_ID);

                var result = db.GetMovie
                    .Where(getMovie => movieIds.Contains(getMovie.Movie_ID))
                    .Join(db.Movie, getMovie => getMovie.Movie_ID, movie => movie.Movie_ID, (getMovie, movie) => new { getMovie, movie })
                    .Join(db.Summary, x => new { x.getMovie.Movie_ID, x.getMovie.Writer_ID }, summary => new { Movie_ID = summary.Movie_ID, Writer_ID = summary.Writer_ID }, (x, summary) => new { x.getMovie, x.movie, summary })
                    .Join(db.Writer, summary => summary.summary.Writer_ID, writer => writer.Writer_ID, (summary, writer) => new { summary.getMovie, summary.movie, summary.summary, writer })
                    .Distinct()
                    .ToList();

                if (result != null)
                {

                    var responseData = result.Select(item => new
                    {
                        Movie = new
                        {
                            item.movie.Movie_ID,
                            item.movie.Name

                        },
                        Writer = new
                        {
                            item.writer.Writer_ID,
                            item.writer.UserName
                        }

                    });
                    return Request.CreateResponse(HttpStatusCode.OK, responseData);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Movie not found");
                }
            }
        }


        [HttpGet]
        public HttpResponseMessage GetSummary(int Writer_ID, int Movie_ID)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();

            db.Configuration.LazyLoadingEnabled = false;
            var result = db.Summary
                .Where(summary => summary.Writer_ID == Writer_ID && summary.Movie_ID == Movie_ID)
                .Join(db.Writer, summary => summary.Writer_ID, writer => writer.Writer_ID, (summary, writer) => new { summary, writer })
                .Join(db.Movie, x => x.summary.Movie_ID, movie => movie.Movie_ID, (x, movie) => new { x.summary, x.writer, movie })
                .ToList();

            if (result != null && result.Any())
            {
                var responseData = result.Select(item => new
                {
                    Summary = new
                    {
                        item.summary.Summary_ID,
                        item.summary.Summary1
                    },
                    Writer = new
                    {
                        item.writer.Writer_ID,
                        item.writer.UserName
                    },
                    Movie = new
                    {
                        item.movie.Movie_ID,
                    }
                });
                return Request.CreateResponse(HttpStatusCode.OK, responseData);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Movie not found");
            }
        }

        [HttpPut]
        public HttpResponseMessage UpdateReaderPassword(int Reader_ID, string newPassword)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();
            try
            {
                var reader = db.Reader.FirstOrDefault(r => r.Reader_ID == Reader_ID);
                if (reader == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Reader not found");
                }

                
                reader.Password = newPassword;

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Reader password updated successfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPut]
        public HttpResponseMessage UpdateReaderName(int Reader_ID, string newName)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();
            try
            {
                var reader = db.Reader.FirstOrDefault(r => r.Reader_ID == Reader_ID);
                if (reader == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Reader not found");
                }

               
                reader.UserName = newName;

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Reader name updated successfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        public HttpResponseMessage SearchMovies(string searchQuery)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();
            try
            {
                
                var searchResults = db.Movie
                    .Where(m => m.Name.Contains(searchQuery) || m.Category.Contains(searchQuery))
                    .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, searchResults);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

       /* [HttpGet]
        public HttpResponseMessage GetClip(int Writer_ID, int Movie_ID)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();

            db.Configuration.LazyLoadingEnabled = false;
            var result = db.Clips
                .Where(clip => clip.Writer_ID == Writer_ID && clip.Movie_ID == Movie_ID)
                .Join(db.Writer, clip => clip.Writer_ID, writer => writer.Writer_ID, (clip, writer) => new { clip, writer })
                .Join(db.Movie, x => x.clip.Movie_ID, movie => movie.Movie_ID, (x, movie) => new { x.clip, x.writer, movie })
                .ToList();

            if (result != null && result.Any())
            {
                var responseData = result.Select(item => new
                {
                    Clips = new
                    {
                        item.clip.Clips_ID,
                        item.clip.Url,
                        item.clip.Start_time,
                        item.clip.End_time
                    },
                    Writer = new
                    {
                        item.writer.Writer_ID,
                    },
                    Movie = new
                    {
                        item.movie.Movie_ID,
                    },
                });
                return Request.CreateResponse(HttpStatusCode.OK, responseData);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Clip not found");
            }
        }*/





        /*[HttpGet]
        public HttpResponseMessage GetFavoriteDetails(int Reader_ID)
        {
            try
            {
                using (var db = new BlinkMovieEntities())
                {
                    var result = db.Favorites
                        .Where(fav => fav.Reader_ID == Reader_ID)
                        .Join(db.Writer, fav => fav.Writer_ID, writer => writer.Writer_ID, (fav, writer) => new { fav, writer })
                        .Join(db.Movie, x => x.fav.Movie_ID, movie => movie.Movie_ID, (x, movie) => new { x.fav, x.writer, movie })
                        .ToList();

                    var details = result.Select(x => new
                    {
                        ReaderId = x.fav.Reader_ID,
                        WriterId = x.fav.Writer_ID,
                        MovieId = x.fav.Movie_ID,
                        WriterName = x.writer.UserName,
                        MovieTitle = x.movie.Name,
                        Director = x.movie.Director,
                        MovieRating = x.movie.Rating,
                        WriterRating = x.writer.Rating

                    }).ToList<object>();

                    return Request.CreateResponse(HttpStatusCode.OK, details);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }*/


        [HttpGet]
        public HttpResponseMessage GetFavoriteDetails(int Reader_ID)
        {
            try
            {
                using (var db = new BlinkMovieEntities())
                {
                    var result = db.Favorites
                        .Where(fav => fav.Reader_ID == Reader_ID)
                        .Join(db.Writer, fav => fav.Writer_ID, writer => writer.Writer_ID, (fav, writer) => new { fav, writer })
                        .Join(db.Movie, x => x.fav.Movie_ID, movie => movie.Movie_ID, (x, movie) => new { x.fav, x.writer, movie })
                        .ToList();

                    var details = result
                        .GroupBy(x => x.fav.Movie_ID)
                        .Select(g => g.First())
                        .Select(x => new
                        {
                            ReaderId = x.fav.Reader_ID,
                            WriterId = x.fav.Writer_ID,
                            MovieId = x.fav.Movie_ID,
                            WriterName = x.writer.UserName,
                            MovieTitle = x.movie.Name,
                            Director = x.movie.Director,
                            MovieRating = x.movie.Rating,
                            WriterRating = x.writer.Rating
                        })
                        .ToList<object>();

                    return Request.CreateResponse(HttpStatusCode.OK, details);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        /*[HttpGet]
        public HttpResponseMessage GetClip(int Writer_ID, int Movie_ID)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();

            db.Configuration.LazyLoadingEnabled = false;
            var result = db.Clips
                .Where(clip => clip.Writer_ID == Writer_ID && clip.Movie_ID == Movie_ID)
                .Join(db.Writer, clip => clip.Writer_ID, writer => writer.Writer_ID, (clip, writer) => new { clip, writer })
                .Join(db.Movie, x => x.clip.Movie_ID, movie => movie.Movie_ID, (x, movie) => new { x.clip, x.writer, movie })
                .ToList();

            if (result != null && result.Any())
            {
                var responseData = result.Select(item => new
                {
                    Clips = new
                    {
                        item.clip.Clips_ID,
                        item.clip.Url,
                        item.clip.Start_time,
                        item.clip.End_time
                    },
                    Writer = new
                    {
                        item.writer.Writer_ID,
                    },
                    Movie = new
                    {
                        item.movie.Movie_ID,
                    },
                });
                return Request.CreateResponse(HttpStatusCode.OK, responseData);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Clip not found");
            }
        }
*/


        [HttpGet]
        public HttpResponseMessage GetClip(int Writer_ID, int Movie_ID)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();

            db.Configuration.LazyLoadingEnabled = false;
            var result = db.Clips
                .Where(clip => clip.Writer_ID == Writer_ID && clip.Movie_ID == Movie_ID)
                .Join(db.Writer, clip => clip.Writer_ID, writer => writer.Writer_ID, (clip, writer) => new { clip, writer })
                .Join(db.Movie, x => x.clip.Movie_ID, movie => movie.Movie_ID, (x, movie) => new { x.clip, x.writer, movie })
                .ToList();

            if (result != null && result.Any())
            {
                var Clips = result.Select(item => new
                {
                 
                        item.clip.Clips_ID,
                        item.clip.Url,
                        item.clip.Start_time,
                        item.clip.End_time
                    


                });
            return Request.CreateResponse(HttpStatusCode.OK, Clips);
        }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Clip not found");
            }
}


[HttpGet]
        public HttpResponseMessage GetHistoryDetails(int Reader_ID)
        {
            try
            {
                using (var db = new BlinkMovieEntities())
                {
                    var result = db.History
                        .Where(history => history.Reader_ID == Reader_ID)
                        .Join(db.Writer, history => history.Writer_ID, writer => writer.Writer_ID, (history, writer) => new { history, writer })
                        .Join(db.Movie, x => x.history.Movie_ID, movie => movie.Movie_ID, (x, movie) => new { x.history, x.writer, movie })
                        .ToList();

                    var details = result.Select(x => new
                    {
                        ReaderId = x.history.Reader_ID,
                        WriterId = x.history.Writer_ID,
                        MovieId = x.history.Movie_ID,
                        WriterName = x.writer.UserName,
                        MovieTitle = x.movie.Name,
                        Director = x.movie.Director,
                        MovieRating = x.movie.Rating,
                        WriterRating = x.writer.Rating
                    }).ToList<object>();

                    return Request.CreateResponse(HttpStatusCode.OK, details);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]

        public HttpResponseMessage GetTopRatedMovies()
        {
            BlinkMovieEntities db = new BlinkMovieEntities();
            db.Configuration.LazyLoadingEnabled = false;

            var movies = db.Movie.Where(m => m.AverageRating > 4.0 && m.Type == "Movie").Select(s => new
            {
                s.Movie_ID,
                s.Rating,
                s.Name,
                s.AverageRating,
                s.Image,
                s.Type,
                s.Category,
                s.Director
            }).ToList();

            if (movies != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, movies);
            }

            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found");
            }

        }


        /* [HttpGet]

         public HttpResponseMessage GetTopRatedMovies()
         {
             BlinkMovieEntities db = new BlinkMovieEntities();
             db.Configuration.LazyLoadingEnabled = false;

             var movies = db.Movie.Where(m => m.Rating > 4.0).ToList<Movie>();
             if (movies != null)
             {
                 return Request.CreateResponse(HttpStatusCode.OK, movies);
             }

             else
             {
                 return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found");
             }

         }*/


        /* [HttpGet]
         public HttpResponseMessage GetMoviesByInterests(int Reader_ID)
         {
             using (BlinkMovieEntities db = new BlinkMovieEntities())
             {
                 db.Configuration.LazyLoadingEnabled = false;


                 var reader = db.Reader.FirstOrDefault(r => r.Reader_ID == Reader_ID);

                 if (reader == null)
                 {
                     return Request.CreateResponse(HttpStatusCode.NotFound, "Reader not found");
                 }


                 string[] readerInterests = reader.Interest.Split(',');


                 var movies = db.Movie
                     .Where(m => m.Category != null)
                     .ToList()
                     .Where(m => m.Category.Split(',').Any(g => readerInterests.Contains(g)))
                     .ToList();

                 if (movies.Count > 0)
                 {
                     return Request.CreateResponse(HttpStatusCode.OK, movies);
                 }
                 else
                 {
                     return Request.CreateResponse(HttpStatusCode.NotFound, "No movies found based on reader's interests");
                 }
             }
         }
 */


        [HttpGet]
        public HttpResponseMessage GetMoviesByInterests(int Reader_ID)
        {
            using (BlinkMovieEntities db = new BlinkMovieEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;


                var reader = db.Reader.FirstOrDefault(r => r.Reader_ID == Reader_ID);

                if (reader == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Reader not found");
                }


                string[] readerInterests = reader.Interest.Split(',');


                var movies = db.Movie
                    .Where(m => m.Category != null)
                    .ToList()
                    .Where(m => m.Category.Split(',').Any(g => readerInterests.Contains(g)))
                    .ToList();

                if (movies.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, movies);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No movies found based on reader's interests");
                }
            }
        }







        [HttpGet]
        public HttpResponseMessage IssueFreeMovie(int readerId)
        {
            using (BlinkMovieEntities db = new BlinkMovieEntities())
            {
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                try
                {

                    var lastIssuedDate = db.FreeMovie
                                    .Where(fm => fm.Reader_ID == readerId)
                                    .OrderByDescending(fm => fm.issueDate)
                                    .Select(fm => fm.issueDate)
                                    .FirstOrDefault();

                    string todayDateString = DateTime.Today.ToString("yyyy-MM-dd");
                    bool hasDayPassed = (lastIssuedDate != null && DateTime.Parse(lastIssuedDate) < DateTime.Today);





                    var readerIssuedFreeMovie = db.FreeMovie.Where(fm => fm.Reader_ID == readerId).ToList();



                    if (hasDayPassed || lastIssuedDate == null)
                    {



                    again:
                        var randomSummary = db.Summary
                                               .OrderBy(r => Guid.NewGuid())
                                               .FirstOrDefault();


                        if (lastIssuedDate == null)
                        {
                            randomSummary = db.Summary
                                              .OrderBy(r => Guid.NewGuid())
                                              .FirstOrDefault();

                            if (randomSummary == null)
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "No available summaries to issue.");
                            }
                        }

                        if (randomSummary.Movie_ID == null)
                        {
                            goto again;
                        }

                        if (readerIssuedFreeMovie.Count == db.Summary.Count())
                        {
                            var recordsToDelete = db.FreeMovie.Where(fm => fm.Reader_ID == readerId);

                            db.FreeMovie.RemoveRange(recordsToDelete);

                            db.SaveChanges();
                        }

                        foreach (var issuedMovie in readerIssuedFreeMovie)
                        {
                            if (issuedMovie.Movie_ID == randomSummary.Movie_ID && issuedMovie.Writer_ID == randomSummary.Writer_ID)
                            {

                                goto again;
                            }
                        }

                        var movie = db.Movie.Where(m => m.Movie_ID == randomSummary.Movie_ID).Select(s => new
                        {
                            s.Movie_ID,
                            s.Name,
                            s.Image,
                            s.CoverImage,
                            s.Type
                        }).FirstOrDefault();


                        var writer = db.Writer.Where(w => w.Writer_ID == randomSummary.Writer_ID).Select(s => new
                        {
                            s.Writer_ID,
                            s.UserName,

                        }).FirstOrDefault();


                        FreeMovie newFreeMovie = new FreeMovie
                        {
                            FreeMovie_ID = GenerateId(),
                            Movie_ID = randomSummary.Movie_ID,
                            Writer_ID = randomSummary.Writer_ID,
                            issueDate = todayDateString,
                            Reader_ID = readerId,
                            Episode = randomSummary.Episode,
                        };

                        db.FreeMovie.Add(newFreeMovie);
                        db.SaveChanges();


                        var result = new
                        {
                            Movie = movie,
                            Writer = writer,
                            Episode = randomSummary.Episode,
                            IssuedMovie = newFreeMovie
                        };


                        string resultJson = JsonConvert.SerializeObject(result, jsonSettings);

                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                        response.Content = new StringContent(resultJson, Encoding.UTF8, "application/json");

                        return response;
                    }
                    else
                    {

                        var lastIssuedMovie = db.FreeMovie
                                                .Where(fm => fm.Reader_ID == readerId && fm.issueDate == lastIssuedDate)
                                                .FirstOrDefault();

                        var movie = db.Movie.Where(m => m.Movie_ID == lastIssuedMovie.Movie_ID).Select(s => new
                        {
                            s.Movie_ID,
                            s.Name,
                            s.Image,
                            s.CoverImage,
                            s.Type
                        }).FirstOrDefault();


                        var writer = db.Writer.Where(w => w.Writer_ID == lastIssuedMovie.Writer_ID).Select(s => new
                        {
                            s.Writer_ID,
                            s.UserName,
                        }).FirstOrDefault();




                        var result = new
                        {
                            issueDate = lastIssuedDate,
                            Movie = movie,
                            Writer = writer,
                            Episode = lastIssuedMovie.Episode,

                        };


                        string resultJson = JsonConvert.SerializeObject(result, jsonSettings);

                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                        response.Content = new StringContent(resultJson, Encoding.UTF8, "application/json");

                        return response;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "An error occurred while processing the request.");
                }
            }
        }



        [HttpGet]
        public HttpResponseMessage IssuePaidMovie()
        {
            using (BlinkMovieEntities db = new BlinkMovieEntities())
            {
                var projects = db.SentProject
                    .Where(s => s.Status == "Accepted")
                    .Select(s => new
                    {
                        s.Movie_ID,
                        s.Writer_ID,
                        s.SentProject_ID,
                        s.SentProposal_ID,
                        ProposalData = db.SentProposals
                            .Where(sp => sp.SentProposal_ID == s.SentProposal_ID)
                            .Select(sp => new
                            {
                                sp.Movie_Name,
                                sp.Image,
                                sp.Director,
                                sp.Type,
                                sp.Genre
                            })
                            .FirstOrDefault(),
                        WriterData = db.Writer
                            .Where(w => w.Writer_ID == s.Writer_ID)
                            .Select(w => new
                            {
                                w.UserName,
                                w.Interest,
                                w.AverageRating
                            })
                            .FirstOrDefault(),
                        MovieData = db.Movie
                            .Where(m => m.Movie_ID == s.Movie_ID)
                            .Select(m => new
                            {
                                m.AverageRating
                            })
                            .FirstOrDefault(),
                        s.Status
                    })
                    .ToList();
                    
                    

                var responseContent = new
                {
                    Project = projects
                };

                return Request.CreateResponse(HttpStatusCode.OK, responseContent);
            }
        }







        [HttpPost]
        public HttpResponseMessage RateWriter(int Reader_ID, int Writer_ID, float writerRate)
        {
            using (BlinkMovieEntities db = new BlinkMovieEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;

                
                var reader = db.Reader.FirstOrDefault(r => r.Reader_ID == Reader_ID);
                if (reader == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Reader not found");
                }

                
                var writer = db.Writer.FirstOrDefault(w => w.Writer_ID == Writer_ID);
                if (writer == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Writer not found");
                }
     
                var existingWriterRating = db.ReaderRate.FirstOrDefault(r => r.Reader_ID == Reader_ID && r.Writer_ID == Writer_ID);

                if (existingWriterRating != null)
                {
                    
                    writer.Rating = ((writer.Rating * writer.Rating) - existingWriterRating.Writer_Rating) / 5;
                    writer.Rating -= 1;

                   
                    db.ReaderRate.Remove(existingWriterRating);
                }

                var writerRateEntity = new ReaderRate
                {
                    ReaderRate_ID = GenerateId(),
                    Reader_ID = Reader_ID,
                    Writer_ID = Writer_ID,
                    Writer_Rating = writerRate
                };
        
                db.ReaderRate.Add(writerRateEntity);
                db.SaveChanges();
          
                writer.Rating = ((writer.Rating * writer.Rating) + writerRate) / 5;
                writer.Rating += 1;

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Writer rating successfully submitted");
            }
        }


        [HttpPost]
        public HttpResponseMessage RateMovie(int Reader_ID, int Movie_ID, float movieRate)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();
            try
            {
                var reader = db.Reader.FirstOrDefault(r => r.Reader_ID == Reader_ID);
                if (reader == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Reader Not Found");
                }

                var movie = db.Movie.FirstOrDefault(m => m.Movie_ID == Movie_ID);
                if (movie == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Movie Not Found");
                }

                var existingReaderRating = db.ReaderRate.FirstOrDefault(r => r.Reader_ID == Reader_ID && r.Movie_ID == Movie_ID);

                if (existingReaderRating != null)
                {
                    movie.Rating = (movie.Rating * movie.TotalNoOfRatings - existingReaderRating.Movie_Rating) / (movie.TotalNoOfRatings - 1);
                    movie.TotalNoOfRatings -= 1;
                    db.ReaderRate.Remove(existingReaderRating);
                }

                var newReaderRating = new ReaderRate
                {
                    ReaderRate_ID = GenerateId(), 
                    Reader_ID = reader.Reader_ID,
                    Movie_ID = movie.Movie_ID,
                    Movie_Rating = movieRate
                };
                db.ReaderRate.Add(newReaderRating);

                var rating =movie.Rating = (movie.Rating * movie.TotalNoOfRatings + movieRate) / (movie.TotalNoOfRatings + 1);
                movie.TotalNoOfRatings += 1;
               
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Movie Rating Updated");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

    }
}


//Free Movie

//in Last
//Balance
//GetNotified
