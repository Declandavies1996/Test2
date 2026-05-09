<template>
  <main class="er-page">
    <header><h1>My Submitted Requests</h1><p>Requests you submitted for triage and follow-up.</p></header>
    <p v-if="error" class="er-error">{{ error }}</p>
    <table class="er-table">
      <thead><tr><th>ID</th><th>Title</th><th>Status</th><th>Priority</th><th>System</th><th>Created</th><th>Updated</th></tr></thead>
      <tbody>
        <tr v-for="request in requests" :key="request.id" @click="openRequest(request.id)">
          <td>{{ request.id }}</td><td>{{ request.title }}</td><td><StatusBadge :value="request.status" /></td><td><StatusBadge :value="request.priority" /></td><td>{{ request.systemName }}</td><td>{{ formatDate(request.createdDate) }}</td><td>{{ formatDate(request.updatedDate) }}</td>
        </tr>
        <tr v-if="requests.length === 0"><td colspan="7">You have not submitted any requests.</td></tr>
      </tbody>
    </table>
  </main>
</template>

<script setup>
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { engineeringRequestsApi } from '../api';
import StatusBadge from '../components/StatusBadge.vue';

const router = useRouter();
const requests = ref([]);
const error = ref('');

function openRequest(id) {
  router.push({ name: 'engineeringRequestDetails', params: { id } });
}

function formatDate(value) {
  return new Date(value).toLocaleDateString();
}

onMounted(async () => {
  try {
    requests.value = await engineeringRequestsApi.getMySubmittedRequests();
  } catch (err) {
    error.value = err.message;
  }
});
</script>

<style scoped>
.er-page { padding: 1rem; display: grid; gap: 1rem; }
h1, p { margin: 0; } h1 { font-size: 1.5rem; } p { color: #5d6875; margin-top: 0.25rem; }
.er-table { width: 100%; border-collapse: collapse; background: #fff; }
th, td { border-bottom: 1px solid #e2e7ed; padding: 0.65rem; text-align: left; }
tbody tr { cursor: pointer; }
tbody tr:hover { background: #f6f8fa; }
.er-error { color: #8b1d10; }
</style>
