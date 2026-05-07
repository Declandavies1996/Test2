export const priorities = [
  { value: 'P1', label: 'P1', description: 'Production/engineering blocked' },
  { value: 'P2', label: 'P2', description: 'Important operational issue' },
  { value: 'P3', label: 'P3', description: 'Planned improvement' },
  { value: 'P4', label: 'P4', description: 'Nice-to-have' }
];

export const statuses = ['Incoming', 'Planned', 'InProgress', 'Waiting', 'Done'];

export const requestTypes = [
  'Bug',
  'Feature',
  'Support',
  'Validation',
  'Investigation',
  'TechnicalDebt'
];

export const criticalities = ['Low', 'Medium', 'High', 'Critical'];

export const runbookCategories = [
  'Deployment',
  'Validation',
  'DataFix',
  'ImportExport',
  'Infrastructure',
  'Troubleshooting',
  'Other'
];
