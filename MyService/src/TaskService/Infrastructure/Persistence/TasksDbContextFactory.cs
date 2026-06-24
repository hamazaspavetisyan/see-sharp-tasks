using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskService.Infrastructure.Persistence;

public class TasksDbContextFactory : IDesignTimeDbContextFactory<TasksDbContext>
{
    public TasksDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TasksDbContext>()
            .UseMySql(
                "Server=localhost;Port=3306;Database=tasks_db;Uid=root;Pwd=design_time_placeholder;",
                new MySqlServerVersion(new Version(8, 0, 0)))
            .Options;
        return new TasksDbContext(options);
    }
}
