<template>
  <main class="er-page">
    <header class="er-page-header">
      <div>
        <h1>Systems Register</h1>
        <p>Small operational memory for supported CAE systems.</p>
      </div>
      <button type="button" class="er-primary" @click="startNewSystem">New system</button>
    </header>

    <form v-if="editing" class="er-form" @submit.prevent="saveSystem">
      <div class="er-form-grid">
        <input v-model.trim="form.name" required placeholder="System name" />
        <input v-model.trim="form.mainUsers" placeholder="Main users" />
        <select v-model="form.criticality">
          <option v-for="criticality in criticalities" :key="criticality" :value="criticality">
            {{ criticality }}
          </option>
        </select>
      </div>
      <textarea v-model.trim="form.purpose" rows="3" placeholder="Purpose"></textarea>
      <textarea v-model.trim="form.knownRisks" rows="3" placeholder="Known risks"></textarea>
      <textarea v-model.trim="form.notes" rows="3" placeholder="Notes"></textarea>
      <div class="er-actions">
        <button type="submit" class="er-primary">Save</button>
        <button type="button" @click="cancelEdit">Cancel</button>
      </div>
    </form>

    <p v-if="error" class="er-error">{{ error }}</p>

    <table class="er-table">
      <thead>
        <tr>
          <th>Name</th>
          <th>Purpose</th>
          <th>Main users</th>
          <th>Criticality</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="system in systems" :key="system.id">
          <td><strong>{{ system.name }}</strong></td>
          <td>{{ system.purpose }}</td>
          <td>{{ system.mainUsers }}</td>
          <td>{{ system.criticality }}</td>
          <td><button type="button" @click="editSystem(system)">Edit</button></td>
        </tr>
        <tr v-if="systems.length === 0">
          <td colspan="5">No systems registered yet.</td>
        </tr>
      </tbody>
    </table>
  </main>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue';
import { engineeringRequestsApi } from '../api';
import { criticalities } from '../constants';

const systems = ref([]);
const editing = ref(false);
const editingId = ref(null);
const error = ref('');

const form = reactive({
  name: '',
  purpose: '',
  mainUsers: '',
  criticality: 'Medium',
  knownRisks: '',
  notes: ''
});

function resetForm() {
  Object.assign(form, {
    name: '',
    purpose: '',
    mainUsers: '',
    criticality: 'Medium',
    knownRisks: '',
    notes: ''
  });
}

function startNewSystem() {
  editingId.value = null;
  resetForm();
  editing.value = true;
}

function editSystem(system) {
  editingId.value = system.id;
  Object.assign(form, {
    name: system.name,
    purpose: system.purpose || '',
    mainUsers: system.mainUsers || '',
    criticality: system.criticality,
    knownRisks: system.knownRisks || '',
    notes: system.notes || ''
  });
  editing.value = true;
}

function cancelEdit() {
  editing.value = false;
  editingId.value = null;
}

async function loadSystems() {
  systems.value = await engineeringRequestsApi.getSystems();
}

async function saveSystem() {
  error.value = '';
  try {
    if (editingId.value) {
      await engineeringRequestsApi.updateSystem(editingId.value, { ...form });
    } else {
      await engineeringRequestsApi.createSystem({ ...form });
    }
    cancelEdit();
    await loadSystems();
  } catch (err) {
    error.value = err.message;
  }
}

onMounted(loadSystems);
</script>

<style scoped>
.er-page {
  padding: 1rem;
}

.er-page-header,
.er-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.75rem;
}

.er-page-header {
  margin-bottom: 1rem;
}

h1 {
  margin: 0;
  font-size: 1.5rem;
}

p {
  margin: 0.25rem 0 0;
  color: #5d6875;
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

input,
select,
textarea,
button {
  border: 1px solid #cfd6df;
  border-radius: 4px;
  padding: 0.55rem 0.65rem;
  font: inherit;
}

.er-primary {
  background: #1f5e8c;
  color: #ffffff;
  border-color: #1f5e8c;
  font-weight: 600;
}

.er-table {
  width: 100%;
  border-collapse: collapse;
}

th,
td {
  border-bottom: 1px solid #e2e7ed;
  padding: 0.7rem;
  text-align: left;
  vertical-align: top;
}

.er-error {
  color: #8b1d10;
}
</style>
