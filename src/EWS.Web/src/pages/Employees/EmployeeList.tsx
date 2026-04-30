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
import { formatCodeName } from '../../utils/display'
import EmployeeDrawer from './EmployeeDrawer'

export default function EmployeeList() {
  const { token } = theme.useToken()
  const columnWidth = token.controlHeightLG
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState<string>('Active')
  const [deptCode, setDeptCode] = useState<string | undefined>()
  const [sectionCode, setSectionCode] = useState<string | undefined>()
  const [pagination, setPagination] = useState({ page: 1, pageSize: 20 })
  const [selectedCode, setSelectedCode] = useState<string | null>(null)

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['employees', search, status, deptCode, sectionCode, pagination],
    queryFn: () => settingsApi.employees.list({
      search: search || undefined,
      status: status || undefined,
      deptCode,
      sectionCode,
      ...pagination,
    }),
  })

  const departments = useQuery({
    queryKey: ['department-options'],
    queryFn: () => settingsApi.departments.list({ isActive: true }),
  })

  const sections = useQuery({
    queryKey: ['section-options', deptCode],
    queryFn: () => settingsApi.sections.list({ deptCode, isActive: true }),
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
          <Select
            showSearch
            allowClear
            placeholder="Department"
            loading={departments.isLoading}
            style={{ width: token.controlHeightLG * 6 }}
            value={deptCode}
            optionFilterProp="label"
            onChange={(value) => {
              setDeptCode(value)
              setSectionCode(undefined)
              setPagination((current) => ({ ...current, page: 1 }))
            }}
            options={(departments.data?.data ?? []).map((department) => ({
              value: department.deptCode,
              label: formatCodeName(department.deptCode, department.deptName),
            }))}
          />
          <Select
            showSearch
            allowClear
            placeholder="Section"
            loading={sections.isLoading}
            style={{ width: token.controlHeightLG * 6 }}
            value={sectionCode}
            optionFilterProp="label"
            onChange={(value) => {
              setSectionCode(value)
              setPagination((current) => ({ ...current, page: 1 }))
            }}
            options={(sections.data?.data ?? []).map((section) => ({
              value: section.sectCode,
              label: formatCodeName(section.sectCode, section.sectName),
            }))}
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
