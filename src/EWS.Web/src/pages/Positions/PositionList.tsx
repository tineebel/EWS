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
import PositionDrawer from './PositionDrawer'

const scopeColors: Record<string, string> = { Branch: 'blue', Ho: 'orange', All: 'green' }

export default function PositionList() {
  const { token } = theme.useToken()
  const columnWidth = token.controlHeightLG
  const [search, setSearch] = useState('')
  const [isActive, setIsActive] = useState<boolean | undefined>(true)
  const [pagination, setPagination] = useState({ page: 1, pageSize: 20 })
  const [selectedCode, setSelectedCode] = useState<string | null>(null)

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['positions', search, isActive, pagination],
    queryFn: () => settingsApi.positions.list({ search: search || undefined, isActive, ...pagination }),
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
      render: (value: string) => <Tag color={scopeColors[value] ?? 'default'}>{value}</Tag>,
    },
    { title: 'Section', dataIndex: 'sectionName', key: 'sectionName', width: columnWidth * 6.5, ellipsis: true },
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
