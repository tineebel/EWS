export interface Position {
  positionId: number
  positionCode: string
  positionName: string
  positionShortName?: string
  jobGrade: string
  wfScopeType: string
  isChiefLevel: boolean
  isActive: boolean
  sectionId: number
  sectionName: string
  parentPositionId?: number
  parentPositionCode?: string
  secretaryPositionId?: number
  secretaryPositionCode?: string
}

export interface Employee {
  employeeId: string
  employeeCode: string
  employeeName: string
  employeeNameEn?: string
  nickname?: string
  email?: string
  tel?: string
  status: string
  startDate: string
  endDate?: string
  isTest: boolean
  positionCodes: string[]
}

export interface PositionOccupant {
  assignmentId: number
  employeeCode: string
  employeeName: string
  employeeNameEn?: string
  email?: string
  startDate: string
  endDate?: string
  isCurrent: boolean
}

export interface PositionDetail extends Position {
  parentPositionName?: string
  secretaryPositionCode?: string
  sectionName: string
  occupants: PositionOccupant[]
}

export interface PositionAssignmentDetail {
  assignmentId: number
  positionCode: string
  positionName: string
  positionShortName?: string
  jobGrade: string
  sectionName: string
  startDate: string
  endDate?: string
  isCurrent: boolean
}

export interface EmployeeDetail extends Employee {
  positionAssignments: PositionAssignmentDetail[]
}

export interface DocumentType {
  documentTypeId: number
  docCode: number
  docName: string
  docNameEn?: string
  description?: string
  category: string
  isActive: boolean
  templateCount: number
}

export interface WorkflowStep {
  stepId: number
  stepOrder: number
  stepName: string
  approverType: string
  specificPositionCode?: string
  isRequired: boolean
}

export interface WorkflowTemplate {
  templateId: number
  docCode: number
  docName: string
  flowCode: number
  flowDesc: string
  wfScopeType: string
  hasSpecialItem: boolean
  isUrgent: boolean
  condition1?: string
  condition2?: string
  isActive: boolean
  steps: WorkflowStep[]
}

export interface TemplateAuditLog {
  auditId: number
  version: number
  changeType: string
  changedBy: string
  changedAt: string
  snapshotJson: string
  changeNote?: string
}

export interface UpdateStepRequest {
  stepId?: number
  stepOrder: number
  stepName: string
  approverType: string
  specificPositionCode?: string
  escalationDays: number
  isRequired: boolean
}

export interface UpdateTemplateRequest {
  flowDesc: string
  wfScopeType: string
  hasSpecialItem: boolean
  isUrgent: boolean
  condition1?: string
  condition2?: string
  condition3?: string
  condition4?: string
  condition5?: string
  isActive: boolean
  steps: UpdateStepRequest[]
  changeNote?: string
}

export interface Delegation {
  delegationId: number
  fromPositionCode: string
  fromPositionName: string
  toPositionCode: string
  toPositionName: string
  startDate: string
  endDate: string
  reason?: string
  isCurrentlyActive: boolean
  createdAt: string
}
