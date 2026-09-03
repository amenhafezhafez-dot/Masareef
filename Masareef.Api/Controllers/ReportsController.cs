using Microsoft.AspNetCore.Mvc;
using Masareef.BusinessLayer.Services;

namespace Masareef.Api.Controllers;

[Route("api/reports")]
public class ReportsController : ApiControllerBase
{
    private readonly ReportService _service;
    public ReportsController(ReportService service) => _service = service;


    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(
        [FromQuery] int? year, [FromQuery] int? month) =>
        FromResult(await _service.GetDashboardAsync(
            CurrentUserId,
            year ?? DateTime.Today.Year,
            month ?? DateTime.Today.Month));


    [HttpGet("home-spending")]
    public async Task<IActionResult> HomeSpending(
        [FromQuery] int? year, [FromQuery] int? month) =>
        FromResult(await _service.GetHomeSpendingAsync(
            CurrentUserId,
            year ?? DateTime.Today.Year,
            month ?? DateTime.Today.Month));


    [HttpGet("home-breakdown")]
    public async Task<IActionResult> HomeBreakdown(
        [FromQuery] int? year, [FromQuery] int? month) =>
        FromResult(await _service.GetHomeBreakdownAsync(
            CurrentUserId,
            year ?? DateTime.Today.Year,
            month ?? DateTime.Today.Month));


    [HttpGet("home-trend")]
    public async Task<IActionResult> HomeTrend([FromQuery] int months = 6) =>
        FromResult(await _service.GetHomeTrendAsync(CurrentUserId, months));


    [HttpGet("business-trend/{businessId:int}")]
    public async Task<IActionResult> BusinessTrend(
        int businessId, [FromQuery] int months = 6) =>
        FromResult(await _service.GetBusinessTrendAsync(
            businessId, CurrentUserId, months));


    [HttpGet("business-breakdown/{businessId:int}")]
    public async Task<IActionResult> BusinessBreakdown(
        int businessId, [FromQuery] int? year, [FromQuery] int? month) =>
        FromResult(await _service.GetBusinessBreakdownAsync(
            businessId, CurrentUserId,
            year ?? DateTime.Today.Year,
            month ?? DateTime.Today.Month));
}