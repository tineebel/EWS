# EWS UI Master System Instruction

Use this instruction as the UI coding and design standard for EWS (Enterprise Workflow System). The goal is to keep every new UI consistent, clean, token-driven, and suitable for an enterprise workflow administration system.

## 1. Project History & Technology

EWS is an Enterprise Workflow System for position-based approval routing. The system focuses on workflow settings, organization structure, delegation, approval-chain visibility, and auditability.

Current stack:

| Area | Technology |
|---|---|
| Frontend | React 18, Vite, TypeScript |
| UI Library | Ant Design v5 |
| Icons | `@ant-design/icons` |
| Data Fetching | `@tanstack/react-query` v5 |
| HTTP Client | `axios` |
| Routing | `react-router-dom` v6 |
| Date Utility | `dayjs` |
| Backend | ASP.NET Core 8 Web API |
| Database | SQL Server via Entity Framework Core |

Primary frontend paths:

```txt
src/EWS.Web/src/
  api/
  pages/
  App.tsx
  main.tsx
```

## 2. Design Philosophy

Design EWS as a practical enterprise admin application, not a marketing site. The UI must be quiet, structured, data-dense, and easy to scan during repeated daily use.

Core principles:

- Prioritize clarity, speed, and predictable workflows.
- Use Ant Design components before creating custom UI.
- Keep layouts dense but readable, especially for tables, filters, drawers, and forms.
- Use status components (`Tag`, `Badge`, `Statistic`, `Descriptions`, `Table`) to make workflow state obvious.
- Design for Thai and English mixed content. Long labels and values must not overflow or overlap.
- Avoid decorative layouts, oversized hero sections, gradients, visual noise, or card-heavy marketing composition.
- Use design tokens as the single source of truth for color, spacing, typography, radius, and shadow.

## 3. Design Tokens

All UI values must come from Ant Design theme tokens or from project utility classes that map back to the same theme source. Do not hard-code visual values in class names or inline styles.

### Color Palette

| Role | Token | Default AntD Value | Usage |
|---|---|---:|---|
| Primary | `token.colorPrimary` | `#1677ff` | Primary actions, selected navigation, links |
| Primary Hover | `token.colorPrimaryHover` | derived | Button hover, interactive hover |
| Secondary | `token.colorTextSecondary` | `#8c8c8c` | Helper text, metadata, secondary labels |
| Success | `token.colorSuccess` | `#52c41a` | Active, approved, completed |
| Warning | `token.colorWarning` | `#faad14` | Pending, attention, information request |
| Error | `token.colorError` | `#ff4d4f` | Rejected, inactive, destructive action |
| Layout Background | `token.colorBgLayout` | derived | App page background |
| Surface | `token.colorBgContainer` | `#ffffff` | Cards, drawers, headers |
| Border | `token.colorBorderSecondary` | derived | Header, card, table, section borders |
| Primary Text | `token.colorText` | derived | Main content text |

Rules:

- Use semantic AntD status colors for `Tag` and `Badge` when possible, such as `green`, `red`, `blue`, `purple`, and `warning`.
- For custom style values, use `theme.useToken()` and reference tokens.
- Never use literal color values such as `#52c41a`, `#ff4d4f`, or `rgba(...)` in new UI code.

### Spacing Scale

| Intent | Value | Token Preference | Usage |
|---|---:|---|---|
| Extra small | `4px` | `token.marginXXS` / `token.paddingXXS` | Icon gaps, compact tag gaps |
| Small | `8px` | `token.marginXS` / `token.paddingXS` | Small vertical rhythm, menu spacing |
| Medium | `16px` | `token.margin` / `token.padding` | Filter toolbar, form groups |
| Large | `24px` | `token.marginLG` / `token.paddingLG` | Page margin, card separation |
| Extra large | `32px` | configured token | Major sections |
| 2XL | `48px` | configured token | Empty states and rare layout gaps |
| 3XL | `64px` | configured token | Rare page-level spacing |

Rules:

