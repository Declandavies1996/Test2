using CaeDashboard.EngineeringRequests.Models;
using Microsoft.EntityFrameworkCore;

namespace CaeDashboard.EngineeringRequests;

public static class EngineeringRequestsDbContextExtensions
{
    public static void ConfigureEngineeringRequests(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EngineeringRequest>(entity =>
        {
            entity.ToTable("Requests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.SystemName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.RequestedBy).HasMaxLength(120);
            entity.Property(x => x.Department).HasMaxLength(120);
            entity.Property(x => x.Priority).HasConversion<string>().HasMaxLength(10).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(4000);
            entity.Property(x => x.OwnerUserName).HasMaxLength(256);
            entity.Property(x => x.CreatedByUserName).HasMaxLength(256);
            entity.Property(x => x.UpdatedByUserName).HasMaxLength(256);
            entity.Property(x => x.SubmittedByUserName).HasMaxLength(256);
            entity.Property(x => x.TriagedByUserName).HasMaxLength(256);
            entity.Property(x => x.UrgencyExplanation).HasMaxLength(2000);
            entity.Property(x => x.BusinessReason).HasMaxLength(4000);
            entity.Property(x => x.ExpectedBehaviour).HasMaxLength(4000);
            entity.Property(x => x.ActualBehaviour).HasMaxLength(4000);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.Priority);
            entity.HasIndex(x => x.SystemName);
            entity.HasIndex(x => x.CreatedDate);
            entity.HasIndex(x => x.OwnerUserName);
            entity.HasIndex(x => x.SubmittedByUserName);
            entity.HasIndex(x => x.RequiresTriage);
        });

        modelBuilder.Entity<EngineeringSystem>(entity =>
        {
            entity.ToTable("Systems");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Purpose).HasMaxLength(1000);
            entity.Property(x => x.MainUsers).HasMaxLength(500);
            entity.Property(x => x.Criticality).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.KnownRisks).HasMaxLength(2000);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<RequestNote>(entity =>
        {
            entity.ToTable("RequestNotes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NoteText).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.CreatedBy).HasMaxLength(120);
            entity.HasOne(x => x.Request)
                .WithMany(x => x.RequestNotes)
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.RequestId, x.CreatedDate });
        });

        modelBuilder.Entity<RequestAttachment>(entity =>
        {
            entity.ToTable("RequestAttachments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.StoredFileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.FilePath).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(120);
            entity.Property(x => x.UploadedBy).HasMaxLength(120);
            entity.HasOne(x => x.Request)
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.RequestId, x.UploadedDate });
        });

        modelBuilder.Entity<RequestHistory>(entity =>
        {
            entity.ToTable("RequestHistory");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ActionType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.OldValue).HasMaxLength(500);
            entity.Property(x => x.NewValue).HasMaxLength(500);
            entity.Property(x => x.ChangedBy).HasMaxLength(120);
            entity.HasOne(x => x.Request)
                .WithMany(x => x.History)
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.RequestId, x.ChangedDate });
        });

        modelBuilder.Entity<Runbook>(entity =>
        {
            entity.ToTable("Runbooks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.SystemName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Category).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.Problem).HasMaxLength(4000);
            entity.Property(x => x.Symptoms).HasMaxLength(4000);
            entity.Property(x => x.Cause).HasMaxLength(4000);
            entity.Property(x => x.FixSteps).HasMaxLength(8000);
            entity.Property(x => x.ResolutionSteps).HasMaxLength(8000);
            entity.Property(x => x.VerificationSteps).HasMaxLength(4000);
            entity.Property(x => x.KnownRisks).HasMaxLength(4000);
            entity.Property(x => x.Notes).HasMaxLength(4000);
            entity.HasIndex(x => x.SystemName);
            entity.HasIndex(x => x.Category);
            entity.HasIndex(x => x.UpdatedDate);
        });

        modelBuilder.Entity<RequestRunbook>(entity =>
        {
            entity.ToTable("RequestRunbooks");
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Request)
                .WithMany(x => x.RequestRunbooks)
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Runbook)
                .WithMany(x => x.RequestRunbooks)
                .HasForeignKey(x => x.RunbookId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.RequestId, x.RunbookId }).IsUnique();
            entity.HasIndex(x => x.RunbookId);
        });

        modelBuilder.Entity<RecurringIssue>(entity =>
        {
            entity.ToTable("RecurringIssues");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SystemName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.IssueSummary).HasMaxLength(500).IsRequired();
            entity.Property(x => x.TemporaryFix).HasMaxLength(4000);
            entity.Property(x => x.SuspectedRootCause).HasMaxLength(4000);
            entity.Property(x => x.RelatedRequestIds).HasMaxLength(1000);
            entity.HasIndex(x => x.SystemName);
            entity.HasIndex(x => x.PermanentFixNeeded);
        });

        modelBuilder.Entity<ReleaseChangeLog>(entity =>
        {
            entity.ToTable("ReleaseChangeLogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SystemName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.FilesChanged).HasMaxLength(4000);
            entity.Property(x => x.DeploymentNotes).HasMaxLength(4000);
            entity.Property(x => x.RollbackNotes).HasMaxLength(4000);
            entity.Property(x => x.VerifiedBy).HasMaxLength(120);
            entity.HasOne(x => x.Request)
                .WithMany(x => x.ReleaseChangeLogs)
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => x.SystemName);
            entity.HasIndex(x => x.ReleaseDate);
            entity.HasIndex(x => x.RequestId);
        });

        modelBuilder.Entity<SubmissionLink>(entity =>
        {
            entity.ToTable("SubmissionLinks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OwnerUserName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Token).HasMaxLength(80).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(200);
            entity.Property(x => x.CreatedByUserName).HasMaxLength(256);
            entity.HasIndex(x => x.Token).IsUnique();
            entity.HasIndex(x => x.OwnerUserName);
            entity.HasIndex(x => x.IsActive);
        });
    }
}
