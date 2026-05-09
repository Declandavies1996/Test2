<template>
  <main class="er-page">
    <header class="er-page-header">
      <div><h1>Release / Change Log</h1><p>Delivery history, rollback notes, and verification.</p></div>
      <button type="button" @click="startNew">New change</button>
    </header>
    <section class="er-toolbar">
      <select v-model="filters.system" @change="loadLogs">
        <option value="">All systems</option>
        <option v-for="system in systems" :key="system.id" :value="system.name">{{ system.name }}</option>
      </select>
      <input v-model.number="filters.requestId" type="number" placeholder="Request ID" @change="loadLogs" />
    </section>
    <form v-if="editing" class="er-form" @submit.prevent="saveLog">
      <div class="er-grid">
        <select v-model="form.systemName" required><option value="" disabled>System</option><option v-for="system in systems" :key="system.id" :value="system.name">{{ system.name }}</option></select>
        <input v-model.number="form.requestId" type="number" placeholder="Request ID" />
        <input v-model="form.releaseDate" type="date" required />
        <input v-model.trim="form.verifiedBy" placeholder="Verified by" />
      </div>
      <input v-model.trim="form.summary" required placeholder="Summary" />
      <textarea v-model.trim="form.filesChanged" rows="3" placeholder="Files changed"></textarea>
      <textarea v-model.trim="form.deploymentNotes" rows="3" placeholder="Deployment notes"></textarea>
      <textarea v-model.trim="form.rollbackNotes" rows="3" placeholder="Rollback notes"></textarea>
      <div class="er-actions"><button type="submit">Save</button><button type="button" @click="cancel">Cancel</button></div>
    </form>
    <p v-if="error" class="er-error">{{ error }}</p>
    <table class="er-table">
      <thead><tr><th>Date</th><th>System</th><th>Request</th><th>Summary</th><th>Verified by</th><th></th></tr></thead>
      <tbody>
        <tr v-for="log in logs" :key="log.id">
          <td>{{ formatDate(log.releaseDate) }}</td><td>{{ log.systemName }}</td><td>{{ log.requestId || '' }}</td><td>{{ log.summary }}</td><td>{{ log.verifiedBy || '' }}</td>
          <td><button type="button" @click="editLog(log)">Edit</button></td>
        </tr>
        <tr v-if="logs.length === 0"><td colspan="6">No release/change log entries.</td></tr>
      </tbody>
    </table>
  </main>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue';
import { engineeringRequestsApi } from '../api';

const logs = ref([]);
const systems = ref([]);
const editing = ref(false);
const editingId = ref(null);
const error = ref('');
const filters = reactive({ system: '', requestId: '' });
const form = reactive({ requestId: null, systemName: '', releaseDate: new Date().toISOString().slice(0, 10), summary: '', filesChanged: '', deploymentNotes: '', rollbackNotes: '', verifiedBy: '' });

function resetForm() { Object.assign(form, { requestId: null, systemName: systems.value[0]?.name || '', releaseDate: new Date().toISOString().slice(0, 10), summary: '', filesChanged: '', deploymentNotes: '', rollbackNotes: '', verifiedBy: '' }); }
function startNew() { editingId.value = null; resetForm(); editing.value = true; }
function cancel() { editing.value = false; editingId.value = null; }
function editLog(log) { editingId.value = log.id; Object.assign(form, { ...log, releaseDate: log.releaseDate.slice(0, 10) }); editing.value = true; }
async function loadLogs() { logs.value = await engineeringRequestsApi.getReleaseChangeLogs(filters); }
async function saveLog() { const payload = { ...form, requestId: form.requestId || null }; if (editingId.value) await engineeringRequestsApi.updateReleaseChangeLog(editingId.value, payload); else await engineeringRequestsApi.createReleaseChangeLog(payload); cancel(); await loadLogs(); }
function formatDate(value) { return new Date(value).toLocaleDateString(); }
onMounted(async () => { systems.value = await engineeringRequestsApi.getSystems(); await loadLogs(); });
</script>

<style scoped>
.er-page { padding: 1rem; display: grid; gap: 1rem; }
.er-page-header, .er-toolbar, .er-actions { display: flex; justify-content: space-between; gap: 0.75rem; align-items: center; flex-wrap: wrap; }
h1, p { margin: 0; } h1 { font-size: 1.5rem; } p { color: #5d6875; margin-top: 0.25rem; }
input, select, textarea, button { border: 1px solid #cfd6df; border-radius: 4px; padding: 0.55rem 0.65rem; font: inherit; }
.er-form { display: grid; gap: 0.75rem; border: 1px solid #d9dee5; border-radius: 6px; padding: 0.875rem; }
.er-grid { display: grid; gap: 0.75rem; grid-template-columns: repeat(auto-fit, minmax(190px, 1fr)); }
.er-table { width: 100%; border-collapse: collapse; } th, td { border-bottom: 1px solid #e2e7ed; padding: 0.65rem; text-align: left; vertical-align: top; }
.er-error { color: #8b1d10; }
</style>
