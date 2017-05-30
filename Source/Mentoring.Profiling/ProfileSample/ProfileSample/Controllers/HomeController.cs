using System.IO;
using System.Linq;
using System.Web.Mvc;
using ProfileSample.DAL;
using ProfileSample.Models;
using System.Drawing;

namespace ProfileSample.Controllers
{
    public class HomeController : Controller
    {
        [OutputCache(Duration = 120)]
        public ActionResult Index()
        {
            var context = new ProfileSampleEntities();

            var model = context.ImgSources.Take(20).Select(item => new ImageModel
            {
                Id = item.Id,
                Name = item.Name
            }).ToList();

            return View(model);
        }

        [OutputCache(Duration = 120)]
        public ActionResult RenderImage(int id)
        {
            var context = new ProfileSampleEntities();

            var item = context.ImgSources.Find(id);
            byte[] photoBack;

            var ms = new MemoryStream(item.Data);
            var image = Image.FromStream(ms);
            using (var resizedImg = new Bitmap(image, new Size(300, 150)))
            {
                using (var resizedImageMs = new MemoryStream())
                {
                    resizedImg.Save(resizedImageMs, System.Drawing.Imaging.ImageFormat.Jpeg);
                    photoBack = resizedImageMs.ToArray();
                }
            }

            return File(photoBack, "image/jpeg");
        }

        public ActionResult Convert()
        {
            var files = Directory.GetFiles(Server.MapPath("~/Content/Img"), "*.jpg");

            using (var context = new ProfileSampleEntities())
            {
                foreach (var file in files)
                {
                    using (var stream = new FileStream(file, FileMode.Open))
                    {
                        byte[] buff = new byte[stream.Length];

                        stream.Read(buff, 0, (int) stream.Length);

                        var entity = new ImgSource()
                        {
                            Name = Path.GetFileName(file),
                            Data = buff,
                        };

                        context.ImgSources.Add(entity);
                        context.SaveChanges();
                    }
                } 
            }

            return RedirectToAction("Index");
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}