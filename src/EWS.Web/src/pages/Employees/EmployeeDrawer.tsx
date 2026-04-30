import { Avatar, Descriptions, Drawer, Space, Table, Tag, Typography, theme } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { UserOutlined } from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { settingsApi } from '../../api/settings'
import type { PositionAssignmentDetail } from '../../api/types'

interface Props {
  employeeCode: string | null
  onClose: () => void
}

export default function EmployeeDrawer({ employeeCode, onClose }: Props) {
  const { token } = theme.useToken()
  const columnWidth = token.controlHeightLG
  const { data, isLoading } = useQuery({
    queryKey: ['employee-detail', employeeCode],
    queryFn: () => settingsApi.employees.get(employeeCode!),
    enabled: !!employeeCode,
  })

  const employee = data?.data

  const columns: ColumnsType<PositionAssignmentDetail> = [
    {
      title: 'Position Code',
      dataIndex: 'positionCode',
      key: 'positionCode',
      width: columnWidth * 3.5,
      render: (value: string) => <code>{value}</code>,
    },
    { title: 'Position Name', dataIndex: 'positionName', key: 'positionName', width: columnWidth * 6, ellipsis: true },
    { title: 'Grade', dataIndex: 'jobGrade', key: 'jobGrade', width: columnWidth * 2 },
    { title: 'Section', dataIndex: 'sectionName', key: 'sectionName', width: columnWidth * 5.5, ellipsis: true },
    {
      title: 'Start Date',
      dataIndex: 'startDate',
      key: 'startDate',
      width: columnWidth * 3,
      render: (value: string) => value.slice(0, 10),
    },
    {
      title: 'End Date',
      key: 'endDate',
      width: columnWidth * 3,
      render: (_: unknown, record) => record.isCurrent
        ? <Tag color="green">Current</Tag>
        : record.endDate?.slice(0, 10) ?? '-',
    },
  ]

  return (
    <Drawer
      title={
        employee ? (
          <Space wrap>
            <Avatar icon={<UserOutlined />} />
            <span>{employee.employeeName}{employee.nickname ? ` (${employee.nickname})` : ''}</span>
            <Tag color={employee.status === 'Active' ? 'green' : 'red'}>
              {employee.status === 'Active' ? 'Active' : 'Resigned'}
            </Tag>
          </Space>
        ) : 'Employee Details'
      }
      open={!!employeeCode}
      onClose={onClose}
      width={columnWidth * 21.5}
      loading={isLoading}
    >
      {employee && (
        <>
          <Descriptions
            bordered
            size="small"
            column={2}
            style={{ marginBottom: token.marginLG }}
          >
            <Descriptions.Item label="Employee Code">{employee.employeeCode}</Descriptions.Item>
            <Descriptions.Item label="Name (EN)">{employee.employeeNameEn ?? '-'}</Descriptions.Item>
            <Descriptions.Item label="Email">{employee.email ?? '-'}</Descriptions.Item>
            <Descriptions.Item label="Tel">{employee.tel ?? '-'}</Descriptions.Item>
            <Descriptions.Item label="Start Date">{employee.startDate.slice(0, 10)}</Descriptions.Item>
            <Descriptions.Item label="End Date">{employee.endDate ? employee.endDate.slice(0, 10) : '-'}</Descriptions.Item>
          </Descriptions>

          <Typography.Title level={5} style={{ marginBottom: token.marginSM }}>
            Position History ({employee.positionAssignments.length})
          </Typography.Title>
          <Table
            columns={columns}
            dataSource={employee.positionAssignments}
            rowKey="assignmentId"
            size="small"
            pagination={false}
            scroll={{ x: true }}
            rowClassName={(record) => record.isCurrent ? 'ant-table-row-selected' : ''}
          />
        </>
      )}
    </Drawer>
  )
}
