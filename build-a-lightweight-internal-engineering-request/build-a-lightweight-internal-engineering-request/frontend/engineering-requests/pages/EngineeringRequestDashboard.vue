<template>
  <main class="er-page">
    <header class="er-page-header">
      <div>
        <h1>Engineering Request Summary</h1>
        <p>Simple operational visibility across supported systems.</p>
      </div>
      <button type="button" @click="loadSummary">Refresh</button>
    </header>

    <p v-if="error" class="er-error">{{ error }}</p>

    <SummaryCards :summary="summary" />

    <section class="er-grid">
      <article class="er-panel">
        <h2>Requests by system</h2>
        <ul>
          <li v-for="item in summary?.requestsBySystem || []" :key="item.name">
            <span>{{ item.name }}</span>
            <strong>{{ item.count }}</strong>
          </li>
        </ul>
      </article>

      <article class="er-panel">
        <h2>Requests by type</h2>
        <ul>
          <li v-for="item in summary?.requestsByType || []" :key="item.name">
            <span>{{ item.name }}</span>
            <strong>{{ item.count }}</strong>
          </li>
        </ul>
      </article>
    </section>
  </main>
</template>

<script setup>
import { onMounted, ref } from 'vue';
import { engineeringRequestsApi } from '../api';
import SummaryCards from '../components/SummaryCards.vue';

const summary = ref(null);
const error = ref('');

async function loadSummary() {
  error.value = '';
  try {
    summary.value = await engineeringRequestsApi.getSummary();
  } catch (err) {
    error.value = err.message;
  }
}

onMounted(loadSummary);
</script>

<style scoped>
.er-page {
  padding: 1rem;
}

.er-page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1rem;
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

button {
  border: 1px solid #cfd6df;
  border-radius: 4px;
  padding: 0.55rem 0.65rem;
  font: inherit;
}

.er-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 0.75rem;
  margin-top: 0.75rem;
}

.er-panel {
  border: 1px solid #d9dee5;
  border-radius: 6px;
  padding: 0.875rem;
  background: #ffffff;
}

ul {
  list-style: none;
  padding: 0;
  margin: 0;
  display: grid;
  gap: 0.5rem;
}

li {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  border-bottom: 1px solid #edf0f3;
  padding-bottom: 0.5rem;
}

li:last-child {
  border-bottom: 0;
  padding-bottom: 0;
}

.er-error {
  color: #8b1d10;
  margin-bottom: 0.75rem;
}
</style>
