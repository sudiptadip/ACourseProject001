using Blog.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Utility.Service.IService
{
    public interface IEmailService
    {
        public void SendEmail(EmailDto request);
    }
}