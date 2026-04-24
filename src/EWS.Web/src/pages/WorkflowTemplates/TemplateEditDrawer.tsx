import { useEffect, useState } from 'react'
import {
  Drawer, Tabs, Form, Input, Select, Switch, Checkbox, Button,
  Space, Table, InputNumber, Tag, message, Typography, Timeline,
  Collapse
} from 'antd'
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons'
import { useMutation, useQuery } from '@tanstack/react-query'
import { settingsApi } from '../../api/settings'
import type { WorkflowTemplate, UpdateStepRequest } from '../../api/types'

const { TextArea } = Input

const APPROVER_TYPE_OPTIONS = [
  { value: 'DirectSupervisor', label: 'DirectSupervisor' },
  { value: 'SectionManager', label: 'SectionManager' },
  { value: 'DeptManager', label: 'DeptManager' },
  { value: 'DivisionDirector', label: 'DivisionDirector' },
  { value: 'CLevel', label: 'CLevel' },
  { value: 'Ceo', label: 'Ceo' },
  { value: 'SpecificPosition', label: 'SpecificPosition' },
  { value: 'AreaManager', label: 'AreaManager' },
]

const SCOPE_OPTIONS = [
  { value: 'Branch', label: 'Branch' },
  { value: 'Ho', label: 'Ho' },
  { value: 'All', label: 'All' },
]

const changeTypeColor: Record<string, string> = {
  Updated: 'blue',
  Deactivated: 'red',
  Activated: 'green',
}

interface Props {
  template: WorkflowTemplate | null
  onClose: () => void
  onSaved: () => void
}

interface StepRow extends UpdateStepRequest {
  key: string
}

