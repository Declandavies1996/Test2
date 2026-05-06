<template>
  <main class="er-page">
    <header class="er-page-header">
      <div>
        <h1>Engineering Requests</h1>
        <p>Operational request queue for CAE systems support.</p>
      </div>
      <button type="button" class="er-primary" @click="startNewRequest">New request</button>
    </header>

    <section class="er-toolbar">
      <input v-model.trim="filters.search" type="search" placeholder="Search requests" @input="loadRequests" />
      <select v-model="filters.status" @change="loadRequests">
        <option value="">All statuses</option>
        <option v-for="status in statuses" :key="status" :value="status">{{ status }}</option>
      </select>
      <select v-model="filters.system" @change="loadRequests">
        <option value="">All systems</option>
        <option v-for="system in systems" :key="system.id" :value="system.name">{{ system.name }}</option>
      </select>
    </section>

    <form v-if="editing" class="er-form" @submit.prevent="saveRequest">
      <input v-model.trim="form.title" required placeholder="Short title" />
      <textarea v-model.trim="form.description" rows="3" placeholder="Description"></textarea>
      <div class="er-form-grid">
        <select v-model="form.systemName" required>
          <option value="" disabled>System</option>
          <option v-for="system in systems" :key="system.id" :value="system.name">{{ system.name }}</option>
        </select>
        <input v-model.trim="form.requestedBy" placeholder="Requested by" />
        <input v-model.trim="form.department" placeholder="Department" />
        <select v-model="form.priority">
          <option v-for="priority in priorities" :key="priority.value" :value="priority.value">
            {{ priority.label }} - {{ priority.description }}
          </option>
        </select>
        <select v-model="form.status">
          <option v-for="status in statuses" :key="status" :value="status">{{ status }}</option>
        </select>
        <select v-model="form.type">
          <option v-for="type in requestTypes" :key="type" :value="type">{{ type }}</option>
        </select>
      </div>
      <textarea v-model.trim="form.notes" rows="2" placeholder="Initial notes"></textarea>
      <div class="er-actions">
        <button type="submit" class="er-primary">Save</button>
        <button type="button" @click="editing = false">Cancel</button>
      </div>
    </form>

    <p v-if="error" class="er-error">{{ error }}</p>

    <table class="er-table">
      <thead>
        <tr>
          <th>Request</th>
          <th>System</th>
          <th>Priority</th>
          <th>Status</th>
          <th>Type</th>
          <th>Updated</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="request in requests" :key="request.id" @click="openRequest(request.id)">
          <td>
            <strong>{{ request.title }}</strong>
            <span>{{ request.requestedBy || 'Unknown requester' }}</span>
          </td>
          <td>{{ request.systemName }}</td>
          <td><StatusBadge :value="request.priority" /></td>
          <td><StatusBadge :value="request.status" /></td>
          <td>{{ request.type }}</td>
          <td>{{ formatDate(request.updatedDate) }}</td>
        </tr>
        <tr v-if="!loading && requests.length === 0">
          <td colspan="6">No requests found.</td>
        </tr>
      </tbody>
    </table>
  </main>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { engineeringRequestsApi } from '../api';
import { priorities, requestTypes, statuses } from '../constants';
import StatusBadge from '../components/StatusBadge.vue';

const router = useRouter();
const requests = ref([]);
const systems = ref([]);
const loading = ref(false);
const editing = ref(false);
const error = ref('');

const filters = reactive({
  search: '',
  status: '',
  system: ''
});

const form = reactive({
  title: '',
  description: '',
  systemName: '',
  requestedBy: '',
  department: '',
  priority: 'P3',
  status: 'Incoming',
  type: 'Support',
  notes: ''
});

function resetForm() {
  Object.assign(form, {
    title: '',
    description: '',
    systemName: systems.value[0]?.name || '',
    requestedBy: '',
    department: '',
    priority: 'P3',
    status: 'Incoming',
    type: 'Support',
    notes: ''
  });
}

function startNewRequest() {
  resetForm();
  editing.value = true;
}

async function loadRequests() {
  loading.value = true;
  error.value = '';
  try {
    requests.value = await engineeringRequestsApi.getRequests(filters);
  } catch (err) {
    error.value = err.message;
  } finally {
    loading.value = false;
  }
}

async function loadSystems() {
  systems.value = await engineeringRequestsApi.getSystems();
}

async function saveRequest() {
  error.value = '';
  try {
    await engineeringRequestsApi.createRequest({ ...form });
    editing.value = false;
    await loadRequests();
  } catch (err) {
    error.value = err.message;
  }
}

function openRequest(id) {
  router.push({ name: 'engineeringRequestDetails', params: { id } });
}

function formatDate(value) {
  return new Date(value).toLocaleDateString();
}

onMounted(async () => {
  await loadSystems();
  await loadRequests();
});
</script>

<style scoped>
.er-page {
  padding: 1rem;
}

.er-page-header,
.er-toolbar,
.er-actions {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.er-page-header {
  justify-content: space-between;
  margin-bottom: 1rem;
}

h1 {
  font-size: 1.5rem;
  margin: 0;
}

p {
  margin: 0.25rem 0 0;
  color: #5d6875;
}

.er-toolbar {
  margin-bottom: 1rem;
  flex-wrap: wrap;
}

input,
select,
textarea,
button {
  border: 1px solid #cfd6df;
  border-radius: 4px;
  padding: 0.55rem 0.65rem;
  font: inherit;
}

input[type='search'] {
  min-width: 260px;
}

.er-primary {
  background: #1f5e8c;
  color: #ffffff;
  border-color: #1f5e8c;
  font-weight: 600;
}

.er-form {
  display: grid;
  gap: 0.75rem;
  border: 1px solid #d9dee5;
  border-radius: 6px;
  padding: 0.875rem;
  margin-bottom: 1rem;
}

.er-form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 0.75rem;
}

.er-table {
  width: 100%;
  border-collapse: collapse;
  background: #ffffff;
}

th,
td {
  border-bottom: 1px solid #e2e7ed;
  padding: 0.7rem;
  text-align: left;
}

tbody tr {
  cursor: pointer;
}

tbody tr:hover {
  background: #f6f8fa;
}

td span {
  display: block;
  color: #6b7580;
  font-size: 0.8125rem;
  margin-top: 0.2rem;
}

.er-error {
  color: #8b1d10;
  margin-bottom: 0.75rem;
}
</style>
