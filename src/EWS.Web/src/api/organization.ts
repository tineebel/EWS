import { api, JsendData } from './client'

export interface OrgChartNode {
  positionId: number
  positionCode: string
  positionName: string
  jobGrade: string
  isChiefLevel: boolean
  isVacant: boolean
  occupantName: string | null
  secretaryCode: string | null
  children: OrgChartNode[]
}

export const organizationApi = {
  orgChart: (rootCode?: string, branchCode?: string, deptCode?: string, sectionCode?: string) =>
    api
      .get<JsendData<OrgChartNode[]>>('/organization/org-chart', {
        params: { rootCode, branchCode, deptCode, sectionCode },
      })
      .then(r => r.data),
}
