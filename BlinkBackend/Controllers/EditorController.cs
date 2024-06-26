﻿using BlinkBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Web;
using Newtonsoft.Json;
using System.Web.Http;
using System.IO;
using System.Web.Http.Cors;
using Microsoft.AspNetCore.Mvc;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using HttpPutAttribute = System.Web.Http.HttpPutAttribute;
using Microsoft.AspNetCore.Hosting.Server;
using System.Web.Mvc;

namespace BlinkBackend.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class EditorController : ApiController
    {


        static int GenerateId()
        {

            long timestamp = DateTime.Now.Ticks;
            Random random = new Random();
            int randomComponent = random.Next();

            int userId = (int)(timestamp ^ randomComponent);

            return Math.Abs(userId);
        }


        [HttpGet]
        public HttpResponseMessage perpossal(string MoviName,string genre,int movieId,string type,int editorId,int episode, int balance, string director, string DueDate,string imagePath, string status, int WriterId)
        {

            try
            {
                var request = HttpContext.Current.Request;
                BlinkMovieEntities db = new BlinkMovieEntities();
                DateTime CurrentDate = DateTime.Now;
                // Assuming GenerateId() generates a unique ID for the proposal


                var proposal = new SentProposals
                {
                    SentProposal_ID = GenerateId(),
                    Type = type,
                    Genre = genre,
                    Editor_ID = editorId,
                    Movie_ID = movieId,
                    Movie_Name = MoviName,
                    Director = director,
                    DueDate = DueDate,
                    Status = status,
                    Writer_ID = WriterId,
                    Sent_at = CurrentDate.ToString(),
                    Image = imagePath,
                    Balance = balance,
                    Episode = episode,


                };
               /* var imageFile = request.Files["Image"];
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    string imagePath = SaveImageToDisk(imageFile);

                    proposal.Image = imagePath;
                }*/



                db.SentProposals.Add(proposal);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "DataInserted");
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return Request.CreateResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }






        [HttpPut]
        public HttpResponseMessage UpdateInterests(int Editor_ID, string newInterests)
        {
            using (BlinkMovieEntities db = new BlinkMovieEntities())
            {
                var editor = db.Editor.FirstOrDefault(r => r.Editor_ID == Editor_ID);

                if (editor != null)
                {
                    editor.Interest = newInterests;
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.OK, "Interests updated successfully");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Editor not found");
                }
            }
        }
       
       

        [HttpGet]
        public HttpResponseMessage GetAllMovies()
        {
            using (BlinkMovieEntities db = new BlinkMovieEntities())
            {
                var movies = db.Movie.Select(e=> new
                {
                   key= e.Movie_ID,
                    value= e.Name,
                    e.Director,
                    e.Category,
/*my shoper*/
                    e.CoverImage,
                    e.Image,
                    e.Type,
                    e.Cast
                }).ToList();

                if (movies.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, movies);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No movies found");
                }
            }
        }

       /*  \HUzaifa update [HttpGet]
        public HttpResponseMessage GetAllMovies()fetch
        {
            using (BlinkMovieEntities db = new BlinkMovieEntities())
            {
                var movies = db.Movie.Where(m => m.Type == "Movie").ToList();

                if (movies.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, movies);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No movies found");
                }
            }
        }*/

        [HttpGet]
        public HttpResponseMessage GetAllWriters()
        {
            using (BlinkMovieEntities db = new BlinkMovieEntities())
            {

                var writer = db.Writer.ToList<Writer>();
                if (writer != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, writer);
                }

                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Writer not found");
                }
            }
        }
        /*
         [HttpPost]
         public HttpResponseMessage AcceptSentProject(int sProId)
         {
             BlinkMovieEntities db = new BlinkMovieEntities();
             try
             {

                 var existingProject = db.SentProject.FirstOrDefault(s => s.SentProject_ID == sProId);
                 if (existingProject == null)
                 {
                     return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found");
                 }

                 existingProject.Status = "Accepted";
                 db.SaveChanges();


                 var proposal = db.SentProposals
                                   .Where(s => s.SentProposal_ID == existingProject.SentProposal_ID).Select(s => new {
                                       s.Sent_at,
                                       s.Balance,
                                       s.Type
                                   }).FirstOrDefault();

                 int? balance = 0;

                 if (DateTime.Parse(proposal.Sent_at) <= DateTime.Parse(existingProject.Send_at))
                 {
                     balance = proposal.Balance;
                 }
                 else
                 {
                     balance = (proposal.Balance - ((proposal.Balance * 20) / 100));
                 }

                 var writerBalace = db.Writer.Where(w => w.Writer_ID == existingProject.Writer_ID).FirstOrDefault();

                 writerBalace.Balance = balance;
                 db.SaveChanges();

                 if (proposal.Type == "Movie")
                 {
                     var clips = db.Clips.Where(c => c.Sent_ID == sProId).ToList();

                     if (clips.Any())
                     {
                         foreach (var clip in clips)
                         {
                             clip.Movie_ID = existingProject.Movie_ID;
                             clip.Sent_ID = null;
                         }
                         db.SaveChanges();
                     }
                     else
                     {
                         return Request.CreateResponse(HttpStatusCode.NotFound, "Clips not found");
                     }
                 }
                 else
                 {
                     var clips = db.DramasClips.Where(c => c.Sent_ID == sProId).ToList();

                     if (clips.Any())
                     {
                         foreach (var clip in clips)
                         {
                             clip.Movie_ID = existingProject.Movie_ID;
                             clip.Sent_ID = null;

                         }
                         db.SaveChanges();
                     }
                     else
                     {
                         return Request.CreateResponse(HttpStatusCode.NotFound, "Clips not found");
                     }
                 }



                 var summary = db.Summary.FirstOrDefault(s => s.Sent_ID == sProId);

                 if (summary != null)
                 {
                     summary.Movie_ID = existingProject.Movie_ID;
                     summary.Sent_ID = null;

                     db.SaveChanges();

                 }
                 else
                 {
                     return Request.CreateResponse(HttpStatusCode.NotFound, "Summary not found");
                 }

                 var setMovie = new GetMovie()
                 {
                     GetMovie_ID = GenerateId(),
                     // Clips_ID = clip.Clips_ID,
                     Writer_ID = existingProject.Writer_ID,
                     Movie_ID = existingProject.Movie_ID,
                     // Summary_ID = summary.Summary_ID,

                 };
                 db.GetMovie.Add(setMovie);
                 db.SaveChanges();
                 return Request.CreateResponse(HttpStatusCode.OK, "Uploaded");
             }
             catch (Exception ex)
             {
                 return Request.CreateResponse(ex);
             }
         }

 */

        [HttpPost]
        public HttpResponseMessage AcceptSentProject(int sProId)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();
            try
            {

                var existingProject = db.SentProject.FirstOrDefault(s => s.SentProject_ID == sProId);
                if (existingProject == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found");
                }

                existingProject.Status = "Accepted";
                db.SaveChanges();


                var proposal = db.SentProposals
                                  .Where(s => s.SentProposal_ID == existingProject.SentProposal_ID).Select(s => new {
                                      s.Sent_at,
                                      s.Balance,
                                      s.Type
                                  }).FirstOrDefault();

                int? balance = 0;

                if (DateTime.Parse(proposal.Sent_at) <= DateTime.Parse(existingProject.Send_at))
                {
                    balance = proposal.Balance;
                }
                else
                {
                    balance = (proposal.Balance - ((proposal.Balance * 20) / 100));
                }

                var writerBalace = db.Writer.Where(w => w.Writer_ID == existingProject.Writer_ID).FirstOrDefault();

                writerBalace.Balance = balance;
                db.SaveChanges();

                if (proposal.Type == "Movie")
                {
                    var clips = db.Clips.Where(c => c.Sent_ID == sProId).ToList();

                    if (clips.Any())
                    {
                        foreach (var clip in clips)
                        {
                            clip.Movie_ID = existingProject.Movie_ID;
                            clip.Sent_ID = null;
                        }
                        db.SaveChanges();
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Clips not found");
                    }
                }
                else
                {
                    var clips = db.DramasClips.Where(c => c.Sent_ID == sProId).ToList();

                    if (clips.Any())
                    {
                        foreach (var clip in clips)
                        {
                            clip.Movie_ID = existingProject.Movie_ID;
                            clip.Sent_ID = null;

                        }
                        db.SaveChanges();
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Clips not found");
                    }
                }



                var summary = db.Summary.FirstOrDefault(s => s.Sent_ID == sProId);

                if (summary != null)
                {
                    summary.Movie_ID = existingProject.Movie_ID;
                    summary.Sent_ID = null;

                    db.SaveChanges();

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Summary not found");
                }

                var setMovie = new GetMovie()
                {
                    GetMovie_ID = GenerateId(),
                    // Clips_ID = clip.Clips_ID,
                    Writer_ID = existingProject.Writer_ID,
                    Movie_ID = existingProject.Movie_ID,
                    // Summary_ID = summary.Summary_ID,

                };
                db.GetMovie.Add(setMovie);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Uploaded");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(ex);
            }
        }





        [HttpPost]
        public HttpResponseMessage SentMovie()
        {
            BlinkMovieEntities db = new BlinkMovieEntities();
            var request = HttpContext.Current.Request;

            try
            {
                int? movieId = Int32.Parse(request["Movie_ID"]);
               /* int? editorId = Int32.Parse(request["Editor_ID"]);
                int? writerId = Int32.Parse(request["Writer_ID"]);*/
                string movieName = request["Movie_Name"];
                string[] genreArray = request.Form.GetValues("Genre");
                HashSet<string> uniqueGenres = new HashSet<string>(genreArray);
                string genre = string.Join(",", uniqueGenres);
                string type = request["Type"];
                string director = request["Director"];
                string cast = request["Cast"];
                /*string dueDate = request["DueDate"];*/

                if (movieId != 0)
                {
                    var proposal = new SentProposals()
                    {
                        SentProposal_ID = GenerateId(),
                        Movie_ID = movieId,
                       /* Editor_ID = editorId,
                        Writer_ID = writerId,*/
                        Movie_Name = movieName,
                        Image = request["Image"],
                        Cover_Image = request["Cover_Image"],
                        Genre = genre,
                        Type = type,
                        Director = director,
                        Cast = cast,
                        /*DueDate = dueDate,*/
                        /*Status = "Sent",*/
                    };
                    /*db.SentProposals.Add(proposal);*/
                    db.SaveChanges();

                    var response = new
                    {
                        proposal,
                        haahaha = " faafafaf"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    int id = GenerateId();

                    var movie = new Movie()
                    {
                        Movie_ID = id,
                        Name = movieName,
                        Category = genre,
                        Type = type,
                        Cast = cast,
                        Director = director,
                        anySummaryOrClip = false
                    };


                    var imageFile = request.Files["Image"];
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        string imagePath = SaveImageToDisk(imageFile);

                        movie.Image = imagePath;
                    }

                    var coverImageFile = request.Files["Cover_Image"];
                    if (coverImageFile != null && coverImageFile.ContentLength > 0)
                    {
                        string imagePath = SaveImageToDisk(coverImageFile);
                        movie.CoverImage = imagePath;
                    }


                    db.Movie.Add(movie);
                    db.SaveChanges();

                    /*var proposal = new SentProposals()
                    {
                        SentProposal_ID = GenerateId(),
                        Movie_ID = id,
                        Editor_ID = editorId,
                        Writer_ID = writerId,
                        Movie_Name = movieName,
                        Image = movie.Image,
                        Cover_Image = movie.CoverImage,
                        Genre = string.Join(",", genreArray),
                        Type = type,
                        Director = director,
                        DueDate = dueDate,
                        Status = "Sent",
                    };*/
                    /*db.SentProposals.Add(proposal);*/
                    db.SaveChanges();

                    var response = new
                    {
                        movie,
                        /*proposal*/
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



        /*
                [HttpPost]
                public HttpResponseMessage SentProposal()
                {
                    BlinkMovieEntities db = new BlinkMovieEntities();
                    DateTime CurrentDate = DateTime.Now;
                    var request = HttpContext.Current.Request;



                    try
                    {
                        int? movieId = Int32.Parse(request["Movie_ID"]);
                        int? editorId = Int32.Parse(request["Editor_ID"]);
                        int? writerId = Int32.Parse(request["Writer_ID"]);
                        string movieName = request["Movie_Name"];
                        string[] genreArray = request.Form.GetValues("Genre");
                        HashSet<string> uniqueGenres = new HashSet<string>(genreArray);
                        string genre = string.Join(",", uniqueGenres);
                        string type = request["Type"];
                        string director = request["Director"];
                        string dueDate = request["DueDate"];



                        var proposal = new SentProposals()
                        {
                            SentProposal_ID = GenerateId(),
                            Movie_ID = movieId,
                            Editor_ID = editorId,
                            Writer_ID = writerId,
                            Movie_Name = movieName,
                            Image = request["Image"],
                            *//*Cover_Image = request["Cover_Image"],*//*
                            Genre = genre,
                            Type = type,
                            Director = director,
                            DueDate = dueDate,
                            Status = "Sent",
                            Sent_at = CurrentDate.ToString()
                        };
                        db.SentProposals.Add(proposal);
                        db.SaveChanges();

                        var response = new
                        {
                            proposal,
                            haahaha = " faafafaf"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);

                    }
                    catch (Exception ex)
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                    }
                }*/



        /* [HttpPost]
          public HttpResponseMessage SentProposal()
         {
             BlinkMovieEntities db = new BlinkMovieEntities();
             DateTime CurrentDate = DateTime.Now;
             var request = HttpContext.Current.Request;



             try
             {
                 int? movieId = Int32.Parse(request["Movie_ID"]);
                 int? editorId = Int32.Parse(request["Editor_ID"]);
                 int? writerId = Int32.Parse(request["Writer_ID"]);
                 string movieName = request["Movie_Name"];
                 string[] genreArray = request.Form.GetValues("Genre");
                 HashSet<string> uniqueGenres = new HashSet<string>(genreArray);
                 string genre = string.Join(",", uniqueGenres);
                 string type = request["Type"];
                 string director = request["Director"];
                 string dueDate = request["DueDate"];
                 int balance = Int32.Parse(request["Balance"]);


                 if (movieId != 0)
                 {
                     var proposal = new SentProposals()
                     {
                         SentProposal_ID = GenerateId(),
                         Movie_ID = movieId,
                         Editor_ID = editorId,
                         Writer_ID = writerId,
                         Movie_Name = movieName,
                         Image = request["Image"],
                         Cover_Image = request["Cover_Image"],
                         Genre = genre,
                         Type = type,
                         Director = director,
                         DueDate = dueDate,
                         Status = "Sent",
                         Sent_at = CurrentDate.ToString(),
                         Balance =balance
                     };
                     db.SentProposals.Add(proposal);
                     db.SaveChanges();

                     var response = new
                     {
                         proposal,
                         haahaha = " faafafaf"
                     };
                     return Request.CreateResponse(HttpStatusCode.OK, response);
                 }
                 else
                 {
                     int id = GenerateId();

                     var movie = new Movie()
                     {
                         Movie_ID = id,
                         Name = movieName,
                         Category = genre,
                         Type = type,
                         Director = director,
                         anySummaryOrClip = false,

                     };


                     var imageFile = request.Files["Image"];
                     if (imageFile != null && imageFile.ContentLength > 0)
                     {
                         string imagePath = SaveImageToDisk(imageFile);

                         movie.Image = imagePath;
                     }

                     var coverImageFile = request.Files["Cover_Image"];
                     if (coverImageFile != null && coverImageFile.ContentLength > 0)
                     {
                         string imagePath = SaveImageToDisk(coverImageFile);
                         movie.CoverImage = imagePath;
                     }


                     db.Movie.Add(movie);
                     db.SaveChanges();

                     var proposal = new SentProposals()
                     {
                         SentProposal_ID = GenerateId(),
                         Movie_ID = id,
                         Editor_ID = editorId,
                         Writer_ID = writerId,
                         Movie_Name = movieName,
                         Image = movie.Image,
                         Cover_Image = movie.CoverImage,
                         Genre = string.Join(",", genreArray),
                         Type = type,
                         Director = director,
                         DueDate = dueDate,
                         Status = "Sent",
                         Sent_at = CurrentDate.ToString(),
                         Balance = balance
                     };
                     db.SentProposals.Add(proposal);
                     db.SaveChanges();

                     var response = new
                     {
                         movie,
                         proposal
                     };
                     return Request.CreateResponse(HttpStatusCode.OK, response);
                 }
             }
             catch (Exception ex)
             {
                 return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
             }
         }

 */
        [HttpPost]
        public HttpResponseMessage SentProposal()
        {
            BlinkMovieEntities db = new BlinkMovieEntities();
            DateTime CurrentDate = DateTime.Now;
            var request = HttpContext.Current.Request;



            try
            {
                int? movieId = Int32.Parse(request["Movie_ID"]);
                int? editorId = Int32.Parse(request["Editor_ID"]);
                int? writerId = Int32.Parse(request["Writer_ID"]);
                string movieName = request["Movie_Name"];
                string[] genreArray = request.Form.GetValues("Genre");
                HashSet<string> uniqueGenres = new HashSet<string>(genreArray);
                string genre = string.Join(",", uniqueGenres);
                string type = request["Type"];
                string director = request["Director"];
                string dueDate = request["DueDate"];
                int balance = Int32.Parse(request["Balance"]);
                string cast = request["Cast"];

                if (movieId != 0)
                {
                    var proposal = new SentProposals()
                    {
                        SentProposal_ID = GenerateId(),
                        Movie_ID = movieId,
                        Editor_ID = editorId,
                        Writer_ID = writerId,
                        Movie_Name = movieName,
                        Image = request["Image"],
                        Cover_Image = request["Cover_Image"],
                        Genre = genre,
                        Type = type,
                        Director = director,
                        DueDate = dueDate,
                        Status = "Sent",
                        Cast = cast,
                        Sent_at = CurrentDate.ToString(),
                        Balance = balance
                    };
                    db.SentProposals.Add(proposal);
                    db.SaveChanges();

                    var response = new
                    {
                        proposal,
                        haahaha = " faafafaf"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    int id = GenerateId();

                    var movie = new Movie()
                    {
                        Movie_ID = id,
                        Name = movieName,
                        Category = genre,
                        Cast = cast,
                        Type = type,
                        Director = director,
                        anySummaryOrClip = false,

                    };


                    var imageFile = request.Files["Image"];
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        string imagePath = SaveImageToDisk(imageFile);

                        movie.Image = imagePath;
                    }

                    var coverImageFile = request.Files["Cover_Image"];
                    if (coverImageFile != null && coverImageFile.ContentLength > 0)
                    {
                        string imagePath = SaveImageToDisk(coverImageFile);
                        movie.CoverImage = imagePath;
                    }


                    db.Movie.Add(movie);
                    db.SaveChanges();

                    var proposal = new SentProposals()
                    {
                        SentProposal_ID = GenerateId(),
                        Movie_ID = id,
                        Editor_ID = editorId,
                        Writer_ID = writerId,
                        Movie_Name = movieName,
                        Image = movie.Image,
                        Cover_Image = movie.CoverImage,
                        Genre = string.Join(",", genreArray),
                        Type = type,
                        Director = director,
                        DueDate = dueDate,
                        Status = "Sent",
                        Sent_at = CurrentDate.ToString(),
                        Balance = balance,
                        Cast = cast,
                    };
                    db.SentProposals.Add(proposal);
                    db.SaveChanges();

                    var response = new
                    {
                        movie,
                        proposal
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



        /*
                [HttpGet]
                public HttpResponseMessage ReceiveSentProject()
                {
                    BlinkMovieEntities db = new BlinkMovieEntities();
                    var projects = db.SentProject
                        .Where(s => s.Status == "Sent")
                        .Select(s => new
                        {
                            s.Movie_ID,
                            s.SentProposal_ID,
                            s.Editor_ID,
                            s.Writer_ID,
                            s.SentProject_ID,
                            ProposalData = db.SentProposals
                                .Where(sp => sp.SentProposal_ID == s.SentProposal_ID)
                                .Select(sp => new
                                {
                                    sp.SentProposal_ID,
                                    sp.Movie_Name,
                                    sp.Image,
                                    sp.Genre,
                                    sp.Type,
                                    sp.Director
                                })
                                .FirstOrDefault(),
                            Writer_Name = db.Writer.FirstOrDefault(w => w.Writer_ID == s.Writer_ID).UserName,
                        })

                        .ToList();


                    var responseContent = new
                    {
                        Project = projects
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, responseContent);
                }*/


        [HttpGet]
        public HttpResponseMessage ReceiveSentProject(int Editor_ID)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();
            try
            {
                var projects = db.SentProject
      .Where(s => s.Status == "Sent" && s.Editor_ID == Editor_ID)
      .Select(s => new
      {
          s.SentProject_ID,
          s.Movie_ID,
          s.SentProposal_ID,
          s.Editor_ID,
          s.Writer_ID,
          s.Send_at,
          ProposalData = db.SentProposals
              .Where(sp => sp.SentProposal_ID == s.SentProposal_ID)
              .Select(sp => new
              {
                  sp.SentProposal_ID,
                  sp.Movie_Name,
                  sp.Image,
                  sp.Genre,
                  sp.Type,
                  sp.Director
              })
              .FirstOrDefault(),
          Writer_Name = db.Writer.FirstOrDefault(w => w.Writer_ID == s.Writer_ID).UserName,
      })
      .OrderByDescending(s => s.Send_at)
      .ToList();


                return Request.CreateResponse(HttpStatusCode.OK, projects);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }



        [HttpGet]
        public HttpResponseMessage ViewSentProject1(int Movie_ID, int WriterID)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();
            var summary = db.Summary.Where(s => s.Movie_ID == Movie_ID && s.Writer_ID == WriterID).Select(s => new {
                s.Movie_ID,
                s.Summary1,
                s.Writer_ID
            });

            var project = db.SentProject.FirstOrDefault(s => s.Movie_ID == Movie_ID);
            if (project == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "SentProject not found");
            }
            var movieData = db.Movie
                                    .FirstOrDefault(m => m.Movie_ID == project.Movie_ID);
            string mediaType = movieData.Type;

            object clipsData = null;
            if (mediaType == "Movie")
            {
                clipsData = db.Clips
                                .Where(s => s.Movie_ID == Movie_ID && s.Writer_ID == WriterID)
                                .Select(s => new
                                {
                                    s.Clips_ID,
                                    s.Url,
                                    s.End_time,
                                    s.Start_time,
                                    s.Title,
                                    s.isCompoundClip,
                                    s.Description

                                });
            }
            else
            {
                clipsData = db.DramasClips
                                .Where(s => s.Movie_ID == Movie_ID)
                                .Select(s => new
                                {
                                    s.DramasClip_ID,
                                    s.Url,
                                    s.End_time,
                                    s.Start_time,
                                    s.Title,
                                    s.Episode,
                                    s.isCompoundClip
                                });
            }


            var responseContent = new
            {
                Movie_ID,
                Summary = summary,
                Clips = clipsData
            };


            return Request.CreateResponse(HttpStatusCode.OK, responseContent);
        }










        [HttpGet]
        public HttpResponseMessage ViewSentProject(int Movie_ID)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();
            var summary = db.Summary.Where(s => s.Movie_ID == Movie_ID).Select( s=> new {
                s.Movie_ID,
                s.Summary1,
                s.Writer_ID
                
                
            });
            var clip = db.Clips.Where(s => s.Movie_ID == Movie_ID).Select(c => new {
                c.Movie_ID,
                c.Url,
                 c.Start_time,
                c.End_time,
                c.Clips_ID,
                c.Title
            });

            var responseContent = new
            {
                Movie_ID,
                Summary =summary,
                Clips = clip
            };


            return Request.CreateResponse(HttpStatusCode.OK,responseContent);
        }
        /* [HttpGet]
         public HttpResponseMessage FetchSummary(int sentProjectId)
         {
             BlinkMovieEntities db = new BlinkMovieEntities();

             try
             {
                 var project = db.SentProject.FirstOrDefault(s => s.SentProject_ID == sentProjectId);

                 if (project == null)
                 {
                     return Request.CreateResponse(HttpStatusCode.NotFound, "SentProject not found");
                 }

                 var summaryData = db.Summary
                                     .FirstOrDefault(s => s.Sent_ID == project.SentProject_ID);

                 var movieData = db.Movie
                                     .FirstOrDefault(m => m.Movie_ID == project.Movie_ID);

                 var writerData = db.Writer
                                     .FirstOrDefault(w => w.Writer_ID == project.Writer_ID);

                 string mediaType = movieData.Type;

                 object clipsData = null;

                 if (mediaType == "Movie")
                 {
                     clipsData = db.Clips
                                     .Where(c => c.Sent_ID == project.SentProject_ID)
                                     .Select(s => new
                                     {
                                         s.Clips_ID,
                                         s.Url,
                                         s.End_time,
                                         s.Start_time,
                                         s.Title,
                                         s.isCompoundClip
                                     }).OrderBy(s => s.Start_time).ToList();
                 }
                 else
                 {
                     clipsData = db.DramasClips
                                     .Where(c => c.Sent_ID == project.SentProject_ID)
                                     .Select(s => new
                                     {
                                         s.DramasClip_ID,
                                         s.Url,
                                         s.End_time,
                                         s.Start_time,
                                         s.Title,
                                         s.Episode,
                                         s.isCompoundClip
                                     }).OrderBy(s => s.Start_time).ToList();
                 }

                 var responseData = new
                 {
                     SentProject = project,
                     SummaryData = summaryData,
                     MovieData = movieData,
                     WriterData = writerData,
                     ClipsData = clipsData
                 };

                 return Request.CreateResponse(HttpStatusCode.OK, responseData);
             }
             catch (Exception ex)
             {
                 return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
             }
         }
 */
        /*
                [HttpGet]
                public HttpResponseMessage FetchSummary(int sentProjectId)
                {
                    BlinkMovieEntities db = new BlinkMovieEntities();

                    try
                    {
                        var project = db.SentProject.FirstOrDefault(s => s.SentProject_ID == sentProjectId);

                        if (project == null)
                        {
                            return Request.CreateResponse(HttpStatusCode.NotFound, "SentProject not found");
                        }

                        var summaryData = db.Summary
                                            .FirstOrDefault(s => s.Sent_ID == project.SentProject_ID);

                        var movieData = db.Movie
                                            .FirstOrDefault(m => m.Movie_ID == project.Movie_ID);

                        var writerData = db.Writer
                                            .FirstOrDefault(w => w.Writer_ID == project.Writer_ID);

                        string mediaType = movieData.Type;

                        object clipsData = null;

                        if (mediaType == "Movie")
                        {
                            clipsData = db.Clips
                                            .Where(c => c.Sent_ID == project.SentProject_ID)
                                            .Select(s => new
                                            {
                                                s.Clips_ID,
                                                s.Url,
                                                s.End_time,
                                                s.Start_time,
                                                s.Title,
                                                s.isCompoundClip
                                            }).OrderBy(s => s.Start_time).ToList();
                        }
                        else
                        {
                            clipsData = db.DramasClips
                                            .Where(c => c.Sent_ID == project.SentProject_ID)
                                            .Select(s => new
                                            {
                                                s.DramasClip_ID,
                                                s.Url,
                                                s.End_time,
                                                s.Start_time,
                                                s.Title,
                                                s.Episode,
                                                s.isCompoundClip
                                            }).OrderBy(s => s.Start_time).ToList();
                        }

                        var responseData = new
                        {
                            SentProject = project,
                            SummaryData = summaryData,
                            MovieData = movieData,
                            WriterData = writerData,
                            ClipsData = clipsData
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, responseData);
                    }
                    catch (Exception ex)
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                    }
                }

        */

        /*[HttpGet]
        public HttpResponseMessage FetchSummary(long sentProjectId)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();

            try
            {
                var project = db.SentProject.FirstOrDefault(s => s.SentProject_ID == sentProjectId);

                if (project == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "SentProject not found");
                }

                var summaryData = db.Summary.Select(e=>new { e.Summary1,e.Sent_ID })
                                    .FirstOrDefault(s => s.Sent_ID == project.SentProject_ID);

                var movieData = db.Movie.Select(e => new { e.Type, e.Movie_ID })
                                    .FirstOrDefault(m => m.Movie_ID == project.Movie_ID);

                var writerData = db.Writer.Select(e => new { e.Writer_ID })
                                    .FirstOrDefault(w => w.Writer_ID == project.Writer_ID);

                string mediaType = movieData.Type;

                object clipsData = null;

                if (mediaType == "Movie")
                {
                    clipsData = db.Clips
                                    .Where(c => c.Sent_ID == project.SentProject_ID)
                                    .Select(s => new
                                    {
                                        s.Clips_ID,
                                        s.Url,
                                        s.End_time,
                                        s.Start_time,
                                        s.Title,
                                        s.isCompoundClip
                                    }).ToList();
                }
                else
                {
                    clipsData = db.DramasClips
                                    .Where(c => c.Sent_ID == project.SentProject_ID)
                                    .Select(s => new
                                    {
                                        s.DramasClip_ID,
                                        s.Url,
                                        s.End_time,
                                        s.Start_time,
                                        s.Title,
                                        s.Episode,
                                        s.isCompoundClip
                                    }).ToList();
                }

                var responseData = new
                {
                    SentProject = project,
                    SummaryData = summaryData,
                    MovieData = movieData,
                    WriterData = writerData,
                    ClipsData = clipsData
                };

                return Request.CreateResponse(HttpStatusCode.OK, responseData);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }*/


        [HttpGet]
        public HttpResponseMessage FetchSummary(long sentProjectId)
        {
            try
            {
                using (BlinkMovieEntities db = new BlinkMovieEntities())
                {
                    var project = db.SentProject.Where(e => e.SentProject_ID == sentProjectId)
                                        .Select(e => new { e.SentProject_ID, e.Movie_ID,e.Writer_ID })
                                        .FirstOrDefault();

                    if (project == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "SentProject not found");
                    }

                    var summaryData = db.Summary
                                        .Where(e => e.Sent_ID == project.SentProject_ID)
                                        .Select(e => new { e.Summary1, e.Sent_ID })
                                        .FirstOrDefault();

                    var movieData = db.Movie
                                      .Where(e => e.Movie_ID == project.Movie_ID)
                                      .Select(e => new { e.Type, e.Movie_ID })
                                      .FirstOrDefault();

                    var writerData = db.Writer
                                       .Where(e => e.Writer_ID == project.Writer_ID)
                                       .Select(e => new { e.Writer_ID })
                                       .FirstOrDefault();

                    if (movieData == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Movie not found");
                    }

                    if (writerData == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Writer not found");
                    }

                    string mediaType = movieData.Type;

                    object clipsData = null;

                    if (mediaType == "Movie")
                    {
                        clipsData = db.Clips
                                      .Where(c => c.Sent_ID == project.SentProject_ID)
                                      .Select(s => new
                                      {
                                          s.Clips_ID,
                                          s.Url,
                                          s.End_time,
                                          s.Start_time,
                                          s.Title,
                                          s.isCompoundClip
                                      }).ToList();
                    }
                    else
                    {
                        clipsData = db.DramasClips
                                      .Where(c => c.Sent_ID == project.SentProject_ID)
                                      .Select(s => new
                                      {
                                          s.DramasClip_ID,
                                          s.Url,
                                          s.End_time,
                                          s.Start_time,
                                          s.Title,
                                          s.Episode,
                                          s.isCompoundClip
                                      }).ToList();
                    }

                    var responseData = new
                    {
                        SentProject = project,
                        SummaryData = summaryData,
                        MovieData = movieData,
                        WriterData = writerData,
                        ClipsData = clipsData
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, responseData);
                }
            }
            catch (ArithmeticException ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Arithmetic error: " + ex.Message);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "An error occurred: " + ex.Message);
            }
        }



        [HttpGet]
        public HttpResponseMessage ShowSentProposals(int editorId)
        {
            try
            {
                BlinkMovieEntities db = new BlinkMovieEntities();

                var proposals = db.SentProposals
                                    .Where(s => s.Editor_ID == editorId && s.Status != "Received")
                                    .Select(s => new
                                    {
                                        SentProposal_ID = s.SentProposal_ID,
                                        Writer_ID = s.Writer_ID,
                                        Image = s.Image,

                                        WriterName = db.Writer.FirstOrDefault(w => w.Writer_ID == s.Writer_ID).UserName,
                                        Movie_Name = s.Movie_Name,
                                        Type = s.Type,
                                        Genre = s.Genre,
                                        DueDate = s.DueDate,
                                        Status = s.Status,
                                        Sent_at =s.Sent_at
                                    })
                                    .OrderByDescending(s => s.Sent_at)
                                    .ToList();

                

                return Request.CreateResponse(HttpStatusCode.OK, proposals);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }




        [HttpGet]
        public HttpResponseMessage HistoryAcceptedprojectByEditor(int Editor_ID)
        {
            using (BlinkMovieEntities db = new BlinkMovieEntities())
            {
                var projects = db.SentProject
                    .Where(s => s.Status == "Accepted" && s.Editor_ID == Editor_ID)
                    .OrderByDescending(s => s.Send_at)
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
                                sp.Type
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
        public HttpResponseMessage RewriteSentProject(int SentProject_ID, string editorsComment)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();
            try
            {
                var sentProject = db.SentProject.FirstOrDefault(s => s.SentProject_ID == SentProject_ID);
                if (sentProject == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Project not found");
                }


                sentProject.Status = "Rewrite";


                sentProject.EditorComment = editorsComment;

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "SentProject status updated to Rewrite");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /*huzaifa updated code*/
        private string SaveImageToDisk(HttpPostedFile imageFile)
        {
            string imagePath = "";
            string fileName = "";
            try
            {
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    imagePath = Path.Combine("F:\\BlinkBackend\\BlinkBackend\\Images\\", fileName);
                    imageFile.SaveAs(imagePath);
                }
                else
                {
                    throw new Exception("Image file is null or empty");
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error saving image to disk: {ex.Message}");
                throw new Exception("Error saving image to disk", ex);
            }

            return fileName;
        }



        /* 
         * my code 
         * private string SaveImageToDisk(HttpPostedFile imageFile)
         {
             string imagePath = "";
             string fileName = "";
             try
             {
                 if (imageFile != null && imageFile.ContentLength > 0)
                 {
                     fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                     imagePath = Path.Combine("F:\\BlinkBackend\\BlinkBackend\\Images\\", fileName);
                     imageFile.SaveAs(imagePath);
                 }
                 else
                 {
                     throw new Exception("Image file is null or empty");
                 }
             }
             catch (Exception ex)
             {

                 Console.WriteLine($"Error saving image to disk: {ex.Message}");
                 throw new Exception("Error saving image to disk", ex);
             }

             return fileName;
         }
 */
        private string SaveBase64ImageToDisk(string base64String)
        {
            string base64Data = base64String.Split(',')[1];

            byte[] imageBytes = Convert.FromBase64String(base64Data);

            // Generate a unique filename or use some logic to determine the filename
            string fileName = Guid.NewGuid().ToString() + ".jpg";

            // Specify the path where you want to save the image
            string filePath = Path.Combine("D:\\Project Files\\BlinkMoviesAndDramaCommunity\\images", fileName);

            // Save the image to disk
            File.WriteAllBytes(filePath, imageBytes);

            return fileName;
        }
        /*      [HttpGet]
              public HttpResponseMessage perpossal(string MoviName,string director,string DueDate)
              {
                  BlinkMovieEntities db = new BlinkMovieEntities();
                  var proposal = new SentProposals();
                  proposal.SentProposal_ID = GenerateId();
                  proposal.Movie_Name= MoviName;
                  proposal.Director=director;
                  proposal.DueDate=DueDate;
                  db.SentProposals.Add(proposal);
                  db.SaveChanges();

                  return Request.CreateResponse(HttpStatusCode.OK,"DataInserted");

              }
      */

        [HttpPost]
        public HttpResponseMessage DeleteProposal(int SentProposal_ID)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();
            try
            {
                var proposal = db.SentProposals.Where(p => p.SentProposal_ID == SentProposal_ID).FirstOrDefault();

                if (proposal != null)
                {
                    db.SentProposals.Remove(proposal);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Proposal deleted successfully.");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Proposal not found.");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

[HttpPost]
        public HttpResponseMessage UpdateEditorNotifications(int editorId)
        {
            try
            {
                BlinkMovieEntities db = new BlinkMovieEntities();

                
                var sentProposals = db.SentProposals.Where(sp => sp.Editor_ID == editorId).ToList();

                if (sentProposals.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No SentProposals found for the specified editor");
                }

               
                foreach (var sentProposal in sentProposals)
                {
                    sentProposal.Editor_Notification = false;
                }

                
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Editor notifications updated successfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetSentProposalsIdsWithEditorNotification(int editorId)
        {
            try
            {
                BlinkMovieEntities db = new BlinkMovieEntities();


                var sentProposalIds = db.SentProposals
                                        .Where(sp => sp.Editor_ID == editorId && sp.Editor_Notification == true && sp.Status != "Received")
                                        .Select(sp => new
                                        {
                                            sp.SentProposal_ID,
                                            sp.Status
                                        })
                                        .ToList();

                if (sentProposalIds.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No SentProposals found with Editor_Notification true for the specified editor");
                }


                int totalCount = sentProposalIds.Count;


                var responseData = new
                {
                    SentProposalIds = sentProposalIds,
                    TotalCount = totalCount
                };

                return Request.CreateResponse(HttpStatusCode.OK, responseData);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage UpdateAllEditorNotificationsToFalse(int editorId)
        {
            try
            {
                BlinkMovieEntities db = new BlinkMovieEntities();


                var sentProposals = db.SentProposals.Where(sp => sp.Editor_ID == editorId).ToList();

                if (sentProposals.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No SentProposals found for the specified editor");
                }


                foreach (var sentProposal in sentProposals)
                {
                    sentProposal.Editor_Notification = false;
                }


                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "All Editor notifications updated successfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetEditorNotificationsSentProject(int editorId)
        {
            try
            {
                BlinkMovieEntities db = new BlinkMovieEntities();


                var editorNotifications = db.SentProject
                                            .Where(sp => sp.Editor_Notification == true && sp.Editor_ID == editorId)
                                            .Select(sp => new
                                            {
                                                SentProject_ID = sp.SentProject_ID,
                                                Status = sp.Status
                                            })
                                            .ToList();

                if (editorNotifications.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No editor notifications found for the specified editor");
                }


                int totalCount = editorNotifications.Count;


                var responseData = new
                {
                    EditorNotifications = editorNotifications,
                    TotalCount = totalCount
                };

                return Request.CreateResponse(HttpStatusCode.OK, responseData);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage UpdateAllEditorNotificationstoFalseSentProject(int editorId)
        {
            try
            {
                BlinkMovieEntities db = new BlinkMovieEntities();


                var sentProjects = db.SentProject.Where(sp => sp.Editor_ID == editorId).ToList();

                if (sentProjects.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No SentProject records found for the specified editor");
                }


                foreach (var sentProject in sentProjects)
                {
                    sentProject.Editor_Notification = false;
                }


                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "All Editor notifications updated to false for the specified editor");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

    }





   /* [HttpGet]
    public HttpResponseMessage perpossal(string MoviName, string director, string DueDate, string status, int WriterId)
    {
        try
        {
            BlinkMovieEntities db = new BlinkMovieEntities();

            // Assuming GenerateId() generates a unique ID for the proposal
            var proposal = new SentProposals
            {
                SentProposal_ID = GenerateId(),
                Movie_Name = MoviName,
                Director = director,
                DueDate = DueDate,
                Status = status,
                Writer_ID = WriterId,


            };

            db.SentProposals.Add(proposal);
            db.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK, "DataInserted");
        }
        catch (Exception ex)
        {
            // Log the exception or handle it appropriately
            return Request.CreateResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
        }
    }*/



}




