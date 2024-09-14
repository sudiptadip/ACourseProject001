﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.DataAccess.Repository.IRepository
{
    public interface IUniteOfWork
    {
        IFacultyRepository Faculty { get; }
        ICategoryRepository Category { get; }
        void Save();
    }
}
