using Microsoft.EntityFrameworkCore;

using Studwork.Api.Entities;



namespace Studwork.Api.Data;



public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)

{

    public DbSet<User> Users => Set<User>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<Bid> Bids => Set<Bid>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<DirectMessage> DirectMessages => Set<DirectMessage>();

    public DbSet<Attachment> Attachments => Set<Attachment>();



    protected override void OnModelCreating(ModelBuilder modelBuilder)

    {

        modelBuilder.Entity<User>()

            .HasIndex(u => u.Email)

            .IsUnique();



        modelBuilder.Entity<Order>()

            .HasOne(o => o.Customer)

            .WithMany(u => u.CreatedOrders)

            .HasForeignKey(o => o.CustomerId)

            .OnDelete(DeleteBehavior.Restrict);



        modelBuilder.Entity<Order>()

            .HasOne(o => o.Executor)

            .WithMany(u => u.AssignedOrders)

            .HasForeignKey(o => o.ExecutorId)

            .OnDelete(DeleteBehavior.SetNull);



        modelBuilder.Entity<Order>()

            .HasOne(o => o.InvitedExecutor)

            .WithMany(u => u.InvitedOrders)

            .HasForeignKey(o => o.InvitedExecutorId)

            .OnDelete(DeleteBehavior.SetNull);



        modelBuilder.Entity<Bid>()

            .HasOne(b => b.Order)

            .WithMany(o => o.Bids)

            .HasForeignKey(b => b.OrderId)

            .OnDelete(DeleteBehavior.Cascade);



        modelBuilder.Entity<Bid>()

            .HasOne(b => b.Executor)

            .WithMany(u => u.Bids)

            .HasForeignKey(b => b.ExecutorId)

            .OnDelete(DeleteBehavior.Restrict);



        modelBuilder.Entity<Message>()

            .HasOne(m => m.Order)

            .WithMany(o => o.Messages)

            .HasForeignKey(m => m.OrderId)

            .OnDelete(DeleteBehavior.Cascade);



        modelBuilder.Entity<Review>()

            .HasOne(r => r.Order)

            .WithOne(o => o.Review)

            .HasForeignKey<Review>(r => r.OrderId)

            .OnDelete(DeleteBehavior.Cascade);



        modelBuilder.Entity<Review>()

            .HasOne(r => r.FromUser)

            .WithMany(u => u.ReviewsGiven)

            .HasForeignKey(r => r.FromUserId)

            .OnDelete(DeleteBehavior.Restrict);



        modelBuilder.Entity<Review>()

            .HasOne(r => r.ToUser)

            .WithMany(u => u.ReviewsReceived)

            .HasForeignKey(r => r.ToUserId)

            .OnDelete(DeleteBehavior.Restrict);



        modelBuilder.Entity<Conversation>()

            .HasIndex(c => new { c.User1Id, c.User2Id })

            .IsUnique();



        modelBuilder.Entity<Conversation>()

            .HasOne(c => c.User1)

            .WithMany()

            .HasForeignKey(c => c.User1Id)

            .OnDelete(DeleteBehavior.Restrict);



        modelBuilder.Entity<Conversation>()

            .HasOne(c => c.User2)

            .WithMany()

            .HasForeignKey(c => c.User2Id)

            .OnDelete(DeleteBehavior.Restrict);



        modelBuilder.Entity<DirectMessage>()

            .HasOne(m => m.Conversation)

            .WithMany(c => c.Messages)

            .HasForeignKey(m => m.ConversationId)

            .OnDelete(DeleteBehavior.Cascade);



        modelBuilder.Entity<DirectMessage>()

            .HasOne(m => m.Sender)

            .WithMany(u => u.DirectMessages)

            .HasForeignKey(m => m.SenderId)

            .OnDelete(DeleteBehavior.Restrict);



        modelBuilder.Entity<Attachment>()

            .HasOne(a => a.UploadedBy)

            .WithMany(u => u.Attachments)

            .HasForeignKey(a => a.UploadedById)

            .OnDelete(DeleteBehavior.Restrict);



        modelBuilder.Entity<Attachment>()

            .HasOne(a => a.Order)

            .WithMany(o => o.Attachments)

            .HasForeignKey(a => a.OrderId)

            .OnDelete(DeleteBehavior.Cascade);



        modelBuilder.Entity<Attachment>()

            .HasOne(a => a.Message)

            .WithMany(m => m.Attachments)

            .HasForeignKey(a => a.MessageId)

            .OnDelete(DeleteBehavior.Cascade);



        modelBuilder.Entity<Attachment>()

            .HasOne(a => a.DirectMessage)

            .WithMany(m => m.Attachments)

            .HasForeignKey(a => a.DirectMessageId)

            .OnDelete(DeleteBehavior.Cascade);

    }

}


