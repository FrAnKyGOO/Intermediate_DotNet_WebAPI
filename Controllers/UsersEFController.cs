using AutoMapper;
using BasicDotNet_WebAPI.Data;
using BasicDotNet_WebAPI.Dtos;
using BasicDotNet_WebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace BasicDotNet_WebAPI.Controllers
{
    public class UsersEFController : Controller
    {
        IMapper _mapper;
        DataContextEF _entityFramework;

        public UsersEFController(IConfiguration config)
        {
            _entityFramework = new DataContextEF(config);

            _mapper = new Mapper(new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UsersDto, UsersDto>();
                cfg.CreateMap<UsersSalaryModels, UsersSalaryModels>().ReverseMap();
                cfg.CreateMap<UsersJobInfoModels, UsersJobInfoModels>().ReverseMap();
            }));
        }

        #region Users

        [HttpGet("GetUsers")]
        public IEnumerable<UsersModels> GetUsers()
        {
            IEnumerable<UsersModels> Users = _entityFramework.Users.ToList<UsersModels>();
            return Users;
        }

        [HttpGet("GetSingleUsers/{UserId}")]
        public UsersModels GetSingleUsers(int UserId)
        {

            UsersModels? Users = _entityFramework.Users
                .Where(u => u.UserId == UserId)
                .FirstOrDefault<UsersModels>();

            if (Users == null)
            {
                return Users;
            }

            throw new Exception("Failed to Get User");

        }

        [HttpPut("EditDataUser")]
        public IActionResult EditDataUser(UsersModels user)
        {
            UsersModels? UserDb = _entityFramework.Users
               .Where(u => u.UserId == user.UserId)
               .FirstOrDefault<UsersModels>();

            if (UserDb != null)
            {

                UserDb.Active = user.Active;
                UserDb.FirstName = user.FirstName;
                UserDb.LastName = user.LastName;
                UserDb.Gender = user.Gender;
                UserDb.Email = user.Email;
                if (_entityFramework.SaveChanges() > 0)
                {
                    return Ok();
                }

                throw new Exception("Failed to Edit User");

            }
            throw new Exception("Failed to Edit User");

        }

        [HttpPost("AddUsers")]
        public IActionResult AddDataUser(UsersDto user)
        {
            UsersDto userDb = _mapper.Map<UsersDto>(user);

            _entityFramework.Add(userDb);
            if (_entityFramework.SaveChanges() > 0)
            {
                return Ok();
            }

            throw new Exception("Failed to Add User");
        }

        [HttpDelete("DeleteUserByID/{UserId}")]
        public IActionResult DeleteUserByID(int UserId)
        {
            UsersModels? userDb = _entityFramework.Users
            .Where(u => u.UserId == UserId)
            .FirstOrDefault<UsersModels>();

            if (userDb != null)
            {
                _entityFramework.Users.Remove(userDb);
                if (_entityFramework.SaveChanges() > 0)
                {
                    return Ok();
                }

                throw new Exception("Failed to Delete User");
            }

            throw new Exception("Failed to Get User");
        }

        #endregion

        #region Users Salary
        [HttpGet("UserSalary/{userId}")]
        public IEnumerable<UsersSalaryModels> GetUserSalaryEF(int userId)
        {
            return _entityFramework.UsersSalary
                .Where(u => u.UserId == userId)
                .ToList();
        }

        [HttpPost("UserSalary")]
        public IActionResult PostUserSalaryEf(UsersSalaryModels userForInsert)
        {
            _entityFramework.UsersSalary.Add(userForInsert);
            if (_entityFramework.SaveChanges() > 0)
            {
                return Ok();
            }
            throw new Exception("Adding UserSalary failed on save");
        }


        [HttpPut("UserSalary")]
        public IActionResult PutUserSalaryEf(UsersSalaryModels userForUpdate)
        {
            UsersSalaryModels? userToUpdate = _entityFramework.UsersSalary
                .Where(u => u.UserId == userForUpdate.UserId)
                .FirstOrDefault();

            if (userToUpdate != null)
            {
                _mapper.Map(userForUpdate, userToUpdate);
                if (_entityFramework.SaveChanges() > 0)
                {
                    return Ok();
                }
                throw new Exception("Updating UserSalary failed on save");
            }
            throw new Exception("Failed to find UserSalary to Update");
        }


        [HttpDelete("UserSalary/{userId}")]
        public IActionResult DeleteUserSalaryEf(int userId)
        {
            UsersSalaryModels? userToDelete = _entityFramework.UsersSalary
                .Where(u => u.UserId == userId)
                .FirstOrDefault();

            if (userToDelete != null)
            {
                _entityFramework.UsersSalary.Remove(userToDelete);
                if (_entityFramework.SaveChanges() > 0)
                {
                    return Ok();
                }
                throw new Exception("Deleting UserSalary failed on save");
            }
            throw new Exception("Failed to find UserSalary to delete");
        }
        #endregion

        #region Users JobInfo

        [HttpGet("UserJobInfo/{userId}")]
        public IEnumerable<UsersJobInfoModels> GetUserJobInfoEF(int userId)
        {
            return _entityFramework.UsersJobInfo
                .Where(u => u.UserId == userId)
                .ToList();
        }

        [HttpPost("UserJobInfo")]
        public IActionResult PostUserJobInfoEf(UsersJobInfoModels userForInsert)
        {
            _entityFramework.UsersJobInfo.Add(userForInsert);
            if (_entityFramework.SaveChanges() > 0)
            {
                return Ok();
            }
            throw new Exception("Adding UserJobInfo failed on save");
        }


        [HttpPut("UserJobInfo")]
        public IActionResult PutUserJobInfoEf(UsersJobInfoModels userForUpdate)
        {
            UsersJobInfoModels? userToUpdate = _entityFramework.UsersJobInfo
                .Where(u => u.UserId == userForUpdate.UserId)
                .FirstOrDefault();

            if (userToUpdate != null)
            {
                _mapper.Map(userForUpdate, userToUpdate);
                if (_entityFramework.SaveChanges() > 0)
                {
                    return Ok();
                }
                throw new Exception("Updating UserJobInfo failed on save");
            }
            throw new Exception("Failed to find UserJobInfo to Update");
        }


        [HttpDelete("UserJobInfo/{userId}")]
        public IActionResult DeleteUserJobInfoEf(int userId)
        {
            UsersJobInfoModels? userToDelete = _entityFramework.UsersJobInfo
                .Where(u => u.UserId == userId)
                .FirstOrDefault();

            if (userToDelete != null)
            {
                _entityFramework.UsersJobInfo.Remove(userToDelete);
                if (_entityFramework.SaveChanges() > 0)
                {
                    return Ok();
                }
                throw new Exception("Deleting UserJobInfo failed on save");
            }
            throw new Exception("Failed to find UserJobInfo to delete");
        }
        #endregion
    }
}