export default function TemplateEditDrawer({ template, onClose, onSaved }: Props) {
  const [form] = Form.useForm()
  const [steps, setSteps] = useState<StepRow[]>([])
  const [activeTab, setActiveTab] = useState('edit')

  const open = template !== null

  // Load history when tab changes to history
  const { data: historyData, isLoading: historyLoading } = useQuery({
    queryKey: ['template-history', template?.templateId],
    queryFn: () => settingsApi.workflowTemplates.history(template!.templateId),
    enabled: open && activeTab === 'history' && template !== null,
  })

  const saveMutation = useMutation({
    mutationFn: (body: Parameters<typeof settingsApi.workflowTemplates.update>[1]) =>
      settingsApi.workflowTemplates.update(template!.templateId, body),
    onSuccess: () => {
      message.success('บันทึก Template เรียบร้อยแล้ว')
      onSaved()
      onClose()
    },
    onError: () => {
      message.error('เกิดข้อผิดพลาดในการบันทึก')
    },
  })

  // Populate form when template changes
  useEffect(() => {
    if (!template) return
    form.setFieldsValue({
      flowDesc: template.flowDesc,
      wfScopeType: template.wfScopeType,
      isActive: template.isActive,
      hasSpecialItem: template.hasSpecialItem,
      isUrgent: template.isUrgent,
      condition1: template.condition1 ?? '',
      condition2: template.condition2 ?? '',
      changeNote: '',
    })
    const initialSteps: StepRow[] = template.steps.map((s, i) => ({
      key: `existing-${s.stepId}-${i}`,
      stepId: s.stepId,
      stepOrder: s.stepOrder,
      stepName: s.stepName,
      approverType: s.approverType,
      specificPositionCode: s.specificPositionCode,
      escalationDays: 0,
      isRequired: s.isRequired,
    }))
    setSteps(initialSteps)
    setActiveTab('edit')
  }, [template, form])

  const addStep = () => {
    const nextOrder = steps.length > 0 ? Math.max(...steps.map(s => s.stepOrder)) + 1 : 1
    setSteps(prev => [...prev, {
      key: `new-${Date.now()}`,
      stepId: undefined,
      stepOrder: nextOrder,
      stepName: '',
      approverType: 'DirectSupervisor',
      specificPositionCode: undefined,
      escalationDays: 0,
      isRequired: true,
    }])
  }

  const removeStep = (key: string) => {
    setSteps(prev => prev.filter(s => s.key !== key))
  }

  const updateStep = (key: string, field: keyof StepRow, value: unknown) => {
    setSteps(prev => prev.map(s => s.key === key ? { ...s, [field]: value } : s))
  }

  const handleSave = () => {
    form.validateFields().then(values => {
      saveMutation.mutate({
        flowDesc: values.flowDesc,
        wfScopeType: values.wfScopeType,
        hasSpecialItem: values.hasSpecialItem ?? false,
        isUrgent: values.isUrgent ?? false,
        condition1: values.condition1 || undefined,
        condition2: values.condition2 || undefined,
        condition3: undefined,
        condition4: undefined,
        condition5: undefined,
        isActive: values.isActive ?? true,
        steps: steps.map((s, idx) => ({
          stepId: s.stepId,
          stepOrder: idx + 1,
          stepName: s.stepName,
          approverType: s.approverType,
          specificPositionCode: s.specificPositionCode || undefined,
          escalationDays: s.escalationDays ?? 0,
          isRequired: s.isRequired ?? true,
        })),
        changeNote: values.changeNote || undefined,
      })
    })
  }

  const stepColumns = [
    {
      title: '#',
      key: 'order',
      width: 40,
      render: (_: unknown, __: StepRow, index: number) => index + 1,
    },
    {
      title: 'Step Name',
      key: 'stepName',
      render: (_: unknown, record: StepRow) => (
        <Input
          value={record.stepName}
          onChange={e => updateStep(record.key, 'stepName', e.target.value)}
          size="small"
          placeholder="Step Name"
          style={{ minWidth: 140 }}
        />
      ),
    },
    {
      title: 'Approver Type',
      key: 'approverType',
      width: 180,
      render: (_: unknown, record: StepRow) => (
        <Select
          value={record.approverType}
          options={APPROVER_TYPE_OPTIONS}
          onChange={v => updateStep(record.key, 'approverType', v)}
          size="small"
          style={{ width: '100%' }}
        />
      ),
    },
    {
      title: 'Position Code',
      key: 'specificPositionCode',
      width: 130,
      render: (_: unknown, record: StepRow) =>
        record.approverType === 'SpecificPosition' ? (
          <Input
            value={record.specificPositionCode ?? ''}
            onChange={e => updateStep(record.key, 'specificPositionCode', e.target.value)}
            size="small"
            placeholder="e.g. HOFIN01"
          />
        ) : (
          <Typography.Text type="secondary" style={{ fontSize: 12 }}>-</Typography.Text>
        ),
    },
    {
      title: 'Esc. Days',
      key: 'escalationDays',
      width: 90,
      render: (_: unknown, record: StepRow) => (
        <InputNumber
          value={record.escalationDays}
          min={0}
          onChange={v => updateStep(record.key, 'escalationDays', v ?? 0)}
          size="small"
          style={{ width: 80 }}
        />
      ),
    },
    {
      title: 'Required',
      key: 'isRequired',
      width: 80,
      render: (_: unknown, record: StepRow) => (
        <Switch
          checked={record.isRequired}
          onChange={v => updateStep(record.key, 'isRequired', v)}
          size="small"
        />
      ),
    },
    {
      title: '',
      key: 'action',
      width: 40,
      render: (_: unknown, record: StepRow) => (
        <Button
          type="text"
          danger
          icon={<DeleteOutlined />}
          size="small"
          onClick={() => removeStep(record.key)}
        />
      ),
    },
  ]

  const historyItems = historyData?.data ?? []

  return (
    <Drawer
      open={open}
      onClose={onClose}
      width={900}
      title={template ? `Edit: ${template.docName} — Flow ${template.flowCode}` : 'Edit Template'}
      footer={
        activeTab === 'edit' ? (
          <Space style={{ justifyContent: 'flex-end', width: '100%' }}>
            <Button onClick={onClose}>Cancel</Button>
            <Button type="primary" onClick={handleSave} loading={saveMutation.isPending}>
              Save
            </Button>
          </Space>
        ) : null
      }
      destroyOnClose
    >
      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        items={[
          {
            key: 'edit',
            label: 'แก้ไข Template',
            children: (
              <Form form={form} layout="vertical">
                {/* Header fields */}
                <Space wrap style={{ width: '100%', marginBottom: 8 }}>
                  <Form.Item name="flowDesc" label="Flow Description" rules={[{ required: true, max: 200 }]} style={{ marginBottom: 0, minWidth: 280 }}>
                    <Input placeholder="e.g. PCV-BR > 1,000" />
                  </Form.Item>
                  <Form.Item name="wfScopeType" label="Scope" rules={[{ required: true }]} style={{ marginBottom: 0, width: 120 }}>
                    <Select options={SCOPE_OPTIONS} />
                  </Form.Item>
                  <Form.Item name="isActive" label="Active" valuePropName="checked" style={{ marginBottom: 0 }}>
                    <Switch />
                  </Form.Item>
                  <Form.Item name="hasSpecialItem" valuePropName="checked" label=" " style={{ marginBottom: 0 }}>
                    <Checkbox>Has Special Item</Checkbox>
                  </Form.Item>
                  <Form.Item name="isUrgent" valuePropName="checked" label=" " style={{ marginBottom: 0 }}>
                    <Checkbox>Urgent</Checkbox>
                  </Form.Item>
                </Space>

                {/* Conditions */}
                <Space wrap style={{ marginBottom: 16 }}>
                  <Form.Item name="condition1" label="Condition 1" style={{ marginBottom: 0, width: 160 }}>
                    <Input placeholder="e.g. <= 1,000" size="small" />
                  </Form.Item>
                  <Form.Item name="condition2" label="Condition 2" style={{ marginBottom: 0, width: 160 }}>
                    <Input placeholder="e.g. > 1,000" size="small" />
                  </Form.Item>
                </Space>

                {/* Steps */}
                <div style={{ marginBottom: 16 }}>
                  <Space style={{ marginBottom: 8 }}>
                    <Typography.Text strong>Approval Steps</Typography.Text>
                    <Button size="small" icon={<PlusOutlined />} onClick={addStep}>
                      Add Step
                    </Button>
                  </Space>
                  <Table
                    dataSource={steps}
                    columns={stepColumns}
                    rowKey="key"
                    size="small"
                    pagination={false}
                    bordered
                  />
                </div>

                {/* Change note */}
                <Form.Item name="changeNote" label="Change Note (optional)">
                  <TextArea rows={2} maxLength={500} showCount placeholder="บันทึกเหตุผลการแก้ไข..." />
                </Form.Item>
              </Form>
            ),
          },
          {
            key: 'history',
            label: 'ประวัติการเปลี่ยนแปลง',
            children: historyLoading ? (
              <Typography.Text>Loading...</Typography.Text>
            ) : historyItems.length === 0 ? (
              <Typography.Text type="secondary">ยังไม่มีประวัติการเปลี่ยนแปลง</Typography.Text>
            ) : (
              <Timeline
                items={historyItems.map(log => ({
                  key: log.auditId,
                  color: changeTypeColor[log.changeType] ?? 'blue',
                  children: (
                    <div style={{ marginBottom: 8 }}>
                      <Space wrap>
                        <Tag color={changeTypeColor[log.changeType] ?? 'blue'}>
                          v{log.version} {log.changeType}
                        </Tag>
                        <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                          by {log.changedBy} · {new Date(log.changedAt).toLocaleString('th-TH')}
                        </Typography.Text>
                      </Space>
                      {log.changeNote && (
                        <div>
                          <Typography.Text italic style={{ fontSize: 12 }}>
                            "{log.changeNote}"
                          </Typography.Text>
                        </div>
                      )}
                      <Collapse
                        ghost
                        size="small"
                        items={[{
                          key: 'snap',
                          label: <Typography.Text type="secondary" style={{ fontSize: 11 }}>ดู Snapshot</Typography.Text>,
                          children: (
                            <pre style={{ fontSize: 11, background: '#f5f5f5', padding: 8, borderRadius: 4, maxHeight: 200, overflow: 'auto' }}>
                              {JSON.stringify(JSON.parse(log.snapshotJson), null, 2)}
                            </pre>
                          ),
                        }]}
                      />
                    </div>
                  ),
                }))}
              />
            ),
          },
        ]}
      />
    </Drawer>
  )
}
