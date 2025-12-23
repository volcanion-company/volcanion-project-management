using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.TimeEntries.DTOs;

namespace VolcanionPM.Application.Features.TimeEntries.Queries.GetTimeEntryById;

public record GetTimeEntryByIdQuery(Guid Id) : IRequest<Result<TimeEntryDto>>;
