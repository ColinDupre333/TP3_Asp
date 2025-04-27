using JsonDemo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Tokenizer.Symbols;
using static JsonDemo.Controllers.AccessControl;

namespace JsonDemo.Controllers
{
    [UserAccess]
    public class PhotosController : Controller
    {
        const string IllegalAccessUrl = "/Accounts/Login?message=Tentative d'accès illégale!";

        public ActionResult SetPhotoOwnerSearchId(int id)
        {
            Session["photoOwnerSearchId"] = id;
            return RedirectToAction("List");
        }
        public ActionResult SetSearchKeywords(string keywords)
        {
            Session["searchKeywords"] = keywords;
            return RedirectToAction("List");
        }
        public ActionResult List(string sortType)
        {
            if (Session["photoOwnerSearchId"] == null) Session["photoOwnerSearchId"] = 0;
            if (Session["searchKeywords"] == null) Session["searchKeywords"] = "";
            if (Session["PhotosSortType"] == null) Session["PhotosSortType"] = "date";
            if (!string.IsNullOrEmpty(sortType)) Session["PhotosSortType"] = sortType;
            List<Photo> list;
            switch ((string)Session["PhotosSortType"])
            {
                case "likes":
                    list = DB.Photos.ToList().Where(p => p != null).OrderByDescending(p => p.Likes).ThenByDescending(p => p.CreationDate).ToList();
                    break;
                case "owner":
                    list = DB.Photos.ToList().Where(p => p != null &&  p.OwnerId == ((User)Session["ConnectedUser"]).Id).OrderByDescending(p => p.CreationDate).ToList();
                    break;
                case "user":
                    if ((int)Session["photoOwnerSearchId"] != 0)
                        list = DB.Photos.ToList().Where(p => p != null && p.OwnerId == (int)Session["photoOwnerSearchId"]).OrderByDescending(p => p.CreationDate).ToList();
                    else
                        list = DB.Photos.ToList().Where(p => p != null).OrderBy(p => p.Owner.Name).ThenByDescending(p => p.CreationDate).ToList();
                    break;
                case "keywords":
                    if (!string.IsNullOrEmpty((string)Session["searchKeywords"]))
                    {
                        list = new List<Photo>();
                        string[] keywords = ((string)Session["searchKeywords"]).Split(' ');
                        foreach (var photo in DB.Photos.ToList())
                        {
                            if (photo == null) continue;
                            bool keep = true;
                            foreach (string keyword in keywords)
                            {
                                string kw = keyword.Trim().ToLower();
                                if (!photo.Title.ToLower().Contains(kw) && !photo.Description.ToLower().Contains(kw))
                                {
                                    keep = false;
                                    break;
                                }
                            }
                            if (keep)
                                list.Add(photo);
                        }
                        list = list.OrderByDescending(p => p.CreationDate).ToList();
                    }
                    else
                        list = DB.Photos.ToList().Where(p => p != null).OrderByDescending(p => p.CreationDate).ToList();
                    break;
                default:
                    list = DB.Photos.ToList().Where(p => p != null).OrderByDescending(p => p.CreationDate).ToList();
                    break;
            }
            return View(list);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View(new Photo());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Photo photo, HttpPostedFileBase ImageFile)
        {
            User connectedUser = (User)Session["ConnectedUser"];
            photo.OwnerId = connectedUser.Id;

            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                try
                {
                    // Générer un nom de fichier unique
                    string fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);

                    // Chemin complet pour enregistrer l'image
                    string path = Path.Combine(Server.MapPath("~/App_Assets/Photos"), fileName);

                    // Enregistrer le fichier sur le serveur
                    ImageFile.SaveAs(path);

                    // Mettre à jour le chemin de l'image dans le modèle
                    photo.Image = "/App_Assets/Photos/" + fileName;
                }
                catch (Exception ex)
                {
                    // Gérer les erreurs d'upload
                    ModelState.AddModelError("", "Une erreur s'est produite lors de l'upload de l'image : " + ex.Message);
                    return View(photo);
                }
            }
            else
            {
                // Si aucune image n'est fournie, utiliser une image par défaut
                photo.Image = "/App_Assets/Photos/No_Image.png";
            }

            // Ajouter la photo à la base de données
            DB.Photos.Add(photo);

            return RedirectToAction("List");
        }

        public ActionResult Edit(int id)
        {
            Photo photo = DB.Photos.Get(id);
            User connectedUser = (User)Session["ConnectedUser"];
            if (photo != null)
            {
                if (connectedUser.IsAdmin || photo.OwnerId == connectedUser.Id)
                {
                    return View(photo);
                }
                return RedirectToAction("List");
            }
            return Redirect(IllegalAccessUrl);
        }
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Edit(Photo photo)
        {
            User connectedUser = ((User)Session["ConnectedUser"]);
            if (connectedUser.IsAdmin || photo.OwnerId == connectedUser.Id)
            {
                Photo existingPhoto = DB.Photos.Get(photo.Id);
                if (existingPhoto != null)
                {
                   photo.OwnerId = existingPhoto.OwnerId;

                   DB.Photos.Update(photo);
                }
                return RedirectToAction("List");
            }
            return Redirect(IllegalAccessUrl);
        }
        public ActionResult Details(int id)
        {
            Photo photo = DB.Photos.Get(id);
            if (photo != null)
            {
                User connectedUser = ((User)Session["ConnectedUser"]);
                if (connectedUser.IsAdmin || photo.Shared)
                    return View(photo);
                else
                    return Redirect(IllegalAccessUrl);
            }
            return RedirectToAction("List");
        }
        public ActionResult Delete(int id)
        {
            Photo photo = DB.Photos.Get(id);
            if (photo != null)
            {
                User connectedUser = (User)Session["ConnectedUser"];
                if (connectedUser.IsAdmin || photo.OwnerId == connectedUser.Id)
                {
                    DB.Photos.Delete(id);
                    return RedirectToAction("List");
                }
                else
                    return Redirect(IllegalAccessUrl);
            }
            return Redirect(IllegalAccessUrl);
        }
        public ActionResult TogglePhotoLike(int id)
        {
            User connectedUser = (User)Session["ConnectedUser"];
            DB.Likes.ToogleLike(id, connectedUser.Id);
            return RedirectToAction("Details/" + id);
        }
    }
}