using Bulcky.DataAccess.Data;
using Bulcky.DataAccess.Repository.IRepository;

namespace Bulcky.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _dp;
        public ICategoryRepository Category { get; }
        public IProductRepository Product { get; }
        public ICompanyRepository Company { get; }
        public IShoppingCartRepository ShoppingCart { get; }
        public IOrderDetailRepository OrderDetail { get; set; }
        public IOrderHeaderRepository OrderHeader { get; set; }
        public IApplicationUserRepository ApplicationUser { get; }

        public UnitOfWork(ApplicationDbContext dp)
        {
            _dp = dp;
            Category = new CategoryRepository(_dp);
            Product = new ProductRepository(_dp);
            Company = new CompanyRepository(_dp);
            ShoppingCart = new ShoppingCartRepository(_dp);
            ApplicationUser = new ApplicationUserRepository(_dp);
            OrderDetail = new OrderDetailRepository(_dp);
            OrderHeader = new OrderHeaderRepository(_dp);
        }
        

        public void Save()
        {
            _dp.SaveChanges();
        }
    }
}
