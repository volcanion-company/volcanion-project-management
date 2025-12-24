using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Reporting.Queries.GetResourceUtilizationReport;

public class GetResourceUtilizationReportQuery : IRequest<Result<ResourceUtilizationReportDto>>
{
    public Guid? ProjectId { get; set; }
    public Guid? OrganizationId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
