namespace TaskTronic.Services
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;

    public abstract class DataService<TEntity> : IDataService<TEntity>
        where TEntity : class
    {
        protected DataService(DbContext dbContext) 
            => this.Data = dbContext;

        protected DbContext Data { get; }

        protected IQueryable<TEntity> All() => this.Data.Set<TEntity>();

        public async Task Save(TEntity entity)
        {
            //this.Data.Update(entity);
            this.Data.Attach(entity);
            this.Data.Entry(entity).State = EntityState.Modified;

            await this.Data.SaveChangesAsync();
        }
    }
}
