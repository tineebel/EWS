import { useState, useMemo } from 'react'
import { Tree, TreeNode } from 'react-organizational-chart'
import {
  Card, Breadcrumb, Input, Tag, Spin, Empty, Typography, Space, Tooltip, Button,
} from 'antd'
import {
  HomeOutlined, SearchOutlined, UserOutlined, TeamOutlined, DownOutlined,
} from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { organizationApi, OrgChartNode } from '../../api/organization'

const { Text } = Typography

const DISPLAY_DEPTH = 2 // levels to render at once

const gradeColor: Record<string, string> = {
  A0: '#cf1322', A1: '#d4380d', A2: '#d46b08', A3: '#d48806',
  B0: '#096dd9', B1: '#0958d9', B2: '#1677ff',
  C1: '#389e0d', C3: '#389e0d', D1: '#8c8c8c',
}

function countDescendants(node: OrgChartNode): number {
  return node.children.reduce((s, c) => s + 1 + countDescendants(c), 0)
}

function flattenTree(nodes: OrgChartNode[]): OrgChartNode[] {
  return nodes.flatMap(n => [n, ...flattenTree(n.children)])
}

// ── Node card ───────────────────────────────────────────────
function NodeBox({
  node,
  isRoot = false,
  onDrillDown,
}: {
  node: OrgChartNode
  isRoot?: boolean
  onDrillDown: (n: OrgChartNode) => void
}) {
  const color = gradeColor[node.jobGrade] ?? '#8c8c8c'
  const subCount = countDescendants(node)
  const hasChildren = node.children.length > 0

  return (
    <div
      style={{
        display: 'inline-block',
        background: '#fff',
        border: `2px solid ${isRoot ? '#1677ff' : color}`,
        borderRadius: 8,
        padding: '8px 12px',
        minWidth: 160,
        maxWidth: 200,
        textAlign: 'left',
        boxShadow: isRoot
          ? '0 0 0 3px #bae0ff, 0 2px 8px rgba(0,0,0,0.12)'
          : '0 1px 4px rgba(0,0,0,0.08)',
        cursor: hasChildren ? 'pointer' : 'default',
        transition: 'box-shadow 0.2s',
        userSelect: 'none',
      }}
      onClick={() => hasChildren && onDrillDown(node)}
      title={hasChildren ? `คลิกเพื่อดู subordinates` : undefined}
    >
      {/* Grade badge */}
      <div style={{ marginBottom: 4 }}>
        <Tag
          color={color}
          style={{ margin: 0, fontSize: 10, padding: '0 5px', lineHeight: '16px' }}
        >
          {node.jobGrade}
        </Tag>
        {node.secretaryCode && (
          <Tooltip title={`เลขา: ${node.secretaryCode}`}>
            <Tag color="purple" style={{ margin: '0 0 0 4px', fontSize: 10, padding: '0 4px', lineHeight: '16px' }}>
              เลขา
            </Tag>
          </Tooltip>
        )}
      </div>

      {/* Position name */}
      <div
        style={{
          fontSize: 12,
          fontWeight: 600,
          lineHeight: '16px',
          marginBottom: 2,
          overflow: 'hidden',
          display: '-webkit-box',
          WebkitLineClamp: 2,
          WebkitBoxOrient: 'vertical',
        }}
        title={node.positionName}
      >
        {node.positionName}
      </div>

      {/* Position code */}
      <div style={{ fontSize: 10, color: '#8c8c8c', marginBottom: 4 }}>
        {node.positionCode}
      </div>

      {/* Occupant */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
        <UserOutlined
          style={{ fontSize: 10, color: node.isVacant ? '#ff4d4f' : '#52c41a' }}
        />
        <span
          style={{
            fontSize: 11,
            color: node.isVacant ? '#ff4d4f' : '#262626',
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
            maxWidth: 140,
          }}
          title={node.occupantName ?? 'ว่าง'}
        >
          {node.occupantName ?? 'ว่าง'}
        </span>
      </div>

      {/* Subordinate count (leaf of current view) */}
      {subCount > 0 && (
        <div
          style={{
            marginTop: 6,
            fontSize: 10,
            color: '#1677ff',
            display: 'flex',
            alignItems: 'center',
            gap: 3,
          }}
        >
          <TeamOutlined />
          {subCount} คน
          {hasChildren && <DownOutlined style={{ fontSize: 8 }} />}
        </div>
      )}
    </div>
  )
}

// ── Recursive TreeNode builder (limited depth) ──────────────
function buildTreeNodes(
  nodes: OrgChartNode[],
  depth: number,
  onDrillDown: (n: OrgChartNode) => void,
): React.ReactNode {
  return nodes.map(node => (
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

// ── Main page ───────────────────────────────────────────────
export default function OrgChart() {
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
    const q = search.toLowerCase()
    return allNodes
      .filter(
        n =>
          n.positionCode.toLowerCase().includes(q) ||
          n.positionName.toLowerCase().includes(q) ||
          (n.occupantName ?? '').toLowerCase().includes(q),
      )
      .slice(0, 15)
  }, [search, allNodes])

  function drillDown(node: OrgChartNode) {
    setPath(prev => [...prev, node])
  }

  function jumpTo(node: OrgChartNode) {
    // Build path from root to node
    function findPath(nodes: OrgChartNode[], target: number): OrgChartNode[] | null {
      for (const n of nodes) {
        if (n.positionId === target) return [n]
        const sub = findPath(n.children, target)
        if (sub) return [n, ...sub]
      }
      return null
    }
    const fullPath = findPath(topNodes, node.positionId)
    if (fullPath) setPath(fullPath)
    setSearch('')
  }

  return (
    <div>
      {/* Search */}
      <Card size="small" style={{ marginBottom: 16 }}>
        <div style={{ position: 'relative', display: 'inline-block' }}>
          <Input
            prefix={<SearchOutlined />}
            placeholder="ค้นหา Position / ชื่อตำแหน่ง / พนักงาน"
            value={search}
            onChange={e => setSearch(e.target.value)}
            allowClear
            style={{ width: 340 }}
          />
          {searchResults.length > 0 && (
            <div
              style={{
                position: 'absolute',
                top: '100%',
                left: 0,
                zIndex: 100,
                background: '#fff',
                border: '1px solid #d9d9d9',
                borderRadius: 6,
                boxShadow: '0 4px 12px rgba(0,0,0,0.12)',
                marginTop: 2,
                width: 340,
                maxHeight: 300,
                overflowY: 'auto',
              }}
            >
              {searchResults.map(n => (
                <div
                  key={n.positionId}
                  onClick={() => jumpTo(n)}
                  style={{
                    padding: '7px 12px',
                    cursor: 'pointer',
                    borderBottom: '1px solid #f5f5f5',
                    display: 'flex',
                    alignItems: 'center',
                    gap: 8,
                  }}
                  onMouseEnter={e => (e.currentTarget.style.background = '#f5f5f5')}
                  onMouseLeave={e => (e.currentTarget.style.background = '')}
                >
                  <Tag
                    color={gradeColor[n.jobGrade] ?? '#8c8c8c'}
                    style={{ margin: 0, fontSize: 10, padding: '0 4px', lineHeight: '16px', flexShrink: 0 }}
                  >
                    {n.jobGrade}
                  </Tag>
                  <Text style={{ fontSize: 12, flex: 1 }} ellipsis>
                    {n.positionName}
                  </Text>
                  <Text type="secondary" style={{ fontSize: 11, flexShrink: 0 }}>
                    {n.occupantName ?? 'ว่าง'}
                  </Text>
                </div>
              ))}
            </div>
          )}
        </div>
      </Card>

      {/* Breadcrumb */}
      <Space style={{ marginBottom: 16 }}>
        <Breadcrumb
          items={[
            {
              title: (
                <Button type="link" size="small" icon={<HomeOutlined />} onClick={() => setPath([])} style={{ padding: 0 }}>
                  Organization
                </Button>
              ),
            },
            ...path.map((n, i) => ({
              title:
                i < path.length - 1 ? (
                  <Button
                    type="link"
                    size="small"
                    onClick={() => setPath(prev => prev.slice(0, i + 1))}
                    style={{ padding: 0, maxWidth: 180, overflow: 'hidden', textOverflow: 'ellipsis' }}
                  >
                    {n.positionName}
                  </Button>
                ) : (
                  <Text strong style={{ fontSize: 13 }}>{n.positionName}</Text>
                ),
            })),
          ]}
        />
      </Space>

      {/* Org Chart */}
      {isLoading ? (
        <div style={{ textAlign: 'center', padding: 80 }}>
          <Spin size="large" />
        </div>
      ) : displayNodes.length === 0 ? (
        <Empty description="ไม่มีผู้ใต้บังคับบัญชา" style={{ padding: 60 }} />
      ) : (
        <div style={{ overflowX: 'auto', paddingBottom: 24 }}>
          <Tree
            lineWidth="2px"
            lineColor="#d9d9d9"
            lineBorderRadius="6px"
            label={
              currentNode ? (
                <NodeBox node={currentNode} isRoot onDrillDown={drillDown} />
              ) : (
                <div
                  style={{
                    display: 'inline-block',
                    background: '#001529',
                    color: '#fff',
                    borderRadius: 8,
                    padding: '8px 20px',
                    fontWeight: 700,
                    fontSize: 14,
                    letterSpacing: 1,
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

      <Text type="secondary" style={{ fontSize: 11 }}>
        * แสดง {DISPLAY_DEPTH} ระดับ — คลิก node เพื่อ drill-down
      </Text>
    </div>
  )
}
