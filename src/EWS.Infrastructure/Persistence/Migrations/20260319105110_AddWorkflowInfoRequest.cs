using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EWS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowInfoRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkflowInfoRequests",
                schema: "wf",
                columns: table => new
                {
                    InfoRequestId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStepOrder = table.Column<int>(type: "int", nullable: false),
                    FromPositionId = table.Column<int>(type: "int", nullable: false),
                    ToStepOrder = table.Column<int>(type: "int", nullable: false),
                    ToPositionId = table.Column<int>(type: "int", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ChildInfoRequestId = table.Column<long>(type: "bigint", nullable: true),
                    ParentInfoRequestId = table.Column<long>(type: "bigint", nullable: true),
                    AnsweredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInfoRequests", x => x.InfoRequestId);
                    table.ForeignKey(
                        name: "FK_WorkflowInfoRequests_Positions_FromPositionId",
                        column: x => x.FromPositionId,
                        principalSchema: "dbo",
                        principalTable: "Positions",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowInfoRequests_Positions_ToPositionId",
                        column: x => x.ToPositionId,
                        principalSchema: "dbo",
                        principalTable: "Positions",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowInfoRequests_WorkflowInfoRequests_ParentInfoRequestId",
                        column: x => x.ParentInfoRequestId,
                        principalSchema: "wf",
                        principalTable: "WorkflowInfoRequests",
                        principalColumn: "InfoRequestId");
                    table.ForeignKey(
                        name: "FK_WorkflowInfoRequests_WorkflowInstances_InstanceId",
                        column: x => x.InstanceId,
                        principalSchema: "wf",
                        principalTable: "WorkflowInstances",
                        principalColumn: "InstanceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InfoRequest_Instance_Steps",
                schema: "wf",
                table: "WorkflowInfoRequests",
                columns: new[] { "InstanceId", "FromStepOrder", "ToStepOrder", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInfoRequests_FromPositionId",
                schema: "wf",
                table: "WorkflowInfoRequests",
                column: "FromPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInfoRequests_ParentInfoRequestId",
                schema: "wf",
                table: "WorkflowInfoRequests",
                column: "ParentInfoRequestId",
                unique: true,
                filter: "[ParentInfoRequestId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInfoRequests_ToPositionId",
                schema: "wf",
                table: "WorkflowInfoRequests",
                column: "ToPositionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowInfoRequests",
                schema: "wf");
        }
    }
}
