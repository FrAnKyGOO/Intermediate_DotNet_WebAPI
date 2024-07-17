namespace Intermediate_DotNet_WebAPI.Data
{
    public interface IUserRepository
    {
        public bool SavaChanges();
        public void AddEntity<T>(T entityToAdd);
        public void RemoveEntity<T>(T entityToAdd);

    }
}
