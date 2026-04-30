import { api, JsendList, JsendData } from './client'
import type { Position, PositionDetail, Employee, EmployeeDetail, DocumentType, WorkflowTemplate, Delegation, TemplateAuditLog, UpdateTemplateRequest, DepartmentOption, SectionOption, BranchOption } from './types'

export const settingsApi = {
  positions: {
    list: (params: { search?: string; isActive?: boolean; branchCode?: string; deptCode?: string; sectionCode?: string; page?: number; pageSize?: number }) =>
      api.get<JsendList<Position>>('/settings/positions', { params }).then(r => r.data),
    get: (positionCode: string) =>
      api.get<JsendData<PositionDetail>>(`/settings/positions/${positionCode}`).then(r => r.data),
  },
  departments: {
    list: (params: { search?: string; isActive?: boolean }) =>
      api.get<JsendData<DepartmentOption[]>>('/settings/departments', { params }).then(r => r.data),
  },
  sections: {
    list: (params: { search?: string; deptCode?: string; isActive?: boolean }) =>
      api.get<JsendData<SectionOption[]>>('/settings/sections', { params }).then(r => r.data),
  },
  branchOptions: {
    list: () =>
      api.get<JsendData<BranchOption[]>>('/settings/branch-options').then(r => r.data),
  },
  employees: {
    list: (params: { search?: string; status?: string; branchCode?: string; deptCode?: string; sectionCode?: string; page?: number; pageSize?: number }) =>
      api.get<JsendList<Employee>>('/settings/employees', { params }).then(r => r.data),
    get: (employeeCode: string) =>
      api.get<JsendData<EmployeeDetail>>(`/settings/employees/${employeeCode}`).then(r => r.data),
  },
  documentTypes: {
    list: (params: { search?: string; isActive?: boolean }) =>
      api.get<JsendData<DocumentType[]>>('/settings/document-types', { params }).then(r => r.data),
  },
  workflowTemplates: {
    list: (params: { docCode?: number; isActive?: boolean }) =>
      api.get<JsendData<WorkflowTemplate[]>>('/settings/workflow-templates', { params }).then(r => r.data),
    update: (templateId: number, body: UpdateTemplateRequest) =>
      api.put<JsendData<number>>(`/settings/workflow-templates/${templateId}`, body).then(r => r.data),
    history: (templateId: number) =>
      api.get<JsendData<TemplateAuditLog[]>>(`/settings/workflow-templates/${templateId}/history`).then(r => r.data),
  },
  delegations: {
    list: (params: { positionCode?: string; activeOnly?: boolean }) =>
      api.get<JsendData<Delegation[]>>('/settings/delegations', { params }).then(r => r.data),
  },
}
