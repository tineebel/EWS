import { useState } from 'react'
import { Button, Card, Input, Space, Switch, Table, Tag, Tooltip, Typography, theme } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { ReloadOutlined, SearchOutlined } from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { settingsApi } from '../../api/settings'
import type { Delegation } from '../../api/types'

export default function DelegationList() {
  const { token } = theme.useToken()
  const columnWidth = token.controlHeightLG
  const [positionCode, setPositionCode] = useState('')
  const [activeOnly, setActiveOnly] = useState(false)

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['delegations', positionCode, activeOnly],
    queryFn: () => settingsApi.delegations.list({ positionCode: positionCode || undefined, activeOnly }),
  })

  const delegations = data?.data ?? []

  const columns: ColumnsType<Delegation> = [
    {
      title: 'From Position',
      key: 'from',
      width: columnWidth * 6.5,
      render: (_: unknown, record) => (
        <Space size={token.marginXXS}>
          <code>{record.fromPositionCode}</code>
          <Typography.Text type="secondary" ellipsis style={{ maxWidth: token.controlHeightLG * 4 }}>
            {record.fromPositionName}
          </Typography.Text>
        </Space>
      ),
    },
    {
      title: 'To Position',
      key: 'to',
      width: columnWidth * 6.5,
      render: (_: unknown, record) => (
        <Space size={token.marginXXS}>
          <code>{record.toPositionCode}</code>
          <Typography.Text type="secondary" ellipsis style={{ maxWidth: token.controlHeightLG * 4 }}>
            {record.toPositionName}
          </Typography.Text>
        </Space>
      ),
    },
    { title: 'Start Date', dataIndex: 'startDate', key: 'startDate', width: columnWidth * 3, render: (value: string) => value.slice(0, 10) },
    { title: 'End Date', dataIndex: 'endDate', key: 'endDate', width: columnWidth * 3, render: (value: string) => value.slice(0, 10) },
    { title: 'Reason', dataIndex: 'reason', key: 'reason', width: columnWidth * 6.5, ellipsis: true, render: (value?: string) => value ?? '-' },
    { title: 'Status', dataIndex: 'isCurrentlyActive', key: 'isCurrentlyActive', width: columnWidth * 2.75, render: (value: boolean) => <Tag color={value ? 'green' : 'default'}>{value ? 'Active' : 'Expired'}</Tag> },
    { title: 'Created At', dataIndex: 'createdAt', key: 'createdAt', width: columnWidth * 4 },
  ]

  return (
    <Card styles={{ body: { padding: token.paddingLG } }}>
      <style>
        {`
          .delegations-table .ant-table-thead > tr > th,
          .delegations-table .ant-table-tbody > tr > td {
            padding-block: ${token.paddingXXS}px;
          }
        `}
      </style>
      <Space style={{ marginBottom: token.marginMD }} wrap>
        <Input
          placeholder="Position Code"
          prefix={<SearchOutlined />}
          style={{ width: token.controlHeightLG * 6 }}
          value={positionCode}
          onChange={(event) => setPositionCode(event.target.value)}
          allowClear
        />
        <Space>
          <Switch checked={activeOnly} onChange={setActiveOnly} />
          <span>Active Only</span>
        </Space>
        <Tooltip title="Refresh">
          <Button icon={<ReloadOutlined />} onClick={() => refetch()} />
        </Tooltip>
      </Space>
      <Table
        columns={columns}
        dataSource={delegations}
        rowKey="delegationId"
        loading={isLoading}
        size="small"
        className="delegations-table"
        pagination={{ pageSize: 50, showTotal: (total) => `Total ${total} delegations` }}
        scroll={{ x: true }}
      />
    </Card>
  )
}
