using JSON_DAL;
using JsonDemo.Models;
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
        const string PhotosFolder = @"/App_Assets/Photos/";
        const string DefaultPhoto = @"No_Image.png";

        public int Id { get; set; }
        public int OwnerId { get; set; }            // Id du propriétaire de la photo

        [Display(Name = "Titre"), Required(ErrorMessage = "Obligatoire")]
        public string Title { get; set; }           // Titre de la photo

        [Display(Name = "Description"), Required(ErrorMessage = "Obligatoire")]
        public string Description { get; set; }     // Description de la photo
        public DateTime CreationDate { get; set; }  // Date de création
        public bool Shared { get; set; }            // Indicateur de partage ("true" ou "false")
                                                    // 
                                                    // compte des likes
        [ImageAsset(PhotosFolder, DefaultPhoto)]
        public string Image { get; set; } = DefaultPhoto;        // Url relatif de l'image

        [JsonIgnore]
        public List<Like> Likes
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
                foreach(var like in Likes)
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

                return Likes.Any(like => like.UserId == connectedUser.Id);
            }
        }

        public Photo()
        {
            Id = 0;
            CreationDate = DateTime.Now;
            Shared = false;
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