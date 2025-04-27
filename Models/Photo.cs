using JSON_DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace JsonDemo.Models
{
    public class Photo
    {
        const string Photos_Folder = @"/App_Assets/Photos/";
        const string Default_Photo = @"No_Image.png";

        [JsonIgnore]
        public static string DefaultImage { get { return Photos_Folder + Default_Photo; } }

        public int Id { get; set; }
        public int OwnerId { get; set; }            // Id du propriétaire de la photo

        [Display(Name = "Titre"), Required(ErrorMessage = "Obligatoire")]
        public string Title { get; set; }           // Titre de la photo

        [Display(Name = "Description"), Required(ErrorMessage = "Obligatoire")]
        public string Description { get; set; }     // Description de la photo
        public DateTime CreationDate { get; set; }  // Date de création
        public bool Shared { get; set; }  // Indicateur de partage ("true" ou "false")

        public int Likes
        {
            get
            {
                return LikesList.Count;
            }
        }

        // 
        // compte des likes
        [ImageAsset(PhotosFolder, DefaultPhoto)]
        public string Image { get; set; } = DefaultImage;       // Url relatif de l'image

        [JsonIgnore]
        public List<Like> LikesList
        {
            get
            {   
                return DB.Likes.ToList().Where(l => l.PhotoId == Id).ToList();     
            }
            
        }
        [JsonIgnore]
        public string UsersLikeList
        {
            get
            {
                string UsersLikeList = "";
                foreach(var like in LikesList)
                {
                    UsersLikeList += DB.Users.Get(like.UserId).Name + "\n";
                }
                return UsersLikeList;
            }
        }
        [JsonIgnore]
        public bool LikedByUser
        {
            get
            {
                var connectedUser = (User)HttpContext.Current.Session["ConnectedUser"];

                return LikesList.Any(like => like.UserId == connectedUser.Id);
            }
        }

        public Photo()
        {
            Id = 0;
            CreationDate = DateTime.Now;
            Shared = false;
            Image = DefaultImage;
        }
        [JsonIgnore]
        public User Owner
        {
            get
            {
                return DB.Users.Get(OwnerId);
            }
        }
    }
}