using Intermediate_DotNet_WebAPI.Models;

namespace Intermediate_DotNet_WebAPI.Data
{
    public interface IUserRepository
    {
        public bool SavaChanges();
        public void AddEntity<T>(T entityToAdd);
        public void RemoveEntity<T>(T entityToAdd);
        public IEnumerable<UsersModels> GetUsers();
        public UsersModels GetSingleUsers(int UserId);
        public UsersSalaryModels GetSingleUsersSalary(int UserId);
        public UsersJobInfoModels GetSingleUsersJobInfo(int UserId);



    }
}
