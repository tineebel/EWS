import { Drawer, Descriptions, Tag, Table, Badge, Space, Typography } from 'antd'
import { useQuery } from '@tanstack/react-query'
import { settingsApi } from '../../api/settings'
import type { PositionOccupant } from '../../api/types'

interface Props {
  positionCode: string | null
  onClose: () => void
}

export default function PositionDrawer({ positionCode, onClose }: Props) {
  const { data, isLoading } = useQuery({
    queryKey: ['position-detail', positionCode],
    queryFn: () => settingsApi.positions.get(positionCode!),
    enabled: !!positionCode,
  })

  const pos = data?.data

  const columns = [
    {
      title: 'รหัสพนักงาน',
      dataIndex: 'employeeCode',
      key: 'employeeCode',
      width: 130,
      render: (v: string) => <code>{v}</code>,
    },
    {
      title: 'ชื่อ',
      key: 'name',
      ellipsis: true,
      render: (_: unknown, r: PositionOccupant) => (
        <span>{r.employeeName}{r.employeeNameEn ? <Typography.Text type="secondary"> ({r.employeeNameEn})</Typography.Text> : ''}</span>
      ),
    },
    { title: 'Email', dataIndex: 'email', key: 'email', ellipsis: true, render: (v?: string) => v ?? '-' },
    {
      title: 'เริ่มดำรง',
      dataIndex: 'startDate',
      key: 'startDate',
      width: 110,
      render: (v: string) => v.slice(0, 10),
    },
    {
      title: 'สิ้นสุด',
      key: 'endDate',
      width: 130,
      render: (_: unknown, r: PositionOccupant) => {
        if (r.isCurrent) return <Tag color="green">ปัจจุบัน</Tag>
        return r.endDate ? r.endDate.slice(0, 10) : '-'
      },
    },
    {
      title: '',
      dataIndex: 'isCurrent',
      key: 'isCurrent',
      width: 80,
      render: (v: boolean) => v ? <Badge status="processing" text="Active" /> : null,
    },
  ]

  return (
    <Drawer
      title={
        pos ? (
          <Space>
            <code>{pos.positionCode}</code>
            <span>{pos.positionName}</span>
            <Tag color={pos.isActive ? 'green' : 'red'}>{pos.isActive ? 'Active' : 'Inactive'}</Tag>
          </Space>
        ) : 'รายละเอียดตำแหน่ง'
      }
      open={!!positionCode}
      onClose={onClose}
      width={820}
      loading={isLoading}
    >
      {pos && (
        <>
          <Descriptions bordered size="small" column={2} style={{ marginBottom: 24 }}>
            <Descriptions.Item label="Grade">{pos.jobGrade}</Descriptions.Item>
            <Descriptions.Item label="Scope">{pos.wfScopeType}</Descriptions.Item>
            <Descriptions.Item label="Section" span={2}>{pos.sectionName}</Descriptions.Item>
            <Descriptions.Item label="Parent">
              {pos.parentPositionCode
                ? <><code>{pos.parentPositionCode}</code> {pos.parentPositionName && `— ${pos.parentPositionName}`}</>
                : '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Flags">
              <Space size={4}>
                {pos.isChiefLevel && <Tag color="blue">Chief</Tag>}
                {pos.secretaryPositionCode && <Tag color="purple">Sec: <code>{pos.secretaryPositionCode}</code></Tag>}
                {!pos.isChiefLevel && !pos.secretaryPositionCode && '-'}
              </Space>
            </Descriptions.Item>
          </Descriptions>

          <Typography.Title level={5} style={{ marginBottom: 12 }}>
            ประวัติผู้ดำรงตำแหน่ง ({pos.occupants.length} คน)
          </Typography.Title>
          <Table<PositionOccupant>
            columns={columns}
            dataSource={pos.occupants}
            rowKey="assignmentId"
            size="small"
            pagination={false}
            scroll={{ x: 650 }}
            rowClassName={(r) => r.isCurrent ? 'ant-table-row-selected' : ''}
          />
        </>
      )}
    </Drawer>
  )
}
