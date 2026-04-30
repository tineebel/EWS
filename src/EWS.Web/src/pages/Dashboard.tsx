import { ApartmentOutlined, BranchesOutlined, FileTextOutlined, TeamOutlined } from '@ant-design/icons'
import { Card, Col, Row, Statistic, Typography, theme } from 'antd'
import { useQuery } from '@tanstack/react-query'
import { settingsApi } from '../api/settings'

export default function Dashboard() {
  const { token } = theme.useToken()
  const positions = useQuery({ queryKey: ['positions-count'], queryFn: () => settingsApi.positions.list({ isActive: true, pageSize: 1 }) })
  const employees = useQuery({ queryKey: ['employees-count'], queryFn: () => settingsApi.employees.list({ status: 'Active', pageSize: 1 }) })
  const docTypes = useQuery({ queryKey: ['doctypes-count'], queryFn: () => settingsApi.documentTypes.list({ isActive: true }) })
  const templates = useQuery({ queryKey: ['templates-count'], queryFn: () => settingsApi.workflowTemplates.list({ isActive: true }) })

  return (
    <div>
      <Typography.Title level={4} style={{ marginBottom: token.marginLG }}>
        System Overview
      </Typography.Title>
      <Row gutter={[token.marginMD, token.marginMD]}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Active Positions"
              value={positions.data?.data.totalRows ?? '-'}
              prefix={<ApartmentOutlined />}
              loading={positions.isLoading}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Active Employees"
              value={employees.data?.data.totalRows ?? '-'}
              prefix={<TeamOutlined />}
              loading={employees.isLoading}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Document Types"
              value={docTypes.data?.data.length ?? '-'}
              prefix={<FileTextOutlined />}
              loading={docTypes.isLoading}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Workflow Templates"
              value={templates.data?.data.length ?? '-'}
              prefix={<BranchesOutlined />}
              loading={templates.isLoading}
            />
          </Card>
        </Col>
      </Row>
    </div>
  )
}
