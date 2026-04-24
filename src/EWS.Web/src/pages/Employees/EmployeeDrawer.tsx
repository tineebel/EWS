import { Drawer, Descriptions, Tag, Table, Badge, Space, Avatar, Typography } from 'antd'
import { UserOutlined } from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { settingsApi } from '../../api/settings'
import type { PositionAssignmentDetail } from '../../api/types'

interface Props {
  employeeCode: string | null
  onClose: () => void
}

export default function EmployeeDrawer({ employeeCode, onClose }: Props) {
  const { data, isLoading } = useQuery({
    queryKey: ['employee-detail', employeeCode],
    queryFn: () => settingsApi.employees.get(employeeCode!),
    enabled: !!employeeCode,
  })

  const emp = data?.data

  const columns = [
    {
      title: 'รหัสตำแหน่ง',
      dataIndex: 'positionCode',
      key: 'positionCode',
      width: 130,
      render: (v: string) => <code>{v}</code>,
    },
    { title: 'ชื่อตำแหน่ง', dataIndex: 'positionName', key: 'positionName', ellipsis: true },
    { title: 'Grade', dataIndex: 'jobGrade', key: 'jobGrade', width: 80 },
    { title: 'แผนก/สาขา', dataIndex: 'sectionName', key: 'sectionName', ellipsis: true },
    {
      title: 'เริ่ม',
      dataIndex: 'startDate',
      key: 'startDate',
      width: 110,
      render: (v: string) => v.slice(0, 10),
    },
    {
      title: 'สิ้นสุด',
      key: 'endDate',
      width: 130,
      render: (_: unknown, r: PositionAssignmentDetail) => {
        if (r.isCurrent) return <Tag color="green">ปัจจุบัน</Tag>
        return r.endDate ? r.endDate.slice(0, 10) : '-'
      },
    },
    {
      title: '',
      dataIndex: 'isCurrent',
      key: 'isCurrent',
      width: 70,
      render: (v: boolean) => v ? <Badge status="processing" text="Active" /> : null,
    },
  ]

  return (
    <Drawer
      title={
        emp ? (
          <Space>
            <Avatar icon={<UserOutlined />} />
            <span>{emp.employeeName}{emp.nickname ? ` (${emp.nickname})` : ''}</span>
            <Tag color={emp.status === 'Active' ? 'green' : 'red'}>{emp.status === 'Active' ? 'Active' : 'ลาออกแล้ว'}</Tag>
          </Space>
        ) : 'รายละเอียดพนักงาน'
      }
      open={!!employeeCode}
      onClose={onClose}
      width={860}
      loading={isLoading}
    >
      {emp && (
        <>
          <Descriptions bordered size="small" column={2} style={{ marginBottom: 24 }}>
            <Descriptions.Item label="รหัสพนักงาน">{emp.employeeCode}</Descriptions.Item>
            <Descriptions.Item label="ชื่อ (EN)">{emp.employeeNameEn ?? '-'}</Descriptions.Item>
            <Descriptions.Item label="Email">{emp.email ?? '-'}</Descriptions.Item>
            <Descriptions.Item label="โทรศัพท์">{emp.tel ?? '-'}</Descriptions.Item>
            <Descriptions.Item label="วันเริ่มงาน">{emp.startDate.slice(0, 10)}</Descriptions.Item>
            <Descriptions.Item label="วันสิ้นสุด">{emp.endDate ? emp.endDate.slice(0, 10) : '-'}</Descriptions.Item>
          </Descriptions>

          <Typography.Title level={5} style={{ marginBottom: 12 }}>
            ประวัติตำแหน่ง ({emp.positionAssignments.length} รายการ)
          </Typography.Title>
          <Table<PositionAssignmentDetail>
            columns={columns}
            dataSource={emp.positionAssignments}
            rowKey="assignmentId"
            size="small"
            pagination={false}
            scroll={{ x: 700 }}
            rowClassName={(r) => r.isCurrent ? 'ant-table-row-selected' : ''}
          />
        </>
      )}
    </Drawer>
  )
}
