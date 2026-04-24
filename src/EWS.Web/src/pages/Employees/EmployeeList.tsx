import { useState } from 'react'
import { Table, Input, Select, Space, Tag, Card, Button, Tooltip, Avatar } from 'antd'
import { SearchOutlined, ReloadOutlined, UserOutlined } from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { settingsApi } from '../../api/settings'
import type { Employee } from '../../api/types'
import type { TablePaginationConfig } from 'antd'
import EmployeeDrawer from './EmployeeDrawer'

export default function EmployeeList() {
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState<string>('Active')
  const [pagination, setPagination] = useState({ page: 1, pageSize: 20 })
  const [selectedCode, setSelectedCode] = useState<string | null>(null)

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['employees', search, status, pagination],
    queryFn: () => settingsApi.employees.list({ search: search || undefined, status: status || undefined, ...pagination }),
  })

  const columns = [
    { title: 'รหัส', dataIndex: 'employeeCode', key: 'employeeCode', width: 110 },
    {
      title: 'ชื่อ', key: 'name', ellipsis: true,
      render: (_: unknown, r: Employee) => (
        <Space>
          <Avatar size="small" icon={<UserOutlined />} />
          <span>{r.employeeName}{r.nickname ? ` (${r.nickname})` : ''}</span>
        </Space>
      )
    },
    { title: 'Email', dataIndex: 'email', key: 'email', ellipsis: true, render: (v?: string) => v ?? '-' },
    { title: 'Tel', dataIndex: 'tel', key: 'tel', width: 130, render: (v?: string) => v ?? '-' },
    {
      title: 'ตำแหน่ง', key: 'positions', width: 200,
      render: (_: unknown, r: Employee) => (
        <Space size={4} wrap>
          {r.positionCodes.length === 0 ? <Tag color="orange">ไม่มีตำแหน่ง</Tag> : r.positionCodes.map(c => <Tag key={c}><code>{c}</code></Tag>)}
        </Space>
      )
    },
    {
      title: 'Status', dataIndex: 'status', key: 'status', width: 110,
      render: (v: string) => <Tag color={v === 'Active' ? 'green' : 'red'}>{v === 'Active' ? 'Active' : 'ลาออกแล้ว'}</Tag>
    },
    { title: 'เริ่มงาน', dataIndex: 'startDate', key: 'startDate', width: 110, render: (v: string) => v.slice(0, 10) },
    {
      title: 'Test', dataIndex: 'isTest', key: 'isTest', width: 60,
      render: (v: boolean) => v ? <Tag color="orange">Test</Tag> : null
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
          placeholder="ค้นหา รหัส / ชื่อ / Email"
          prefix={<SearchOutlined />}
          style={{ width: 280 }}
          value={search}
          onChange={e => { setSearch(e.target.value); setPagination(p => ({ ...p, page: 1 })) }}
          allowClear
        />
        <Select
          style={{ width: 140 }}
          value={status}
          onChange={v => setStatus(v)}
          options={[
            { value: '', label: 'ทุกสถานะ' },
            { value: 'Active', label: 'Active (ปัจจุบัน)' },
            { value: 'Resigned', label: 'ลาออกแล้ว' },
          ]}
        />
        <Tooltip title="Refresh">
          <Button icon={<ReloadOutlined />} onClick={() => refetch()} />
        </Tooltip>
      </Space>
      <Table
        columns={columns}
        dataSource={data?.data.items}
        rowKey="employeeId"
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
        onRow={(r) => ({ onClick: () => setSelectedCode(r.employeeCode), style: { cursor: 'pointer' } })}
      />
    </Card>
    <EmployeeDrawer employeeCode={selectedCode} onClose={() => setSelectedCode(null)} />
    </>
  )
}
