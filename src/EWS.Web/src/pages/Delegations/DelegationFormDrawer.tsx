import { useEffect, useState } from 'react'
import dayjs from 'dayjs'
import {
  Button,
  DatePicker,
  Drawer,
  Form,
  Input,
  Select,
  Space,
  Switch,
  Typography,
  message,
  theme,
} from 'antd'
import { useMutation, useQuery } from '@tanstack/react-query'
import { settingsApi } from '../../api/settings'
import { formatShortCodeName } from '../../utils/display'

const { TextArea } = Input

interface Props {
  delegationId: number | null
  open: boolean
  onClose: () => void
  onSaved: () => void
}

export default function DelegationFormDrawer({ delegationId, open, onClose, onSaved }: Props) {
  const { token } = theme.useToken()
  const [form] = Form.useForm()
  const [positionSearch, setPositionSearch] = useState('')

  const detailQuery = useQuery({
    queryKey: ['delegation-detail', delegationId],
    queryFn: () => settingsApi.delegations.get(delegationId!),
    enabled: open && delegationId !== null,
  })

  const positionsQuery = useQuery({
    queryKey: ['delegation-position-options', positionSearch],
    queryFn: () =>
      settingsApi.positions.list({
        isActive: true,
        search: positionSearch || undefined,
        page: 1,
        pageSize: 50,
      }),
    enabled: open,
    staleTime: 5 * 60 * 1000,
  })

  const createMutation = useMutation({
    mutationFn: settingsApi.delegations.create,
    onSuccess: () => {
      message.success('Delegation created')
      onSaved()
      onClose()
    },
    onError: (error: unknown) => {
      const errorMessage = error instanceof Error ? error.message : 'Unable to create delegation'
      message.error(errorMessage)
    },
  })

  const updateMutation = useMutation({
    mutationFn: (body: Parameters<typeof settingsApi.delegations.update>[1]) =>
      settingsApi.delegations.update(delegationId!, body),
    onSuccess: () => {
      message.success('Delegation updated')
      onSaved()
      onClose()
    },
    onError: (error: unknown) => {
      const errorMessage = error instanceof Error ? error.message : 'Unable to update delegation'
      message.error(errorMessage)
    },
  })

  useEffect(() => {
    if (!open) {
      form.resetFields()
      setPositionSearch('')
      return
    }

    if (delegationId === null) {
      form.setFieldsValue({
        isActive: true,
      })
      return
    }

    const delegation = detailQuery.data?.data
    if (!delegation) return

    form.setFieldsValue({
      fromPositionCode: delegation.fromPositionCode,
      toPositionCode: delegation.toPositionCode,
      startDate: dayjs(delegation.startDate),
      endDate: dayjs(delegation.endDate),
      reason: delegation.reason,
      isActive: delegation.isActive,
    })
  }, [delegationId, detailQuery.data, form, open])

  const handleSubmit = async () => {
    const values = await form.validateFields()
    const body = {
      fromPositionCode: String(values.fromPositionCode).trim(),
      toPositionCode: String(values.toPositionCode).trim(),
      startDate: values.startDate.startOf('day').format('YYYY-MM-DDTHH:mm:ss'),
      endDate: values.endDate.endOf('day').format('YYYY-MM-DDTHH:mm:ss'),
      reason: values.reason?.trim() || undefined,
      isActive: values.isActive ?? true,
    }

    if (delegationId === null) {
      createMutation.mutate(body)
      return
    }

    updateMutation.mutate(body)
  }

  const isSaving = createMutation.isPending || updateMutation.isPending
  const positionOptions = (positionsQuery.data?.data.items ?? []).map((position) => ({
    value: position.positionCode,
    label: `${position.positionCode} - ${formatShortCodeName(position.positionShortName, position.positionCode, position.positionName)}`,
  }))

  return (
    <Drawer
      open={open}
      onClose={onClose}
      destroyOnClose
      width={token.controlHeightLG * 20}
      title={delegationId === null ? 'Create Delegation' : 'Edit Delegation'}
      footer={
        <Space style={{ display: 'flex', justifyContent: 'flex-end', width: '100%' }}>
          <Button onClick={onClose}>Cancel</Button>
          <Button type="primary" loading={isSaving} onClick={() => void handleSubmit()}>
            Save
          </Button>
        </Space>
      }
    >
      <Form form={form} layout="vertical">
        <Form.Item
          name="fromPositionCode"
          label="From Position"
          rules={[{ required: true, message: 'Please select from position' }]}
        >
          <Select
            showSearch
            optionFilterProp="label"
            filterOption={false}
            placeholder="Select from position"
            loading={positionsQuery.isLoading}
            options={positionOptions}
            onSearch={setPositionSearch}
          />
        </Form.Item>

        <Form.Item
          name="toPositionCode"
          label="To Position"
          rules={[
            { required: true, message: 'Please select to position' },
            ({ getFieldValue }) => ({
              validator(_, value) {
                const fromPositionCode = getFieldValue('fromPositionCode')

                if (!value || !fromPositionCode || value !== fromPositionCode) {
                  return Promise.resolve()
                }

                return Promise.reject(new Error('To position must be different from from position'))
              },
            }),
          ]}
        >
          <Select
            showSearch
            optionFilterProp="label"
            filterOption={false}
            placeholder="Select to position"
            loading={positionsQuery.isLoading}
            options={positionOptions}
            onSearch={setPositionSearch}
          />
        </Form.Item>

        <Space wrap style={{ width: '100%' }} size={token.marginSM}>
          <Form.Item
            name="startDate"
            label="Start Date"
            rules={[{ required: true, message: 'Please select start date' }]}
            style={{ minWidth: token.controlHeightLG * 5 }}
          >
            <DatePicker style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item
            name="endDate"
            label="End Date"
            rules={[
              { required: true, message: 'Please select end date' },
              ({ getFieldValue }) => ({
                validator(_, value) {
                  const startDate = getFieldValue('startDate')

                  if (!value || !startDate || !value.isBefore(startDate, 'day')) {
                    return Promise.resolve()
                  }

                  return Promise.reject(new Error('End date must be greater than or equal to start date'))
                },
              }),
            ]}
            style={{ minWidth: token.controlHeightLG * 5 }}
          >
            <DatePicker style={{ width: '100%' }} />
          </Form.Item>
        </Space>

        <Form.Item name="reason" label="Reason">
          <TextArea rows={3} maxLength={500} showCount placeholder="Optional reason for delegation" />
        </Form.Item>

        <Form.Item name="isActive" label="Active" valuePropName="checked">
          <Switch />
        </Form.Item>

        {delegationId !== null && detailQuery.data?.data ? (
          <Typography.Text type="secondary" style={{ fontSize: token.fontSizeSM }}>
            {`Created at ${detailQuery.data.data.createdAt}${detailQuery.data.data.updatedAt ? ` | Updated at ${detailQuery.data.data.updatedAt}` : ''}`}
          </Typography.Text>
        ) : null}
      </Form>
    </Drawer>
  )
}
