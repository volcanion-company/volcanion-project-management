using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.TimeEntries.DTOs;

namespace VolcanionPM.Application.Features.TimeEntries.Queries.GetTimeEntriesByTask;

public record GetTimeEntriesByTaskQuery(Guid TaskId) : IRequest<Result<List<TimeEntryDto>>>;
