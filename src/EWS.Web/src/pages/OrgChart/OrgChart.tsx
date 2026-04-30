import { useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { Tree, TreeNode } from 'react-organizational-chart'
import {
  Breadcrumb,
  Button,
  Card,
  Empty,
  Input,
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
import { OrgChartNode, organizationApi } from '../../api/organization'

const { Text } = Typography

const DISPLAY_DEPTH = 2

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
  const subCount = countDescendants(node)
  const hasChildren = node.children.length > 0

  return (
    <div
      style={{
        display: 'inline-block',
        background: token.colorBgContainer,
        border: `${token.lineWidthBold}px ${token.lineType} ${isRoot ? token.colorPrimary : gradeColor}`,
        borderRadius: token.borderRadiusLG,
        padding: `${token.paddingXS}px ${token.paddingSM}px`,
        minWidth: token.controlHeightLG * 4,
        maxWidth: token.controlHeightLG * 5,
        textAlign: 'left',
        boxShadow: isRoot ? token.boxShadowSecondary : token.boxShadowTertiary,
        cursor: hasChildren ? 'pointer' : 'default',
        transition: `box-shadow ${token.motionDurationMid}`,
        userSelect: 'none',
      }}
      onClick={() => hasChildren && onDrillDown(node)}
      title={hasChildren ? 'Click to view subordinates' : undefined}
    >
      <div style={{ marginBottom: token.marginXXS }}>
        <Tag
          color={gradeColor}
          style={{ margin: 0, fontSize: token.fontSizeSM, paddingInline: token.paddingXXS, lineHeight: `${token.controlHeightSM}px` }}
        >
          {node.jobGrade}
        </Tag>
        {node.secretaryCode && (
          <Tooltip title={`Secretary: ${node.secretaryCode}`}>
            <Tag color="purple" style={{ margin: `0 0 0 ${token.marginXXS}px`, fontSize: token.fontSizeSM, paddingInline: token.paddingXXS, lineHeight: `${token.controlHeightSM}px` }}>
              Secretary
            </Tag>
          </Tooltip>
        )}
      </div>

      <div
        style={{
          fontSize: token.fontSizeSM,
          fontWeight: token.fontWeightStrong,
          lineHeight: `${token.lineHeightSM * token.fontSizeSM}px`,
          marginBottom: token.marginXXS / 2,
          overflow: 'hidden',
          display: '-webkit-box',
          WebkitLineClamp: 2,
          WebkitBoxOrient: 'vertical',
        }}
        title={node.positionName}
      >
        {node.positionName}
      </div>

      <div style={{ fontSize: token.fontSizeSM, color: token.colorTextTertiary, marginBottom: token.marginXXS }}>
        {node.positionCode}
      </div>

      <div style={{ display: 'flex', alignItems: 'center', gap: token.marginXXS }}>
        <UserOutlined
          style={{ fontSize: token.fontSizeSM, color: node.isVacant ? token.colorError : token.colorSuccess }}
        />
        <span
          style={{
            fontSize: token.fontSizeSM,
            color: node.isVacant ? token.colorError : token.colorText,
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
            maxWidth: token.controlHeightLG * 3.5,
          }}
          title={node.occupantName ?? 'Vacant'}
        >
          {node.occupantName ?? 'Vacant'}
        </span>
      </div>

      {subCount > 0 && (
        <div
          style={{
            marginTop: token.marginXS,
            fontSize: token.fontSizeSM,
            color: token.colorPrimary,
            display: 'flex',
            alignItems: 'center',
            gap: token.marginXXS,
          }}
        >
          <TeamOutlined />
          {subCount} people
          {hasChildren && <DownOutlined style={{ fontSize: token.fontSizeSM }} />}
        </div>
      )}
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

  const { data, isLoading } = useQuery({
    queryKey: ['org-chart'],
    queryFn: () => organizationApi.orgChart(),
    staleTime: 5 * 60 * 1000,
  })

  const topNodes = data?.data ?? []
  const currentNode = path.length > 0 ? path[path.length - 1] : null
  const displayNodes = currentNode ? currentNode.children : topNodes
  const allNodes = useMemo(() => flattenTree(topNodes), [topNodes])

  const searchResults = useMemo(() => {
    if (!search.trim()) return []
    const query = search.toLowerCase()
    return allNodes
      .filter(
        (node) =>
          node.positionCode.toLowerCase().includes(query) ||
          node.positionName.toLowerCase().includes(query) ||
          (node.occupantName ?? '').toLowerCase().includes(query),
      )
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
                    {node.occupantName ?? 'Vacant'}
                  </Text>
                </div>
              ))}
            </div>
          )}
        </div>
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
        <Empty description="No subordinates" style={{ padding: token.paddingXL }} />
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
            {buildTreeNodes(displayNodes, DISPLAY_DEPTH - 1, drillDown)}
          </Tree>
        </div>
      )}

      <Text type="secondary" style={{ fontSize: token.fontSizeSM }}>
        * Showing {DISPLAY_DEPTH} levels. Click a node to drill down.
      </Text>
    </div>
  )
}
