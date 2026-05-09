<template>
  <main class="er-page">
    <header>
      <h1>My Request Submission Link</h1>
      <p>Send this link to colleagues so submitted requests arrive in your triage inbox.</p>
    </header>

    <p v-if="error" class="er-error">{{ error }}</p>

    <section v-if="link" class="er-panel">
      <label>
        Submission link
        <input :value="submissionUrl" readonly />
      </label>
      <div class="er-actions">
        <button type="button" @click="copyLink">Copy link</button>
      </div>
      <p>Owner: {{ link.ownerUserName }}</p>
      <p>Display name: {{ link.displayName || link.ownerUserName }}</p>
      <p>Status: {{ link.isActive ? 'Active' : 'Inactive' }}</p>
    </section>
  </main>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue';
import { engineeringRequestsApi } from '../api';

const link = ref(null);
const error = ref('');

const submissionUrl = computed(() => {
  if (!link.value) return '';
  return `${window.location.origin}/Requests/Submit/${link.value.token}`;
});

async function loadLink() {
  error.value = '';
  try {
    link.value = await engineeringRequestsApi.getMySubmissionLink();
  } catch (err) {
    error.value = err.message;
  }
}

async function copyLink() {
  await navigator.clipboard.writeText(submissionUrl.value);
}

onMounted(loadLink);
</script>

<style scoped>
.er-page { padding: 1rem; display: grid; gap: 1rem; max-width: 820px; }
h1, p { margin: 0; } h1 { font-size: 1.5rem; } p { color: #5d6875; margin-top: 0.25rem; }
.er-panel { border: 1px solid #d9dee5; border-radius: 6px; padding: 1rem; background: #fff; display: grid; gap: 0.75rem; }
label { display: grid; gap: 0.35rem; font-weight: 700; }
input, button { border: 1px solid #cfd6df; border-radius: 4px; padding: 0.6rem 0.7rem; font: inherit; }
button { background: #1f5e8c; color: #fff; border-color: #1f5e8c; font-weight: 700; }
.er-actions { display: flex; gap: 0.75rem; }
.er-error { color: #8b1d10; }
</style>
