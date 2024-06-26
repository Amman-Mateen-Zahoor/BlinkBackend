﻿/*using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.UI.WebControls;
using BlinkBackend.Models;
using Newtonsoft.Json;

namespace BlinkBackend.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UserController : ApiController

    {
        static int GenerateUserId()
        {

            long timestamp = DateTime.Now.Ticks;
            Random random = new Random();
            int randomComponent = random.Next();

            int userId = (int)(timestamp ^ randomComponent);

            return Math.Abs(userId);
        }








        [HttpPost]
        public async Task<HttpResponseMessage> SignUp()
        {
            BlinkMovieEntities db = new BlinkMovieEntities();

            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            try
            {
                var httpRequest = HttpContext.Current.Request;

                string role = httpRequest["Role"];
                string email = httpRequest["Email"];
                string username = httpRequest["UserName"];
                string password = httpRequest["Password"];
               

                // Create a user based on the role
                if (role == "Reader")
                {
                    var newUser = new Reader()
                    {
                        Reader_ID = GenerateUserId(),
                        Email = email,
                        UserName = username,
                        Password = password,
                    };
                    db.Reader.Add(newUser);

                    
                    var imageFile = httpRequest.Files["Image"];
                    if (imageFile != null)
                    {
                        string imagePath = SaveImageToDisk(imageFile);
                        newUser.Image = imagePath;
                    }

                    await Task.Run(() => db.SaveChanges());

                    var cUser = new Users()
                    {
                        User_ID = GenerateUserId(),
                        Editor_ID = null,
                        Writer_ID =null,
                        Reader_ID = newUser.Reader_ID,
                        Email = email,
                        Password = password,
                        Role = role,
                    };
                    db.Users.Add(cUser);
                    await Task.Run(() => db.SaveChanges());

                    string newUserJson = JsonConvert.SerializeObject(newUser, jsonSettings);
                    string cUserJson = JsonConvert.SerializeObject(cUser, jsonSettings);

                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                    response.Content = new StringContent(newUserJson, Encoding.UTF8, "application/json");

                    return response;
                }

                else if (role == "Writer")
                {
                    var newUser = new Writer()
                    {
                        Writer_ID = GenerateUserId(),
                        Email = email,
                        UserName = username,
                        Password = password,
                    };
                    db.Writer.Add(newUser);


                    var imageFile = httpRequest.Files["Image"];
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        string imagePath = SaveImageToDisk(imageFile);
                        newUser.Image = imagePath;
                    }

                    await Task.Run(() => db.SaveChanges());

                    var cUser = new Users()
                    {
                        User_ID = GenerateUserId(),
                        Editor_ID = null,
                        Writer_ID = newUser.Writer_ID,
                        Reader_ID = null,
                        Email = email,
                        Password = password,
                        Role = role,
                    };
                    db.Users.Add(cUser);
                    await Task.Run(() => db.SaveChanges());

                    string newUserJson = JsonConvert.SerializeObject(newUser, jsonSettings);
                    string cUserJson = JsonConvert.SerializeObject(cUser, jsonSettings);

                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                    response.Content = new StringContent(newUserJson, Encoding.UTF8, "application/json");

                    return response;
                }
                else
                {

                    var newUser = new Editor()
                    {
                        Editor_ID = GenerateUserId(),
                        Email = email,
                        UserName = username,
                        Password = password,
                    };
                    db.Editor.Add(newUser);


                    await Task.Run(() => db.SaveChanges());

                    var cUser = new Users()
                    {
                        User_ID = GenerateUserId(),
                        Editor_ID = newUser.Editor_ID,
                        Writer_ID = null,
                        Reader_ID = null,
                        Email = email,
                        Password = password,
                        Role = role,
                    };
                    db.Users.Add(cUser);
                    await Task.Run(() => db.SaveChanges());
                }

                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid role specified");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpGet]
        public HttpResponseMessage Login(string email , string password)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();
            try
            {
                var user = db.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
                if (user == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Invalid email or password");
                }

                object additionalInfo = null;

                switch (user.Role.ToLower())
                {
                    case "reader":
                        additionalInfo = db.Reader.Where(r => r.Reader_ID == user.Reader_ID).Select(r => new
                        {
                            r.Reader_ID,
                            r.Email,
                            r.UserName,
                            r.Password,
                            r.Image,
                            r.Interest,
                            r.Balance,
                       
                        });
                        break;

                    case "editor":
                        additionalInfo = db.Editor.Where(e => e.Editor_ID == user.Editor_ID).Select(e => new
                        {
                            e.Editor_ID,
                            e.Email,
                            e.UserName,
                            e.Password,
                          
                            e.Interest,
                      
                            // Add other properties you want to include
                        });
                        break;

                    case "writer":
                        additionalInfo = db.Writer
                    .Where(w => w.Writer_ID == user.Writer_ID)
                    .Select(w => new
                    {
                        w.Writer_ID,
                        w.Email,
                        w.UserName,
                        w.Password,
                        w.Image,
                        w.Interest,
                        w.Balance,
                        w.Rating
                    })
                    .FirstOrDefault();
                        break;

                    default:
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user role");
                }




                var userInfo = new { Role = user.Role, UserData = additionalInfo };

                return Request.CreateResponse(HttpStatusCode.OK, userInfo);
               
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private object FilterUserData(string role, object additionalInfo)
        {
            
            switch (role)
            {
                case "reader":
                    var reader = additionalInfo as Reader;
                    return new
                    {
                        UserName = reader?.UserName,
                        Email = reader?.Email,
                        Password = reader?.Password,
                        Subscription = reader?.Subscription,
                        Balance = reader?.Balance
                    };

                case "editor":
                    var editor = additionalInfo as Editor;
                    return new
                    {
                        UserName = editor?.UserName,
                        Email = editor?.Email,
                        Password = editor?.Password,
                        
                    };

                case "writer":
                    var writer = additionalInfo as Writer;
                    return new
                    {
                        UserName = writer?.UserName,
                        Email = writer?.Email,
                        Password = writer?.Password,
                        Balance = writer?.Balance
                    };

                default:
                    return null; 
            }
        }


        private string SaveImageToDisk(HttpPostedFile imageFile)
        {
            string imagePath = ""; 
            try
            {
                if (imageFile != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    imagePath = Path.Combine("D:\\Images", fileName); 
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

            return imagePath;
        }
    }
}
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.UI.WebControls;
using BlinkBackend.Models;
using Newtonsoft.Json;

namespace BlinkBackend.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UserController : ApiController

    {
        static int GenerateUserId()
        {

            long timestamp = DateTime.Now.Ticks;
            Random random = new Random();
            int randomComponent = random.Next();

            int userId = (int)(timestamp ^ randomComponent);

            return Math.Abs(userId);
        }








        [HttpPost]
        public HttpResponseMessage SignUp()
        {
            BlinkMovieEntities db = new BlinkMovieEntities();

            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            try
            {
                var request = HttpContext.Current.Request;


                string email = request["Email"];

                var imageFile = request.Files["Image"];
                string username = request["UserName"];

                string role = request["Role"];
                string password = request["Password"];
                string interest = request["Interest"];




                // Create a user based on the role
                if (role == "Reader")
                {
                    var newUser = new Reader()
                    {
                        Reader_ID = GenerateUserId(),
                        Email = email,
                        UserName = username,
                        Password = password,
                        Interest = interest,
                    };
                    db.Reader.Add(newUser);



                    if (imageFile != null)
                    {
                        /*string imagePath = SaveBase64ImageToDisk(imageFile);*/
                        string imagePath = SaveImageToDisk(imageFile);
                        newUser.Image = imagePath;
                    }


                    db.SaveChanges();

                    var cUser = new Users()
                    {
                        User_ID = GenerateUserId(),
                        Editor_ID = null,
                        Writer_ID = null,
                        Reader_ID = newUser.Reader_ID,
                        Email = email,
                        Password = password,
                        Role = role,
                    };
                    db.Users.Add(cUser);
                    db.SaveChanges();

                    string newUserJson = JsonConvert.SerializeObject(newUser, jsonSettings);
                    string cUserJson = JsonConvert.SerializeObject(cUser, jsonSettings);

                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                    response.Content = new StringContent(newUserJson, Encoding.UTF8, "application/json");

                    var userInfo = new { Role = cUser.Role, UserData = newUser };

                    return Request.CreateResponse(HttpStatusCode.OK, userInfo);
                }

                else if (role == "Writer")
                {
                    var newUser = new Writer()
                    {
                        Writer_ID = GenerateUserId(),
                        Email = email,
                        UserName = username,
                        Password = password,
                        Interest = interest,
                    };
                    db.Writer.Add(newUser);



                    if (imageFile != null)
                    {
                        string imagePath = SaveImageToDisk(imageFile);
                        /* string imagePath = SaveBase64ImageToDisk(imageFile);*/
                        newUser.Image = imagePath;
                    }

                    db.SaveChanges();

                    var cUser = new Users()
                    {
                        User_ID = GenerateUserId(),
                        Editor_ID = null,
                        Writer_ID = newUser.Writer_ID,
                        Reader_ID = null,
                        Email = email,
                        Password = password,
                        Role = role,
                    };
                    db.Users.Add(cUser);
                    db.SaveChanges();

                    string newUserJson = JsonConvert.SerializeObject(newUser, jsonSettings);
                    string cUserJson = JsonConvert.SerializeObject(cUser, jsonSettings);

                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                    response.Content = new StringContent(newUserJson, Encoding.UTF8, "application/json");

                    var data = new
                    {
                        newUser.Writer_ID,
                        newUser.Email,
                        newUser.Interest,
                        newUser.Balance,
                        newUser.Image,
                        newUser.UserName,
                        newUser.Rating
                    };

                    var userInfo = new { Role = cUser.Role, UserData = data };

                    return Request.CreateResponse(HttpStatusCode.OK, userInfo);
                }
                else if (role == "Editor")
                {

                    var newUser = new Editor()
                    {
                        Editor_ID = GenerateUserId(),
                        Email = email,
                        UserName = username,
                        Password = password,
                        Interest = interest,
                    };
                    db.Editor.Add(newUser);

                    var data = new
                    {
                        newUser.Editor_ID,
                        newUser.Email,
                        newUser.Interest,


                        newUser.UserName,

                    };
                    db.SaveChanges();

                    var cUser = new Users()
                    {
                        User_ID = GenerateUserId(),
                        Editor_ID = newUser.Editor_ID,
                        Writer_ID = null,
                        Reader_ID = null,
                        Email = email,
                        Password = password,
                        Role = role,
                    };
                    db.Users.Add(cUser);
                    db.SaveChanges();

                    var userInfo = new { Role = cUser.Role, UserData = data };

                    return Request.CreateResponse(HttpStatusCode.OK, userInfo);
                }

                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid role specified");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpGet]
        public HttpResponseMessage Login(string email, string password)
        {
            BlinkMovieEntities db = new BlinkMovieEntities();
            try
            {
                var user = db.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
                if (user == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Invalid email or password");
                }

                object additionalInfo = null;

                switch (user.Role.ToLower())
                {
                    case "reader":
                        additionalInfo = db.Reader.Where(r => r.Reader_ID == user.Reader_ID).Select(r => new
                        {
                            r.Reader_ID,
                            r.Email,
                            r.UserName,
                            r.Password,
                            r.Image,
                            r.Interest,
                            r.Balance,

                        });
                        break;

                    case "editor":
                        additionalInfo = db.Editor.Where(e => e.Editor_ID == user.Editor_ID).Select(e => new
                        {
                            e.Editor_ID,
                            e.Email,
                            e.UserName,
                            e.Password,

                            e.Interest,

                            // Add other properties you want to include
                        });
                        break;

                    case "admin":
                        additionalInfo = db.Editor.Where(e => e.Editor_ID == user.Editor_ID).Select(e => new
                        {
                            e.Editor_ID,
                            e.Email,
                            e.UserName,
                            e.Password,

                            e.Interest,

                            // Add other properties you want to include
                        });
                        break;

                    case "writer":
                        additionalInfo = db.Writer
                    .Where(w => w.Writer_ID == user.Writer_ID)
                    .Select(w => new
                    {
                        w.Writer_ID,
                        w.Email,
                        w.UserName,
                        w.Password,
                        w.Image,
                        w.Interest,
                        w.Balance,
                        w.Rating
                    })
                    .FirstOrDefault();
                        break;

                    default:
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user role");
                }




                var userInfo = new { Role = user.Role, UserData = additionalInfo };

                return Request.CreateResponse(HttpStatusCode.OK, userInfo);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }





        /*  [HttpPost]
          public HttpResponseMessage Login(Log loginModel)
          {
              BlinkMovieEntities db = new BlinkMovieEntities();
              try
              {
                  var user = db.Users.FirstOrDefault(u => u.Email == loginModel.Email && u.Password == loginModel.Password);
                  if (user == null)
                  {
                      return Request.CreateResponse(HttpStatusCode.NotFound, "Invalid email or password");
                  }

                  object additionalInfo = null;

                  switch (user.Role.ToLower())
                  {
                      case "reader":
                          additionalInfo = db.Reader.Where(r => r.Reader_ID == user.Reader_ID).Select(r => new
                          {
                              r.Reader_ID,
                              r.Email,
                              r.UserName,
                              r.Password,
                              r.Image,
                              r.Interest,
                              r.Balance,

                          }).FirstOrDefault();
                          break;

                      case "editor":
                          additionalInfo = db.Editor.Where(e => e.Editor_ID == user.Editor_ID).Select(e => new
                          {
                              e.Editor_ID,
                              e.Email,
                              e.UserName,
                              e.Password,

                              e.Interest,

                              // Add other properties you want to include
                          }).FirstOrDefault();
                          break;

                      case "writer":
                          additionalInfo = db.Writer
                      .Where(w => w.Writer_ID == user.Writer_ID)
                      .Select(w => new
                      {
                          w.Writer_ID,
                          w.Email,
                          w.UserName,
                          w.Password,
                          w.Image,
                          w.Interest,
                          w.Balance,
                          w.Rating
                      })
                      .FirstOrDefault();
                          break;

                      default:
                          return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user role");
                  }




                  var userInfo = new { Role = user.Role, UserData = additionalInfo };

                  return Request.CreateResponse(HttpStatusCode.OK, userInfo);

              }
              catch (Exception ex)
              {
                  return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
              }
          }
  */




        /*private string SaveImageToDisk(HttpPostedFile imageFile)
        {
            string imagePath = "";
            string fileName = "";
            try
            {
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    imagePath = Path.Combine("D:\\Project Files\\BlinkMoviesAndDramaCommunity\\images", fileName);
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
        private string SaveImageToDisk(HttpPostedFile imageFile)
        {
            try
            {
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string imagePath = Path.Combine("F:\\BlinkBackend\\BlinkBackend\\Images\\", fileName);

                    imageFile.SaveAs(imagePath);

                    return fileName;
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
        }





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
    }
}