- Use Ant Design `Space`, `Row`, `Col`, `Flex`, `Form`, and `Table` layout props where possible.
- Avoid arbitrary spacing values such as `13px`, `18px`, or `22px`.
- If a value is repeated, promote it to a local theme/layout constant.

### Typography

| Role | Token / Component | Usage |
|---|---|---|
| Font Family | `token.fontFamily` | All UI text |
| Page Title | `Typography.Title level={4}` | Current route or main page title |
| Section Title | `Typography.Title level={5}` | Detail sections and drawer sections |
| Body | `token.fontSize` | Tables, descriptions, forms |
| Small Text | `token.fontSizeSM` | Metadata and helper text |
| Code | `<code>` | Position code, employee code, document code |
| Line Height | `token.lineHeight` | Normal reading text |
| Compact Line Height | `token.lineHeightSM` | Dense metadata only |

Rules:

- Do not use arbitrary font sizes in new UI code.
- Do not use negative letter spacing.
- Table columns with long content must use `ellipsis: true`.
- Keep labels short and consistent across pages.

### Border Radius & Shadow

| Role | Token | Usage |
|---|---|---|
| Base Radius | `token.borderRadius` | Buttons, inputs, cards |
| Small Radius | `token.borderRadiusSM` | Tags and compact controls |
| Large Radius | `token.borderRadiusLG` | Modal or drawer containers only |
| Main Shadow | `token.boxShadow` | Modal, dropdown, popover |
| Secondary Shadow | `token.boxShadowSecondary` | Elevated surfaces when needed |

Rules:

- Do not nest cards inside cards unless it represents a repeated item or a framed tool.
- Avoid decorative shadows.
- Use borders and spacing before shadows for layout structure.

## 4. Component Rules

### Buttons

Use Ant Design `Button`.

Rules:

- Primary command: `type="primary"`.
- Secondary command: default button.
- Destructive command: `danger`.
- Icon-only buttons must use `@ant-design/icons` and have a `Tooltip`.
- Async submit buttons must use `loading={mutation.isPending}` or an equivalent loading state.
- Disabled buttons must not trigger API calls.

Required states:

| State | Standard |
|---|---|
| Hover | Use AntD default token behavior |
| Active | Use AntD default token behavior |
| Disabled | Show disabled state and block action |
| Loading | Prevent duplicate submit |
| Error | Show via `message`, `notification`, or form validation |

### Inputs & Forms

Use Ant Design `Input`, `Select`, `DatePicker`, `Form`, `Checkbox`, `Switch`, and related form controls.

Rules:

- Search inputs use `prefix={<SearchOutlined />}` and `allowClear`.
- Filter toolbars use `Space wrap`.
- Required fields use AntD `rules`.
- Form submit must validate before API calls.
- Use controlled state for filters and selected records.
- Avoid fixed widths unless they are tokenized or paired with responsive behavior.

Required states:

| State | Standard |
|---|---|
| Focus | Use AntD default focus treatment |
| Disabled | Use for read-only or unavailable fields |
| Error | Use `Form.Item` validation |
| Loading | Use skeleton, table loading, or button loading based on context |

### Modals & Drawers

Current EWS pattern uses `Drawer` for detail and edit flows.

Rules:

- Detail view: use `Drawer`.
- Long edit form: use `Drawer`.
- Destructive confirmation: use `Modal.confirm`.
- Drawer title should include entity name and useful status when available.
- Detail loading should use `loading`, `Skeleton`, or equivalent AntD state.
- Parent component controls open/close state.
- Closing a drawer must clear the selected id or code.

Required states:

| State | Standard |
|---|---|
| Open | Controlled by parent state |
| Close | Clear selected entity |
| Loading | Show detail loading state |
| Success | Invalidate/refetch related queries |
| Error | Show actionable error message |

### Navigation

Use Ant Design `Layout`, `Sider`, `Header`, and `Menu`.

Rules:

- Main navigation lives in `Sider`.
- Menu keys must match route paths.
- Header title should derive from current route.
- Menu icons must use `@ant-design/icons`.
- Navigation state must sync with `useLocation()`.

