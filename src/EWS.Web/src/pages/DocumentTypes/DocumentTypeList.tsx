import { useState } from 'react'
import { Table, Input, Select, Space, Tag, Card, Button, Tooltip, Badge } from 'antd'
import { SearchOutlined, ReloadOutlined } from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { settingsApi } from '../../api/settings'
import type { DocumentType } from '../../api/types'

const categoryColors: Record<string, string> = {
  Memo: 'blue', Finance: 'green', IT: 'purple', SIS: 'orange', Contract: 'red',
}

export default function DocumentTypeList() {
  const [search, setSearch] = useState('')
  const [isActive, setIsActive] = useState<boolean | undefined>(true)

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['document-types', search, isActive],
    queryFn: () => settingsApi.documentTypes.list({ search: search || undefined, isActive }),
  })

  const columns = [
    { title: 'DocCode', dataIndex: 'docCode', key: 'docCode', width: 100 },
    { title: 'ชื่อเอกสาร', dataIndex: 'docName', key: 'docName', ellipsis: true },
    { title: 'ชื่อ (EN)', dataIndex: 'docNameEn', key: 'docNameEn', ellipsis: true, render: (v?: string) => v ?? '-' },
    { title: 'Category', dataIndex: 'category', key: 'category', width: 110, render: (v: string) => <Tag color={categoryColors[v] ?? 'default'}>{v}</Tag> },
    { title: 'Templates', dataIndex: 'templateCount', key: 'templateCount', width: 100, render: (v: number) => <Badge count={v} showZero style={{ backgroundColor: v > 0 ? '#52c41a' : '#d9d9d9' }} /> },
    { title: 'คำอธิบาย', dataIndex: 'description', key: 'description', ellipsis: true, render: (v?: string) => v ?? '-' },
    { title: 'Status', dataIndex: 'isActive', key: 'isActive', width: 90, render: (v: boolean) => <Tag color={v ? 'green' : 'red'}>{v ? 'Active' : 'Inactive'}</Tag> },
  ]

  return (
    <Card>
      <Space style={{ marginBottom: 16 }} wrap>
        <Input
          placeholder="ค้นหา DocCode / ชื่อเอกสาร"
          prefix={<SearchOutlined />}
          style={{ width: 280 }}
          value={search}
          onChange={e => setSearch(e.target.value)}
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
        dataSource={data?.data as DocumentType[] | undefined}
        rowKey="documentTypeId"
        loading={isLoading}
        size="small"
        pagination={{ pageSize: 50, showTotal: (t) => `ทั้งหมด ${t} รายการ` }}
      />
    </Card>
  )
}
