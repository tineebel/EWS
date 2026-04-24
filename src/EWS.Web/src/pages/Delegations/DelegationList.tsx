import { useState } from 'react'
import { Table, Input, Space, Tag, Card, Button, Tooltip, Switch, Typography } from 'antd'
import { SearchOutlined, ReloadOutlined } from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { settingsApi } from '../../api/settings'
import type { Delegation } from '../../api/types'

export default function DelegationList() {
  const [positionCode, setPositionCode] = useState('')
  const [activeOnly, setActiveOnly] = useState(false)

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['delegations', positionCode, activeOnly],
    queryFn: () => settingsApi.delegations.list({ positionCode: positionCode || undefined, activeOnly }),
  })

  const columns = [
    {
      title: 'From Position', key: 'from',
      render: (_: unknown, r: Delegation) => (
        <div>
          <code>{r.fromPositionCode}</code>
          <br />
          <Typography.Text type="secondary" style={{ fontSize: 12 }}>{r.fromPositionName}</Typography.Text>
        </div>
      )
    },
    {
      title: 'To Position', key: 'to',
      render: (_: unknown, r: Delegation) => (
        <div>
          <code>{r.toPositionCode}</code>
          <br />
          <Typography.Text type="secondary" style={{ fontSize: 12 }}>{r.toPositionName}</Typography.Text>
        </div>
      )
    },
    { title: 'เริ่ม', dataIndex: 'startDate', key: 'startDate', width: 110, render: (v: string) => v.slice(0, 10) },
    { title: 'สิ้นสุด', dataIndex: 'endDate', key: 'endDate', width: 110, render: (v: string) => v.slice(0, 10) },
    { title: 'เหตุผล', dataIndex: 'reason', key: 'reason', ellipsis: true, render: (v?: string) => v ?? '-' },
    { title: 'Status', dataIndex: 'isCurrentlyActive', key: 'isCurrentlyActive', width: 100, render: (v: boolean) => <Tag color={v ? 'green' : 'default'}>{v ? 'Active' : 'Expired'}</Tag> },
    { title: 'สร้างเมื่อ', dataIndex: 'createdAt', key: 'createdAt', width: 140 },
  ]

  return (
    <Card>
      <Space style={{ marginBottom: 16 }} wrap>
        <Input
          placeholder="Position Code"
          prefix={<SearchOutlined />}
          style={{ width: 200 }}
          value={positionCode}
          onChange={e => setPositionCode(e.target.value)}
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
        dataSource={data?.data as Delegation[] | undefined}
        rowKey="delegationId"
        loading={isLoading}
        size="small"
        pagination={{ pageSize: 50, showTotal: (t) => `ทั้งหมด ${t} รายการ` }}
      />
    </Card>
  )
}
