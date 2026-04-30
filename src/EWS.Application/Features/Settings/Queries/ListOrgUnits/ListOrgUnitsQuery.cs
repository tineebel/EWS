using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Settings.Queries.ListOrgUnits;

public record ListDepartmentsQuery(string? Search, bool? IsActive)
    : IRequest<Result<List<DepartmentOptionDto>>>;

public record ListSectionsQuery(string? Search, string? DeptCode, bool? IsActive)
    : IRequest<Result<List<SectionOptionDto>>>;

public record ListBranchOptionsQuery()
    : IRequest<Result<List<BranchOptionDto>>>;

public record DepartmentOptionDto(
    int DepartmentId,
    string DeptCode,
    string? DeptShortCode,
    string DeptName,
    string? DeptNameEn,
    string DivisionCode,
    string DivisionName,
    bool IsActive);

public record SectionOptionDto(
    int SectionId,
    string SectCode,
    string? SectShortCode,
    string SectName,
    string? SectNameEn,
    string DeptCode,
    string DeptName,
    bool IsActive);

public record BranchOptionDto(
    int SectionId,
    string BranchCode,
    string? BranchShortCode,
    string BranchName,
    int PositionCount);
