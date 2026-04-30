import { useState } from 'react'
import {
  Button,
  Card,
  InputNumber,
  Select,
  Space,
  Table,
  Tag,
  Tooltip,
  Typography,
  theme,
} from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { EditOutlined, ReloadOutlined } from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { settingsApi } from '../../api/settings'
import type { WorkflowStep, WorkflowTemplate } from '../../api/types'
import { displayWfScope } from '../../utils/display'
import TemplateEditDrawer from './TemplateEditDrawer'

const scopeColors: Record<string, string> = { Branch: 'blue', Ho: 'orange', All: 'green' }

export default function TemplateList() {
  const { token } = theme.useToken()
  const columnWidth = token.controlHeightLG
  const [docCode, setDocCode] = useState<number | undefined>()
  const [isActive, setIsActive] = useState<boolean | undefined>(true)
  const [editingTemplate, setEditingTemplate] = useState<WorkflowTemplate | null>(null)

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['workflow-templates', docCode, isActive],
    queryFn: () => settingsApi.workflowTemplates.list({ docCode, isActive }),
  })

  const templates = data?.data ?? []

  const stepColumns: ColumnsType<WorkflowStep> = [
    { title: 'Order', dataIndex: 'stepOrder', key: 'stepOrder', width: columnWidth * 2 },
    { title: 'Step Name', dataIndex: 'stepName', key: 'stepName', width: columnWidth * 6.5 },
    { title: 'Approver Type', dataIndex: 'approverType', key: 'approverType', width: columnWidth * 5.5 },
    {
      title: 'Specific Position',
      dataIndex: 'specificPositionCode',
      key: 'specificPositionCode',
      width: columnWidth * 5.5,
      render: (value?: string) => value ? <code>{value}</code> : '-',
    },
    {
      title: 'Required',
      dataIndex: 'isRequired',
      key: 'isRequired',
      width: columnWidth * 3,
      render: (value: boolean) => <Tag color={value ? 'red' : 'default'}>{value ? 'Yes' : 'No'}</Tag>,
    },
  ]

  const columns: ColumnsType<WorkflowTemplate> = [
    { title: 'DocCode', dataIndex: 'docCode', key: 'docCode', width: columnWidth * 2.25 },
    { title: 'DocName', dataIndex: 'docName', key: 'docName', width: columnWidth * 6.5, ellipsis: true },
    { title: 'Flow', dataIndex: 'flowCode', key: 'flowCode', width: columnWidth * 1.75 },
    { title: 'Description', dataIndex: 'flowDesc', key: 'flowDesc', width: columnWidth * 7, ellipsis: true },
    {
      title: 'Scope',
      dataIndex: 'wfScopeType',
      key: 'wfScopeType',
      width: columnWidth * 2.25,
      render: (value: string) => <Tag color={scopeColors[value] ?? 'default'}>{displayWfScope(value)}</Tag>,
    },
    {
      title: 'Flags',
      key: 'flags',
      width: columnWidth * 3.5,
      render: (_: unknown, record: WorkflowTemplate) => (
        <Space size={token.marginXXS}>
          {record.hasSpecialItem && <Tag color="purple">Special</Tag>}
          {record.isUrgent && <Tag color="red">Urgent</Tag>}
        </Space>
      ),
    },
    {
      title: 'Conditions',
      key: 'conditions',
      width: columnWidth * 4,
      render: (_: unknown, record: WorkflowTemplate) => (
        <Typography.Text type="secondary" style={{ fontSize: token.fontSizeSM }}>
          {[record.condition1, record.condition2].filter(Boolean).join(', ') || '-'}
        </Typography.Text>
      ),
    },
    {
      title: 'Steps',
      key: 'steps',
      width: columnWidth * 2,
      render: (_: unknown, record: WorkflowTemplate) => record.steps.length,
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
      render: (_: unknown, record: WorkflowTemplate) => (
        <Tooltip title="Edit">
          <Button
            icon={<EditOutlined />}
            size="small"
            onClick={(event) => {
              event.stopPropagation()
              setEditingTemplate(record)
            }}
          />
        </Tooltip>
      ),
    },
  ]

  return (
    <>
      <Card styles={{ body: { padding: token.paddingLG } }}>
        <Space style={{ marginBottom: token.marginMD }} wrap>
          <InputNumber
            placeholder="DocCode"
            style={{ width: token.controlHeightLG * 3 }}
            value={docCode}
            onChange={(value) => setDocCode(value ?? undefined)}
            min={1}
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
          dataSource={templates}
          rowKey="templateId"
          loading={isLoading}
          size="small"
          pagination={{ pageSize: 50, showTotal: (total) => `Total ${total} templates` }}
          scroll={{ x: true }}
          expandable={{
            rowExpandable: (record) => record.steps.length > 0,
            expandRowByClick: true,
            expandedRowRender: (record) => (
              <div
                style={{
                  padding: `${token.paddingSM}px ${token.paddingLG}px ${token.paddingLG}px`,
                  background: token.colorFillTertiary,
                  borderTop: `1px solid ${token.colorBorderSecondary}`,
                }}
              >
                <Typography.Text strong>
                  Approval Steps ({record.steps.length})
                </Typography.Text>
                <Table
                  columns={stepColumns}
                  dataSource={record.steps}
                  rowKey="stepId"
                  size="small"
                  pagination={false}
                  style={{ marginTop: token.marginXS }}
                  scroll={{ x: true }}
                />
              </div>
            ),
          }}
        />
      </Card>

      <TemplateEditDrawer
        template={editingTemplate}
        onClose={() => setEditingTemplate(null)}
        onSaved={() => refetch()}
      />
    </>
  )
}
