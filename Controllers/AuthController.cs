using Intermediate_DotNet_WebAPI.Data;
using Intermediate_DotNet_WebAPI.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Intermediate_DotNet_WebAPI.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly IConfiguration _config;
        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
        }


        [HttpPost("UsersRegister")]
        public IActionResult UserRegister(UsersRegistrationDto usersRegistration)
        {
            if (usersRegistration.Password == usersRegistration.PasswordConfirm)
            {
                string sqlCheckEmailUser = @"
                    SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '" + usersRegistration.Email + "'";

                IEnumerable<string> EmailUsers = _dapper.LoadData<string>(sqlCheckEmailUser);

                if (EmailUsers.Count() == 0)
                {
                    byte[] passwordSalt = new byte[128 / 8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }

                    string passwordSaltPlusString = _config.GetSection("AppSettings: PasswordKey").Value +
                        Convert.ToBase64String(passwordSalt);

                    byte[] passwordHash = KeyDerivation.Pbkdf2(
                        password: usersRegistration.Password,
                        salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                        prf: KeyDerivationPrf.HMACSHA256,
                        iterationCount: 1000000,
                        numBytesRequested: 256 / 8
                    );

                    string sqlAddAuth = @"
                        INSERT INTO TutorialAppSchema.Auth(
                            [Email],
                            [PasswordHash],
                            [PasswordSalt]) 
                        VALUES (
                        '" + usersRegistration.Email + "'" +
                        "', @PasswordHash, @PasswordSalt)";

                    List<SqlParameter> sqlParams = new List<SqlParameter>();

                    SqlParameter passwordSaltParam = new SqlParameter("@PasswordSalt", System.Data.SqlDbType.VarBinary);
                    passwordSaltParam.Value = passwordSalt;

                    SqlParameter passwordHashParam = new SqlParameter("@PasswordHash", System.Data.SqlDbType.VarBinary);
                    passwordHashParam.Value = passwordHash;

                    sqlParams.Add(passwordSaltParam);
                    sqlParams.Add(passwordHashParam);

                    if (_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParams))
                    {

                        return Ok();
                    }

                    throw new Exception("Failed to register user!");
                }
                throw new Exception("User with this email already exists!");
            }
            throw new Exception("Passwords do not macth!");
        }


        [HttpPost("UsersLogin")]
        public IActionResult UsersLogint(UsersLogingDto usersLoging)
        {
            return Ok();
        }

    }
}
