using Intermediate_DotNet_WebAPI.Data;
using Intermediate_DotNet_WebAPI.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
                    SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '" + usersRegistration.Email + "' AND Active = 'Y'";

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

                    byte[] passwordHash = GetPasswordHash(usersRegistration.Password, passwordSalt);

                    string sqlAddAuth = @"
                        INSERT INTO TutorialAppSchema.Auth(
                            [Email],
                            [PasswordHash],
                            [PasswordSalt]) 
                        VALUES (
                        '" + usersRegistration.Email + "'" +
                        ", @PasswordHash, @PasswordSalt)";

                    List<SqlParameter> sqlParams = new List<SqlParameter>();

                    SqlParameter passwordSaltParam = new SqlParameter("@PasswordSalt", System.Data.SqlDbType.VarBinary);
                    passwordSaltParam.Value = passwordSalt;

                    SqlParameter passwordHashParam = new SqlParameter("@PasswordHash", System.Data.SqlDbType.VarBinary);
                    passwordHashParam.Value = passwordHash;

                    sqlParams.Add(passwordSaltParam);
                    sqlParams.Add(passwordHashParam);

                    if (_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParams))
                    {
                        string sqlAddUsers = @"
                            INSERT INTO TutorialAppSchema.Users (
                                [FirstName],
                                [LastName],
                                [Email],
                                [Gender] )
                            VALUES (" +
                                "'" + usersRegistration.FirstName +
                                "', '" + usersRegistration.LastName +
                                "', '" + usersRegistration.Email +
                                "', '" + usersRegistration.Gender +
                                "')";
                        if (_dapper.ExecuteSql(sqlAddUsers))
                        {
                            return Ok();
                        }
                        throw new Exception("Failed to Add user!");
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
            string sqlForHashAndSalt = @"
                SELECT 
                    [Email],
                    [PasswordHash],
                    [PasswordSalt] 
                FROM TutorialAppSchema.Auth 
                WHERE Email = '" + usersLoging.Email + "' AND Active = 'Y'";

            UserLoginConfirmationDto userLoginConfirmation = _dapper
                .LoadDataSingle<UserLoginConfirmationDto>(sqlForHashAndSalt);

            byte[] passwordHash = GetPasswordHash(usersLoging.Password, userLoginConfirmation.PasswordSalt);

            //if (passwordHash == userLoginConfirmation.PasswordHash)
            //{

            //}

            for (int i = 0; i < passwordHash.Length; i++)
            {
                if (passwordHash[i] != userLoginConfirmation.PasswordHash[i])
                {
                    return StatusCode(401, "Incorrent password!");
                }
            }

            string userIdSql = @"
                SELECT UserId 
                FROM TutorialAppSchema.Users 
                WHERE Email = '" + usersLoging.Email + "' AND Active = 'Y'";

            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            return Ok(new Dictionary<string, string> {
                {"token", CreateToken(userId)}
            });
        }

        private byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = _config.GetSection("AppSettings: PasswordKey").Value +
                        Convert.ToBase64String(passwordSalt);

            return KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 1000000,
                numBytesRequested: 256 / 8
            );
        }

        private string CreateToken(int userId)
        {
            Claim[] claims = new Claim[] {
                new Claim("userId", userId.ToString())
            };

            string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;

            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        tokenKeyString != null ? tokenKeyString : ""
                    )
                );

            SigningCredentials credentials = new SigningCredentials(
                    tokenKey,
                    SecurityAlgorithms.HmacSha512Signature
                );

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddDays(1)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);

        }
    }
}
