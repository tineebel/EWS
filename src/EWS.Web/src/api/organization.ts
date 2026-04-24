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
  orgChart: (rootCode?: string) =>
    api
      .get<JsendData<OrgChartNode[]>>('/organization/org-chart', {
        params: rootCode ? { rootCode } : {},
      })
      .then(r => r.data),
}
