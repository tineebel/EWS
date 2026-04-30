import { useState } from 'react'
import {
  Button,
  Card,
  Input,
  Select,
  Space,
  Table,
  Tag,
  Tooltip,
  Typography,
  theme,
} from 'antd'
import type { TablePaginationConfig } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { EyeOutlined, ReloadOutlined, SearchOutlined } from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { settingsApi } from '../../api/settings'
import type { Position } from '../../api/types'
import { displayWfScope, formatShortCodeName } from '../../utils/display'
import PositionDrawer from './PositionDrawer'

const scopeColors: Record<string, string> = { Branch: 'blue', Ho: 'orange', All: 'green' }

export default function PositionList() {
  const { token } = theme.useToken()
  const columnWidth = token.controlHeightLG
  const [search, setSearch] = useState('')
  const [isActive, setIsActive] = useState<boolean | undefined>(true)
  const [branchCode, setBranchCode] = useState('')
  const [deptCode, setDeptCode] = useState<string | undefined>()
  const [sectionCode, setSectionCode] = useState<string | undefined>()
  const [pagination, setPagination] = useState({ page: 1, pageSize: 20 })
  const [selectedCode, setSelectedCode] = useState<string | null>(null)

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['positions', search, isActive, branchCode, deptCode, sectionCode, pagination],
    queryFn: () => settingsApi.positions.list({
      search: search || undefined,
      isActive,
      branchCode: branchCode || undefined,
      deptCode,
      sectionCode,
      ...pagination,
    }),
  })

  const departments = useQuery({
    queryKey: ['department-options'],
    queryFn: () => settingsApi.departments.list({ isActive: true }),
  })

  const branchOptions = useQuery({
    queryKey: ['branch-options'],
    queryFn: () => settingsApi.branchOptions.list(),
  })

  const sections = useQuery({
    queryKey: ['section-options', deptCode],
    queryFn: () => settingsApi.sections.list({ deptCode, isActive: true }),
  })

  const positions = data?.data.items ?? []

  const columns: ColumnsType<Position> = [
    {
      title: 'Code',
      dataIndex: 'positionCode',
      key: 'positionCode',
      width: columnWidth * 3.5,
      fixed: 'left',
      render: (value: string) => <code>{value}</code>,
    },
    {
      title: 'Short Code',
      dataIndex: 'positionShortName',
      key: 'positionShortName',
      width: columnWidth * 3.5,
      render: (value?: string) => value ? <Typography.Text type="secondary">{value}</Typography.Text> : '-',
    },
    {
      title: 'Position Name',
      dataIndex: 'positionName',
      key: 'positionName',
      width: columnWidth * 6.5,
      ellipsis: true,
      render: (value: string) => <Typography.Text ellipsis>{value}</Typography.Text>,
    },
    { title: 'Grade', dataIndex: 'jobGrade', key: 'jobGrade', width: columnWidth * 2.25 },
    {
      title: 'Scope',
      dataIndex: 'wfScopeType',
      key: 'wfScopeType',
      width: columnWidth * 2.5,
      render: (value: string) => <Tag color={scopeColors[value] ?? 'default'}>{displayWfScope(value)}</Tag>,
    },
    { title: 'Section', key: 'section', width: columnWidth * 6.5, ellipsis: true, render: (_: unknown, record) => formatShortCodeName(record.sectionShortCode, record.sectionCode, record.sectionName) },
    {
      title: 'Parent',
      dataIndex: 'parentPositionCode',
      key: 'parentPositionCode',
      width: columnWidth * 3.75,
      render: (value?: string) => value ? <code>{value}</code> : '-',
    },
    {
      title: 'Flags',
      key: 'flags',
      width: columnWidth * 3.5,
      render: (_: unknown, record) => (
        <Space size={token.marginXXS}>
          {record.isChiefLevel && <Tag color="blue">Chief</Tag>}
          {record.secretaryPositionCode && <Tag color="purple">Sec</Tag>}
          {!record.isChiefLevel && !record.secretaryPositionCode && (
            <Typography.Text type="secondary">-</Typography.Text>
          )}
        </Space>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      width: columnWidth * 2.5,
      render: (value: boolean) => <Tag color={value ? 'green' : 'red'}>{value ? 'Active' : 'Inactive'}</Tag>,
    },
    {
      title: 'Action',
      key: 'action',
      width: columnWidth * 2.25,
      fixed: 'right',
      render: (_: unknown, record) => (
        <Tooltip title="View details">
          <Button
            icon={<EyeOutlined />}
            size="small"
            type="text"
            onClick={(event) => {
              event.stopPropagation()
              setSelectedCode(record.positionCode)
            }}
          />
        </Tooltip>
      ),
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
            .positions-table .ant-table-thead > tr > th {
              padding-block: ${token.paddingXXS}px;
            }

            .positions-table .ant-table-tbody > tr > td {
              padding-block: ${token.paddingXXS}px;
            }

            .positions-table .ant-space-vertical {
              gap: 0 !important;
            }
          `}
        </style>
        <Space style={{ marginBottom: token.marginMD }} wrap>
          <Input
            placeholder="Search position code or name"
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
            showSearch
            loading={branchOptions.isLoading}
            optionFilterProp="label"
            style={{ width: token.controlHeightLG * 6 }}
            value={branchCode}
            onChange={(value) => {
              setBranchCode(value)
              setPagination((current) => ({ ...current, page: 1 }))
            }}
            options={[
              { value: '', label: 'All' },
              { value: 'HO', label: 'HO' },
              ...(branchOptions.data?.data ?? []).map((branch) => ({
                value: branch.branchCode,
                label: formatShortCodeName(branch.branchShortCode, branch.branchCode, branch.branchName),
              })),
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
              label: formatShortCodeName(department.deptShortCode, department.deptCode, department.deptName),
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
              label: formatShortCodeName(section.sectShortCode, section.sectCode, section.sectName),
            }))}
          />
          <Select
            style={{ width: token.controlHeightLG * 4 }}
            value={isActive === undefined ? 'all' : isActive ? 'active' : 'inactive'}
            onChange={(value) => {
              setIsActive(value === 'all' ? undefined : value === 'active')
              setPagination((current) => ({ ...current, page: 1 }))
            }}
            options={[
              { value: 'all', label: 'All' },
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
          dataSource={positions}
          rowKey="positionId"
          loading={isLoading}
          size="small"
          pagination={{
            current: pagination.page,
            pageSize: pagination.pageSize,
            total: data?.data.totalRows,
            showSizeChanger: true,
            showTotal: (total) => `Total ${total} positions`,
            pageSizeOptions: ['20', '50', '100'],
          }}
          onChange={handleTableChange}
          className="positions-table"
          scroll={{ x: columnWidth * 36 }}
          onRow={(record) => ({
            onClick: () => setSelectedCode(record.positionCode),
            style: { cursor: 'pointer' },
          })}
        />
      </Card>

      <PositionDrawer positionCode={selectedCode} onClose={() => setSelectedCode(null)} />
    </>
  )
}
