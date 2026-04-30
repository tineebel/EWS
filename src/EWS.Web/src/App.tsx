import { Layout, Menu, Typography, theme } from 'antd'
import {
  ApartmentOutlined,
  TeamOutlined,
  FileTextOutlined,
  BranchesOutlined,
  SwapOutlined,
  DashboardOutlined,
} from '@ant-design/icons'
import { Routes, Route, useNavigate, useLocation } from 'react-router-dom'
import Dashboard from './pages/Dashboard'
import PositionList from './pages/Positions/PositionList'
import EmployeeList from './pages/Employees/EmployeeList'
import DocumentTypeList from './pages/DocumentTypes/DocumentTypeList'
import TemplateList from './pages/WorkflowTemplates/TemplateList'
import DelegationList from './pages/Delegations/DelegationList'
import OrgChart from './pages/OrgChart/OrgChart'

const { Sider, Header, Content } = Layout

const menuItems = [
  { key: '/', icon: <DashboardOutlined />, label: 'Dashboard' },
  { key: '/positions', icon: <ApartmentOutlined />, label: 'Positions' },
  { key: '/employees', icon: <TeamOutlined />, label: 'Employees' },
  { key: '/document-types', icon: <FileTextOutlined />, label: 'Document Types' },
  { key: '/workflow-templates', icon: <BranchesOutlined />, label: 'Workflow Templates' },
  { key: '/delegations', icon: <SwapOutlined />, label: 'Delegations' },
  { key: '/org-chart', icon: <ApartmentOutlined />, label: 'Org Chart' },
]

export default function App() {
  const navigate = useNavigate()
  const location = useLocation()
  const { token } = theme.useToken()

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider theme="dark" width={220}>
        <div
          style={{
            padding: `${token.paddingMD}px ${token.paddingLG}px`,
            borderBottom: `${token.lineWidth}px ${token.lineType} ${token.colorSplit}`,
          }}
        >
          <Typography.Text style={{ color: token.colorTextLightSolid, fontWeight: token.fontWeightStrong, fontSize: token.fontSizeLG }}>
            EWS Settings
          </Typography.Text>
          <br />
          <Typography.Text style={{ color: token.colorTextQuaternary, fontSize: token.fontSizeSM }}>
            Enterprise Workflow System
          </Typography.Text>
        </div>
        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[location.pathname]}
          items={menuItems}
          onClick={({ key }) => navigate(key)}
          style={{ marginTop: token.marginXS }}
        />
      </Sider>
      <Layout>
        <Header
          style={{
            background: token.colorBgContainer,
            paddingInline: token.paddingLG,
            borderBottom: `${token.lineWidth}px ${token.lineType} ${token.colorBorderSecondary}`,
          }}
        >
          <Typography.Title level={4} style={{ margin: `${token.marginMD}px 0 0` }}>
            {menuItems.find(m => m.key === location.pathname)?.label ?? 'EWS Settings'}
          </Typography.Title>
        </Header>
        <Content style={{ margin: token.marginLG }}>
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/positions" element={<PositionList />} />
            <Route path="/employees" element={<EmployeeList />} />
            <Route path="/document-types" element={<DocumentTypeList />} />
            <Route path="/workflow-templates" element={<TemplateList />} />
            <Route path="/delegations" element={<DelegationList />} />
            <Route path="/org-chart" element={<OrgChart />} />
          </Routes>
        </Content>
      </Layout>
    </Layout>
  )
}
