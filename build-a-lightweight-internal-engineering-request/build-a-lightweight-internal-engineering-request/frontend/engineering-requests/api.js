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

  getRequests(filters = {}) {
    const params = new URLSearchParams();
    if (filters.search) params.set('search', filters.search);
    if (filters.status) params.set('status', filters.status);
    if (filters.system) params.set('system', filters.system);

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
  }
};
