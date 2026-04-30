import { useState } from 'react'
import { Badge, Button, Card, Input, Select, Space, Table, Tag, Tooltip, theme } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { ReloadOutlined, SearchOutlined } from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { settingsApi } from '../../api/settings'
import type { DocumentType } from '../../api/types'

const categoryColors: Record<string, string> = {
  Memo: 'blue',
  Finance: 'green',
  IT: 'purple',
  SIS: 'orange',
  Contract: 'red',
}

export default function DocumentTypeList() {
  const { token } = theme.useToken()
  const columnWidth = token.controlHeightLG
  const [search, setSearch] = useState('')
  const [isActive, setIsActive] = useState<boolean | undefined>(true)

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['document-types', search, isActive],
    queryFn: () => settingsApi.documentTypes.list({ search: search || undefined, isActive }),
  })

  const documentTypes = data?.data ?? []

  const columns: ColumnsType<DocumentType> = [
    { title: 'DocCode', dataIndex: 'docCode', key: 'docCode', width: columnWidth * 2.5 },
    { title: 'Document Name', dataIndex: 'docName', key: 'docName', width: columnWidth * 6.5, ellipsis: true },
    { title: 'Name (EN)', dataIndex: 'docNameEn', key: 'docNameEn', width: columnWidth * 5.5, ellipsis: true, render: (value?: string) => value ?? '-' },
    { title: 'Category', dataIndex: 'category', key: 'category', width: columnWidth * 3, render: (value: string) => <Tag color={categoryColors[value] ?? 'default'}>{value}</Tag> },
    {
      title: 'Templates',
      dataIndex: 'templateCount',
      key: 'templateCount',
      width: columnWidth * 2.75,
      render: (value: number) => <Badge count={value} showZero color={value > 0 ? token.colorSuccess : token.colorTextDisabled} />,
    },
    { title: 'Description', dataIndex: 'description', key: 'description', width: columnWidth * 7, ellipsis: true, render: (value?: string) => value ?? '-' },
    { title: 'Status', dataIndex: 'isActive', key: 'isActive', width: columnWidth * 2.5, render: (value: boolean) => <Tag color={value ? 'green' : 'red'}>{value ? 'Active' : 'Inactive'}</Tag> },
  ]

  return (
    <Card styles={{ body: { padding: token.paddingLG } }}>
      <style>
        {`
          .document-types-table .ant-table-thead > tr > th,
          .document-types-table .ant-table-tbody > tr > td {
            padding-block: ${token.paddingXXS}px;
          }
        `}
      </style>
      <Space style={{ marginBottom: token.marginMD }} wrap>
        <Input
          placeholder="Search DocCode or document name"
          prefix={<SearchOutlined />}
          style={{ width: token.controlHeightLG * 8 }}
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          allowClear
        />
        <Select
          style={{ width: token.controlHeightLG * 4 }}
          value={isActive === undefined ? 'all' : isActive ? 'active' : 'inactive'}
          onChange={(value) => setIsActive(value === 'all' ? undefined : value === 'active')}
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
        dataSource={documentTypes}
        rowKey="documentTypeId"
        loading={isLoading}
        size="small"
        className="document-types-table"
        pagination={{ pageSize: 50, showTotal: (total) => `Total ${total} document types` }}
        scroll={{ x: true }}
      />
    </Card>
  )
}
