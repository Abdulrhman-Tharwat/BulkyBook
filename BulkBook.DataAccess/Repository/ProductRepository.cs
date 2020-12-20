using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            var Obj = _db.Products.FirstOrDefault(m => m.Id == product.Id);
            if (Obj != null)
            {
                if (product.ImageUrl != null)
                {
                    Obj.ImageUrl = product.ImageUrl;
                }
                Obj.Title = product.Title;
                Obj.ListPrice = product.ListPrice;
                Obj.ISBN = product.ISBN;
                Obj.Price = product.Price;
                Obj.Author = product.Author;
                Obj.Price100 = product.Price100;
                Obj.Price50 = product.Price50;
                Obj.Description = product.Description;
                Obj.CategoryID = product.CategoryID;
                Obj.CovertypeId = product.CovertypeId;


            }
        }
    }
}
