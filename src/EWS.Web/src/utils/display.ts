export function formatCodeName(code?: string | number | null, name?: string | null) {
  if (!code && !name) return '-'
  if (!code) return name ?? '-'
  if (!name) return String(code)
  return `${code} | ${name}`
}

export function formatShortCodeName(shortCode?: string | null, code?: string | number | null, name?: string | null) {
  return formatCodeName(shortCode || code, name)
}

export function displayWfScope(scope?: string | null) {
  if (scope === 'Ho') return 'HO'
  return scope ?? '-'
}
