using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using HireFlow.DTOs;
using HireFlow.Infrastructure.Data;
using HireFlow.Services.Applications;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace HireFlow.Tests
{
    public class ApplicationServiceTests
    {
        private HireFlowDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<HireFlowDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new HireFlowDbContext(options);
        }

        [Fact]
        public async Task ApplyAsync_Should_Create_Application()
        {
            // Arrange
            var context = GetDbContext();

            context.Jobs.Add(new Job
            {
                Id = 1,
                Title = "Backend Dev",
                Description = "Test",
                Location = "Accra",
                Status = JobStatus.Open
            });

            await context.SaveChangesAsync();

            var queue = new FakeBackgroundQueue();
            var service = new ApplicationService(context, queue);

            var dto = new CreateApplicationDto
            {
                CandidateName = "John Doe",
                CandidateEmail = "john@test.com"
            };

            // Act
            var result = await service.ApplyAsync(1, dto);

            // Assert
            result.Should().NotBeNull();
            result.JobId.Should().Be(1);
            result.CandidateEmail.Should().Be("john@test.com");

            var saved = await context.Applications.FirstOrDefaultAsync();
            saved.Should().NotBeNull();
        }
        
        [Fact]
        public async Task ApplyAsync_Should_Throw_When_Duplicate_Application()
        {
            // Arrange
            var context = GetDbContext();

            context.Jobs.Add(new Job
            {
                Id = 1,
                Title = "Dev",
                Description = "Test",
                Location = "Accra",
                Status = JobStatus.Open
            });

            await context.SaveChangesAsync();

            var queue = new FakeBackgroundQueue();
            var service = new ApplicationService(context, queue);

            var dto = new CreateApplicationDto
            {
                CandidateName = "John",
                CandidateEmail = "john@test.com"
            };

            // First apply (should succeed)
            await service.ApplyAsync(1, dto);

            // Act + Assert (second apply should fail)
            Func<Task> act = async () => await service.ApplyAsync(1, dto);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("You already applied for this job");
        }
        
        [Fact]
        public async Task ApplyAsync_Should_Throw_When_Job_Not_Found()
        {
            // Arrange
            var context = GetDbContext();
            var queue = new FakeBackgroundQueue();
            var service = new ApplicationService(context, queue);

            var dto = new CreateApplicationDto
            {
                CandidateName = "John",
                CandidateEmail = "john@test.com"
            };

            // Act
            Func<Task> act = async () => await service.ApplyAsync(999, dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Job not found");
        }
        
        [Fact]
        public async Task UpdateStage_Should_Change_To_Screening()
        {
            // Arrange
            var context = GetDbContext();

            context.Jobs.Add(new Job
            {
                Id = 1,
                Title = "Dev",
                Description = "Test",
                Location = "Accra",
                Status = JobStatus.Open
            });

            await context.SaveChangesAsync();

            var queue = new FakeBackgroundQueue();
            var service = new ApplicationService(context, queue);

            var app = await service.ApplyAsync(1, new CreateApplicationDto
            {
                CandidateName = "John",
                CandidateEmail = "john@test.com"
            });

            var dto = new UpdateStageDto
            {
                Stage = "Screening",
                Comment = "Moving to screening"
            };

            // Act
            var result = await service.UpdateStageAsync(app.Id, dto, 1);

            // Assert
            result.Stage.Should().Be("Screening");
        }
        
        [Fact]
        public async Task AddNote_Should_Save_Note_Successfully()
        {
            // Arrange
            var context = GetDbContext();

            context.Jobs.Add(new Job
            {
                Id = 1,
                Title = "Dev",
                Description = "Test",
                Location = "Accra",
                Status = JobStatus.Open
            });

            await context.SaveChangesAsync();

            var queue = new FakeBackgroundQueue();
            var service = new ApplicationService(context, queue);

            var app = await service.ApplyAsync(1, new CreateApplicationDto
            {
                CandidateName = "John",
                CandidateEmail = "john@test.com"
            });

            var dto = new CreateNoteDto
            {
                Type = "General",
                Description = "Good candidate"
            };

            // Act
            var result = await service.AddNoteAsync(app.Id, dto, 1);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Be("Good candidate");

            var notes = await context.ApplicationNotes.ToListAsync();
            notes.Should().HaveCount(1);
        }
    }
}