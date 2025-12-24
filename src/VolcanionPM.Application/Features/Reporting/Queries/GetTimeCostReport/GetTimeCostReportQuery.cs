using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Reporting.Queries.GetTimeCostReport;

public class GetTimeCostReportQuery : IRequest<Result<TimeCostReportDto>>
{
    public Guid? ProjectId { get; set; }
    public Guid? OrganizationId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
