using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EWS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FilteredStepOrderIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkflowStep_TemplateOrder",
                schema: "wf",
                table: "WorkflowSteps");

            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IX_WorkflowStep_TemplateOrder_Active
                ON [wf].[WorkflowSteps] ([TemplateId], [StepOrder])
                WHERE [IsActive] = 1
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_WorkflowStep_TemplateOrder_Active ON [wf].[WorkflowSteps]");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStep_TemplateOrder",
                schema: "wf",
                table: "WorkflowSteps",
                columns: new[] { "TemplateId", "StepOrder" },
                unique: true);
        }
    }
}
