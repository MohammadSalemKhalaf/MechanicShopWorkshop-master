using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;

public class GetRepairTaskByIdQueryHandler(
    ILogger<GetRepairTaskByIdQueryHandler> logger,
    IAppDbContext context
    )
    : IRequestHandler<GetRepairTaskByIdQuery, Result<RepairTaskDto>>
{
    private readonly ILogger<GetRepairTaskByIdQueryHandler> _logger = logger;
    private readonly IAppDbContext _context = context;

    public async Task<Result<RepairTaskDto>> Handle(GetRepairTaskByIdQuery query, CancellationToken ct)
    {
        var repairTask = await _context.RepairTasks.AsNoTracking().Include(c => c.Parts)
                                     .FirstOrDefaultAsync(c => c.Id == query.RepairTaskId, ct);

        if (repairTask is null)
        {
            _logger.LogWarning("Repair task with id {RepairTaskId} was not found", query.RepairTaskId);

            return ApplicationErrors.RepairTaskNotFound;
        }

        return repairTask.ToDto();
    }
}