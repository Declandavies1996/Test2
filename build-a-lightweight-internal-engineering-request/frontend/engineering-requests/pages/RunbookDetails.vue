<template>
  <main class="er-page">
    <button type="button" class="er-link" @click="router.back()">Back</button>

    <header class="er-page-header">
      <div>
        <h1>{{ isNew ? 'New Runbook' : form.title }}</h1>
        <p>{{ isNew ? 'Create a reusable fix guide.' : `${form.systemName} - ${form.category}` }}</p>
      </div>
      <button type="button" class="er-primary" @click="saveRunbook">Save</button>
    </header>

    <p v-if="error" class="er-error">{{ error }}</p>

    <section class="er-form">
      <div class="er-form-grid">
        <label>
          Title
          <input v-model.trim="form.title" required />
        </label>
        <label>
          Related system
          <select v-model="form.systemName" required>
            <option value="" disabled>System</option>
            <option v-for="system in systems" :key="system.id" :value="system.name">{{ system.name }}</option>
          </select>
        </label>
        <label>
          Category
          <select v-model="form.category">
            <option v-for="category in runbookCategories" :key="category" :value="category">{{ category }}</option>
          </select>
        </label>
      </div>

      <label>
        Problem
        <textarea v-model.trim="form.problem" rows="4"></textarea>
      </label>
      <label>
        Symptoms
        <textarea v-model.trim="form.symptoms" rows="4"></textarea>
      </label>
      <label>
        Cause
        <textarea v-model.trim="form.cause" rows="4"></textarea>
      </label>
      <label>
        Fix steps
        <textarea v-model.trim="form.fixSteps" rows="7"></textarea>
      </label>
      <label>
        Resolution steps
        <textarea v-model.trim="form.resolutionSteps" rows="7"></textarea>
      </label>
      <label>
        Verification steps
        <textarea v-model.trim="form.verificationSteps" rows="5"></textarea>
      </label>
      <label>
        Known risks
        <textarea v-model.trim="form.knownRisks" rows="4"></textarea>
      </label>
      <label>
        Notes
        <textarea v-model.trim="form.notes" rows="4"></textarea>
      </label>

      <dl v-if="!isNew" class="er-dates">
        <div>
          <dt>Created</dt>
          <dd>{{ formatDateTime(createdDate) }}</dd>
        </div>
        <div>
          <dt>Updated</dt>
          <dd>{{ formatDateTime(updatedDate) }}</dd>
        </div>
      </dl>
    </section>
  </main>
</template>

<script setup>
import { computed, onMounted, reactive, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { engineeringRequestsApi } from '../api';
import { runbookCategories } from '../constants';

const props = defineProps({
  id: {
    type: [String, Number],
    default: null
  }
});

const router = useRouter();
const systems = ref([]);
const error = ref('');
const createdDate = ref(null);
const updatedDate = ref(null);

const isNew = computed(() => !props.id);

const form = reactive({
  title: '',
  systemName: '',
  category: 'Troubleshooting',
  problem: '',
  symptoms: '',
  cause: '',
  fixSteps: '',
  resolutionSteps: '',
  verificationSteps: '',
  knownRisks: '',
  notes: ''
});

function syncForm(runbook) {
  Object.assign(form, {
    title: runbook.title,
    systemName: runbook.systemName,
    category: runbook.category,
    problem: runbook.problem || '',
    symptoms: runbook.symptoms || '',
    cause: runbook.cause || '',
    fixSteps: runbook.fixSteps || '',
    resolutionSteps: runbook.resolutionSteps || '',
    verificationSteps: runbook.verificationSteps || '',
    knownRisks: runbook.knownRisks || '',
    notes: runbook.notes || ''
  });
  createdDate.value = runbook.createdDate;
  updatedDate.value = runbook.updatedDate;
}

async function loadSystems() {
  systems.value = await engineeringRequestsApi.getSystems();
  if (!form.systemName) {
    form.systemName = systems.value[0]?.name || '';
  }
}

async function loadRunbook() {
  if (isNew.value) return;
  const runbook = await engineeringRequestsApi.getRunbook(props.id);
  syncForm(runbook);
}

async function saveRunbook() {
  error.value = '';
  try {
    if (isNew.value) {
      const result = await engineeringRequestsApi.createRunbook({ ...form });
      router.replace({ name: 'runbookDetails', params: { id: result.id } });
    } else {
      await engineeringRequestsApi.updateRunbook(props.id, { ...form });
      await loadRunbook();
    }
  } catch (err) {
    error.value = err.message;
  }
}

function formatDateTime(value) {
  return value ? new Date(value).toLocaleString() : '';
}

onMounted(async () => {
  await loadSystems();
  await loadRunbook();
});

watch(
  () => props.id,
  async () => {
    await loadRunbook();
  }
);
</script>

<style scoped>
.er-page {
  padding: 1rem;
}

.er-link {
  background: transparent;
  border: 0;
  color: #1f5e8c;
  padding: 0;
  margin-bottom: 1rem;
}

.er-page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.75rem;
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
  gap: 0.875rem;
}

.er-form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 0.75rem;
}

label {
  display: grid;
  gap: 0.35rem;
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

.er-dates {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  margin: 0;
  color: #5d6875;
}

dt {
  font-weight: 700;
}

dd {
  margin: 0.2rem 0 0;
}

.er-error {
  color: #8b1d10;
}
</style>
