import { useEffect, useState } from 'react'
import {
  Button,
  Checkbox,
  Collapse,
  Drawer,
  Form,
  Input,
  InputNumber,
  Select,
  Space,
  Switch,
  Table,
  Tabs,
  Tag,
  Timeline,
  Typography,
  message,
  theme,
} from 'antd'
import { DeleteOutlined, PlusOutlined } from '@ant-design/icons'
import { useMutation, useQuery } from '@tanstack/react-query'
import { settingsApi } from '../../api/settings'
import type { UpdateStepRequest, WorkflowTemplate } from '../../api/types'

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
  const { token } = theme.useToken()
  const drawerWidth = token.controlHeightLG * 22.5
  const [form] = Form.useForm()
  const [steps, setSteps] = useState<StepRow[]>([])
  const [activeTab, setActiveTab] = useState('edit')

  const open = template !== null

  const { data: historyData, isLoading: historyLoading } = useQuery({
    queryKey: ['template-history', template?.templateId],
    queryFn: () => settingsApi.workflowTemplates.history(template!.templateId),
    enabled: open && activeTab === 'history' && template !== null,
  })

  const saveMutation = useMutation({
    mutationFn: (body: Parameters<typeof settingsApi.workflowTemplates.update>[1]) =>
      settingsApi.workflowTemplates.update(template!.templateId, body),
    onSuccess: () => {
      message.success('Template saved')
      onSaved()
      onClose()
    },
    onError: () => {
      message.error('Unable to save template')
    },
  })

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
    const initialSteps: StepRow[] = template.steps.map((step, index) => ({
      key: `existing-${step.stepId}-${index}`,
      stepId: step.stepId,
      stepOrder: step.stepOrder,
      stepName: step.stepName,
      approverType: step.approverType,
      specificPositionCode: step.specificPositionCode,
      escalationDays: 0,
      isRequired: step.isRequired,
    }))
    setSteps(initialSteps)
    setActiveTab('edit')
  }, [template, form])

  const addStep = () => {
    const nextOrder = steps.length > 0 ? Math.max(...steps.map((step) => step.stepOrder)) + 1 : 1
    setSteps((current) => [
      ...current,
      {
        key: `new-${Date.now()}`,
        stepId: undefined,
        stepOrder: nextOrder,
        stepName: '',
        approverType: 'DirectSupervisor',
        specificPositionCode: undefined,
        escalationDays: 0,
        isRequired: true,
      },
    ])
  }

  const removeStep = (key: string) => {
    setSteps((current) => current.filter((step) => step.key !== key))
  }

  const updateStep = (key: string, field: keyof StepRow, value: unknown) => {
    setSteps((current) => current.map((step) => step.key === key ? { ...step, [field]: value } : step))
  }

  const handleSave = () => {
    form.validateFields().then((values) => {
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
        steps: steps.map((step, index) => ({
          stepId: step.stepId,
          stepOrder: index + 1,
          stepName: step.stepName,
          approverType: step.approverType,
          specificPositionCode: step.specificPositionCode || undefined,
          escalationDays: step.escalationDays ?? 0,
          isRequired: step.isRequired ?? true,
        })),
        changeNote: values.changeNote || undefined,
      })
    })
  }

  const stepColumns = [
    {
      title: '#',
      key: 'order',
      width: token.controlHeightLG,
      render: (_: unknown, __: StepRow, index: number) => index + 1,
    },
    {
      title: 'Step Name',
      key: 'stepName',
      render: (_: unknown, record: StepRow) => (
        <Input
          value={record.stepName}
          onChange={(event) => updateStep(record.key, 'stepName', event.target.value)}
          size="small"
          placeholder="Step Name"
          style={{ minWidth: token.controlHeightLG * 4 }}
        />
      ),
    },
    {
      title: 'Approver Type',
      key: 'approverType',
      width: token.controlHeightLG * 5,
      render: (_: unknown, record: StepRow) => (
        <Select
          value={record.approverType}
          options={APPROVER_TYPE_OPTIONS}
          onChange={(value) => updateStep(record.key, 'approverType', value)}
          size="small"
          style={{ width: '100%' }}
        />
      ),
    },
    {
      title: 'Position Code',
      key: 'specificPositionCode',
      width: token.controlHeightLG * 4,
      render: (_: unknown, record: StepRow) =>
        record.approverType === 'SpecificPosition' ? (
          <Input
            value={record.specificPositionCode ?? ''}
            onChange={(event) => updateStep(record.key, 'specificPositionCode', event.target.value)}
            size="small"
            placeholder="e.g. HOFIN01"
          />
        ) : (
          <Typography.Text type="secondary" style={{ fontSize: token.fontSizeSM }}>-</Typography.Text>
        ),
    },
    {
      title: 'Esc. Days',
      key: 'escalationDays',
      width: token.controlHeightLG * 3,
      render: (_: unknown, record: StepRow) => (
        <InputNumber
          value={record.escalationDays}
          min={0}
          onChange={(value) => updateStep(record.key, 'escalationDays', value ?? 0)}
          size="small"
          style={{ width: token.controlHeightLG * 2 }}
        />
      ),
    },
    {
      title: 'Required',
      key: 'isRequired',
      width: token.controlHeightLG * 2,
      render: (_: unknown, record: StepRow) => (
        <Switch
          checked={record.isRequired}
          onChange={(value) => updateStep(record.key, 'isRequired', value)}
          size="small"
        />
      ),
    },
    {
      title: '',
      key: 'action',
      width: token.controlHeightLG,
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
      width={drawerWidth}
      title={template ? `Edit: ${template.docName} - Flow ${template.flowCode}` : 'Edit Template'}
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
            label: 'Edit Template',
            children: (
              <Form form={form} layout="vertical">
                <Space wrap style={{ width: '100%', marginBottom: token.marginXS }}>
                  <Form.Item name="flowDesc" label="Flow Description" rules={[{ required: true, max: 200 }]} style={{ marginBottom: 0, minWidth: token.controlHeightLG * 8 }}>
                    <Input placeholder="e.g. PCV-BR > 1,000" />
                  </Form.Item>
                  <Form.Item name="wfScopeType" label="Scope" rules={[{ required: true }]} style={{ marginBottom: 0, width: token.controlHeightLG * 3 }}>
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

                <Space wrap style={{ marginBottom: token.marginMD }}>
                  <Form.Item name="condition1" label="Condition 1" style={{ marginBottom: 0, width: token.controlHeightLG * 4 }}>
                    <Input placeholder="e.g. <= 1,000" size="small" />
                  </Form.Item>
                  <Form.Item name="condition2" label="Condition 2" style={{ marginBottom: 0, width: token.controlHeightLG * 4 }}>
                    <Input placeholder="e.g. > 1,000" size="small" />
                  </Form.Item>
                </Space>

                <div style={{ marginBottom: token.marginMD }}>
                  <Space style={{ marginBottom: token.marginXS }}>
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

                <Form.Item name="changeNote" label="Change Note (optional)">
                  <TextArea rows={2} maxLength={500} showCount placeholder="Reason for this change..." />
                </Form.Item>
              </Form>
            ),
          },
          {
            key: 'history',
            label: 'Change History',
            children: historyLoading ? (
              <Typography.Text>Loading...</Typography.Text>
            ) : historyItems.length === 0 ? (
              <Typography.Text type="secondary">No change history</Typography.Text>
            ) : (
              <Timeline
                items={historyItems.map((log) => ({
                  key: log.auditId,
                  color: changeTypeColor[log.changeType] ?? 'blue',
                  children: (
                    <div style={{ marginBottom: token.marginXS }}>
                      <Space wrap>
                        <Tag color={changeTypeColor[log.changeType] ?? 'blue'}>
                          v{log.version} {log.changeType}
                        </Tag>
                        <Typography.Text type="secondary" style={{ fontSize: token.fontSizeSM }}>
                          by {log.changedBy} - {new Date(log.changedAt).toLocaleString('th-TH')}
                        </Typography.Text>
                      </Space>
                      {log.changeNote && (
                        <div>
                          <Typography.Text italic style={{ fontSize: token.fontSizeSM }}>
                            "{log.changeNote}"
                          </Typography.Text>
                        </div>
                      )}
                      <Collapse
                        ghost
                        size="small"
                        items={[{
                          key: 'snapshot',
                          label: <Typography.Text type="secondary" style={{ fontSize: token.fontSizeSM }}>View Snapshot</Typography.Text>,
                          children: (
                            <pre
                              style={{
                                fontSize: token.fontSizeSM,
                                background: token.colorFillQuaternary,
                                padding: token.paddingXS,
                                borderRadius: token.borderRadiusSM,
                                maxHeight: token.controlHeightLG * 5,
                                overflow: 'auto',
                              }}
                            >
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
