<template>
  <main class="er-page">
    <header class="er-page-header">
      <div>
        <h1>Runbooks</h1>
        <p>Searchable fix guides for repeated engineering support work.</p>
      </div>
      <button type="button" class="er-primary" @click="newRunbook">New runbook</button>
    </header>

    <section class="er-toolbar">
      <input v-model.trim="filters.search" type="search" placeholder="Search title, system, symptoms, cause" @input="loadRunbooks" />
      <select v-model="filters.system" @change="loadRunbooks">
        <option value="">All systems</option>
        <option v-for="system in systems" :key="system.id" :value="system.name">{{ system.name }}</option>
      </select>
      <select v-model="filters.category" @change="loadRunbooks">
        <option value="">All categories</option>
        <option v-for="category in runbookCategories" :key="category" :value="category">{{ category }}</option>
      </select>
    </section>

    <p v-if="error" class="er-error">{{ error }}</p>

    <table class="er-table">
      <thead>
        <tr>
          <th>Id</th>
          <th>Title</th>
          <th>Related system</th>
          <th>Category</th>
          <th>Last updated</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="runbook in runbooks" :key="runbook.id" @click="openRunbook(runbook.id)">
          <td>{{ runbook.id }}</td>
          <td><strong>{{ runbook.title }}</strong></td>
          <td>{{ runbook.systemName }}</td>
          <td>{{ runbook.category }}</td>
          <td>{{ formatDate(runbook.updatedDate) }}</td>
        </tr>
        <tr v-if="!loading && runbooks.length === 0">
          <td colspan="5">No runbooks found.</td>
        </tr>
      </tbody>
    </table>
  </main>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { engineeringRequestsApi } from '../api';
import { runbookCategories } from '../constants';

const router = useRouter();
const runbooks = ref([]);
const systems = ref([]);
const loading = ref(false);
const error = ref('');

const filters = reactive({
  search: '',
  system: '',
  category: ''
});

async function loadRunbooks() {
  loading.value = true;
  error.value = '';
  try {
    runbooks.value = await engineeringRequestsApi.getRunbooks(filters);
  } catch (err) {
    error.value = err.message;
  } finally {
    loading.value = false;
  }
}

async function loadSystems() {
  systems.value = await engineeringRequestsApi.getSystems();
}

function openRunbook(id) {
  router.push({ name: 'runbookDetails', params: { id } });
}

function newRunbook() {
  router.push({ name: 'newRunbook' });
}

function formatDate(value) {
  return new Date(value).toLocaleDateString();
}

onMounted(async () => {
  await loadSystems();
  await loadRunbooks();
});
</script>

<style scoped>
.er-page {
  padding: 1rem;
}

.er-page-header,
.er-toolbar {
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
  flex-wrap: wrap;
  margin-bottom: 1rem;
}

input,
select,
button {
  border: 1px solid #cfd6df;
  border-radius: 4px;
  padding: 0.55rem 0.65rem;
  font: inherit;
}

input[type='search'] {
  min-width: 320px;
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

.er-error {
  color: #8b1d10;
}
</style>
