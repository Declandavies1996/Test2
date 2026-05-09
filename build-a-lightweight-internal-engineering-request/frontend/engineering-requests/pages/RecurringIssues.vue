<template>
  <main class="er-page">
    <header class="er-page-header">
      <div><h1>Recurring Issues</h1><p>Repeated problems and technical debt evidence.</p></div>
      <button type="button" @click="startNew">New issue</button>
    </header>
    <section class="er-toolbar">
      <input v-model.trim="filters.search" placeholder="Search issues" @input="loadIssues" />
      <select v-model="filters.system" @change="loadIssues">
        <option value="">All systems</option>
        <option v-for="system in systems" :key="system.id" :value="system.name">{{ system.name }}</option>
      </select>
    </section>
    <form v-if="editing" class="er-form" @submit.prevent="saveIssue">
      <div class="er-grid">
        <select v-model="form.systemName" required>
          <option value="" disabled>System</option>
          <option v-for="system in systems" :key="system.id" :value="system.name">{{ system.name }}</option>
        </select>
        <input v-model.number="form.recurrenceCount" type="number" min="1" placeholder="Recurrence count" />
        <label class="er-check"><input v-model="form.permanentFixNeeded" type="checkbox" /> Permanent fix needed</label>
      </div>
      <input v-model.trim="form.issueSummary" required placeholder="Issue summary" />
      <textarea v-model.trim="form.temporaryFix" rows="3" placeholder="Temporary fix"></textarea>
      <textarea v-model.trim="form.suspectedRootCause" rows="3" placeholder="Suspected root cause"></textarea>
      <input v-model.trim="form.relatedRequestIds" placeholder="Related request IDs, e.g. 12, 18, 22" />
      <div class="er-actions"><button type="submit">Save</button><button type="button" @click="cancel">Cancel</button></div>
    </form>
    <p v-if="error" class="er-error">{{ error }}</p>
    <table class="er-table">
      <thead><tr><th>System</th><th>Issue</th><th>Count</th><th>Permanent fix</th><th>Updated</th><th></th></tr></thead>
      <tbody>
        <tr v-for="issue in issues" :key="issue.id">
          <td>{{ issue.systemName }}</td><td>{{ issue.issueSummary }}</td><td>{{ issue.recurrenceCount }}</td>
          <td>{{ issue.permanentFixNeeded ? 'Yes' : 'No' }}</td><td>{{ formatDate(issue.updatedDate) }}</td>
          <td><button type="button" @click="editIssue(issue)">Edit</button></td>
        </tr>
        <tr v-if="issues.length === 0"><td colspan="6">No recurring issues recorded.</td></tr>
      </tbody>
    </table>
  </main>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue';
import { engineeringRequestsApi } from '../api';

const issues = ref([]);
const systems = ref([]);
const editing = ref(false);
const editingId = ref(null);
const error = ref('');
const filters = reactive({ search: '', system: '' });
const form = reactive({ systemName: '', issueSummary: '', recurrenceCount: 1, temporaryFix: '', suspectedRootCause: '', permanentFixNeeded: false, relatedRequestIds: '' });

function resetForm() { Object.assign(form, { systemName: systems.value[0]?.name || '', issueSummary: '', recurrenceCount: 1, temporaryFix: '', suspectedRootCause: '', permanentFixNeeded: false, relatedRequestIds: '' }); }
function startNew() { editingId.value = null; resetForm(); editing.value = true; }
function cancel() { editing.value = false; editingId.value = null; }
function editIssue(issue) { editingId.value = issue.id; Object.assign(form, issue); editing.value = true; }
async function loadIssues() { issues.value = await engineeringRequestsApi.getRecurringIssues(filters); }
async function saveIssue() { if (editingId.value) await engineeringRequestsApi.updateRecurringIssue(editingId.value, { ...form }); else await engineeringRequestsApi.createRecurringIssue({ ...form }); cancel(); await loadIssues(); }
function formatDate(value) { return new Date(value).toLocaleDateString(); }
onMounted(async () => { systems.value = await engineeringRequestsApi.getSystems(); await loadIssues(); });
</script>

<style scoped>
.er-page { padding: 1rem; display: grid; gap: 1rem; }
.er-page-header, .er-toolbar, .er-actions { display: flex; justify-content: space-between; gap: 0.75rem; align-items: center; flex-wrap: wrap; }
h1, p { margin: 0; } h1 { font-size: 1.5rem; } p { color: #5d6875; margin-top: 0.25rem; }
input, select, textarea, button { border: 1px solid #cfd6df; border-radius: 4px; padding: 0.55rem 0.65rem; font: inherit; }
.er-form { display: grid; gap: 0.75rem; border: 1px solid #d9dee5; border-radius: 6px; padding: 0.875rem; }
.er-grid { display: grid; gap: 0.75rem; grid-template-columns: repeat(auto-fit, minmax(190px, 1fr)); }
.er-check { display: flex; align-items: center; gap: 0.5rem; }
.er-table { width: 100%; border-collapse: collapse; } th, td { border-bottom: 1px solid #e2e7ed; padding: 0.65rem; text-align: left; vertical-align: top; }
.er-error { color: #8b1d10; }
</style>
