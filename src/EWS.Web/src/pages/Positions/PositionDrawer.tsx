import { Descriptions, Drawer, Space, Table, Tag, Typography, theme } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { useQuery } from '@tanstack/react-query'
import { settingsApi } from '../../api/settings'
import type { PositionOccupant } from '../../api/types'
import { displayWfScope, formatCodeName } from '../../utils/display'

interface Props {
  positionCode: string | null
  onClose: () => void
}

export default function PositionDrawer({ positionCode, onClose }: Props) {
  const { token } = theme.useToken()
  const columnWidth = token.controlHeightLG
  const { data, isLoading } = useQuery({
    queryKey: ['position-detail', positionCode],
    queryFn: () => settingsApi.positions.get(positionCode!),
    enabled: !!positionCode,
  })

  const position = data?.data

  const columns: ColumnsType<PositionOccupant> = [
    {
      title: 'Employee Code',
      dataIndex: 'employeeCode',
      key: 'employeeCode',
      width: columnWidth * 3.2,
      render: (value: string) => <code>{value}</code>,
    },
    {
      title: 'Name',
      key: 'name',
      width: columnWidth * 5.5,
      ellipsis: true,
      render: (_: unknown, record) => (
        <Space direction="vertical" size={0}>
          <Typography.Text>{record.employeeName}</Typography.Text>
          {record.employeeNameEn && (
            <Typography.Text type="secondary" style={{ fontSize: token.fontSizeSM }}>
              {record.employeeNameEn}
            </Typography.Text>
          )}
        </Space>
      ),
    },
    {
      title: 'Email',
      dataIndex: 'email',
      key: 'email',
      width: columnWidth * 4.75,
      ellipsis: true,
      render: (value?: string) => value ?? '-',
    },
    {
      title: 'Start Date',
      dataIndex: 'startDate',
      key: 'startDate',
      width: columnWidth * 2.8,
      render: (value: string) => value.slice(0, 10),
    },
    {
      title: 'End Date',
      key: 'endDate',
      width: columnWidth * 2.75,
      render: (_: unknown, record) => record.isCurrent
        ? <Tag color="green">Current</Tag>
        : record.endDate?.slice(0, 10) ?? '-',
    },
  ]

  return (
    <Drawer
      title={
        position ? (
          <Space wrap>
            <code>{position.positionCode}</code>
            <span>{position.positionName}</span>
            <Tag color={position.isActive ? 'green' : 'red'}>
              {position.isActive ? 'Active' : 'Inactive'}
            </Tag>
          </Space>
        ) : 'Position Details'
      }
      open={!!positionCode}
      onClose={onClose}
      width={columnWidth * 21.5}
      loading={isLoading}
    >
      {position && (
        <>
          <Descriptions
            bordered
            size="small"
            column={2}
            style={{ marginBottom: token.marginLG }}
          >
            <Descriptions.Item label="Grade">{position.jobGrade}</Descriptions.Item>
            <Descriptions.Item label="Scope">{displayWfScope(position.wfScopeType)}</Descriptions.Item>
            <Descriptions.Item label="Department" span={2}>{formatCodeName(position.deptCode, position.deptName)}</Descriptions.Item>
            <Descriptions.Item label="Section" span={2}>{formatCodeName(position.sectionCode, position.sectionName)}</Descriptions.Item>
            <Descriptions.Item label="Parent">
              {position.parentPositionCode ? (
                <Space size={token.marginXXS}>
                  <code>{position.parentPositionCode}</code>
                  {position.parentPositionName && (
                    <Typography.Text type="secondary">
                      {position.parentPositionName}
                    </Typography.Text>
                  )}
                </Space>
              ) : '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Flags">
              <Space size={token.marginXXS} wrap>
                {position.isChiefLevel && <Tag color="blue">Chief</Tag>}
                {position.secretaryPositionCode && (
                  <Tag color="purple">
                    Sec: <code>{position.secretaryPositionCode}</code>
                  </Tag>
                )}
                {!position.isChiefLevel && !position.secretaryPositionCode && '-'}
              </Space>
            </Descriptions.Item>
          </Descriptions>

          <Typography.Title level={5} style={{ marginBottom: token.marginSM }}>
            Occupant History ({position.occupants.length})
          </Typography.Title>
          <Table
            columns={columns}
            dataSource={position.occupants}
            rowKey="assignmentId"
            size="small"
            pagination={false}
            rowClassName={(record) => record.isCurrent ? 'ant-table-row-selected' : ''}
          />
        </>
      )}
    </Drawer>
  )
}
