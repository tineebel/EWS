using EWS.API.Middlewares;
using EWS.Application;
using EWS.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "EWS — Enterprise Workflow System API",
        Version = "v1",
        Description = """
            ## Enterprise Workflow System

            ระบบ Approval Workflow แบบ **Position-Based** สำหรับองค์กร SF Cinema

            ### การตรวจสอบ Workflow Flow

            1. **Organization** → `GET /api/organization/positions/{code}/hierarchy` — ดูสายบังคับบัญชา
            2. **Simulate** → `GET /api/organization/positions/{code}/approval-chain` — Simulate ผู้อนุมัติ
            3. **Submit** → `POST /api/workflows/submit` — ยื่นเอกสาร (Response แสดง Approval Chain ทันที)
            4. **Approve/Reject** → `POST /api/workflows/{id}/approve|reject` — อนุมัติ/ปฏิเสธ
            5. **Audit** → `GET /api/workflows/{id}/audit` — ดู History ทุก Event

            ### Error Codes
            WF_TEMPLATE_NOT_FOUND | WF_TEMPLATE_AMBIGUOUS | WF_UNAUTHORIZED | WF_UNAUTHORIZED_APPROVER | WF_PREAPPROVAL_REQUIRED | WF_ESCALATION_NO_APPROVER
            """
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EWS API v1");
    c.RoutePrefix = string.Empty;
    c.DisplayRequestDuration();
});

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
