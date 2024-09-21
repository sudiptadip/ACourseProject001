﻿using Blog.DataAccess.Data;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.DataAccess.Repository
{
    public class UniteOfWork : IUniteOfWork
    {
        public ICategoryRepository Category {  get; private set; }

        public IFacultyRepository Faculty { get; private set; }

        public IProductRepository Product { get; private set; }

        public ISubjectRepository Subject { get; private set; }

        public IProductPriceRepository ProductPrice { get; private set; }

        public ICartItemRepository CartItem { get; private set; }

        public ICartRepository Cart { get; private set; }

        public IBlogRepository Blog { get; private set; }

        private readonly ApplicationDbContext _db;
        public UniteOfWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new CategoryRepository(_db);
            Faculty = new FacultyRepository(_db);
            Product = new ProductRepository(_db);
            Subject = new SubjectRepository(_db);
            ProductPrice = new ProductPriceRepository(_db);
            Cart = new CartRepository(_db);
            CartItem = new CartItemRepository(_db);
            Blog = new BlogRepository(_db);
        }

        public void Save()
        {
           _db.SaveChanges();
        }
    }
}
