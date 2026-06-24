using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskService.Infrastructure.Persistence;

namespace TaskService.Application.Commands.Tasks;

public record DeleteTaskCommand(Guid Id, Guid UserId) : IRequest;

public class DeleteTaskCommandHandler(TasksDbContext db)
    : IRequestHandler<DeleteTaskCommand>
{
    public async Task Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(
            t => t.Id == request.Id && t.UserId == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Task not found.");

        db.Tasks.Remove(task);
        await db.SaveChangesAsync(cancellationToken);
    }
}
