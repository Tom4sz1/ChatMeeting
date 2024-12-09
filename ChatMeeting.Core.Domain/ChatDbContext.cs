using ChatMeeting.Core.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatMeeting.Core.Domain
{
    public class ChatDbContext :DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) 
        {
            Database.EnsureCreated();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>().HasOne(x => x.Chat).WithMany(x => x.Messages).HasForeignKey(m => m.ChatId);

            modelBuilder.Entity<Message>().HasOne(x => x.Sender).WithMany(x => x.Messages).HasForeignKey(m => m.SenderId);

            modelBuilder.Entity<Chat>().HasData(
                new Chat { ChatId = Guid.NewGuid(), Name = "Global", CreatedAt =  DateTime.Now }
                );
        }
    }
}
