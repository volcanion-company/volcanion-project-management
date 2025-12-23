using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Tasks;

namespace VolcanionPM.Application.Features.Tasks.Queries.GetById;

public record GetTaskByIdQuery(Guid Id) : IRequest<Result<TaskDto>>;
