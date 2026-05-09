<template>
  <main class="er-page">
    <header class="er-page-header">
      <div>
        <h1>Engineering Request Reporting</h1>
        <p>Operational workload, risk, and blocker visibility.</p>
      </div>
      <button type="button" @click="loadDashboard">Refresh</button>
    </header>

    <section class="er-filters">
      <label>
        From
        <input v-model="filters.fromDate" type="date" @change="loadDashboard" />
      </label>
      <label>
        To
        <input v-model="filters.toDate" type="date" @change="loadDashboard" />
      </label>
      <label>
        System
        <select v-model="filters.system" @change="loadDashboard">
          <option value="">All systems</option>
          <option v-for="system in systems" :key="system.id" :value="system.name">{{ system.name }}</option>
        </select>
      </label>
      <label>
        Priority
        <select v-model="filters.priority" @change="loadDashboard">
          <option value="">All priorities</option>
          <option v-for="priority in priorities" :key="priority.value" :value="priority.value">{{ priority.label }}</option>
        </select>
      </label>
      <label>
        Status
        <select v-model="filters.status" @change="loadDashboard">
          <option value="">All statuses</option>
          <option v-for="status in statuses" :key="status" :value="status">{{ status }}</option>
        </select>
      </label>
      <label>
        Type
        <select v-model="filters.type" @change="loadDashboard">
          <option value="">All types</option>
          <option v-for="type in requestTypes" :key="type" :value="type">{{ type }}</option>
        </select>
      </label>
      <label class="er-check">
        <input v-model="filters.allRequests" type="checkbox" @change="loadDashboard" />
        All requests
      </label>
    </section>

    <p v-if="error" class="er-error">{{ error }}</p>

    <section class="er-card-grid">
      <article class="er-card er-card--p1">
        <span>Open P1</span>
        <strong>{{ cards.openP1Requests }}</strong>
      </article>
      <article class="er-card er-card--p2">
        <span>Open P2</span>
        <strong>{{ cards.openP2Requests }}</strong>
      </article>
      <article class="er-card">
        <span>Total open</span>
        <strong>{{ cards.totalOpenRequests }}</strong>
      </article>
      <article class="er-card">
        <span>Created this week</span>
        <strong>{{ cards.requestsCreatedThisWeek }}</strong>
      </article>
      <article class="er-card">
        <span>Completed this week</span>
        <strong>{{ cards.requestsCompletedThisWeek }}</strong>
      </article>
      <article class="er-card er-card--waiting">
        <span>Waiting / blocked</span>
        <strong>{{ cards.requestsWaitingBlocked }}</strong>
      </article>
    </section>

    <section class="er-grid">
      <article class="er-panel">
        <h2>Open Requests by System</h2>
        <table class="er-table">
          <thead>
            <tr>
              <th>System</th>
              <th>Open</th>
              <th>P1</th>
              <th>P2</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in dashboard?.openRequestsBySystem || []" :key="item.systemName">
              <td>{{ item.systemName }}</td>
              <td>{{ item.openCount }}</td>
              <td :class="{ 'er-hot': item.p1Count > 0 }">{{ item.p1Count }}</td>
              <td :class="{ 'er-warn': item.p2Count > 0 }">{{ item.p2Count }}</td>
            </tr>
            <tr v-if="dashboard && dashboard.openRequestsBySystem.length === 0">
              <td colspan="4">No open requests.</td>
            </tr>
          </tbody>
        </table>
      </article>

      <article class="er-panel">
        <h2>Requests by Type</h2>
        <table class="er-table">
          <thead>
            <tr>
              <th>Type</th>
              <th>Count</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in dashboard?.requestsByType || []" :key="item.type">
              <td>{{ item.type }}</td>
              <td>{{ item.count }}</td>
            </tr>
          </tbody>
        </table>
      </article>
    </section>

    <section class="er-panel">
      <h2>Oldest Open Requests</h2>
      <table class="er-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Title</th>
            <th>System</th>
            <th>Priority</th>
            <th>Status</th>
            <th>Created</th>
            <th>Age</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="request in dashboard?.oldestOpenRequests || []" :key="request.id" @click="openRequest(request.id)">
            <td>{{ request.id }}</td>
            <td><strong>{{ request.title }}</strong></td>
            <td>{{ request.systemName }}</td>
            <td :class="{ 'er-hot': request.priority === 'P1', 'er-warn': request.priority === 'P2' }">{{ request.priority }}</td>
            <td>{{ request.status }}</td>
            <td>{{ formatDate(request.createdDate) }}</td>
            <td>{{ request.ageInDays }} days</td>
          </tr>
          <tr v-if="dashboard && dashboard.oldestOpenRequests.length === 0">
            <td colspan="7">No open requests.</td>
          </tr>
        </tbody>
      </table>
    </section>

    <section class="er-panel">
      <h2>Waiting Requests</h2>
      <table class="er-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Title</th>
            <th>System</th>
            <th>Reason / latest note</th>
            <th>Last updated</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="request in dashboard?.waitingRequests || []" :key="request.id" @click="openRequest(request.id)">
            <td>{{ request.id }}</td>
            <td><strong>{{ request.title }}</strong></td>
            <td>{{ request.systemName }}</td>
            <td>{{ request.reason || 'No note recorded' }}</td>
            <td>{{ formatDate(request.updatedDate) }}</td>
          </tr>
          <tr v-if="dashboard && dashboard.waitingRequests.length === 0">
            <td colspan="5">No waiting requests.</td>
          </tr>
        </tbody>
      </table>
    </section>
  </main>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { engineeringRequestsApi } from '../api';
