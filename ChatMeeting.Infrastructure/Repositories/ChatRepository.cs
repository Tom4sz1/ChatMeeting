using ChatMeeting.Core.Domain;
using ChatMeeting.Core.Domain.Exceptions;
using ChatMeeting.Core.Domain.Interfaces.Repositories;
using ChatMeeting.Core.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatMeeting.Infrastructure.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ChatDbContext _context;
        private readonly ILogger<ChatRepository> _logger;

        public ChatRepository(ChatDbContext context, ILogger<ChatRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task<Chat> GetChatWithmessages(string chatName, int pageNumber, int pageSize)
        {
            try
            {
                Chat? chat = await GetChat(chatName, pageNumber, pageSize);

                if (chat == null)
                {
                    _logger.LogError($"Chat with name: {chat.Name} not found");
                    throw new ChatNotFoundException(chatName);
                }

                return chat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error occurred while fetching chat with message for {chatName}");
                throw;
            }
        }

        private async Task<Chat?> GetChat(string chatName, int pageNumber, int pageSize)
        {
            return await _context.Chats
                            .Where(x => x.Name == chatName)
                            .Include(x => x.Messages.OrderByDescending(x => x.CreatedAt).Skip((pageNumber - 1) * pageSize).Take(pageSize))
                            .ThenInclude(u => u.Sender)
                            .FirstOrDefaultAsync();
        }
    }
}
