import { useState } from 'react'
import {
  Avatar,
  Button,
  Card,
  Input,
  Select,
  Space,
  Table,
  Tag,
  Tooltip,
  theme,
} from 'antd'
import type { TablePaginationConfig } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { ReloadOutlined, SearchOutlined, UserOutlined } from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { settingsApi } from '../../api/settings'
import type { Employee } from '../../api/types'
import EmployeeDrawer from './EmployeeDrawer'

export default function EmployeeList() {
  const { token } = theme.useToken()
  const columnWidth = token.controlHeightLG
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState<string>('Active')
  const [pagination, setPagination] = useState({ page: 1, pageSize: 20 })
  const [selectedCode, setSelectedCode] = useState<string | null>(null)

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['employees', search, status, pagination],
    queryFn: () => settingsApi.employees.list({
      search: search || undefined,
      status: status || undefined,
      ...pagination,
    }),
  })

  const columns: ColumnsType<Employee> = [
    { title: 'Code', dataIndex: 'employeeCode', key: 'employeeCode', width: columnWidth * 3 },
    {
      title: 'Name',
      key: 'name',
      width: columnWidth * 6.5,
      ellipsis: true,
      render: (_: unknown, record) => (
        <Space size={token.marginXS}>
          <Avatar size="small" icon={<UserOutlined />} />
          <span>{record.employeeName}{record.nickname ? ` (${record.nickname})` : ''}</span>
        </Space>
      ),
    },
    { title: 'Email', dataIndex: 'email', key: 'email', width: columnWidth * 5.5, ellipsis: true, render: (value?: string) => value ?? '-' },
    { title: 'Tel', dataIndex: 'tel', key: 'tel', width: columnWidth * 3.25, render: (value?: string) => value ?? '-' },
    {
      title: 'Positions',
      key: 'positions',
      width: columnWidth * 5.5,
      render: (_: unknown, record) => (
        <Space size={token.marginXXS} wrap>
          {record.positionCodes.length === 0
            ? <Tag color="orange">No position</Tag>
            : record.positionCodes.map((code) => <Tag key={code}><code>{code}</code></Tag>)}
        </Space>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: columnWidth * 2.75,
      render: (value: string) => (
        <Tag color={value === 'Active' ? 'green' : 'red'}>
          {value === 'Active' ? 'Active' : 'Resigned'}
        </Tag>
      ),
    },
    {
      title: 'Start Date',
      dataIndex: 'startDate',
      key: 'startDate',
      width: columnWidth * 3,
      render: (value: string) => value.slice(0, 10),
    },
    {
      title: 'Test',
      dataIndex: 'isTest',
      key: 'isTest',
      width: columnWidth * 1.75,
      render: (value: boolean) => value ? <Tag color="orange">Test</Tag> : null,
    },
  ]

  const handleTableChange = (nextPagination: TablePaginationConfig) => {
    setPagination({
      page: nextPagination.current ?? 1,
      pageSize: nextPagination.pageSize ?? 20,
    })
  }

  return (
    <>
      <Card styles={{ body: { padding: token.paddingLG } }}>
        <style>
          {`
            .employees-table .ant-table-thead > tr > th,
            .employees-table .ant-table-tbody > tr > td {
              padding-block: ${token.paddingXXS}px;
            }
          `}
        </style>
        <Space style={{ marginBottom: token.marginMD }} wrap>
          <Input
            placeholder="Search code, name, or email"
            prefix={<SearchOutlined />}
            style={{ width: token.controlHeightLG * 8 }}
            value={search}
            onChange={(event) => {
              setSearch(event.target.value)
              setPagination((current) => ({ ...current, page: 1 }))
            }}
            allowClear
          />
          <Select
            style={{ width: token.controlHeightLG * 4 }}
            value={status}
            onChange={(value) => {
              setStatus(value)
              setPagination((current) => ({ ...current, page: 1 }))
            }}
            options={[
              { value: '', label: 'All statuses' },
              { value: 'Active', label: 'Active' },
              { value: 'Resigned', label: 'Resigned' },
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
          className="employees-table"
          pagination={{
            current: pagination.page,
            pageSize: pagination.pageSize,
            total: data?.data.totalRows,
            showSizeChanger: true,
            showTotal: (total) => `Total ${total} employees`,
            pageSizeOptions: ['20', '50', '100'],
          }}
          onChange={handleTableChange}
          scroll={{ x: true }}
          onRow={(record) => ({
            onClick: () => setSelectedCode(record.employeeCode),
            style: { cursor: 'pointer' },
          })}
        />
      </Card>
      <EmployeeDrawer employeeCode={selectedCode} onClose={() => setSelectedCode(null)} />
    </>
  )
}
