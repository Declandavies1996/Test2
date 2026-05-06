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
  }
};