Required states:

| State | Standard |
|---|---|
| Selected | Use `selectedKeys` |
| Hover | Use AntD default |
| Responsive | Add collapse or mobile-safe navigation when needed |

### Tables

Use Ant Design `Table` for list pages.

Rules:

- Every table must have `rowKey`.
- Every table must support loading state.
- Paginated API pages start at `page = 1`.
- Use `scroll={{ x: ... }}` when there are many columns.
- Long text columns must use `ellipsis: true`.
- Status values must use `Tag` or `Badge`.
- Clickable rows must have an obvious pointer cursor from tokenized styles or approved utility classes.

## 5. Coding Standards

### Folder Structure

Follow the current frontend structure:

```txt
src/EWS.Web/src/
  api/
    client.ts
    settings.ts
    organization.ts
    types.ts
  pages/
    Dashboard.tsx
    Positions/
    Employees/
    DocumentTypes/
    WorkflowTemplates/
    Delegations/
    OrgChart/
  App.tsx
  main.tsx
```

When adding new code:

- Put API clients in `src/api`.
- Put API and DTO types in `src/api/types.ts` or split by domain if the file becomes too large.
- Put page-level components in `src/pages/{Domain}`.
- Put reusable UI components in `src/components`.
- Put shared hooks in `src/hooks`.
- Put theme and design token configuration in `src/theme`.

### React & TypeScript

Rules:

- Use function components.
- Define explicit props interfaces.
- Avoid `any`.
- Type all API responses.
- Keep React Query keys descriptive and include filters or pagination values that affect data.
- On mutation success, invalidate or refetch related queries.
- Do not duplicate API call logic inside components.
- Split components when one file owns too many responsibilities.

### Comments & Docstrings

Rules:

- Do not comment obvious code.
- Comment only important business rules, such as approval routing, escalation, permission checks, and audit behavior.
- Comments must be short and useful for future maintenance.
- Do not leave broad TODO comments without a clear action.

### Responsive Design

Use mobile-first thinking even though EWS is an admin system.

Rules:

- Filter toolbars must wrap.
- Data tables must support horizontal scroll when needed.
- Drawer widths must be responsive.
- Text must never overlap or overflow its container.
- Avoid fixed width without a responsive fallback.
- Use AntD grid breakpoints: `xs`, `sm`, `md`, `lg`, `xl`.
- Critical actions must remain available on smaller screens.

## 6. Utility-first & Theme-only Rule

Every new UI must use a utility-first approach that is backed by the project theme configuration.

Allowed:

- Ant Design components and props.
- Ant Design theme tokens from `theme.useToken()`.
- Project utility classes that map to theme tokens.
- Semantic AntD status colors for components such as `Tag` and `Badge`.

Forbidden in new UI:

- Hard-coded colors such as `#1677ff`, `#52c41a`, `#ff4d4f`, or `rgba(...)`.
- Arbitrary spacing such as `13px`, `17px`, or `22px`.
- Arbitrary utility classes such as `p-[13px]` or `text-[#ff4d4f]`.
- Inline styles that do not use tokens.

Preferred:

```tsx
import { Card, theme } from 'antd'

export function Example() {
  const { token } = theme.useToken()

  return (
    <Card style={{ marginBottom: token.margin }}>
      Content
    </Card>
  )
}
```

Avoid:

```tsx
<Card style={{ marginBottom: 16 }}>
  Content
</Card>

<Tag color="#52c41a">Active</Tag>
```

## 7. UI Quality Checklist

Before finishing any UI task, verify:

- Colors and spacing come from token/config.
- New UI has no arbitrary hard-coded style values.
- Loading, empty, error, disabled, hover, and active states are handled.
- Tables have `rowKey`, loading, pagination, and scroll when needed.
- Forms validate before submit and prevent double submission.
- Text does not overflow, overlap, or disappear on small screens.
- Ant Design components are used before custom components.
- Icons come from `@ant-design/icons`.
- API and data fetching follow the existing project pattern.
- Code is clean, typed, scoped, and easy to maintain.

