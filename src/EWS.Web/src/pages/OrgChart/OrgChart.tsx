import { useDeferredValue, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { Tree, TreeNode } from 'react-organizational-chart'
import {
  Avatar,
  Breadcrumb,
  Button,
  Card,
  Empty,
  Input,
  Select,
  Space,
  Spin,
  Tag,
  Tooltip,
  Typography,
  theme,
} from 'antd'
import {
  DownOutlined,
  HomeOutlined,
  SearchOutlined,
  TeamOutlined,
  UserOutlined,
} from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { settingsApi } from '../../api/settings'
import { formatShortCodeName } from '../../utils/display'
import { OrgChartNode, organizationApi } from '../../api/organization'

const { Text } = Typography

const DEFAULT_DEPTH = 2

function countDescendants(node: OrgChartNode): number {
  return node.children.reduce((sum, child) => sum + 1 + countDescendants(child), 0)
}

function flattenTree(nodes: OrgChartNode[]): OrgChartNode[] {
  return nodes.flatMap((node) => [node, ...flattenTree(node.children)])
}

function NodeBox({
  node,
  isRoot = false,
  onDrillDown,
}: {
  node: OrgChartNode
  isRoot?: boolean
  onDrillDown: (node: OrgChartNode) => void
}) {
  const { token } = theme.useToken()
  const gradeColor = getGradeColor(node.jobGrade, token)
  const borderColor = isRoot ? token.colorPrimary : gradeColor
  const subCount = countDescendants(node)
  const hasChildren = node.children.length > 0
  const occupantLabel = node.occupantCount > 1 ? `${node.occupantCount} occupants` : node.occupantName ?? 'Vacant'
  const occupantTitle = node.occupantNames.length > 0 ? node.occupantNames.join(', ') : 'Vacant'

  return (
    <div
      style={{
        position: 'relative',
        display: 'inline-block',
        marginTop: 32, // Space for the half-avatar sticking out
        cursor: hasChildren ? 'pointer' : 'default',
        userSelect: 'none',
      }}
      onClick={() => hasChildren && onDrillDown(node)}
      title={hasChildren ? 'Click to view subordinates' : undefined}
    >
      {/* Avatar placed at the top center */}
      <div
        style={{
          position: 'absolute',
          top: 0,
          left: '50%',
          transform: 'translate(-50%, -50%)',
          zIndex: 2,
        }}
      >
        <Avatar
          size={64}
          icon={<UserOutlined />}
          style={{
            border: `4px solid ${borderColor}`,
            backgroundColor: token.colorBgContainer,
            color: node.isVacant ? token.colorError : token.colorTextSecondary,
          }}
        />
      </div>

      {/* Main Card */}
      <div
        style={{
          background: token.colorBgContainer,
          borderTop: `4px solid ${borderColor}`,
          borderRadius: token.borderRadiusLG,
          padding: `${token.paddingLG + token.paddingSM}px ${token.paddingSM}px ${token.paddingSM}px`,
          minWidth: 200,
          maxWidth: 240,
          boxShadow: isRoot ? token.boxShadowSecondary : token.boxShadowTertiary,
          transition: `all ${token.motionDurationMid}`,
          textAlign: 'center',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          gap: token.marginXXS,
        }}
        onMouseEnter={(e) => {
          if (hasChildren) {
            e.currentTarget.style.transform = 'translateY(-2px)'
            e.currentTarget.style.boxShadow = token.boxShadow
          }
        }}
        onMouseLeave={(e) => {
          if (hasChildren) {
            e.currentTarget.style.transform = 'none'
            e.currentTarget.style.boxShadow = isRoot ? token.boxShadowSecondary : token.boxShadowTertiary
          }
        }}
      >
        <Tooltip title={occupantTitle}>
          <div
            style={{
              fontSize: token.fontSize,
              fontWeight: token.fontWeightStrong,
              color: node.isVacant ? token.colorError : token.colorText,
              lineHeight: token.lineHeight,
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
              width: '100%',
            }}
          >
            {occupantLabel}
          </div>
        </Tooltip>

        {/* Position Name (Role) */}
        <div
          style={{
            fontSize: token.fontSizeSM,
            color: token.colorTextSecondary,
            lineHeight: token.lineHeightSM,
            overflow: 'hidden',
            display: '-webkit-box',
            WebkitLineClamp: 2,
            WebkitBoxOrient: 'vertical',
            width: '100%',
          }}
          title={node.positionName}
        >
          {node.positionName}
        </div>

        {/* Badges / Job Grade */}
        <div style={{ display: 'flex', justifyContent: 'center', gap: token.marginXXS, marginTop: token.marginXXS }}>
          <Tag
            color={gradeColor}
            style={{ margin: 0, fontSize: token.fontSizeSM, paddingInline: token.paddingXXS, lineHeight: `${token.controlHeightSM}px` }}
          >
            {node.jobGrade}
          </Tag>
          {node.secretaryCode && (
            <Tooltip title={`Secretary: ${node.secretaryCode}`}>
              <Tag color="purple" style={{ margin: 0, fontSize: token.fontSizeSM, paddingInline: token.paddingXXS, lineHeight: `${token.controlHeightSM}px` }}>
                Sec
              </Tag>
            </Tooltip>
          )}
        </div>

        {/* Subordinates Count */}
        {subCount > 0 && (
          <div
            style={{
              marginTop: token.marginXS,
              fontSize: token.fontSizeSM,
              color: token.colorPrimary,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: token.marginXXS,
            }}
          >
            <TeamOutlined />
            {subCount} people
            {hasChildren && <DownOutlined style={{ fontSize: token.fontSizeSM }} />}
          </div>
        )}
      </div>
    </div>
  )
}

function getGradeColor(jobGrade: string, token: ReturnType<typeof theme.useToken>['token']) {
  if (jobGrade.startsWith('A')) return token.colorError
  if (jobGrade.startsWith('B')) return token.colorPrimary
  if (jobGrade.startsWith('C')) return token.colorSuccess
  if (jobGrade.startsWith('D')) return token.colorTextTertiary
  return token.colorWarning
}

function buildTreeNodes(
  nodes: OrgChartNode[],
  depth: number,
  onDrillDown: (node: OrgChartNode) => void,
): ReactNode {
  return nodes.map((node) => (
    <TreeNode
      key={node.positionId}
      label={<NodeBox node={node} onDrillDown={onDrillDown} />}
    >
      {depth > 0 && node.children.length > 0
        ? buildTreeNodes(node.children, depth - 1, onDrillDown)
        : null}
    </TreeNode>
  ))
}

export default function OrgChart() {
  const { token } = theme.useToken()
  const [path, setPath] = useState<OrgChartNode[]>([])
  const [search, setSearch] = useState('')
  const deferredSearch = useDeferredValue(search.trim())
  const [branchCode, setBranchCode] = useState('')
  const [deptCode, setDeptCode] = useState<string | undefined>()
  const [sectionCode, setSectionCode] = useState<string | undefined>()
  const [depth, setDepth] = useState(DEFAULT_DEPTH)

  const { data, isLoading } = useQuery({
    queryKey: ['org-chart', branchCode, deptCode, sectionCode, deferredSearch],
    queryFn: () => organizationApi.orgChart(
      undefined,
      branchCode || undefined,
      deptCode,
      sectionCode,
      deferredSearch || undefined,
    ),
    staleTime: 5 * 60 * 1000,
  })

  const branchOptions = useQuery({
    queryKey: ['branch-options'],
    queryFn: () => settingsApi.branchOptions.list(),
  })

  const departments = useQuery({
    queryKey: ['department-options'],
    queryFn: () => settingsApi.departments.list({ isActive: true }),
  })

  const sections = useQuery({
    queryKey: ['section-options', deptCode],
    queryFn: () => settingsApi.sections.list({ deptCode, isActive: true }),
  })

  const topNodes = data?.data ?? []
  const currentNode = path.length > 0 ? path[path.length - 1] : null
  const displayNodes = currentNode ? currentNode.children : topNodes
  const allNodes = useMemo(() => flattenTree(topNodes), [topNodes])

  const searchResults = useMemo(() => {
    if (!search.trim()) return []
    const queryTerms = search.toLowerCase().split(/\s+/).filter(Boolean)
    return allNodes
      .filter((node) => {
        const searchableValues = [
          node.positionCode,
          node.positionName,
          node.occupantName ?? '',
          ...node.occupantNames,
        ].map((value) => value.toLowerCase())

        return queryTerms.every((term) =>
          searchableValues.some((value) => value.includes(term)),
        )
      })
      .slice(0, 15)
  }, [search, allNodes])

  function drillDown(node: OrgChartNode) {
    setPath((current) => [...current, node])
  }

  function jumpTo(node: OrgChartNode) {
    function findPath(nodes: OrgChartNode[], target: number): OrgChartNode[] | null {
      for (const current of nodes) {
        if (current.positionId === target) return [current]
        const subPath = findPath(current.children, target)
        if (subPath) return [current, ...subPath]
      }
      return null
    }
    const fullPath = findPath(topNodes, node.positionId)
    if (fullPath) setPath(fullPath)
    setSearch('')
  }

  return (
    <div>
      <Card size="small" style={{ marginBottom: token.marginMD }}>
        <Space wrap>
          <div style={{ position: 'relative', display: 'inline-block' }}>
            <Input
              prefix={<SearchOutlined />}
              placeholder="Search position, name, or employee"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              allowClear
              style={{ width: token.controlHeightLG * 8.5 }}
            />
            {searchResults.length > 0 && (
              <div
                style={{
                  position: 'absolute',
                  top: '100%',
                  left: 0,
                  zIndex: token.zIndexPopupBase,
                  background: token.colorBgContainer,
                  border: `${token.lineWidth}px ${token.lineType} ${token.colorBorder}`,
                  borderRadius: token.borderRadius,
                  boxShadow: token.boxShadowSecondary,
                  marginTop: token.marginXXS,
                  width: token.controlHeightLG * 8.5,
                  maxHeight: token.controlHeightLG * 7.5,
                  overflowY: 'auto',
                }}
              >
                {searchResults.map((node) => (
                  <div
                    key={node.positionId}
                    onClick={() => jumpTo(node)}
                    style={{
                      padding: `${token.paddingXXS}px ${token.paddingSM}px`,
                      cursor: 'pointer',
                      borderBottom: `${token.lineWidth}px ${token.lineType} ${token.colorSplit}`,
                      display: 'flex',
                      alignItems: 'center',
                      gap: token.marginXS,
                    }}
                  >
                    <Tag
                      color={getGradeColor(node.jobGrade, token)}
                      style={{ margin: 0, fontSize: token.fontSizeSM, paddingInline: token.paddingXXS, lineHeight: `${token.controlHeightSM}px`, flexShrink: 0 }}
                    >
                      {node.jobGrade}
                    </Tag>
                    <Text style={{ fontSize: token.fontSizeSM, flex: 1 }} ellipsis>
                      {node.positionName}
                    </Text>
                    <Text type="secondary" style={{ fontSize: token.fontSizeSM, flexShrink: 0 }}>
                      {node.occupantCount > 1 ? `${node.occupantCount} occupants` : node.occupantName ?? 'Vacant'}
                    </Text>
                  </div>
                ))}
              </div>
            )}
          </div>

          <Select
            showSearch
            loading={branchOptions.isLoading}
            optionFilterProp="label"
            style={{ width: token.controlHeightLG * 6 }}
            value={branchCode}
            onChange={(value) => {
              setBranchCode(value)
              setDeptCode(undefined)
              setSectionCode(undefined)
              setPath([])
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
              setPath([])
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
              setPath([])
            }}
            options={(sections.data?.data ?? []).map((section) => ({
              value: section.sectCode,
              label: formatShortCodeName(section.sectShortCode, section.sectCode, section.sectName),
            }))}
          />

          <Space size={token.marginXXS}>
            <Text type="secondary">Depth:</Text>
            <Select
              style={{ width: token.controlHeightLG * 2.5 }}
              value={depth}
              onChange={(value: number) => setDepth(value)}
              options={[
                { value: 2, label: '2 Levels' },
                { value: 3, label: '3 Levels' },
                { value: 4, label: '4 Levels' },
                { value: 5, label: '5 Levels' },
              ]}
            />
          </Space>
        </Space>
      </Card>

      <Space style={{ marginBottom: token.marginMD }}>
        <Breadcrumb
          items={[
            {
              title: (
                <Button type="link" size="small" icon={<HomeOutlined />} onClick={() => setPath([])} style={{ padding: 0 }}>
                  Organization
                </Button>
              ),
            },
            ...path.map((node, index) => ({
              title:
                index < path.length - 1 ? (
                  <Button
                    type="link"
                    size="small"
                    onClick={() => setPath((current) => current.slice(0, index + 1))}
                    style={{ padding: 0, maxWidth: token.controlHeightLG * 4.5, overflow: 'hidden', textOverflow: 'ellipsis' }}
                  >
                    {node.positionName}
                  </Button>
                ) : (
                  <Text strong style={{ fontSize: token.fontSize }}>{node.positionName}</Text>
                ),
            })),
          ]}
        />
      </Space>

      {isLoading ? (
        <div style={{ textAlign: 'center', padding: token.paddingXL }}>
          <Spin size="large" />
        </div>
      ) : displayNodes.length === 0 ? (
        <Empty description={search.trim() ? 'No matching positions' : 'No subordinates'} style={{ padding: token.paddingXL }} />
      ) : (
        <div style={{ overflowX: 'auto', paddingBottom: token.paddingLG }}>
          <Tree
            lineWidth={`${token.lineWidthBold}px`}
            lineColor={token.colorBorder}
            lineBorderRadius={`${token.borderRadius}px`}
            label={
              currentNode ? (
                <NodeBox node={currentNode} isRoot onDrillDown={drillDown} />
              ) : (
                <div
                  style={{
                    display: 'inline-block',
                    background: token.colorBgSpotlight,
                    color: token.colorTextLightSolid,
                    borderRadius: token.borderRadiusLG,
                    padding: `${token.paddingXS}px ${token.paddingLG}px`,
                    fontWeight: token.fontWeightStrong,
                    fontSize: token.fontSize,
                    letterSpacing: 0,
                  }}
                >
                  Organization
                </div>
              )
            }
          >
            {buildTreeNodes(displayNodes, depth - 1, drillDown)}
          </Tree>
        </div>
      )}

      <Text type="secondary" style={{ fontSize: token.fontSizeSM }}>
        * Showing {depth} levels. Click a node to drill down.
      </Text>
    </div>
  )
}
