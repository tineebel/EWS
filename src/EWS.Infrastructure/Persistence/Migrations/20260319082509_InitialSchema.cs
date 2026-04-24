using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EWS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.EnsureSchema(
                name: "wf");

            migrationBuilder.CreateTable(
                name: "Divisions",
                schema: "dbo",
                columns: table => new
                {
                    DivisionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DivisionCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DivisionName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DivisionNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Divisions", x => x.DivisionId);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                schema: "wf",
                columns: table => new
                {
                    DocumentTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocCode = table.Column<int>(type: "int", nullable: false),
                    DocName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DocNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.DocumentTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                schema: "dbo",
                columns: table => new
                {
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EmployeeNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Nickname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Tel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsTest = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeId);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                schema: "dbo",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeptCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeptName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DeptNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentId);
                    table.ForeignKey(
                        name: "FK_Departments_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalSchema: "dbo",
                        principalTable: "Divisions",
                        principalColumn: "DivisionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTemplates",
                schema: "wf",
                columns: table => new
                {
                    TemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentTypeId = table.Column<int>(type: "int", nullable: false),
                    FlowCode = table.Column<int>(type: "int", nullable: false),
                    FlowDesc = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    WfScopeType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    HasSpecialItem = table.Column<bool>(type: "bit", nullable: false),
                    IsUrgent = table.Column<bool>(type: "bit", nullable: false),
                    Condition1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Condition2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Condition3 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Condition4 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Condition5 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTemplates", x => x.TemplateId);
                    table.ForeignKey(
                        name: "FK_WorkflowTemplates_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalSchema: "wf",
                        principalTable: "DocumentTypes",
                        principalColumn: "DocumentTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                schema: "dbo",
                columns: table => new
                {
                    SectionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SectNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.SectionId);
                    table.ForeignKey(
                        name: "FK_Sections_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "dbo",
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowSteps",
                schema: "wf",
                columns: table => new
                {
                    StepId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    StepName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApproverType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SpecificPositionCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    EscalationDays = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowSteps", x => x.StepId);
                    table.ForeignKey(
                        name: "FK_WorkflowSteps_WorkflowTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "wf",
                        principalTable: "WorkflowTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                schema: "dbo",
                columns: table => new
                {
                    PositionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PositionCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PositionName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PositionShortName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    JobGrade = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    WfScopeType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    ParentPositionId = table.Column<int>(type: "int", nullable: true),
                    IsChiefLevel = table.Column<bool>(type: "bit", nullable: false),
                    SecretaryPositionId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.PositionId);
                    table.ForeignKey(
                        name: "FK_Positions_Positions_ParentPositionId",
                        column: x => x.ParentPositionId,
                        principalSchema: "dbo",
                        principalTable: "Positions",
                        principalColumn: "PositionId");
                    table.ForeignKey(
                        name: "FK_Positions_Positions_SecretaryPositionId",
                        column: x => x.SecretaryPositionId,
                        principalSchema: "dbo",
                        principalTable: "Positions",
                        principalColumn: "PositionId");
                    table.ForeignKey(
                        name: "FK_Positions_Sections_SectionId",
                        column: x => x.SectionId,
                        principalSchema: "dbo",
                        principalTable: "Sections",
                        principalColumn: "SectionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Delegations",
                schema: "dbo",
                columns: table => new
                {
                    DelegationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromPositionId = table.Column<int>(type: "int", nullable: false),
                    ToPositionId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Delegations", x => x.DelegationId);
                    table.ForeignKey(
                        name: "FK_Delegations_Positions_FromPositionId",
                        column: x => x.FromPositionId,
                        principalSchema: "dbo",
                        principalTable: "Positions",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Delegations_Positions_ToPositionId",
                        column: x => x.ToPositionId,
                        principalSchema: "dbo",
                        principalTable: "Positions",
                        principalColumn: "PositionId");
                });

            migrationBuilder.CreateTable(
                name: "PositionAssignments",
                schema: "dbo",
                columns: table => new
                {
                    AssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsVacant = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionAssignments", x => x.AssignmentId);
                    table.ForeignKey(
                        name: "FK_PositionAssignments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PositionAssignments_Positions_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "dbo",
                        principalTable: "Positions",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstances",
                schema: "wf",
                columns: table => new
                {
                    InstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    DocumentNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExternalDocRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SubmitterPositionId = table.Column<int>(type: "int", nullable: false),
                    SubmitterEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActingAsPositionId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PreApprovalStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedBySecretaryPositionId = table.Column<int>(type: "int", nullable: true),
                    PreApprovalChiefPositionId = table.Column<int>(type: "int", nullable: true),
                    PreApprovalConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsSpecialItem = table.Column<bool>(type: "bit", nullable: false),
                    IsUrgent = table.Column<bool>(type: "bit", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstances", x => x.InstanceId);
                    table.ForeignKey(
                        name: "FK_WorkflowInstances_Employees_SubmitterEmployeeId",
                        column: x => x.SubmitterEmployeeId,
                        principalSchema: "dbo",
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowInstances_Positions_ActingAsPositionId",
                        column: x => x.ActingAsPositionId,
                        principalSchema: "dbo",
                        principalTable: "Positions",
                        principalColumn: "PositionId");
                    table.ForeignKey(
                        name: "FK_WorkflowInstances_Positions_CreatedBySecretaryPositionId",
                        column: x => x.CreatedBySecretaryPositionId,
                        principalSchema: "dbo",
                        principalTable: "Positions",
                        principalColumn: "PositionId");
                    table.ForeignKey(
                        name: "FK_WorkflowInstances_Positions_PreApprovalChiefPositionId",
                        column: x => x.PreApprovalChiefPositionId,
                        principalSchema: "dbo",
                        principalTable: "Positions",
                        principalColumn: "PositionId");
                    table.ForeignKey(
                        name: "FK_WorkflowInstances_Positions_SubmitterPositionId",
                        column: x => x.SubmitterPositionId,
                        principalSchema: "dbo",
                        principalTable: "Positions",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowInstances_WorkflowTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "wf",
                        principalTable: "WorkflowTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowApprovals",
                schema: "wf",
                columns: table => new
                {
                    ApprovalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StepId = table.Column<int>(type: "int", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    AssignedPositionId = table.Column<int>(type: "int", nullable: false),
                    ActorEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActorActingAsPositionId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ActionAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EscalatedFromPositionId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowApprovals", x => x.ApprovalId);
                    table.ForeignKey(
                        name: "FK_WorkflowApprovals_Employees_ActorEmployeeId",
                        column: x => x.ActorEmployeeId,
                        principalSchema: "dbo",
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_WorkflowApprovals_Positions_ActorActingAsPositionId",
                        column: x => x.ActorActingAsPositionId,
                        principalSchema: "dbo",
                        principalTable: "Positions",
                        principalColumn: "PositionId");
                    table.ForeignKey(
                        name: "FK_WorkflowApprovals_Positions_AssignedPositionId",
                        column: x => x.AssignedPositionId,
                        principalSchema: "dbo",
                        principalTable: "Positions",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowApprovals_WorkflowInstances_InstanceId",
                        column: x => x.InstanceId,
                        principalSchema: "wf",
                        principalTable: "WorkflowInstances",
                        principalColumn: "InstanceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowApprovals_WorkflowSteps_StepId",
                        column: x => x.StepId,
                        principalSchema: "wf",
                        principalTable: "WorkflowSteps",
                        principalColumn: "StepId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowHistories",
                schema: "wf",
                columns: table => new
                {
                    HistoryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: true),
                    ActorPositionId = table.Column<int>(type: "int", nullable: true),
                    ActorEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DataSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowHistories", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_WorkflowHistories_Employees_ActorEmployeeId",
                        column: x => x.ActorEmployeeId,
                        principalSchema: "dbo",
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_WorkflowHistories_Positions_ActorPositionId",
                        column: x => x.ActorPositionId,
                        principalSchema: "dbo",
                        principalTable: "Positions",
                        principalColumn: "PositionId");
                    table.ForeignKey(
                        name: "FK_WorkflowHistories_WorkflowInstances_InstanceId",
                        column: x => x.InstanceId,
                        principalSchema: "wf",
                        principalTable: "WorkflowInstances",
                        principalColumn: "InstanceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Delegation_Active",
                schema: "dbo",
                table: "Delegations",
                columns: new[] { "FromPositionId", "IsActive", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Delegations_ToPositionId",
                schema: "dbo",
                table: "Delegations",
                column: "ToPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DeptCode",
                schema: "dbo",
                table: "Departments",
                column: "DeptCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DivisionId",
                schema: "dbo",
                table: "Departments",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_DivisionCode",
                schema: "dbo",
                table: "Divisions",
                column: "DivisionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_DocCode",
                schema: "wf",
                table: "DocumentTypes",
                column: "DocCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeCode",
                schema: "dbo",
                table: "Employees",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PositionAssignment_Active",
                schema: "dbo",
                table: "PositionAssignments",
                columns: new[] { "PositionId", "IsActive", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PositionAssignments_EmployeeId",
                schema: "dbo",
                table: "PositionAssignments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_ParentPositionId",
                schema: "dbo",
                table: "Positions",
                column: "ParentPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_PositionCode",
                schema: "dbo",
                table: "Positions",
                column: "PositionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Positions_SecretaryPositionId",
                schema: "dbo",
                table: "Positions",
                column: "SecretaryPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_SectionId",
                schema: "dbo",
                table: "Positions",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_DepartmentId",
                schema: "dbo",
                table: "Sections",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_SectCode",
                schema: "dbo",
                table: "Sections",
                column: "SectCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowApproval_InstanceStep",
                schema: "wf",
                table: "WorkflowApprovals",
                columns: new[] { "InstanceId", "StepOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowApprovals_ActorActingAsPositionId",
                schema: "wf",
                table: "WorkflowApprovals",
                column: "ActorActingAsPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowApprovals_ActorEmployeeId",
                schema: "wf",
                table: "WorkflowApprovals",
                column: "ActorEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowApprovals_AssignedPositionId",
                schema: "wf",
                table: "WorkflowApprovals",
                column: "AssignedPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowApprovals_StepId",
                schema: "wf",
                table: "WorkflowApprovals",
                column: "StepId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowHistories_ActorEmployeeId",
                schema: "wf",
                table: "WorkflowHistories",
                column: "ActorEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowHistories_ActorPositionId",
                schema: "wf",
                table: "WorkflowHistories",
                column: "ActorPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowHistory_Instance",
                schema: "wf",
                table: "WorkflowHistories",
                columns: new[] { "InstanceId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_ActingAsPositionId",
                schema: "wf",
                table: "WorkflowInstances",
                column: "ActingAsPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_CreatedBySecretaryPositionId",
                schema: "wf",
                table: "WorkflowInstances",
                column: "CreatedBySecretaryPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_DocumentNo",
                schema: "wf",
                table: "WorkflowInstances",
                column: "DocumentNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_PreApprovalChiefPositionId",
                schema: "wf",
                table: "WorkflowInstances",
                column: "PreApprovalChiefPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_SubmitterEmployeeId",
                schema: "wf",
                table: "WorkflowInstances",
                column: "SubmitterEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_SubmitterPositionId",
                schema: "wf",
                table: "WorkflowInstances",
                column: "SubmitterPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstance_Status",
                schema: "wf",
                table: "WorkflowInstances",
                columns: new[] { "Status", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_TemplateId",
                schema: "wf",
                table: "WorkflowInstances",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStep_TemplateOrder",
                schema: "wf",
                table: "WorkflowSteps",
                columns: new[] { "TemplateId", "StepOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTemplates_DocumentTypeId_FlowCode",
                schema: "wf",
                table: "WorkflowTemplates",
                columns: new[] { "DocumentTypeId", "FlowCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Delegations",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PositionAssignments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "WorkflowApprovals",
                schema: "wf");

            migrationBuilder.DropTable(
                name: "WorkflowHistories",
                schema: "wf");

            migrationBuilder.DropTable(
                name: "WorkflowSteps",
                schema: "wf");

            migrationBuilder.DropTable(
                name: "WorkflowInstances",
                schema: "wf");

            migrationBuilder.DropTable(
                name: "Employees",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Positions",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "WorkflowTemplates",
                schema: "wf");

            migrationBuilder.DropTable(
                name: "Sections",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DocumentTypes",
                schema: "wf");

            migrationBuilder.DropTable(
                name: "Departments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Divisions",
                schema: "dbo");
        }
    }
}
