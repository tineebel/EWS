import { useState } from 'react'
import { Table, Select, Space, Tag, Card, Button, Tooltip, Typography, InputNumber } from 'antd'
import { ReloadOutlined, EditOutlined } from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { settingsApi } from '../../api/settings'
import type { WorkflowTemplate, WorkflowStep } from '../../api/types'
import TemplateEditDrawer from './TemplateEditDrawer'

const scopeColors: Record<string, string> = { Branch: 'blue', Ho: 'orange', All: 'green' }

export default function TemplateList() {
  const [docCode, setDocCode] = useState<number | undefined>()
  const [isActive, setIsActive] = useState<boolean | undefined>(true)
  const [editingTemplate, setEditingTemplate] = useState<WorkflowTemplate | null>(null)

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['workflow-templates', docCode, isActive],
    queryFn: () => settingsApi.workflowTemplates.list({ docCode, isActive }),
  })

  const stepColumns = [
    { title: 'Order', dataIndex: 'stepOrder', key: 'stepOrder', width: 70 },
    { title: 'Step Name', dataIndex: 'stepName', key: 'stepName', width: 180 },
    { title: 'Approver Type', dataIndex: 'approverType', key: 'approverType', width: 160 },
    { title: 'Specific Position', dataIndex: 'specificPositionCode', key: 'specificPositionCode', width: 160, render: (v?: string) => v ? <code>{v}</code> : '-' },
    { title: 'Required', dataIndex: 'isRequired', key: 'isRequired', width: 90, render: (v: boolean) => <Tag color={v ? 'red' : 'default'}>{v ? 'Yes' : 'No'}</Tag> },
  ]

  const columns = [
    { title: 'DocCode', dataIndex: 'docCode', key: 'docCode', width: 90 },
    { title: 'DocName', dataIndex: 'docName', key: 'docName', ellipsis: true },
    { title: 'Flow', dataIndex: 'flowCode', key: 'flowCode', width: 70 },
    { title: 'Description', dataIndex: 'flowDesc', key: 'flowDesc', ellipsis: true },
    { title: 'Scope', dataIndex: 'wfScopeType', key: 'wfScopeType', width: 80, render: (v: string) => <Tag color={scopeColors[v] ?? 'default'}>{v}</Tag> },
    {
      title: 'Flags', key: 'flags', width: 130,
      render: (_: unknown, r: WorkflowTemplate) => (
        <Space size={4}>
          {r.hasSpecialItem && <Tag color="purple">Special</Tag>}
          {r.isUrgent && <Tag color="red">Urgent</Tag>}
        </Space>
      )
    },
    {
      title: 'Conditions', key: 'cond', width: 140,
      render: (_: unknown, r: WorkflowTemplate) => (
        <Typography.Text type="secondary" style={{ fontSize: 12 }}>
          {[r.condition1, r.condition2].filter(Boolean).join(', ') || '-'}
        </Typography.Text>
      )
    },
    { title: 'Steps', key: 'steps', width: 70, render: (_: unknown, r: WorkflowTemplate) => r.steps.length },
    { title: 'Status', dataIndex: 'isActive', key: 'isActive', width: 90, render: (v: boolean) => <Tag color={v ? 'green' : 'red'}>{v ? 'Active' : 'Inactive'}</Tag> },
    {
      title: 'Action', key: 'action', width: 80,
      render: (_: unknown, r: WorkflowTemplate) => (
        <Tooltip title="Edit">
          <Button
            icon={<EditOutlined />}
            size="small"
            onClick={() => setEditingTemplate(r)}
          />
        </Tooltip>
      ),
    },
  ]

  return (
    <>
      <Card>
        <Space style={{ marginBottom: 16 }} wrap>
          <InputNumber
            placeholder="DocCode"
            style={{ width: 120 }}
            value={docCode}
            onChange={v => setDocCode(v ?? undefined)}
            min={1}
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
          dataSource={data?.data as WorkflowTemplate[] | undefined}
          rowKey="templateId"
          loading={isLoading}
          size="small"
          pagination={{ pageSize: 50, showTotal: (t) => `ทั้งหมด ${t} รายการ` }}
          expandable={{
            expandedRowRender: (r: WorkflowTemplate) => (
              <div style={{ padding: '0 16px 16px' }}>
                <Typography.Text strong>Approval Steps ({r.steps.length})</Typography.Text>
                <Table
                  columns={stepColumns}
                  dataSource={r.steps as WorkflowStep[]}
                  rowKey="stepId"
                  size="small"
                  pagination={false}
                  style={{ marginTop: 8 }}
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
