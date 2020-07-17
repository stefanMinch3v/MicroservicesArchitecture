namespace TaskTronic.Messages.ScopedServices
{
    public interface IScopedProcessingService
    {
        TDbContext GetDbContext<TDbContext>();
    }

    public class ScopedProcessingService : IScopedProcessingService
    {
        public TDbContext GetDbContext<TDbContext>()
        {
            throw new System.NotImplementedException();
        }
    }
}
