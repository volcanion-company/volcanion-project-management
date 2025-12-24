using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Reporting.Queries.GetProjectProgressReport;

public class GetProjectProgressReportQuery : IRequest<Result<ProjectProgressReportDto>>
{
    public Guid ProjectId { get; set; }
}
