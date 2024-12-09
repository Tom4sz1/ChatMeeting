﻿using ChatMeeting.Core.Domain.Dtos;
using ChatMeeting.Core.Domain.Interfaces.Repositories;
using ChatMeeting.Core.Domain.Interfaces.Services;
using ChatMeeting.Core.Domain.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChatMeeting.Core.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthService> _logger;
        private readonly IJwtService _jwtService;

        public AuthService(IUserRepository userRepository, ILogger<AuthService> logger, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _logger = logger;
            _jwtService = jwtService;
        }

        public async Task<AuthDto> GetToken(LoginDto loginModel)
        {
            try
            {
                var user = await _userRepository.GetUserByLogin(loginModel.Username);

                if (user == null)
                {
                    _logger.LogWarning($"User not found: {loginModel.Username}");
                    throw new InvalidOperationException("User with this login does not exist");
                }

                if (VerifyPassword(loginModel.Password, user.Password))
                {
                    var authData = _jwtService.GenerateJwtToken(user);

                    return authData;
                }
                else
                {
                    throw new UnauthorizedAccessException("Invalid credencials");
                }

            }
            catch (Exception ex) { _logger.LogError(ex, $"Authentication failed for user: {loginModel.Username}");throw; }
        }

        private bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            var parts = storedPassword.Split(':');

            if (parts.Length != 2) 
            {
                throw new FormatException("Unexpected hash format. The stored hash should be in the format 'salt:hashPassword'");
            }

            var salt = Convert.FromBase64String(parts[0]);
            var storedHashPassword = parts[1];

            var enterHashedPassword = Hash(enteredPassword, salt);

            return enterHashedPassword == storedHashPassword;
        }

        public async Task RegisterUser(RegisterUserDto registerUser)
        {
            try 
            {
                var existingUser = await _userRepository.GetUserByLogin(registerUser.Username);
                if (existingUser != null) 
                {
                    _logger.LogWarning($"User with login {registerUser.Username} alredy exists");
                    throw new InvalidOperationException("User with this login already exists");
                }

                var user = new User(registerUser.Username, HashPassword(registerUser.Password));
                await _userRepository.AddUser(user);
            }
            catch(Exception ex)  
            {
                _logger.LogError(ex, $"Error occurred while registering user {registerUser.Username}");
                throw new InvalidProgramException();
            }
        }

        private string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            string hashed = Hash(password, salt);
            return $"{Convert.ToBase64String(salt)}:{hashed}";
        }

        private string Hash(string password, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256/8
                ));
        }
    }
}
