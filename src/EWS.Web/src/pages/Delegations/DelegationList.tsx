import { useState } from 'react'
import { Button, Card, Input, Modal, Space, Switch, Table, Tag, Tooltip, Typography, message, theme } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { DeleteOutlined, EditOutlined, PlusOutlined, ReloadOutlined, SearchOutlined } from '@ant-design/icons'
import { useMutation, useQuery } from '@tanstack/react-query'
import { settingsApi } from '../../api/settings'
import type { Delegation } from '../../api/types'
import DelegationFormDrawer from './DelegationFormDrawer'

export default function DelegationList() {
  const { token } = theme.useToken()
  const columnWidth = token.controlHeightLG
  const [positionCode, setPositionCode] = useState('')
  const [activeOnly, setActiveOnly] = useState(false)
  const [editingDelegationId, setEditingDelegationId] = useState<number | null>(null)
  const [drawerOpen, setDrawerOpen] = useState(false)

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['delegations', positionCode, activeOnly],
    queryFn: () => settingsApi.delegations.list({ positionCode: positionCode || undefined, activeOnly }),
  })

  const deleteMutation = useMutation({
    mutationFn: settingsApi.delegations.remove,
    onSuccess: () => {
      message.success('Delegation deleted')
      void refetch()
    },
    onError: (error: unknown) => {
      const errorMessage = error instanceof Error ? error.message : 'Unable to delete delegation'
      message.error(errorMessage)
    },
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
    {
      title: 'Status',
      key: 'status',
      width: columnWidth * 3,
      render: (_: unknown, record) => (
        <Space size={token.marginXXS} wrap>
          <Tag color={record.isActive ? 'blue' : 'default'}>{record.isActive ? 'Enabled' : 'Disabled'}</Tag>
          <Tag color={record.isCurrentlyActive ? 'green' : 'default'}>{record.isCurrentlyActive ? 'Current' : 'Inactive'}</Tag>
        </Space>
      ),
    },
    { title: 'Created At', dataIndex: 'createdAt', key: 'createdAt', width: columnWidth * 4 },
    {
      title: 'Action',
      key: 'action',
      width: columnWidth * 2.75,
      render: (_: unknown, record) => (
        <Space size={token.marginXXS}>
          <Tooltip title="Edit">
            <Button
              icon={<EditOutlined />}
              size="small"
              onClick={() => {
                setEditingDelegationId(record.delegationId)
                setDrawerOpen(true)
              }}
            />
          </Tooltip>
          <Tooltip title="Delete">
            <Button
              danger
              icon={<DeleteOutlined />}
              size="small"
              onClick={() => {
                Modal.confirm({
                  title: 'Delete delegation',
                  content: `Delete delegation from ${record.fromPositionCode} to ${record.toPositionCode}?`,
                  okText: 'Delete',
                  okButtonProps: { danger: true, loading: deleteMutation.isPending },
                  onOk: async () => {
                    await deleteMutation.mutateAsync(record.delegationId)
                  },
                })
              }}
            />
          </Tooltip>
        </Space>
      ),
    },
  ]

  return (
    <>
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
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => {
              setEditingDelegationId(null)
              setDrawerOpen(true)
            }}
          >
            Add Delegation
          </Button>
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

      <DelegationFormDrawer
        delegationId={editingDelegationId}
        open={drawerOpen}
        onClose={() => {
          setDrawerOpen(false)
          setEditingDelegationId(null)
        }}
        onSaved={() => {
          void refetch()
        }}
      />
    </>
  )
}