import { priorities, requestTypes, statuses } from '../constants';

const router = useRouter();
const dashboard = ref(null);
const systems = ref([]);
const error = ref('');

const filters = reactive({
  fromDate: '',
  toDate: '',
  system: '',
  priority: '',
  status: '',
  type: '',
  allRequests: false
});

const cards = computed(() => dashboard.value?.cards || {
  openP1Requests: 0,
  openP2Requests: 0,
  totalOpenRequests: 0,
  requestsCreatedThisWeek: 0,
  requestsCompletedThisWeek: 0,
  requestsWaitingBlocked: 0
});

async function loadSystems() {
  systems.value = await engineeringRequestsApi.getSystems();
}

async function loadDashboard() {
  error.value = '';
  try {
    dashboard.value = await engineeringRequestsApi.getReportingDashboard(filters);
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
  await loadDashboard();
});
</script>

<style scoped>
.er-page {
  padding: 1rem;
  display: grid;
  gap: 1rem;
}

.er-page-header,
.er-filters,
.er-card-grid,
.er-grid {
  display: grid;
  gap: 0.75rem;
}

.er-page-header {
  grid-template-columns: 1fr auto;
  align-items: center;
}

.er-filters {
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  align-items: end;
}

.er-card-grid {
  grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
}

.er-grid {
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
}

h1,
h2,
p {
  margin: 0;
}

h1 {
  font-size: 1.5rem;
}

h2 {
  font-size: 1rem;
  margin-bottom: 0.75rem;
}

p {
  margin-top: 0.25rem;
  color: #5d6875;
}

label {
  display: grid;
  gap: 0.35rem;
  color: #3f4a56;
  font-size: 0.875rem;
}

input,
select,
button {
  border: 1px solid #cfd6df;
  border-radius: 4px;
  padding: 0.55rem 0.65rem;
  font: inherit;
}

.er-check {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
}

.er-card,
.er-panel {
  border: 1px solid #d9dee5;
  border-radius: 6px;
  background: #ffffff;
}

.er-card {
  padding: 0.875rem;
}

.er-card span {
  display: block;
  color: #5d6875;
  font-size: 0.875rem;
  margin-bottom: 0.35rem;
}

.er-card strong {
  font-size: 1.75rem;
  line-height: 1;
}

.er-card--p1 {
  border-color: #e8a497;
  background: #fff3f1;
}

.er-card--p2,
.er-card--waiting {
  border-color: #e4c56d;
  background: #fff8df;
}

.er-panel {
  padding: 0.875rem;
  overflow-x: auto;
}

.er-table {
  width: 100%;
  border-collapse: collapse;
}

th,
td {
  border-bottom: 1px solid #e2e7ed;
  padding: 0.65rem;
  text-align: left;
  vertical-align: top;
}

tbody tr {
  cursor: pointer;
}

tbody tr:hover {
  background: #f6f8fa;
}

.er-hot {
  color: #8b1d10;
  font-weight: 700;
}

.er-warn {
  color: #765100;
  font-weight: 700;
}

.er-error {
  color: #8b1d10;
}
</style>
