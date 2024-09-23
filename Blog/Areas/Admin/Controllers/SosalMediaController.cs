using Blog.DataAccess.Data;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Models;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SosalMediaController : Controller
    {
        private readonly IUniteOfWork _uniteOfWork;
        public SosalMediaController(IUniteOfWork uniteOfWork)
        {
            _uniteOfWork = uniteOfWork;
        }

        public async Task<IActionResult> Index()
        {

            var preLink = await _uniteOfWork.SosalMedia.GetAllAsync();

            if(preLink != null && preLink.Any())
            {
                 return View(preLink.FirstOrDefault());
            }
            else
            {
                
                return View(new SosalMedia());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(SosalMedia sosalMedia)
        {

            if(ModelState.IsValid)
            {
                var links = await _uniteOfWork.SosalMedia.GetAllAsync();

                if (links != null && links.Any())
                {
                    await _uniteOfWork.SosalMedia.DeleteRangeAsync(links);
                }

                await _uniteOfWork.SosalMedia.AddAsync(sosalMedia);

                _uniteOfWork.Save();

                TempData["success"] = "Successfully Updated";

               // var preLink2 = await _uniteOfWork.SosalMedia.GetAllAsync();

                return View(sosalMedia);
            }
            else
            {
                return View(sosalMedia);
            }

          
        }

    }
}
