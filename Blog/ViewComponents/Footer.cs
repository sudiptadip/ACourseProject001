using Blog.DataAccess.Data;
using Blog.DataAccess.Repository;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Models;
using Blog.Models.VM;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Security.Claims;

namespace Blog.ViewComponents
{
    [ViewComponent(Name = "Footer")]
    public class Footer : ViewComponent
    {
        private readonly IUniteOfWork _uniteOfWork;
        public Footer(IUniteOfWork uniteOfWork)
        {
            _uniteOfWork = uniteOfWork;


        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var sosalMediaLinks = await _uniteOfWork.SosalMedia.GetAllAsync();


                var sosalMedia = new SosalMedia();

            if(sosalMediaLinks == null)
            {
                return View("Index", sosalMedia);
            }
            
           return View("Index", sosalMediaLinks.FirstOrDefault());
        
        }

    }
}
