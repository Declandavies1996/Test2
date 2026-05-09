<template>
  <main class="er-page">
    <header>
      <h1>Submit Engineering Request</h1>
      <p>Your request will be reviewed and categorised during triage.</p>
      <p v-if="linkInfo">This request will be routed to {{ linkInfo.displayName || 'the link owner' }}.</p>
    </header>

    <RequestGuidance />

    <form class="er-form" @submit.prevent="submit">
      <input v-model.trim="form.title" required placeholder="Title" />
      <textarea v-model.trim="form.description" rows="4" placeholder="Description"></textarea>
      <textarea v-model.trim="form.businessReason" rows="3" placeholder="Business / engineering reason"></textarea>
      <textarea v-model.trim="form.expectedBehaviour" rows="3" placeholder="What did you expect to happen?"></textarea>
      <textarea v-model.trim="form.actualBehaviour" rows="3" placeholder="What actually happened?"></textarea>
      <div class="er-grid">
        <select v-model="form.suggestedSystemName">
          <option value="">Suggested system (optional)</option>
          <option v-for="system in systems" :key="system.id" :value="system.name">{{ system.name }}</option>
        </select>
        <input v-model.trim="form.department" placeholder="Department (optional)" />
      </div>
      <textarea v-model.trim="form.urgencyExplanation" rows="3" placeholder="Urgency explanation (optional)"></textarea>
      <input type="file" multiple accept=".msg,.pdf,.xlsx,.xls,.csv,.txt,.log,.png,.jpg,.jpeg,.zip" @change="setFiles" />
      <button type="submit">Submit request</button>
    </form>

    <p v-if="error" class="er-error">{{ error }}</p>

    <section v-if="confirmation" class="er-confirmation">
      <h2>Request submitted</h2>
      <p><strong>ID:</strong> {{ confirmation.id }}</p>
      <p><strong>Title:</strong> {{ confirmation.title }}</p>
      <p><strong>Submitted:</strong> {{ formatDate(confirmation.submittedDate) }}</p>
      <p><strong>Submitted by:</strong> {{ confirmation.submittedByUserName }}</p>
      <p>Your request has been submitted and will be reviewed. Priority, system, and category will be assigned during triage.</p>
    </section>
  </main>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue';
import { useRoute } from 'vue-router';
import { engineeringRequestsApi } from '../api';
import RequestGuidance from '../components/RequestGuidance.vue';

const route = useRoute();
const systems = ref([]);
const files = ref([]);
const error = ref('');
const confirmation = ref(null);
const ownerToken = ref('');
const linkInfo = ref(null);
const form = reactive({ title: '', description: '', businessReason: '', expectedBehaviour: '', actualBehaviour: '', suggestedSystemName: '', urgencyExplanation: '', department: '' });

function setFiles(event) {
  files.value = Array.from(event.target.files || []);
}

async function submit() {
  error.value = '';
  confirmation.value = null;
  try {
    confirmation.value = await engineeringRequestsApi.submitRequest({ ...form }, files.value, ownerToken.value);
  } catch (err) {
    error.value = err.message;
  }
}

function formatDate(value) {
  return new Date(value).toLocaleString();
}

onMounted(async () => {
  ownerToken.value = route.params.ownerToken || '';
  systems.value = await engineeringRequestsApi.getSystems();
  if (ownerToken.value) {
    linkInfo.value = await engineeringRequestsApi.getSubmissionLink(ownerToken.value);
  }
});
</script>

<style scoped>
.er-page { padding: 1rem; display: grid; gap: 1rem; max-width: 960px; }
h1, h2, p { margin: 0; } h1 { font-size: 1.5rem; } p { color: #5d6875; margin-top: 0.25rem; }
.er-form { display: grid; gap: 0.75rem; }
.er-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 0.75rem; }
input, select, textarea, button { border: 1px solid #cfd6df; border-radius: 4px; padding: 0.6rem 0.7rem; font: inherit; }
button { background: #1f5e8c; color: #fff; border-color: #1f5e8c; font-weight: 700; }
.er-confirmation { border: 1px solid #b7ddc2; background: #f1fbf4; border-radius: 6px; padding: 1rem; display: grid; gap: 0.35rem; }
.er-error { color: #8b1d10; }
</style>
