const baseUrl = '/api';

async function requestJson(url, options = {}) {
  const response = await fetch(url, {
    headers: {
      'Content-Type': 'application/json',
      ...(options.headers || {})
    },
    ...options
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed: ${response.status}`);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

export const engineeringRequestsApi = {
  getSummary() {
    return requestJson(`${baseUrl}/engineering-requests/summary`);
  },

  getReportingDashboard(filters = {}) {
    const params = new URLSearchParams();
    if (filters.fromDate) params.set('fromDate', filters.fromDate);
    if (filters.toDate) params.set('toDate', filters.toDate);
    if (filters.system) params.set('system', filters.system);
    if (filters.priority) params.set('priority', filters.priority);
    if (filters.status) params.set('status', filters.status);
    if (filters.type) params.set('type', filters.type);
    if (filters.allRequests) params.set('allRequests', 'true');

    const query = params.toString();
    return requestJson(`${baseUrl}/engineering-requests/reporting${query ? `?${query}` : ''}`);
  },

  getWeeklyManagementSummary(filters = {}) {
    const params = new URLSearchParams();
    if (filters.allRequests) params.set('allRequests', 'true');
    const query = params.toString();
    return requestJson(`${baseUrl}/engineering-requests/weekly-management-summary${query ? `?${query}` : ''}`);
  },

  getSystemRiskDashboard() {
    return requestJson(`${baseUrl}/engineering-requests/system-risk-dashboard`);
  },

  getRequests(filters = {}) {
    const params = new URLSearchParams();
    if (filters.search) params.set('search', filters.search);
    if (filters.status) params.set('status', filters.status);
    if (filters.system) params.set('system', filters.system);
    if (filters.allRequests) params.set('allRequests', 'true');

    const query = params.toString();
    return requestJson(`${baseUrl}/engineering-requests${query ? `?${query}` : ''}`);
  },

  getRequest(id) {
    return requestJson(`${baseUrl}/engineering-requests/${id}`);
  },

  createRequest(payload) {
    return requestJson(`${baseUrl}/engineering-requests`, {
      method: 'POST',
      body: JSON.stringify(payload)
    });
  },

  updateRequest(id, payload) {
    return requestJson(`${baseUrl}/engineering-requests/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload)
    });
  },

  deleteRequest(id) {
    return requestJson(`${baseUrl}/engineering-requests/${id}`, {
      method: 'DELETE'
    });
  },

  addNote(requestId, payload) {
    return requestJson(`${baseUrl}/engineering-requests/${requestId}/notes`, {
      method: 'POST',
      body: JSON.stringify(payload)
    });
  },

  uploadAttachment(requestId, file, uploadedBy) {
    const formData = new FormData();
    formData.append('file', file);
    if (uploadedBy) formData.append('uploadedBy', uploadedBy);

    return fetch(`${baseUrl}/engineering-requests/${requestId}/attachments`, {
      method: 'POST',
      body: formData
    }).then(async response => {
      if (!response.ok) {
        const message = await response.text();
        throw new Error(message || `Upload failed: ${response.status}`);
      }
      return response.json();
    });
  },

  attachmentUrl(attachmentId) {
    return `${baseUrl}/engineering-requests/attachments/${attachmentId}`;
  },

  submitRequest(payload, files = [], ownerToken = '') {
    const formData = new FormData();
    Object.entries(payload).forEach(([key, value]) => {
      if (value !== undefined && value !== null) formData.append(key, value);
    });
    if (ownerToken) formData.append('ownerToken', ownerToken);
    files.forEach(file => formData.append('attachments', file));

    return fetch(`${baseUrl}/engineering-requests/submit`, {
      method: 'POST',
      body: formData
    }).then(async response => {
      if (!response.ok) {
        const message = await response.text();
        throw new Error(message || `Submit failed: ${response.status}`);
      }
      return response.json();
    });
  },

  getMySubmittedRequests() {
    return requestJson(`${baseUrl}/engineering-requests/my-submitted`);
  },

  getTriageRequests() {
    return requestJson(`${baseUrl}/engineering-requests/triage`);
  },

  getMySubmissionLink() {
    return requestJson(`${baseUrl}/submission-links/mine`);
  },

  getSubmissionLink(token) {
    return requestJson(`${baseUrl}/submission-links/${token}`);
  },

  triageRequest(id, payload) {
    return requestJson(`${baseUrl}/engineering-requests/${id}/triage`, {
      method: 'POST',
      body: JSON.stringify(payload)
    });
  },

  getSystems() {
    return requestJson(`${baseUrl}/engineering-systems`);
  },

  createSystem(payload) {
    return requestJson(`${baseUrl}/engineering-systems`, {
      method: 'POST',
      body: JSON.stringify(payload)
    });
  },

  updateSystem(id, payload) {
    return requestJson(`${baseUrl}/engineering-systems/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload)
    });
  },

  deleteSystem(id) {
    return requestJson(`${baseUrl}/engineering-systems/${id}`, {
      method: 'DELETE'
    });
  },

  getRunbooks(filters = {}) {
    const params = new URLSearchParams();
    if (filters.search) params.set('search', filters.search);
    if (filters.system) params.set('system', filters.system);
    if (filters.category) params.set('category', filters.category);

    const query = params.toString();
    return requestJson(`${baseUrl}/runbooks${query ? `?${query}` : ''}`);
  },

  getRunbook(id) {
    return requestJson(`${baseUrl}/runbooks/${id}`);
  },

  createRunbook(payload) {
    return requestJson(`${baseUrl}/runbooks`, {
      method: 'POST',
      body: JSON.stringify(payload)
    });
  },

  updateRunbook(id, payload) {
    return requestJson(`${baseUrl}/runbooks/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload)
    });
  },

  deleteRunbook(id) {
    return requestJson(`${baseUrl}/runbooks/${id}`, {
      method: 'DELETE'
    });
  },

  linkRunbookToRequest(requestId, runbookId, changedBy) {
    const params = new URLSearchParams();
    if (changedBy) params.set('changedBy', changedBy);
    const query = params.toString();

    return requestJson(`${baseUrl}/engineering-requests/${requestId}/runbooks${query ? `?${query}` : ''}`, {
      method: 'POST',
      body: JSON.stringify({ runbookId })
    });
  },

  unlinkRunbookFromRequest(requestId, runbookId, changedBy) {
    const params = new URLSearchParams();
    if (changedBy) params.set('changedBy', changedBy);
    const query = params.toString();

    return requestJson(`${baseUrl}/engineering-requests/${requestId}/runbooks/${runbookId}${query ? `?${query}` : ''}`, {
      method: 'DELETE'
    });
  },

  getRecurringIssues(filters = {}) {
    const params = new URLSearchParams();
    if (filters.search) params.set('search', filters.search);
    if (filters.system) params.set('system', filters.system);
    const query = params.toString();
    return requestJson(`${baseUrl}/recurring-issues${query ? `?${query}` : ''}`);
  },

  createRecurringIssue(payload) {
    return requestJson(`${baseUrl}/recurring-issues`, {
      method: 'POST',
      body: JSON.stringify(payload)
    });
  },

  updateRecurringIssue(id, payload) {
    return requestJson(`${baseUrl}/recurring-issues/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload)
    });
  },

  deleteRecurringIssue(id) {
    return requestJson(`${baseUrl}/recurring-issues/${id}`, {
      method: 'DELETE'
    });
  },

  getReleaseChangeLogs(filters = {}) {
    const params = new URLSearchParams();
    if (filters.system) params.set('system', filters.system);
    if (filters.requestId) params.set('requestId', filters.requestId);
    const query = params.toString();
    return requestJson(`${baseUrl}/release-change-logs${query ? `?${query}` : ''}`);
  },

  createReleaseChangeLog(payload) {
    return requestJson(`${baseUrl}/release-change-logs`, {
      method: 'POST',
      body: JSON.stringify(payload)
    });
  },

  updateReleaseChangeLog(id, payload) {
    return requestJson(`${baseUrl}/release-change-logs/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload)
    });
  },

  deleteReleaseChangeLog(id) {
    return requestJson(`${baseUrl}/release-change-logs/${id}`, {
      method: 'DELETE'
    });
  }
};
