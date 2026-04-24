import { useState } from 'react'
import { Table, Input, Select, Space, Tag, Card, Button, Tooltip } from 'antd'
import { SearchOutlined, ReloadOutlined } from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { settingsApi } from '../../api/settings'
import type { Position } from '../../api/types'
import type { TablePaginationConfig } from 'antd'
import PositionDrawer from './PositionDrawer'

export default function PositionList() {
  const [search, setSearch] = useState('')
  const [isActive, setIsActive] = useState<boolean | undefined>(true)
  const [pagination, setPagination] = useState({ page: 1, pageSize: 20 })
  const [selectedCode, setSelectedCode] = useState<string | null>(null)

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['positions', search, isActive, pagination],
    queryFn: () => settingsApi.positions.list({ search: search || undefined, isActive, ...pagination }),
  })

  const columns = [
    { title: 'Code', dataIndex: 'positionCode', key: 'positionCode', width: 130, render: (v: string) => <code>{v}</code> },
    { title: 'ชื่อตำแหน่ง', dataIndex: 'positionName', key: 'positionName', ellipsis: true },
    { title: 'Grade', dataIndex: 'jobGrade', key: 'jobGrade', width: 80 },
    { title: 'Scope', dataIndex: 'wfScopeType', key: 'wfScopeType', width: 80 },
    { title: 'Section', dataIndex: 'sectionName', key: 'sectionName', ellipsis: true },
    { title: 'Parent', dataIndex: 'parentPositionCode', key: 'parentPositionCode', width: 130, render: (v?: string) => v ? <code>{v}</code> : '-' },
    {
      title: 'Flags', key: 'flags', width: 120,
      render: (_: unknown, r: Position) => (
        <Space size={4}>
          {r.isChiefLevel && <Tag color="blue">Chief</Tag>}
          {r.secretaryPositionCode && <Tag color="purple">Sec</Tag>}
        </Space>
      )
    },
    {
      title: 'Status', dataIndex: 'isActive', key: 'isActive', width: 90,
      render: (v: boolean) => <Tag color={v ? 'green' : 'red'}>{v ? 'Active' : 'Inactive'}</Tag>
    },
  ]

  const handleTableChange = (p: TablePaginationConfig) => {
    setPagination({ page: p.current ?? 1, pageSize: p.pageSize ?? 20 })
  }

  return (
    <>
    <Card>
      <Space style={{ marginBottom: 16 }} wrap>
        <Input
          placeholder="ค้นหา Position Code / ชื่อตำแหน่ง"
          prefix={<SearchOutlined />}
          style={{ width: 280 }}
          value={search}
          onChange={e => { setSearch(e.target.value); setPagination(p => ({ ...p, page: 1 })) }}
          allowClear
        />
        <Select
          style={{ width: 140 }}
          value={isActive === undefined ? 'all' : isActive ? 'active' : 'inactive'}
          onChange={v => setIsActive(v === 'all' ? undefined : v === 'active')}
          options={[
            { value: 'all', label: 'ทั้งหมด' },
            { value: 'active', label: 'Active' },
            { value: 'inactive', label: 'Inactive' },
          ]}
        />
        <Tooltip title="Refresh">
          <Button icon={<ReloadOutlined />} onClick={() => refetch()} />
        </Tooltip>
      </Space>
      <Table
        columns={columns}
        dataSource={data?.data.items}
        rowKey="positionId"
        loading={isLoading}
        size="small"
        pagination={{
          current: pagination.page,
          pageSize: pagination.pageSize,
          total: data?.data.totalRows,
          showSizeChanger: true,
          showTotal: (t) => `ทั้งหมด ${t} รายการ`,
          pageSizeOptions: ['20', '50', '100'],
        }}
        onChange={handleTableChange}
        scroll={{ x: 900 }}
        onRow={(r) => ({ onClick: () => setSelectedCode(r.positionCode), style: { cursor: 'pointer' } })}
      />
    </Card>
    <PositionDrawer positionCode={selectedCode} onClose={() => setSelectedCode(null)} />
    </>
  )
}
