<template>
  <section class="er-summary-grid">
    <article class="er-summary-card">
      <span class="er-summary-label">Open P1</span>
      <strong>{{ summary?.openP1Count ?? 0 }}</strong>
    </article>
    <article class="er-summary-card">
      <span class="er-summary-label">This week</span>
      <strong>{{ summary?.requestsThisWeek ?? 0 }}</strong>
    </article>
  </section>
</template>

<script setup>
defineProps({
  summary: {
    type: Object,
    default: null
  }
});
</script>

<style scoped>
.er-summary-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
  gap: 0.75rem;
}

.er-summary-card {
  border: 1px solid #d9dee5;
  border-radius: 6px;
  padding: 0.875rem;
  background: #ffffff;
}

.er-summary-label {
  display: block;
  color: #5d6875;
  font-size: 0.875rem;
  margin-bottom: 0.35rem;
}

strong {
  font-size: 1.75rem;
  line-height: 1;
}
</style>
