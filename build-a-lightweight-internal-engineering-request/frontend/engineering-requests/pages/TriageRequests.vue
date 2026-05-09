<template>
  <main class="er-page">
    <header><h1>Request Triage</h1><p>Review incoming user-submitted requests and assign ownership, priority, type, and system.</p></header>
    <RequestGuidance />
    <p v-if="error" class="er-error">{{ error }}</p>
    <section class="er-list">
      <article v-for="request in requests" :key="request.id" class="er-card">
        <header class="er-card-header">
          <div><h2>#{{ request.id }} {{ request.title }}</h2><p>{{ request.submittedByUserName || request.requestedBy || 'Unknown user' }} - {{ formatDate(request.createdDate) }}</p></div>
        </header>
        <form class="er-form" @submit.prevent="triage(request.id)">
          <div class="er-grid">
            <select v-model="forms[request.id].systemName" required>
              <option value="" disabled>System</option>
              <option v-for="system in systems" :key="system.id" :value="system.name">{{ system.name }}</option>
            </select>
            <select v-model="forms[request.id].priority"><option v-for="priority in priorities" :key="priority.value" :value="priority.value">{{ priority.label }}</option></select>
            <select v-model="forms[request.id].status"><option v-for="status in statuses" :key="status" :value="status">{{ status }}</option></select>
            <select v-model="forms[request.id].type"><option v-for="type in requestTypes" :key="type" :value="type">{{ type }}</option></select>
            <input v-model.trim="forms[request.id].ownerUserName" placeholder="Owner username" />
          </div>
          <textarea v-model.trim="forms[request.id].notes" rows="3" placeholder="Triage notes"></textarea>
          <button type="submit">Complete triage</button>
        </form>
      </article>
      <p v-if="requests.length === 0" class="er-empty">No requests waiting for triage.</p>
    </section>
  </main>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue';
import { engineeringRequestsApi } from '../api';
import { priorities, requestTypes, statuses } from '../constants';
import RequestGuidance from '../components/RequestGuidance.vue';

const requests = ref([]);
const systems = ref([]);
const forms = reactive({});
const error = ref('');

async function load() {
  requests.value = await engineeringRequestsApi.getTriageRequests();
  requests.value.forEach(request => {
    forms[request.id] = {
      systemName: request.systemName === 'Unassigned' ? '' : request.systemName,
      priority: request.priority || 'P3',
      status: 'Planned',
      type: request.type || 'Investigation',
      ownerUserName: request.ownerUserName || '',
      notes: ''
    };
  });
}

async function triage(id) {
  error.value = '';
  try {
    await engineeringRequestsApi.triageRequest(id, forms[id]);
    await load();
  } catch (err) {
    error.value = err.message;
  }
}

function formatDate(value) {
  return new Date(value).toLocaleString();
}

onMounted(async () => {
  systems.value = await engineeringRequestsApi.getSystems();
  await load();
});
</script>

<style scoped>
.er-page { padding: 1rem; display: grid; gap: 1rem; }
h1, h2, p { margin: 0; } h1 { font-size: 1.5rem; } h2 { font-size: 1rem; }
p { color: #5d6875; margin-top: 0.25rem; }
.er-list, .er-form { display: grid; gap: 0.75rem; }
.er-card { border: 1px solid #d9dee5; border-radius: 6px; padding: 0.875rem; background: #fff; }
.er-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(170px, 1fr)); gap: 0.75rem; }
input, select, textarea, button { border: 1px solid #cfd6df; border-radius: 4px; padding: 0.55rem 0.65rem; font: inherit; }
button { background: #1f5e8c; color: #fff; border-color: #1f5e8c; font-weight: 700; }
.er-error { color: #8b1d10; }
.er-empty { border: 1px solid #d9dee5; border-radius: 6px; padding: 1rem; background: #fff; }
</style>
